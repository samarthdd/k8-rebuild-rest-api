using System;
using System.ComponentModel.DataAnnotations;
using Glasswall.CloudSdk.Common;
using Glasswall.CloudSdk.Common.Web.Abstraction;
using Glasswall.CloudSdk.Common.Web.Models;
using Glasswall.Core.Engine.Common.FileProcessing;
using Glasswall.Core.Engine.Common.PolicyConfig;
using Glasswall.Core.Engine.Messaging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Glasswall.CloudSdk.AWS.Rebuild.Controllers
{
    public class RebuildController : CloudSdkController<RebuildController>
    {
        private readonly IGlasswallVersionService _glasswallVersionService;
        private readonly IFileTypeDetector _fileTypeDetector;
        private readonly IFileProtector _fileProtector;

        public RebuildController(
            IGlasswallVersionService glasswallVersionService,
            IFileTypeDetector fileTypeDetector,
            IFileProtector fileProtector,
            IMetricService metricService,
            ILogger<RebuildController> logger) : base(logger, metricService)
        {
            _glasswallVersionService = glasswallVersionService ?? throw new ArgumentNullException(nameof(glasswallVersionService));
            _fileTypeDetector = fileTypeDetector ?? throw new ArgumentNullException(nameof(fileTypeDetector));
            _fileProtector = fileProtector ?? throw new ArgumentNullException(nameof(fileProtector));
        }

        [HttpPost("file")]
        public IActionResult RebuildFromFormFile([FromForm]string contentManagementFlagJson, [FromForm][Required]IFormFile file)
        {
            try
            {
                Logger.LogInformation("'{0}' method invoked", nameof(RebuildFromFormFile));

                ContentManagementFlags contentManagementFlags = null;
                if (!string.IsNullOrWhiteSpace(contentManagementFlagJson))
                    contentManagementFlags = Newtonsoft.Json.JsonConvert.DeserializeObject<ContentManagementFlags>(contentManagementFlagJson);

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (!TryReadFormFile(file, out var fileBytes))
                    return BadRequest("Input file could not be read.");

                RecordEngineVersion();

                var fileType = DetectFromBytes(fileBytes);

                if (fileType.FileType == FileType.Unknown)
                    return UnprocessableEntity("File could not be determined to be a supported file");

                var protectedFileResponse = RebuildFromBytes(
                    contentManagementFlags, fileType.FileTypeName, fileBytes);

                if (!string.IsNullOrWhiteSpace(protectedFileResponse.ErrorMessage))
                {
                    if (protectedFileResponse.IsDisallowed)
                        return Ok(protectedFileResponse);

                    return UnprocessableEntity(
                        $"File could not be rebuilt. Error Message: {protectedFileResponse.ErrorMessage}");
                }

                return new FileContentResult(protectedFileResponse.ProtectedFile, "application/octet-stream") { FileDownloadName = file.FileName ?? "Unknown" };
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Exception occured processing file: {e.Message}");
                throw;
            }
        }

        [HttpPost("base64")]
        public IActionResult RebuildFromBase64([FromBody][Required]Base64Request request)
        {
            try
            {
                Logger.LogInformation("'{0}' method invoked", nameof(RebuildFromBase64));

                if (!ModelState.IsValid) 
                    return BadRequest(ModelState);
                
                if (!TryGetBase64File(request.Base64, out var file))
                    return BadRequest("Input file could not be decoded from base64.");

                RecordEngineVersion();

                var fileType = DetectFromBytes(file);

                if (fileType.FileType == FileType.Unknown)
                    return UnprocessableEntity("File could not be determined to be a supported file");

                var protectedFileResponse = RebuildFromBytes(
                    request.ContentManagementFlags, fileType.FileTypeName, file);

                if (!string.IsNullOrWhiteSpace(protectedFileResponse.ErrorMessage))
                {
                    if (protectedFileResponse.IsDisallowed)
                        return Ok(protectedFileResponse);

                    return UnprocessableEntity(
                        $"File could not be rebuilt. Error Message: {protectedFileResponse.ErrorMessage}");
                }

                return Ok(Convert.ToBase64String(protectedFileResponse.ProtectedFile));
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Exception occured processing file: {e.Message}");
                throw;
            }
        }
        
        [HttpPost]
        public IActionResult RebuildUrlToUrl([FromBody][Required] UrlToUrlRequest request)
        {
            try
            {
                Logger.LogInformation("'{0}' method invoked", nameof(RebuildUrlToUrl));

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (!TryGetFile(request.InputGetUrl, out var file))
                    return BadRequest("Input file could not be downloaded.");

                RecordEngineVersion();

                var fileType = DetectFromBytes(file);

                if (fileType.FileType == FileType.Unknown)
                    return UnprocessableEntity("File could not be determined to be a supported file");

                var protectedFileResponse = RebuildFromBytes(
                    request.ContentManagementFlags, fileType.FileTypeName, file);

                if (!string.IsNullOrWhiteSpace(protectedFileResponse.ErrorMessage))
                {
                    if (protectedFileResponse.IsDisallowed)
                        return Ok(protectedFileResponse);

                    return UnprocessableEntity(
                        $"File could not be rebuilt. Error Message: {protectedFileResponse.ErrorMessage}");
                }

                if (!TryPutFile(request.OutputPutUrl, protectedFileResponse.ProtectedFile))
                    return BadRequest("Could not put protected file to the supplied output url");

                return Ok();
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Exception occured processing file: {e.Message}");
                throw;
            }
        }

        private void RecordEngineVersion()
        {
            var version = _glasswallVersionService.GetVersion();
            MetricService.Record(Metric.Version, version);
        }

        private FileTypeDetectionResponse DetectFromBytes(byte[] bytes)
        {
            TimeMetricTracker.Restart();
            var fileTypeResponse = _fileTypeDetector.DetermineFileType(bytes);
            TimeMetricTracker.Stop();
            
            MetricService.Record(Metric.DetectFileTypeTime, TimeMetricTracker.Elapsed);
            return fileTypeResponse;
        }

        private IFileProtectResponse RebuildFromBytes(ContentManagementFlags contentManagementFlags, string fileType, byte[] bytes)
        {
            contentManagementFlags = contentManagementFlags.ValidatedOrDefault();

            TimeMetricTracker.Restart();
            var response = _fileProtector.GetProtectedFile(contentManagementFlags, fileType, bytes);
            TimeMetricTracker.Stop();

            MetricService.Record(Metric.RebuildTime, TimeMetricTracker.Elapsed);
            return response;
        }
    }
}
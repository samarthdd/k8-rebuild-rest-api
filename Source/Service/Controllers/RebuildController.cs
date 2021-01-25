using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Glasswall.CloudSdk.AWS.Rebuild.Models;
using Glasswall.CloudSdk.AWS.Rebuild.Services;
using Glasswall.CloudSdk.Common;
using Glasswall.CloudSdk.Common.Web.Abstraction;
using Glasswall.CloudSdk.Common.Web.Models;
using Glasswall.Core.Engine.Common.FileProcessing;
using Glasswall.Core.Engine.Common.PolicyConfig;
using Glasswall.Core.Engine.Messaging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Glasswall.CloudSdk.AWS.Rebuild.Controllers
{
    public class RebuildController : CloudSdkController<RebuildController>
    {
        private readonly IGlasswallVersionService _glasswallVersionService;
        private readonly IFileTypeDetector _fileTypeDetector;
        private readonly IFileProtector _fileProtector;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IZipUtility _zipUtility;

        public RebuildController(
            IGlasswallVersionService glasswallVersionService,
            IFileTypeDetector fileTypeDetector,
            IFileProtector fileProtector,
            IMetricService metricService,
            ILogger<RebuildController> logger,
            IWebHostEnvironment hostingEnvironment,
            IZipUtility zipUtility) : base(logger, metricService)
        {
            _glasswallVersionService = glasswallVersionService ?? throw new ArgumentNullException(nameof(glasswallVersionService));
            _fileTypeDetector = fileTypeDetector ?? throw new ArgumentNullException(nameof(fileTypeDetector));
            _fileProtector = fileProtector ?? throw new ArgumentNullException(nameof(fileProtector));
            _hostingEnvironment = hostingEnvironment ?? throw new ArgumentNullException(nameof(hostingEnvironment));
            _zipUtility = zipUtility ?? throw new ArgumentNullException(nameof(zipUtility));
        }

        [HttpPost("file")]
        public async Task<IActionResult> RebuildFromFormFile([FromForm] string contentManagementFlagJson, [FromForm][Required] IFormFile file)
        {
            try
            {
                Logger.LogInformation("'{0}' method invoked", nameof(RebuildFromFormFile));

                ContentManagementFlags contentManagementFlags = null;
                if (!string.IsNullOrWhiteSpace(contentManagementFlagJson))
                    contentManagementFlags = await Task.Run(() => JsonConvert.DeserializeObject<ContentManagementFlags>(contentManagementFlagJson));

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (!TryReadFormFile(file, out byte[] fileBytes))
                    return BadRequest("Input file could not be read.");

                RecordEngineVersion();

                FileTypeDetectionResponse fileType = await Task.Run(() => DetectFromBytes(fileBytes));

                if (fileType.FileType == FileType.Unknown)
                    return UnprocessableEntity("File could not be determined to be a supported file");

                IFileProtectResponse protectedFileResponse = await Task.Run(() => RebuildFromBytes(
                    contentManagementFlags, fileType.FileTypeName, fileBytes));

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

        [HttpPost("zipfile")]
        public async Task<IActionResult> RebuildFromFormZipFile([FromForm] string contentManagementFlagJson, [FromForm][Required] IFormFile file)
        {
            string uploads = Path.Combine(_hostingEnvironment.ContentRootPath, Constants.UPLOADS_FOLDER);
            string tempFolderPath = Path.Combine(uploads, Guid.NewGuid().ToString());

            try
            {
                Logger.LogInformation("'{0}' method invoked", nameof(RebuildFromFormZipFile));

                ContentManagementFlags contentManagementFlags = null;
                if (!string.IsNullOrWhiteSpace(contentManagementFlagJson))
                    contentManagementFlags = await Task.Run(() => JsonConvert.DeserializeObject<ContentManagementFlags>(contentManagementFlagJson));

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (!TryReadFormFile(file, out byte[] fileBytes))
                    return BadRequest("Input file could not be read.");

                RecordEngineVersion();

                FileTypeDetectionResponse fileType = await Task.Run(() => DetectFromBytes(fileBytes));

                if (fileType.FileType != FileType.Zip)
                    return UnprocessableEntity("Input file could not be processed.");

                string zipFolderName = $"{Guid.NewGuid()}";
                string protectedZipFolderPath = Path.Combine(tempFolderPath, Guid.NewGuid().ToString());
                string zipFolderPath = Path.Combine(tempFolderPath, zipFolderName);
                string zipFilePath = $"{zipFolderPath}.{fileType.FileTypeName}";
                if (!Directory.Exists(uploads))
                {
                    Directory.CreateDirectory(uploads);
                }

                if (!Directory.Exists(tempFolderPath))
                {
                    Directory.CreateDirectory(tempFolderPath);
                }

                if (!Directory.Exists(protectedZipFolderPath))
                {
                    Directory.CreateDirectory(protectedZipFolderPath);
                }

                using (Stream fileStream = new FileStream(zipFilePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                _zipUtility.ExtractZipFile(zipFilePath, null, zipFolderPath);
                List<IFileProtectResponse> processDirectoryResp = await ProcessDirectory(zipFolderPath, protectedZipFolderPath, contentManagementFlags);
                string statusMessage = string.Empty;
                processDirectoryResp.Cast<IFileProcessStatus>().ToList().ForEach(x =>
                {
                    if (!string.IsNullOrWhiteSpace(x.ErrorMessage) && !x.IsDisallowed)
                    {
                        statusMessage += $"An error {x.ErrorMessage} occurred while processing the file {x.FileName}{Environment.NewLine}";
                    }
                    else
                    {
                        statusMessage += $"File {x.FileName} is successfully processed.{Environment.NewLine}";
                    }

                    using StreamWriter sw = System.IO.File.CreateText(Path.Combine(protectedZipFolderPath, Constants.STATUS_FILE));
                    sw.WriteLine(statusMessage);
                });

                if (processDirectoryResp.All(x => !string.IsNullOrEmpty(x.ErrorMessage) && !x.IsDisallowed))
                {
                    return BadRequest("Input file could not be read.");
                }

                _zipUtility.CreateZipFile($"{protectedZipFolderPath}.{FileType.Zip}", null, protectedZipFolderPath);
                byte[] protectedZipBytes = System.IO.File.ReadAllBytes($"{protectedZipFolderPath}.{FileType.Zip}");
                return new FileContentResult(protectedZipBytes, "application/octet-stream") { FileDownloadName = file.FileName ?? "Unknown" };
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Exception occured processing file: {e.Message}");
                throw;
            }
            finally
            {
                if (Directory.Exists(tempFolderPath))
                    Directory.Delete(tempFolderPath, true);
            }
        }

        [HttpPost("s3tozip")]
        public async Task<IActionResult> RebuildFromFormS3ToZipFile([FromForm] string contentManagementFlagJson, [FromForm][Required] string presignedURL)
        {
            string uploads = Path.Combine(_hostingEnvironment.ContentRootPath, Constants.UPLOADS_FOLDER);
            string tempFolderPath = Path.Combine(uploads, Guid.NewGuid().ToString());
            try
            {
                Logger.LogInformation("'{0}' method invoked", nameof(RebuildFromFormS3ToZipFile));

                ContentManagementFlags contentManagementFlags = null;
                if (!string.IsNullOrWhiteSpace(contentManagementFlagJson))
                    contentManagementFlags = await Task.Run(() => JsonConvert.DeserializeObject<ContentManagementFlags>(contentManagementFlagJson));

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                AmazonS3Client amazonS3Client = new AmazonS3Client(Environment.GetEnvironmentVariable(Constants.EnvironmentVariables.AWS_ACCESS_KEY_ID),
                                                                   Environment.GetEnvironmentVariable(Constants.EnvironmentVariables.AWS_SECRET_ACCESS_KEY),
                                                                   RegionEndpoint.EUWest1);

                AmazonS3Uri amazonS3Uri = new AmazonS3Uri(presignedURL);
                GetObjectRequest request = new GetObjectRequest()
                {
                    BucketName = amazonS3Uri.Bucket,
                    Key = amazonS3Uri.Key
                };

                GetObjectResponse s3objectResponse = await amazonS3Client.GetObjectAsync(request);

                MemoryStream memStream = new MemoryStream();
                s3objectResponse.ResponseStream.CopyTo(memStream);
                memStream.Seek(0, SeekOrigin.Begin);
                IFormFile file = new FormFile(memStream, 0, memStream.Length, null, Path.GetFileName(amazonS3Uri.Key));
                if (!TryReadFormFile(file, out byte[] fileBytes))
                    return BadRequest("Input file could not be read.");

                RecordEngineVersion();

                FileTypeDetectionResponse fileType = await Task.Run(() => DetectFromBytes(fileBytes));

                if (fileType.FileType != FileType.Zip)
                    return UnprocessableEntity("Input file could not be processed.");

                string zipFolderName = $"{Guid.NewGuid()}";
                string protectedZipFolderPath = Path.Combine(tempFolderPath, Guid.NewGuid().ToString());
                string zipFolderPath = Path.Combine(tempFolderPath, zipFolderName);
                string zipFilePath = $"{zipFolderPath}.{fileType.FileTypeName}";
                if (!Directory.Exists(uploads))
                {
                    Directory.CreateDirectory(uploads);
                }

                if (!Directory.Exists(tempFolderPath))
                {
                    Directory.CreateDirectory(tempFolderPath);
                }

                if (!Directory.Exists(protectedZipFolderPath))
                {
                    Directory.CreateDirectory(protectedZipFolderPath);
                }

                using (Stream fileStream = new FileStream(zipFilePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                _zipUtility.ExtractZipFile(zipFilePath, null, zipFolderPath);
                List<IFileProtectResponse> processDirectoryResp = await ProcessDirectory(zipFolderPath, protectedZipFolderPath, contentManagementFlags);
                string statusMessage = string.Empty;
                processDirectoryResp.Cast<IFileProcessStatus>().ToList().ForEach(x =>
                {
                    if (!string.IsNullOrWhiteSpace(x.ErrorMessage) && !x.IsDisallowed)
                    {
                        statusMessage += $"An error {x.ErrorMessage} occurred while processing the file {x.FileName}{Environment.NewLine}";
                    }
                    else
                    {
                        statusMessage += $"File {x.FileName} is successfully processed.{Environment.NewLine}";
                    }

                    using StreamWriter sw = System.IO.File.CreateText(Path.Combine(protectedZipFolderPath, Constants.STATUS_FILE));
                    sw.WriteLine(statusMessage);
                });

                if (processDirectoryResp.All(x => !string.IsNullOrEmpty(x.ErrorMessage) && !x.IsDisallowed))
                {
                    return BadRequest("Input file could not be read.");
                }

                _zipUtility.CreateZipFile($"{protectedZipFolderPath}.{FileType.Zip}", null, protectedZipFolderPath);
                byte[] protectedZipBytes = System.IO.File.ReadAllBytes($"{protectedZipFolderPath}.{FileType.Zip}");
                await memStream.DisposeAsync();
                return new FileContentResult(protectedZipBytes, "application/octet-stream") { FileDownloadName = file.FileName ?? "Unknown" };
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Exception occured processing file: {e.Message}");
                throw;
            }
            finally
            {
                if (Directory.Exists(tempFolderPath))
                    Directory.Delete(tempFolderPath, true);
            }
        }

        [HttpPost("s3tos3")]
        public async Task<IActionResult> RebuildFromFormS3ToS3([FromForm] string contentManagementFlagJson, [FromForm][Required] string sourcePresignedURL, [FromForm][Required] string targetPresignedURL)
        {
            string uploads = Path.Combine(_hostingEnvironment.ContentRootPath, Constants.UPLOADS_FOLDER);
            string tempFolderPath = Path.Combine(uploads, Guid.NewGuid().ToString());

            try
            {
                Logger.LogInformation("'{0}' method invoked", nameof(RebuildFromFormS3ToS3));

                ContentManagementFlags contentManagementFlags = null;
                if (!string.IsNullOrWhiteSpace(contentManagementFlagJson))
                    contentManagementFlags = await Task.Run(() => JsonConvert.DeserializeObject<ContentManagementFlags>(contentManagementFlagJson));

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                AmazonS3Client amazonS3Client = new AmazonS3Client(Environment.GetEnvironmentVariable(Constants.EnvironmentVariables.AWS_ACCESS_KEY_ID),
                                                                   Environment.GetEnvironmentVariable(Constants.EnvironmentVariables.AWS_SECRET_ACCESS_KEY),
                                                                   RegionEndpoint.EUWest1);
                AmazonS3Uri amazonS3Uri = new AmazonS3Uri(sourcePresignedURL);
                GetObjectRequest request = new GetObjectRequest()
                {
                    BucketName = amazonS3Uri.Bucket,
                    Key = amazonS3Uri.Key
                };

                GetObjectResponse s3objectResponse = await amazonS3Client.GetObjectAsync(request);

                MemoryStream memStream = new MemoryStream();
                s3objectResponse.ResponseStream.CopyTo(memStream);
                memStream.Seek(0, SeekOrigin.Begin);
                IFormFile file = new FormFile(memStream, 0, memStream.Length, null, Path.GetFileName(amazonS3Uri.Key));
                if (!TryReadFormFile(file, out byte[] fileBytes))
                    return BadRequest("Input file could not be read.");

                RecordEngineVersion();

                FileTypeDetectionResponse fileType = await Task.Run(() => DetectFromBytes(fileBytes));

                if (fileType.FileType != FileType.Zip)
                    return UnprocessableEntity("Input file could not be processed.");

                string zipFolderName = $"{Guid.NewGuid()}";
                string protectedZipFolderPath = Path.Combine(tempFolderPath, Guid.NewGuid().ToString());
                string zipFolderPath = Path.Combine(tempFolderPath, zipFolderName);
                string zipFilePath = $"{zipFolderPath}.{fileType.FileTypeName}";
                if (!Directory.Exists(uploads))
                {
                    Directory.CreateDirectory(uploads);
                }

                if (!Directory.Exists(tempFolderPath))
                {
                    Directory.CreateDirectory(tempFolderPath);
                }

                if (!Directory.Exists(protectedZipFolderPath))
                {
                    Directory.CreateDirectory(protectedZipFolderPath);
                }

                using (Stream fileStream = new FileStream(zipFilePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                _zipUtility.ExtractZipFile(zipFilePath, null, zipFolderPath);
                List<IFileProtectResponse> processDirectoryResp = await ProcessDirectory(zipFolderPath, protectedZipFolderPath, contentManagementFlags);
                string statusMessage = string.Empty;
                processDirectoryResp.Cast<IFileProcessStatus>().ToList().ForEach(x =>
                {
                    if (!string.IsNullOrWhiteSpace(x.ErrorMessage) && !x.IsDisallowed)
                    {
                        statusMessage += $"An error {x.ErrorMessage} occurred while processing the file {x.FileName}{Environment.NewLine}";
                    }
                    else
                    {
                        statusMessage += $"File {x.FileName} is successfully processed.{Environment.NewLine}";
                    }

                    using StreamWriter sw = System.IO.File.CreateText(Path.Combine(protectedZipFolderPath, Constants.STATUS_FILE));
                    sw.WriteLine(statusMessage);
                });

                if (processDirectoryResp.All(x => !string.IsNullOrEmpty(x.ErrorMessage) && !x.IsDisallowed))
                {
                    return BadRequest("Input file could not be read.");
                }

                _zipUtility.CreateZipFile($"{protectedZipFolderPath}.{FileType.Zip}", null, protectedZipFolderPath);
                using (Stream fs = System.IO.File.OpenRead($"{protectedZipFolderPath}.{FileType.Zip}"))
                {
                    AmazonS3Uri amazonS3TargetUri = new AmazonS3Uri(targetPresignedURL);
                    PutObjectRequest putRequest = new PutObjectRequest()
                    {
                        InputStream = fs,
                        BucketName = amazonS3TargetUri.Bucket,
                        Key = amazonS3TargetUri.Key
                    };

                    PutObjectResponse response = await amazonS3Client.PutObjectAsync(putRequest);
                    await memStream.DisposeAsync();
                    if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
                    {
                        return BadRequest("S3 target updation failed.");
                    }
                }

                return Ok();
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Exception occured processing file: {e.Message}");
                throw;
            }
            finally
            {
                if (Directory.Exists(tempFolderPath))
                    Directory.Delete(tempFolderPath, true);
            }
        }

        [HttpPost("ziptos3")]
        public async Task<IActionResult> RebuildFromFormZipFileToS3([FromForm] string contentManagementFlagJson, [FromForm][Required] IFormFile file, [FromForm][Required] string targetPresignedURL)
        {
            string uploads = Path.Combine(_hostingEnvironment.ContentRootPath, Constants.UPLOADS_FOLDER);
            string tempFolderPath = Path.Combine(uploads, Guid.NewGuid().ToString());

            try
            {
                Logger.LogInformation("'{0}' method invoked", nameof(RebuildFromFormZipFileToS3));

                ContentManagementFlags contentManagementFlags = null;
                if (!string.IsNullOrWhiteSpace(contentManagementFlagJson))
                    contentManagementFlags = await Task.Run(() => JsonConvert.DeserializeObject<ContentManagementFlags>(contentManagementFlagJson));

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (!TryReadFormFile(file, out byte[] fileBytes))
                    return BadRequest("Input file could not be read.");

                RecordEngineVersion();

                FileTypeDetectionResponse fileType = await Task.Run(() => DetectFromBytes(fileBytes));

                if (fileType.FileType != FileType.Zip)
                    return UnprocessableEntity("Input file could not be processed.");

                string zipFolderName = $"{Guid.NewGuid()}";
                string protectedZipFolderPath = Path.Combine(tempFolderPath, Guid.NewGuid().ToString());
                string zipFolderPath = Path.Combine(tempFolderPath, zipFolderName);
                string zipFilePath = $"{zipFolderPath}.{fileType.FileTypeName}";
                if (!Directory.Exists(uploads))
                {
                    Directory.CreateDirectory(uploads);
                }

                if (!Directory.Exists(tempFolderPath))
                {
                    Directory.CreateDirectory(tempFolderPath);
                }

                if (!Directory.Exists(protectedZipFolderPath))
                {
                    Directory.CreateDirectory(protectedZipFolderPath);
                }

                using (Stream fileStream = new FileStream(zipFilePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                _zipUtility.ExtractZipFile(zipFilePath, null, zipFolderPath);
                List<IFileProtectResponse> processDirectoryResp = await ProcessDirectory(zipFolderPath, protectedZipFolderPath, contentManagementFlags);
                string statusMessage = string.Empty;
                processDirectoryResp.Cast<IFileProcessStatus>().ToList().ForEach(x =>
                {
                    if (!string.IsNullOrWhiteSpace(x.ErrorMessage) && !x.IsDisallowed)
                    {
                        statusMessage += $"An error {x.ErrorMessage} occurred while processing the file {x.FileName}{Environment.NewLine}";
                    }
                    else
                    {
                        statusMessage += $"File {x.FileName} is successfully processed.{Environment.NewLine}";
                    }

                    using StreamWriter sw = System.IO.File.CreateText(Path.Combine(protectedZipFolderPath, Constants.STATUS_FILE));
                    sw.WriteLine(statusMessage);
                });

                if (processDirectoryResp.All(x => !string.IsNullOrEmpty(x.ErrorMessage) && !x.IsDisallowed))
                {
                    return BadRequest("Input file could not be read.");
                }

                _zipUtility.CreateZipFile($"{protectedZipFolderPath}.{FileType.Zip}", null, protectedZipFolderPath);
                AmazonS3Client amazonS3Client = new AmazonS3Client(Environment.GetEnvironmentVariable(Constants.EnvironmentVariables.AWS_ACCESS_KEY_ID),
                                                                   Environment.GetEnvironmentVariable(Constants.EnvironmentVariables.AWS_SECRET_ACCESS_KEY),
                                                                   RegionEndpoint.EUWest1);
                AmazonS3Uri amazonS3Uri = new AmazonS3Uri(targetPresignedURL);
                using (Stream fs = System.IO.File.OpenRead($"{protectedZipFolderPath}.{FileType.Zip}"))
                {
                    AmazonS3Uri amazonS3TargetUri = new AmazonS3Uri(targetPresignedURL);
                    PutObjectRequest putRequest = new PutObjectRequest()
                    {
                        InputStream = fs,
                        BucketName = amazonS3TargetUri.Bucket,
                        Key = amazonS3TargetUri.Key
                    };

                    PutObjectResponse response = await amazonS3Client.PutObjectAsync(putRequest);
                    if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
                    {
                        return BadRequest("S3 target updation failed.");
                    }

                    return Ok();
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Exception occured processing file: {e.Message}");
                throw;
            }
            finally
            {
                if (Directory.Exists(tempFolderPath))
                    Directory.Delete(tempFolderPath, true);
            }
        }

        [HttpPost("jsontoxml")]
        public async Task<IActionResult> JsonToXml([FromForm][Required] IFormFile file)
        {
            Logger.LogInformation("'{0}' method invoked", nameof(JsonToXml));

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!TryReadFormFile(file, out byte[] fileBytes))
                return BadRequest("Input file could not be read.");

            RecordEngineVersion();

            FileTypeDetectionResponse fileType = await Task.Run(() => DetectFromBytes(fileBytes));

            if (fileType.FileType == FileType.Unknown)
                return UnprocessableEntity("Input file could not be processed.");

            string fileExt = Path.GetExtension(file.FileName);
            if (fileExt?.ToLower() != ".json")
            {
                return UnprocessableEntity("File could not be determined to be a JSON file");
            }

            try
            {
                XDocument xDocument = null;
                using (MemoryStream stream = new MemoryStream(fileBytes))
                {
                    XmlDictionaryReaderQuotas quotas = new XmlDictionaryReaderQuotas();
                    xDocument = XDocument.Load(JsonReaderWriterFactory.CreateJsonReader(stream, quotas));
                }

                byte[] bytes = Encoding.Default.GetBytes(RemoveAllNamespaces(xDocument.Root).ToString());
                return new FileContentResult(bytes, "application/octet-stream")
                {
                    FileDownloadName = !string.IsNullOrWhiteSpace(file.FileName) ? $"{file.FileName.Substring(0, file.FileName.IndexOf(fileExt))}.xml" : "Unknown.xml"
                };
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Exception occured processing file: {ex.Message}");
                throw;
            }
        }

        [HttpPost("base64")]
        public async Task<IActionResult> RebuildFromBase64([FromBody][Required] Base64Request request)
        {
            try
            {
                Logger.LogInformation("'{0}' method invoked", nameof(RebuildFromBase64));

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (!TryGetBase64File(request.Base64, out byte[] file))
                    return BadRequest("Input file could not be decoded from base64.");

                RecordEngineVersion();

                FileTypeDetectionResponse fileType = await Task.Run(() => DetectFromBytes(file));

                if (fileType.FileType == FileType.Unknown)
                    return UnprocessableEntity("File could not be determined to be a supported file");

                IFileProtectResponse protectedFileResponse = await Task.Run(() => RebuildFromBytes(
                    request.ContentManagementFlags, fileType.FileTypeName, file));

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
        public async Task<IActionResult> RebuildUrlToUrl([FromBody][Required] UrlToUrlRequest request)
        {
            try
            {
                Logger.LogInformation("'{0}' method invoked", nameof(RebuildUrlToUrl));

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (!TryGetFile(request.InputGetUrl, out byte[] file))
                    return BadRequest("Input file could not be downloaded.");

                RecordEngineVersion();

                FileTypeDetectionResponse fileType = await Task.Run(() => DetectFromBytes(file));

                if (fileType.FileType == FileType.Unknown)
                    return UnprocessableEntity("File could not be determined to be a supported file");

                IFileProtectResponse protectedFileResponse = await Task.Run(() => RebuildFromBytes(
                    request.ContentManagementFlags, fileType.FileTypeName, file));

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
            string version = _glasswallVersionService.GetVersion();
            MetricService.Record(Metric.Version, version);
        }

        private FileTypeDetectionResponse DetectFromBytes(byte[] bytes)
        {
            TimeMetricTracker.Restart();
            FileTypeDetectionResponse fileTypeResponse = _fileTypeDetector.DetermineFileType(bytes);
            TimeMetricTracker.Stop();

            MetricService.Record(Metric.DetectFileTypeTime, TimeMetricTracker.Elapsed);
            return fileTypeResponse;
        }

        private IFileProtectResponse RebuildFromBytes(ContentManagementFlags contentManagementFlags, string fileType, byte[] bytes)
        {
            contentManagementFlags = contentManagementFlags.ValidatedOrDefault();

            TimeMetricTracker.Restart();
            IFileProtectResponse response = _fileProtector.GetProtectedFile(contentManagementFlags, fileType, bytes);
            TimeMetricTracker.Stop();

            MetricService.Record(Metric.RebuildTime, TimeMetricTracker.Elapsed);
            return response;
        }

        private async Task<List<IFileProtectResponse>> ProcessDirectory(string zipFolderPath,
                                                           string protectedZipFolderPath,
                                                           ContentManagementFlags contentManagementFlags)
        {
            List<IFileProtectResponse> responseList = new List<IFileProtectResponse>();
            // Process the list of files found in the directory.
            foreach (string extractedFile in Directory.GetFiles(zipFolderPath))
            {
                IFileProtectResponse processFileResp = await ProcessFile(extractedFile, protectedZipFolderPath, contentManagementFlags);
                responseList.Add(processFileResp);
            }

            // Recurse into subdirectories of this directory.
            foreach (string subdirectory in Directory.GetDirectories(zipFolderPath))
            {
                if (subdirectory.EndsWith(Constants.MACOSX))
                    continue;

                List<IFileProtectResponse> processDirectoryResp = await ProcessDirectory(subdirectory, protectedZipFolderPath, contentManagementFlags);
                responseList.AddRange(processDirectoryResp);
            }
            return responseList;
        }

        private async Task<IFileProtectResponse> ProcessFile(string extractedFile,
                                                      string protectedZipFolderPath,
                                                      ContentManagementFlags contentManagementFlags)
        {
            using FileStream stream = System.IO.File.OpenRead(extractedFile);
            IFormFile iFormFile = new FormFile(stream, 0, stream.Length, null, Path.GetFileName(stream.Name));

            string fileName = Path.GetFileName(extractedFile);
            IFileProcessStatus fileProcessStatus = new FileProcessStatus { FileName = fileName };
            if (!TryReadFormFile(iFormFile, out byte[] fileBytes))
            {
                fileProcessStatus.ErrorMessage = "Input file could not be read.";
                fileProcessStatus.StatusCode = 400;
                fileProcessStatus.FileName = fileName;
                return fileProcessStatus;
            }

            FileTypeDetectionResponse fileType = await Task.Run(() => DetectFromBytes(fileBytes));

            if (fileType.FileType == FileType.Unknown)
            {
                fileProcessStatus.ErrorMessage = "Input file could not be read.";
                fileProcessStatus.StatusCode = 400;
                fileProcessStatus.FileName = fileName;
                return fileProcessStatus;
            }

            IFileProtectResponse protectedFileResponse = await Task.Run(() => RebuildFromBytes(
            contentManagementFlags, fileType.FileTypeName, fileBytes));

            if (!string.IsNullOrWhiteSpace(protectedFileResponse.ErrorMessage))
            {
                if (protectedFileResponse.IsDisallowed)
                {
                    fileProcessStatus.FileName = fileName;
                    fileProcessStatus.ErrorMessage = protectedFileResponse.ErrorMessage;
                    fileProcessStatus.IsDisallowed = protectedFileResponse.IsDisallowed;
                    fileProcessStatus.Outcome = protectedFileResponse.Outcome;
                    fileProcessStatus.ProtectedFile = protectedFileResponse.ProtectedFile;
                    return protectedFileResponse;
                }

                fileProcessStatus.ErrorMessage = $"File could not be rebuilt. Error Message: {protectedFileResponse.ErrorMessage}";
                fileProcessStatus.StatusCode = 422;
                fileProcessStatus.FileName = fileName;
                return fileProcessStatus;
            }

            System.IO.File.WriteAllBytes(Path.Combine(protectedZipFolderPath, Path.GetFileName(extractedFile)), protectedFileResponse.ProtectedFile);
            return fileProcessStatus;
        }

        private XElement RemoveAllNamespaces(XElement xmlDocument)
        {
            if (!xmlDocument.HasElements)
            {
                XElement xElement = new XElement(xmlDocument.Name.LocalName);
                xElement.Value = xmlDocument.Value;
                return xElement;
            }
            return new XElement(xmlDocument.Name.LocalName, xmlDocument.Elements().Select(el => RemoveAllNamespaces(el)));
        }
    }
}

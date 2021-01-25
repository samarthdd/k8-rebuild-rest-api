using Glasswall.Core.Engine.Common.FileProcessing;

namespace Glasswall.CloudSdk.AWS.Rebuild.Models
{
    public interface IFileProcessStatus : IFileProtectResponse
    {
        public string FileName { get; set; }
        public int StatusCode { get; set; }
        public new bool IsDisallowed { get; set; }
    }
}

using Glasswall.Core.Engine.Common;

namespace Glasswall.CloudSdk.AWS.Rebuild.Models
{
    public class FileProcessStatus : IFileProcessStatus
    {
        public string FileName { get; set; }
        public string ErrorMessage { get; set; }
        public int StatusCode { get; set; }
        public byte[] ProtectedFile { get; set; }
        public EngineOutcome Outcome { get; set; }
        public bool IsDisallowed { get; set; }
    }
}

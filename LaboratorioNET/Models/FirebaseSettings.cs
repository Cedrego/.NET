namespace LaboratorioNET.Models
{
    public class FirebaseSettings
    {
        public string DatabaseUrl { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string AuthDomain { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public string StorageBucket { get; set; } = string.Empty;
        public string MessagingSenderId { get; set; } = string.Empty;
        public string AppId { get; set; } = string.Empty;
        public string ServiceAccountKeyPath { get; set; } = string.Empty;
    }
}
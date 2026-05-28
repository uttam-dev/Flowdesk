namespace FlowDesk.Infrastructure.Services
{
    public class GroqSettings
    {
        public const string SectionName = "Groq";

        public string ApiKey { get; set; } = string.Empty;
        public string Model { get; set; } = "llama3-70b-8192";
        public string BaseUrl { get; set; } = "https://api.groq.com";
    }
}

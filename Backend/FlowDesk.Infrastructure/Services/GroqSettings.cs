namespace FlowDesk.Infrastructure.Services
{
    public class GroqSettings
    {
        public const string SectionName = "Groq";

        public string ApiKey { get; set; } = string.Empty;
        public string Model { get; set; } = "llama-3.1-8b-instant";
        public string FallbackModel { get; set; } = "llama-3.1-8b-instant";
        public string BaseUrl { get; set; } = "https://api.groq.com";
        public int TimeoutSeconds { get; set; } = 10;
        public double Temperature { get; set; } = 0.15;
    }
}

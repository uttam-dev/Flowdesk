namespace FlowDesk.Infrastructure.Services
{
    public class GroqSettings
    {
        public const string SectionName = "Groq";

        public string ApiKey { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string FallbackModel { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
        public int TimeoutSeconds { get; set; } = 10;
        public double Temperature { get; set; } = 0.15;
    }
}

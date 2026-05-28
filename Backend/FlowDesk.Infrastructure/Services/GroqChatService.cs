using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FlowDesk.Application.Features.Chat.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FlowDesk.Infrastructure.Services
{
    public class GroqChatService : IChatbotService
    {
        private readonly HttpClient _httpClient;
        private readonly GroqSettings _settings;
        private readonly ILogger<GroqChatService> _logger;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public GroqChatService(
            IHttpClientFactory httpClientFactory,
            IOptions<GroqSettings> settings,
            ILogger<GroqChatService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("GroqApi");
            _settings = settings.Value;
            _logger = logger;

            _httpClient.BaseAddress = new Uri(_settings.BaseUrl.TrimEnd('/') + "/");
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _settings.ApiKey);
        }

        public async Task<string> GetResponseAsync(
            string message,
            string systemPrompt,
            string contextData,
            CancellationToken cancellationToken)
        {
            try
            {
                var userMessage = string.IsNullOrWhiteSpace(contextData)
                    ? message
                    : $"User Context:\n{contextData}\n\nUser Question:\n{message}";

                var requestBody = new
                {
                    model = _settings.Model,
                    messages = new[]
                    {
                        new { role = "system", content = systemPrompt },
                        new { role = "user", content = userMessage }
                    },
                    max_tokens = 1024,
                    temperature = 0.3
                };

                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(requestBody, JsonOptions),
                    Encoding.UTF8,
                    "application/json");

                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(30));

                _logger.LogInformation("Sending request to Groq API with model {Model}", _settings.Model);

                var response = await _httpClient.PostAsync(
                    "openai/v1/chat/completions",
                    jsonContent,
                    cts.Token);

                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync(cts.Token);
                var result = JsonSerializer.Deserialize<GroqResponse>(responseJson, JsonOptions);

                var reply = result?.Choices?.FirstOrDefault()?.Message?.Content ?? "I'm sorry, I couldn't process that request.";
                return reply;
            }
            catch (TaskCanceledException)
            {
                _logger.LogWarning("Groq API request timed out");
                return "I'm sorry, the request timed out. Please try again.";
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Groq API request failed");
                return "I'm sorry, I'm having trouble connecting to my knowledge base. Please try again later.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in GroqChatService");
                return "An unexpected error occurred. Please try again.";
            }
        }

        private class GroqResponse
        {
            public GroqChoice[]? Choices { get; set; }
        }

        private class GroqChoice
        {
            public GroqMessage? Message { get; set; }
        }

        private class GroqMessage
        {
            public string? Content { get; set; }
        }
    }
}

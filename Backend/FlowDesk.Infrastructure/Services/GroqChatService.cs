using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FlowDesk.Application.Features.Chat.DTOs;
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

        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public GroqChatService(
            HttpClient httpClient,
            IOptions<GroqSettings> settings,
            ILogger<GroqChatService> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _logger = logger;

            _httpClient.BaseAddress = new Uri(_settings.BaseUrl.TrimEnd('/') + "/");
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _settings.ApiKey);
            _httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);
        }

        public async Task<string> GetResponseAsync(
            string message,
            string systemPrompt,
            string contextData,
            IReadOnlyList<ConversationMessage> conversationHistory,
            int maxTokens,
            CancellationToken cancellationToken)
        {
            try
            {
                var userMessage = string.IsNullOrWhiteSpace(contextData)
                    ? message
                    : $"Context: {contextData}\n\nQuestion: {message}";

                var messages = new List<object>
                {
                    new { role = "system", content = systemPrompt }
                };

                foreach (var hist in conversationHistory)
                    messages.Add(new { role = hist.Role, content = hist.Content });

                messages.Add(new { role = "user", content = userMessage });

                var model = maxTokens <= 250 ? _settings.FallbackModel : _settings.Model;

                var requestBody = new
                {
                    model,
                    messages,
                    max_tokens = maxTokens,
                    temperature = _settings.Temperature
                };

                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(requestBody, JsonOpts),
                    Encoding.UTF8,
                    "application/json");

                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(_settings.TimeoutSeconds));

                _logger.LogInformation("Groq model={Model} tokens={Tokens} history={Count}",
                    model, maxTokens, conversationHistory.Count);

                var response = await _httpClient.PostAsync("openai/v1/chat/completions", jsonContent, cts.Token);
                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync(cts.Token);
                var result = JsonSerializer.Deserialize<GroqResponse>(responseJson, JsonOpts);

                return result?.Choices?.FirstOrDefault()?.Message?.Content
                    ?? "Service temporarily unavailable.";
            }
            catch (TaskCanceledException)
            {
                _logger.LogWarning("Groq API timed out after {Timeout}s", _settings.TimeoutSeconds);
                return "Service temporarily unavailable.";
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Groq API request failed");
                return "Service temporarily unavailable. Please try again.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected Groq error");
                return "Service temporarily unavailable.";
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

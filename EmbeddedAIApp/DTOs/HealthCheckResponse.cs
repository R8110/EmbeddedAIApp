using System.Text.Json.Serialization;

namespace EmbeddedAIApp.DTOs;

/// <summary>
/// Health check response
/// </summary>
public class HealthCheckResponse
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("ollamaConnected")]
    public bool OllamaConnected { get; set; }

    [JsonPropertyName("ollamaModel")]
    public string? OllamaModel { get; set; }
}

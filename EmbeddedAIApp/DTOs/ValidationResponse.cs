using System.Text.Json.Serialization;

namespace EmbeddedAIApp.DTOs;

/// <summary>
/// Response from structure validation
/// </summary>
public class ValidationResponse
{
    [JsonPropertyName("isValid")]
    public bool IsValid { get; set; }

    [JsonPropertyName("errors")]
    public List<string> Errors { get; set; } = new();

    [JsonPropertyName("suggestions")]
    public List<string> Suggestions { get; set; } = new();
}

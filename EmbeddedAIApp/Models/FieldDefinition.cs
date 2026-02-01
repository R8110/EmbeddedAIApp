using System.Text.Json.Serialization;

namespace EmbeddedAIApp.Models;

/// <summary>
/// Defines a field in the data structure
/// </summary>
public class FieldDefinition
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("constraints")]
    public Dictionary<string, object>? Constraints { get; set; }

    [JsonPropertyName("fields")]
    public List<FieldDefinition>? Fields { get; set; }
}

using System.Text.Json.Serialization;

namespace EmbeddedAIApp.DTOs;

/// <summary>
/// Request to generate a structure from natural language description
/// </summary>
public class GenerateStructureRequest
{
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}

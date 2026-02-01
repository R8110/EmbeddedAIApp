using System.Text.Json.Serialization;

namespace EmbeddedAIApp.Models;

/// <summary>
/// Defines the structure for data generation
/// </summary>
public class StructureDefinition
{
    [JsonPropertyName("entityName")]
    public string EntityName { get; set; } = string.Empty;

    [JsonPropertyName("fields")]
    public List<FieldDefinition> Fields { get; set; } = new();
}

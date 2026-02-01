using System.Text.Json.Serialization;
using EmbeddedAIApp.Models;

namespace EmbeddedAIApp.DTOs;

/// <summary>
/// Request to generate sample data
/// </summary>
public class GenerateDataRequest
{
    [JsonPropertyName("structure")]
    public StructureDefinition Structure { get; set; } = new();

    [JsonPropertyName("count")]
    public int Count { get; set; } = 10;

    [JsonPropertyName("format")]
    public string Format { get; set; } = "json";
}

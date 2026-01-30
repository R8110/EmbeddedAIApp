using System.Text.Json.Serialization;
using EmbeddedAIApp.Models;

namespace EmbeddedAIApp.DTOs;

/// <summary>
/// Request to validate a structure definition
/// </summary>
public class ValidateStructureRequest
{
    [JsonPropertyName("structure")]
    public StructureDefinition Structure { get; set; } = new();
}

using EmbeddedAIApp.Models;

namespace EmbeddedAIApp.Services;

/// <summary>
/// Service for AI operations using Ollama
/// </summary>
public interface IAIService
{
    /// <summary>
    /// Check if Ollama is available
    /// </summary>
    Task<bool> IsAvailableAsync();

    /// <summary>
    /// Get the current model name
    /// </summary>
    string GetModelName();

    /// <summary>
    /// Validate a structure definition using AI
    /// </summary>
    Task<(bool isValid, List<string> suggestions)> ValidateStructureAsync(StructureDefinition structure);

    /// <summary>
    /// Generate a structure definition from natural language description
    /// </summary>
    Task<StructureDefinition?> GenerateStructureFromDescriptionAsync(string description);

    /// <summary>
    /// Get contextually appropriate data type suggestions
    /// </summary>
    Task<string> SuggestDataTypeAsync(string fieldName, string? context = null);
}

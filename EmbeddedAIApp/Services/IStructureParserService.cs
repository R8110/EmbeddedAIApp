using EmbeddedAIApp.Models;

namespace EmbeddedAIApp.Services;

/// <summary>
/// Service for parsing and validating structure definitions
/// </summary>
public interface IStructureParserService
{
    /// <summary>
    /// Validate a structure definition
    /// </summary>
    (bool isValid, List<string> errors) ValidateStructure(StructureDefinition structure);

    /// <summary>
    /// Get supported field types
    /// </summary>
    List<string> GetSupportedFieldTypes();
}

using EmbeddedAIApp.Models;

namespace EmbeddedAIApp.Services;

/// <summary>
/// Service for generating sample data
/// </summary>
public interface IDataGeneratorService
{
    /// <summary>
    /// Generate sample data based on structure definition
    /// </summary>
    Task<List<Dictionary<string, object>>> GenerateDataAsync(StructureDefinition structure, int count);

    /// <summary>
    /// Convert generated data to CSV format
    /// </summary>
    string ConvertToCsv(List<Dictionary<string, object>> data);

    /// <summary>
    /// Convert generated data to SQL INSERT statements
    /// </summary>
    string ConvertToSql(List<Dictionary<string, object>> data, string tableName);
}

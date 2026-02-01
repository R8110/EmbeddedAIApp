namespace EmbeddedAIApp.Models;

/// <summary>
/// Configuration settings for data generation
/// </summary>
public class DataGenerationSettings
{
    public int DefaultCount { get; set; } = 10;
    public int MaxCount { get; set; } = 10000;
}

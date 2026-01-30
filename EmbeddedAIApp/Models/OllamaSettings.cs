namespace EmbeddedAIApp.Models;

/// <summary>
/// Configuration settings for Ollama integration
/// </summary>
public class OllamaSettings
{
    public string BaseUrl { get; set; } = "http://localhost:11434";
    public string Model { get; set; } = "llama2";
    public int Timeout { get; set; } = 30;
}

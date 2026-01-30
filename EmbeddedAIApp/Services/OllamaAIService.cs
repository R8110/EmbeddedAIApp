using EmbeddedAIApp.Models;
using Microsoft.Extensions.Options;
using OllamaSharp;
using OllamaSharp.Models;
using System.Text;
using System.Text.Json;

namespace EmbeddedAIApp.Services;

/// <summary>
/// Implementation of AI service using Ollama
/// </summary>
public class OllamaAIService : IAIService
{
    private readonly OllamaApiClient _ollamaClient;
    private readonly string _modelName;
    private readonly ILogger<OllamaAIService> _logger;

    public OllamaAIService(IOptions<OllamaSettings> settings, ILogger<OllamaAIService> logger)
    {
        _logger = logger;
        var config = settings.Value;
        _modelName = config.Model;

        var uri = new Uri(config.BaseUrl);
        _ollamaClient = new OllamaApiClient(uri)
        {
            SelectedModel = _modelName
        };

        _logger.LogInformation("OllamaAIService initialized with base URL: {BaseUrl}, Model: {Model}", 
            config.BaseUrl, _modelName);
    }

    public string GetModelName() => _modelName;

    public async Task<bool> IsAvailableAsync()
    {
        try
        {
            _logger.LogInformation("Checking Ollama availability...");
            var models = await _ollamaClient.ListLocalModelsAsync();
            var isAvailable = models.Any(m => m.Name.Contains(_modelName, StringComparison.OrdinalIgnoreCase));
            _logger.LogInformation("Ollama available: {IsAvailable}", isAvailable);
            return isAvailable;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Ollama is not available");
            return false;
        }
    }

    public async Task<(bool isValid, List<string> suggestions)> ValidateStructureAsync(StructureDefinition structure)
    {
        try
        {
            var prompt = $@"Analyze the following data structure definition and provide validation feedback.
Structure: {JsonSerializer.Serialize(structure)}

Please validate:
1. Are all field names valid?
2. Are the data types appropriate?
3. Are there any inconsistencies?
4. Suggest improvements if any.

Respond with 'VALID' if the structure is good, or 'INVALID: <reasons>' if there are issues.
Then provide suggestions on separate lines starting with 'SUGGESTION: '";

            _logger.LogInformation("Sending validation request to Ollama for entity: {EntityName}", structure.EntityName);

            var response = await GenerateCompletionAsync(prompt);
            
            _logger.LogInformation("Received validation response from Ollama");

            var isValid = response.Contains("VALID", StringComparison.OrdinalIgnoreCase) && 
                         !response.Contains("INVALID", StringComparison.OrdinalIgnoreCase);

            var suggestions = new List<string>();
            var lines = response.Split('\n');
            foreach (var line in lines)
            {
                if (line.TrimStart().StartsWith("SUGGESTION:", StringComparison.OrdinalIgnoreCase))
                {
                    suggestions.Add(line.Replace("SUGGESTION:", "", StringComparison.OrdinalIgnoreCase).Trim());
                }
            }

            return (isValid, suggestions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating structure with AI");
            // Graceful degradation - assume valid if AI is unavailable
            return (true, new List<string> { "AI validation unavailable - basic validation passed" });
        }
    }

    public async Task<StructureDefinition?> GenerateStructureFromDescriptionAsync(string description)
    {
        try
        {
            var prompt = $@"Generate a JSON data structure definition based on this description:
""{description}""

Respond ONLY with a valid JSON object in this exact format (no markdown, no code blocks, just raw JSON):
{{
  ""entityName"": ""EntityName"",
  ""fields"": [
    {{
      ""name"": ""FieldName"",
      ""type"": ""string"",
      ""constraints"": {{}}
    }}
  ]
}}

Common types: string, int, decimal, boolean, date, datetime, email, phone, address, zipcode, url, uuid
For nested objects, include a ""fields"" array in the field definition.";

            _logger.LogInformation("Generating structure from description via Ollama");

            var response = await GenerateCompletionAsync(prompt);

            _logger.LogDebug("Raw AI response: {Response}", response);

            // Clean up the response - remove markdown code blocks if present
            var jsonResponse = response.Trim();
            if (jsonResponse.StartsWith("```json"))
            {
                jsonResponse = jsonResponse.Substring(7);
            }
            else if (jsonResponse.StartsWith("```"))
            {
                jsonResponse = jsonResponse.Substring(3);
            }
            
            if (jsonResponse.EndsWith("```"))
            {
                jsonResponse = jsonResponse.Substring(0, jsonResponse.Length - 3);
            }

            jsonResponse = jsonResponse.Trim();

            var structure = JsonSerializer.Deserialize<StructureDefinition>(jsonResponse);
            _logger.LogInformation("Successfully generated structure: {EntityName}", structure?.EntityName);

            return structure;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating structure from description");
            return null;
        }
    }

    public async Task<string> SuggestDataTypeAsync(string fieldName, string? context = null)
    {
        try
        {
            var prompt = $@"Suggest the most appropriate data type for a field named '{fieldName}'.
{(context != null ? $"Context: {context}" : "")}

Choose from: string, int, decimal, boolean, date, datetime, email, phone, address, zipcode, url, uuid

Respond with ONLY the data type name, nothing else.";

            var response = await GenerateCompletionAsync(prompt);
            var suggestion = response.Trim().ToLower();

            _logger.LogInformation("Suggested data type for '{FieldName}': {Suggestion}", fieldName, suggestion);

            return suggestion;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error suggesting data type");
            return "string"; // Default fallback
        }
    }

    private async Task<string> GenerateCompletionAsync(string prompt)
    {
        var responseBuilder = new StringBuilder();
        
        var request = new GenerateRequest
        {
            Model = _modelName,
            Prompt = prompt,
            Stream = false
        };

        await foreach (var streamResponse in _ollamaClient.GenerateAsync(request))
        {
            if (streamResponse?.Response != null)
            {
                responseBuilder.Append(streamResponse.Response);
            }
        }
        
        return responseBuilder.ToString();
    }
}


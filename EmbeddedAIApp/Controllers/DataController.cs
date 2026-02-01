using EmbeddedAIApp.DTOs;
using EmbeddedAIApp.Models;
using EmbeddedAIApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace EmbeddedAIApp.Controllers;

/// <summary>
/// Controller for data generation operations
/// </summary>
[ApiController]
[Route("api/data")]
public class DataController : ControllerBase
{
    private readonly IDataGeneratorService _dataGenerator;
    private readonly IAIService _aiService;
    private readonly IStructureParserService _structureParser;
    private readonly DataGenerationSettings _settings;
    private readonly ILogger<DataController> _logger;

    public DataController(
        IDataGeneratorService dataGenerator,
        IAIService aiService,
        IStructureParserService structureParser,
        IOptions<DataGenerationSettings> settings,
        ILogger<DataController> logger)
    {
        _dataGenerator = dataGenerator;
        _aiService = aiService;
        _structureParser = structureParser;
        _settings = settings.Value;
        _logger = logger;
    }

    /// <summary>
    /// Generate sample data based on structure definition
    /// </summary>
    /// <param name="request">Data generation request</param>
    /// <returns>Generated data in specified format</returns>
    [HttpPost("generate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GenerateData([FromBody] GenerateDataRequest request)
    {
        try
        {
            _logger.LogInformation("Received data generation request for entity: {EntityName}, Count: {Count}, Format: {Format}",
                request.Structure.EntityName, request.Count, request.Format);

            // Validate count
            if (request.Count <= 0)
            {
                return BadRequest(new { error = "Count must be greater than 0" });
            }

            if (request.Count > _settings.MaxCount)
            {
                return BadRequest(new { error = $"Count exceeds maximum allowed ({_settings.MaxCount})" });
            }

            // Validate structure
            var (isValid, errors) = _structureParser.ValidateStructure(request.Structure);
            if (!isValid)
            {
                return BadRequest(new { error = "Invalid structure", details = errors });
            }

            // Generate data
            var data = await _dataGenerator.GenerateDataAsync(request.Structure, request.Count);

            // Return in requested format
            return request.Format.ToLower() switch
            {
                "csv" => Ok(new { format = "csv", data = _dataGenerator.ConvertToCsv(data) }),
                "sql" => Ok(new { format = "sql", data = _dataGenerator.ConvertToSql(data, request.Structure.EntityName) }),
                "json" or _ => Ok(new { format = "json", data })
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating data");
            return StatusCode(500, new { error = "An error occurred while generating data" });
        }
    }

    /// <summary>
    /// Validate a structure definition
    /// </summary>
    /// <param name="request">Structure validation request</param>
    /// <returns>Validation result with suggestions</returns>
    [HttpPost("validate-structure")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ValidateStructure([FromBody] ValidateStructureRequest request)
    {
        try
        {
            _logger.LogInformation("Validating structure for entity: {EntityName}", request.Structure.EntityName);

            // Basic validation
            var (isValid, errors) = _structureParser.ValidateStructure(request.Structure);

            // AI-enhanced validation if available
            List<string> suggestions = new();
            if (await _aiService.IsAvailableAsync())
            {
                var (aiValid, aiSuggestions) = await _aiService.ValidateStructureAsync(request.Structure);
                suggestions.AddRange(aiSuggestions);
            }
            else
            {
                suggestions.Add("AI validation is unavailable - only basic validation performed");
            }

            return Ok(new ValidationResponse
            {
                IsValid = isValid,
                Errors = errors,
                Suggestions = suggestions
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating structure");
            return StatusCode(500, new { error = "An error occurred while validating structure" });
        }
    }

    /// <summary>
    /// Generate a structure definition from natural language description
    /// </summary>
    /// <param name="request">Structure generation request</param>
    /// <returns>Generated structure definition</returns>
    [HttpPost("generate-structure")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GenerateStructure([FromBody] GenerateStructureRequest request)
    {
        try
        {
            _logger.LogInformation("Generating structure from description");

            if (string.IsNullOrWhiteSpace(request.Description))
            {
                return BadRequest(new { error = "Description is required" });
            }

            // Limit description length to prevent excessive token usage
            if (request.Description.Length > 1000)
            {
                return BadRequest(new { error = "Description must not exceed 1000 characters" });
            }

            if (!await _aiService.IsAvailableAsync())
            {
                return StatusCode(503, new { error = "AI service is unavailable" });
            }

            var structure = await _aiService.GenerateStructureFromDescriptionAsync(request.Description);

            if (structure == null)
            {
                return StatusCode(500, new { error = "Failed to generate structure from description" });
            }

            return Ok(structure);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating structure");
            return StatusCode(500, new { error = "An error occurred while generating structure" });
        }
    }

    /// <summary>
    /// Get list of supported field types
    /// </summary>
    /// <returns>List of supported field types</returns>
    [HttpGet("supported-types")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetSupportedTypes()
    {
        var types = _structureParser.GetSupportedFieldTypes();
        return Ok(new { types });
    }
}

using EmbeddedAIApp.DTOs;
using EmbeddedAIApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmbeddedAIApp.Controllers;

/// <summary>
/// Controller for health check operations
/// </summary>
[ApiController]
[Route("api")]
public class HealthController : ControllerBase
{
    private readonly IAIService _aiService;
    private readonly ILogger<HealthController> _logger;

    public HealthController(IAIService aiService, ILogger<HealthController> logger)
    {
        _aiService = aiService;
        _logger = logger;
    }

    /// <summary>
    /// Health check endpoint
    /// </summary>
    /// <returns>Health status including Ollama connectivity</returns>
    [HttpGet("health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> HealthCheck()
    {
        try
        {
            _logger.LogInformation("Health check requested");

            var ollamaConnected = await _aiService.IsAvailableAsync();
            var ollamaModel = ollamaConnected ? _aiService.GetModelName() : null;

            return Ok(new HealthCheckResponse
            {
                Status = "healthy",
                OllamaConnected = ollamaConnected,
                OllamaModel = ollamaModel
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during health check");
            return Ok(new HealthCheckResponse
            {
                Status = "degraded",
                OllamaConnected = false,
                OllamaModel = null
            });
        }
    }
}

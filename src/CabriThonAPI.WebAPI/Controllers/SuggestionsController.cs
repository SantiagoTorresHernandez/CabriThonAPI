using Microsoft.AspNetCore.Mvc;
using CabriThonAPI.Application.Interfaces;

namespace CabriThonAPI.WebAPI.Controllers;

[Route("api/v1/suggestions")]
public class SuggestionsController : BaseApiController
{
    private readonly ISuggestionService _suggestionService;
    private readonly ILogger<SuggestionsController> _logger;

    public SuggestionsController(
        ISuggestionService suggestionService,
        ILogger<SuggestionsController> logger)
    {
        _suggestionService = suggestionService;
        _logger = logger;
    }

    /// <summary>
    /// Get promotion suggestions for the authenticated client
    /// </summary>
    /// <param name="status">Filter by status (Draft, Approved, Rejected, Applied)</param>
    /// <param name="limit">Maximum number of suggestions to return</param>
    /// <returns>List of promotion suggestions</returns>
    [HttpGet("promotions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPromotionSuggestions(
        [FromQuery] string? status = null,
        [FromQuery] int? limit = null)
    {
        try
        {
            var clientId = GetClientId();
            _logger.LogInformation("Getting promotion suggestions for client {ClientId}", clientId);

            var result = await _suggestionService.GetPromotionSuggestionsAsync(clientId, status, limit);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access attempt");
            return Unauthorized(new { error = "Invalid or missing authentication token" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting promotion suggestions");
            return StatusCode(500, new { error = "An error occurred while processing your request" });
        }
    }

    /// <summary>
    /// Get order suggestions for the authenticated client
    /// </summary>
    /// <param name="status">Filter by status (Draft, Approved, Rejected, Applied)</param>
    /// <returns>List of order suggestions</returns>
    [HttpGet("orders")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOrderSuggestions([FromQuery] string? status = null)
    {
        try
        {
            var clientId = GetClientId();
            _logger.LogInformation("Getting order suggestions for client {ClientId}", clientId);

            var result = await _suggestionService.GetOrderSuggestionsAsync(clientId, status);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access attempt");
            return Unauthorized(new { error = "Invalid or missing authentication token" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order suggestions");
            return StatusCode(500, new { error = "An error occurred while processing your request" });
        }
    }
}


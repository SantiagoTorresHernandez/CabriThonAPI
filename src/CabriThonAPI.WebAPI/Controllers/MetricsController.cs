using Microsoft.AspNetCore.Mvc;
using CabriThonAPI.Application.Interfaces;

namespace CabriThonAPI.WebAPI.Controllers;

[Route("api/v1/metrics/impact")]
public class MetricsController : BaseApiController
{
    private readonly IMetricsService _metricsService;
    private readonly ILogger<MetricsController> _logger;

    public MetricsController(
        IMetricsService metricsService,
        ILogger<MetricsController> logger)
    {
        _metricsService = metricsService;
        _logger = logger;
    }

    /// <summary>
    /// Get metrics for stockout prevention (suggested orders impact)
    /// </summary>
    /// <param name="year">Year for metrics (default: current year)</param>
    /// <returns>Impact metrics with monthly breakdown</returns>
    [HttpGet("suggested-orders")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSuggestedOrdersImpact([FromQuery] int? year = null)
    {
        try
        {
            var clientId = GetClientId();
            var targetYear = year ?? DateTime.UtcNow.Year;

            _logger.LogInformation("Getting suggested orders impact for client {ClientId}, year {Year}", 
                clientId, targetYear);

            var result = await _metricsService.GetOrderImpactAsync(clientId, targetYear);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access attempt");
            return Unauthorized(new { error = "Invalid or missing authentication token" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting suggested orders impact");
            return StatusCode(500, new { error = "An error occurred while processing your request" });
        }
    }

    /// <summary>
    /// Get metrics for promotional sales impact
    /// </summary>
    /// <param name="year">Year for metrics (default: current year)</param>
    /// <returns>Impact metrics with monthly breakdown</returns>
    [HttpGet("promotions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPromotionsImpact([FromQuery] int? year = null)
    {
        try
        {
            var clientId = GetClientId();
            var targetYear = year ?? DateTime.UtcNow.Year;

            _logger.LogInformation("Getting promotions impact for client {ClientId}, year {Year}", 
                clientId, targetYear);

            var result = await _metricsService.GetPromotionImpactAsync(clientId, targetYear);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access attempt");
            return Unauthorized(new { error = "Invalid or missing authentication token" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting promotions impact");
            return StatusCode(500, new { error = "An error occurred while processing your request" });
        }
    }
}


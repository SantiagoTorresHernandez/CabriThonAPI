namespace CabriThonAPI.Application.Interfaces;

/// <summary>
/// Service interface for AI agent operations
/// </summary>
public interface IAIAgentService
{
    Task ExecutePromotionAgentAsync(string clientId);
    Task ExecuteReplenishmentAgentAsync(string clientId);
}


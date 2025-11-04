namespace CabriThonAPI.Application.Interfaces;

/// <summary>
/// Service interface for Gemini AI integration
/// </summary>
public interface IGeminiAIService
{
    Task<string> GeneratePromotionSuggestionAsync(string prompt);
    Task<string> AnalyzeInventoryAsync(string data);
}


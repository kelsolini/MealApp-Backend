using MealAppAPI.DTOs;

namespace MealAppAPI.Services;

public interface IRecipeImportService
{
    Task<RecipeDraftDto> ImportFromUrlAsync(string url);
}

using MealAppAPI.DTOs;

namespace MealAppAPI.Services;

public class RecipeNotFoundException(string message) : Exception(message);
public class PageFetchException(string message, Exception inner) : Exception(message, inner);

public class RecipeImportService(IHttpClientFactory _httpClientFactory) : IRecipeImportService
{
    public async Task<RecipeDraftDto> ImportFromUrlAsync(string url)
    {
        string html;
        try
        {
            var client = _httpClientFactory.CreateClient("import");
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            html = await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            throw new PageFetchException($"Kunne ikke hente siden: {ex.Message}", ex);
        }

        var draft = RecipeJsonLdParser.Parse(html, url);
        if (draft is null) throw new RecipeNotFoundException("Fant ingen oppskrift på siden");

        return draft;
    }
}

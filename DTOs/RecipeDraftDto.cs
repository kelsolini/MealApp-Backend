namespace MealAppAPI.DTOs;

public class IngredientDto
{
    public string Name { get; set; } = string.Empty;
    public double? Amount { get; set; }
    public string Unit { get; set; } = string.Empty;
}

public class RecipeDraftDto
{
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Cuisine { get; set; }
    public string? Source { get; set; }
    public string? Description { get; set; }
    public string? Image { get; set; }
    public int? Portions { get; set; }
    public List<IngredientDto> Ingredients { get; set; } = new();
    public List<string> Method { get; set; } = new();
}

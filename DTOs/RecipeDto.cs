namespace MealAppAPI.DTOs;

public class CreateRecipeDto
{
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Cuisine { get; set; }
    public string? Source { get; set; }
    public string? Description { get; set; }
    public string? Image { get; set; }
    public int Portions { get; set; }
    public List<IngredientDto> Ingredients { get; set; } = new();
    public List<string> Method { get; set; } = new();
}

public class RecipeDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Cuisine { get; set; }
    public string? Source { get; set; }
    public string? Description { get; set; }
    public string? Image { get; set; }
    public int Portions { get; set; }
    public List<IngredientDto> Ingredients { get; set; } = new();
    public List<string> Method { get; set; } = new();
}

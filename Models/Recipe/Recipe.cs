namespace MealAppAPI.Models;

using System.ComponentModel.DataAnnotations;

public class Recipe
{
    public Guid? UserId { get; set; }
    public int Id { get; set; }
    [Required]
    [MinLength(1)]
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Cuisine { get; set; }
    public string? Source { get; set; }
    public string? Description { get; set; }
    public string? Image { get; set; }
    [Range(1, 100)]
    public int Portions { get; set; }
    public List<Ingredient> Ingredients { get; set; } = new();
    public List<string> Method { get; set; } = new();
    public Nutrition? Nutrition { get; set; }
}



namespace MealAppAPI.Interfaces;

interface IRecipe
{
    int Id { get; set; }
    string Title { get; set; }
    string Type { get; set; }
    string Category { get; set; }
    string? Cuisine { get; set; }
    string? Source { get; set; }
    string? Image { get; set; }
    int Portions { get; set; }
    List<IIngredient> Ingredients { get; set; }
    List<string> Method { get; set; }
    INutrition? Nutrition { get; set; }
}
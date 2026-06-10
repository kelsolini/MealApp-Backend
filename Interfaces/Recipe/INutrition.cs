namespace MealAppAPI.Interfaces;

public interface INutrition
{
    int Calories { get; set; }
    int Protein { get; set; }
    int Carbs { get; set; }
    int Fat { get; set; }
}
namespace MealAppAPI.Models;
using MealAppAPI.Interfaces;

public class Nutrition : INutrition
{
    public int Calories { get; set; }
    public int Protein { get; set; }
    public int Carbs { get; set; }
    public int Fat { get; set; }
}
namespace MealAppAPI.Interfaces;

public interface IIngredient
{
    string Name { get; set; }
    double Amount { get; set; }
    string Unit { get; set; }
}
using Microsoft.EntityFrameworkCore;
using MealAppAPI.Models;

namespace MealAppAPI.Context;

public class MealAppContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Recipe> Recipes { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Recipe>().OwnsMany(r => r.Ingredients, a =>
        {
            a.HasKey("Id");
            a.Property<int>("Id").ValueGeneratedOnAdd();
        });
        modelBuilder.Entity<Recipe>().OwnsOne(r => r.Nutrition);
    }
}

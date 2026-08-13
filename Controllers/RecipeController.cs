using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MealAppAPI.Models;
using MealAppAPI.Context;

using Microsoft.AspNetCore.Authorization;
using MealAppApi.Extensions;
using MealAppAPI.DTOs;
using Microsoft.AspNetCore.RateLimiting;

namespace MealAppAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecipeController(MealAppContext _mealAppContext) : ControllerBase
{
    /* GET ALL RECIPES (egne + felles) */
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<RecipeDto>>> GetRecipes()
    {
        var userId = User.GetUserId();
        var recipes = await _mealAppContext.Recipes
            .Include(r => r.Ingredients)
            .Where(r => r.UserId == null || r.UserId == userId)
            .ToListAsync();

        return Ok(recipes.Select(MapToDto));
    }

    /* CREATE RECIPE */
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<RecipeDto>> CreateRecipe(CreateRecipeDto dto)
    {
        var recipe = MapFromDto(dto);
        recipe.UserId = User.GetUserId();

        _mealAppContext.Recipes.Add(recipe);
        await _mealAppContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = recipe.Id }, MapToDto(recipe));
    }

    /* GET RECIPE ON ID */
    [HttpGet]
    [Route("[action]/{id}")]
    [Authorize]
    public async Task<ActionResult<RecipeDto>> GetById(int id)
    {
        try
        {
            var userId = User.GetUserId();
            Recipe? recipe = await _mealAppContext.Recipes
                .Include(r => r.Ingredients)
                .FirstOrDefaultAsync(r => r.Id == id && (r.UserId == null || r.UserId == userId));
            if (recipe != null)
            {
                return Ok(MapToDto(recipe));
            }
            else
            {
                return NotFound();
            }
        }
        catch
        {
            return StatusCode(500, "Server error when getting: [Recipes on id]");
        }
    }

    /* GET RECIPES ON TYPE */
    [HttpGet]
    [Route("[action]/{type}")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<RecipeDto>>> GetByType(string type)
    {
        try
        {
            var userId = User.GetUserId();
            List<Recipe> recipes = await _mealAppContext.Recipes
                .Include(r => r.Ingredients)
                .Where(r => r.Type.ToLower() == type.ToLower() && (r.UserId == null || r.UserId == userId))
                .ToListAsync();
            return Ok(recipes.Select(MapToDto));
        }
        catch
        {
            return StatusCode(500, "Server error when getting: [Recipes on type]");
        }
    }

    /* GET RECIPES ON CATEGORY */
    [HttpGet]
    [Route("[action]/{category}")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<RecipeDto>>> GetByCategory(string category)
    {
        try
        {
            var userId = User.GetUserId();
            List<Recipe> recipes = await _mealAppContext.Recipes
                .Include(r => r.Ingredients)
                .Where(r => r.Category.ToLower() == category.ToLower() && (r.UserId == null || r.UserId == userId))
                .ToListAsync();
            return Ok(recipes.Select(MapToDto));
        }
        catch
        {
            return StatusCode(500, "Server error when getting: [Recipes on category]");
        }
    }

    /* GET RECIPES ON TITLE */
    [HttpGet]
    [Route("[action]/{title}")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<RecipeDto>>> GetByTitle(string title)
    {
        try
        {
            var userId = User.GetUserId();
            List<Recipe> recipes = await _mealAppContext.Recipes
                .Include(r => r.Ingredients)
                .Where(r => r.Title.ToLower().Contains(title.ToLower()) && (r.UserId == null || r.UserId == userId))
                .ToListAsync();

            if (recipes.Count > 0)
            {
                return Ok(recipes.Select(MapToDto));
            }
            else
            {
                return NotFound();
            }
        }
        catch
        {
            return StatusCode(500, "Server error when getting: [Recipes on title]");
        }
    }

    /* PUT RECIPE */
    [HttpPut]
    [Authorize]
    public async Task<ActionResult> Put(Recipe editedRecipe)
    {
        try
        {
            Recipe? existing = await _mealAppContext.Recipes
                .Include(r => r.Ingredients)
                .FirstOrDefaultAsync(r => r.Id == editedRecipe.Id);

            if (existing == null) return NotFound();

            var userId = User.GetUserId();
            if (existing.UserId != null && existing.UserId != userId) return Forbid();

            existing.Title = editedRecipe.Title;
            existing.Type = editedRecipe.Type;
            existing.Category = editedRecipe.Category;
            existing.Cuisine = editedRecipe.Cuisine;
            existing.Source = editedRecipe.Source;
            existing.Description = editedRecipe.Description;
            existing.Image = editedRecipe.Image;
            existing.Portions = editedRecipe.Portions;
            existing.Method = editedRecipe.Method;

            existing.Ingredients.Clear();
            foreach (var ing in editedRecipe.Ingredients)
            {
                existing.Ingredients.Add(new Ingredient { Name = ing.Name, Amount = ing.Amount, Unit = ing.Unit });
            }

            await _mealAppContext.SaveChangesAsync();
            return Ok(existing);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.ToString());
        }
    }

    /* DELETE RECIPE */
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            Recipe? recipe = await _mealAppContext.Recipes.FindAsync(id);
            if (recipe != null)
            {
                var userId = User.GetUserId();
                if (recipe.UserId != null && recipe.UserId != userId) return Forbid();

                _mealAppContext.Recipes.Remove(recipe);
                await _mealAppContext.SaveChangesAsync();
                return NoContent();
            }
            else
            {
                return NotFound();
            }
        }
        catch
        {
            return StatusCode(500, "Server error when deleting recipe");
        }
    }

    private static Recipe MapFromDto(CreateRecipeDto dto) => new()
    {
        Title = dto.Title,
        Type = dto.Type,
        Category = dto.Category,
        Cuisine = dto.Cuisine,
        Source = dto.Source,
        Description = dto.Description,
        Image = dto.Image,
        Portions = dto.Portions,
        Method = dto.Method,
        Ingredients = dto.Ingredients
            .Select(i => new Ingredient { Name = i.Name, Amount = i.Amount ?? 0, Unit = i.Unit })
            .ToList()
    };

    private static RecipeDto MapToDto(Recipe recipe) => new()
    {
        Id = recipe.Id,
        Title = recipe.Title,
        Type = recipe.Type,
        Category = recipe.Category,
        Cuisine = recipe.Cuisine,
        Source = recipe.Source,
        Description = recipe.Description,
        Image = recipe.Image,
        Portions = recipe.Portions,
        Method = recipe.Method,
        Ingredients = recipe.Ingredients
            .Select(i => new IngredientDto { Name = i.Name, Amount = i.Amount, Unit = i.Unit })
            .ToList()
    };
}
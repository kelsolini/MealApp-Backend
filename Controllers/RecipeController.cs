using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MealAppAPI.Models;
using MealAppAPI.Context;

namespace MealAppAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecipeController(MealAppContext _mealAppContext) : ControllerBase
{
    /* GET ALL RECIPES */
    [HttpGet]
    public async Task<ActionResult<List<Recipe>>> Get()
    {
        try
        {
            List<Recipe> recipes = await _mealAppContext.Recipes
                .Include(r => r.Ingredients)
                .ToListAsync();
            return Ok(recipes);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.ToString());
        }
    }

    /* GET RECIPE ON ID */
    [HttpGet]
    [Route("[action]/{id}")]
    public async Task<ActionResult<List<Recipe>>> GetById(int id)
    {
        try
        {
            Recipe? recipe = await _mealAppContext.Recipes
                .Include(r => r.Ingredients)
                .FirstOrDefaultAsync(r => r.Id == id);
            if(recipe != null)
            {
                return Ok(recipe);
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
    public async Task<ActionResult<List<Recipe>>> GetByType(string type)
    {
        try
        {
            List<Recipe> recipes = await _mealAppContext.Recipes
                .Include(r => r.Ingredients)
                .Where(r => r.Type.ToLower() == type.ToLower())
                .ToListAsync();
            return Ok(recipes);
        }
        catch
        {
            return StatusCode(500, "Server error when getting: [Recipes on type]");
        }
    }

    /* GET RECIPES ON CATEGORY */
    [HttpGet]
    [Route("[action]/{category}")]
    public async Task<ActionResult<List<Recipe>>> GetByCategory(string category)
    {
        try
        {
            List<Recipe> recipes = await _mealAppContext.Recipes
                .Include(r => r.Ingredients)
                .Where(r => r.Category.ToLower() == category.ToLower())
                .ToListAsync();
            return Ok(recipes);
        }
        catch
        {
            return StatusCode(500, "Server error when getting: [Recipes on category]");
        }
    }

    /* GET RECIPES ON TITLE */
    [HttpGet]
    [Route("[action]/{title}")]
    public async Task<ActionResult<List<Recipe>>> GetByTitle(string title)
    {
        try
        {
            List<Recipe> recipes = await _mealAppContext.Recipes
                .Where(r => r.Title.ToLower().Contains(title.ToLower()))
                .ToListAsync();

            if (recipes.Count > 0)
            {
                return Ok(recipes);
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
    public async Task<ActionResult> Put(Recipe editedRecipe)
    {
        try
        {
            Recipe? existing = await _mealAppContext.Recipes
                .Include(r => r.Ingredients)
                .FirstOrDefaultAsync(r => r.Id == editedRecipe.Id);

            if (existing == null) return NotFound();

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

    /* POST RECIPE */
    [HttpPost]
    public async Task<ActionResult> Post(Recipe newRecipe)
    {
        try
        {
            _mealAppContext.Recipes.Add(newRecipe);
            await _mealAppContext.SaveChangesAsync();
            return CreatedAtAction("Get", new { id = newRecipe.Id }, newRecipe);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.ToString());
        }
    }

    /* DELETE RECIPE */
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            Recipe? recipe = await _mealAppContext.Recipes.FindAsync(id);
            if (recipe != null)
            {
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
}
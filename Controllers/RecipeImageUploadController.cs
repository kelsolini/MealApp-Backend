using Microsoft.AspNetCore.Mvc;

namespace MealAppAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecipeImageUploadController(IWebHostEnvironment _webHostEnvironment) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Post(IFormFile file)
    {
        try
        {
            if (file != null && file.Length > 0)
            {
                string webRootPath = _webHostEnvironment.WebRootPath;
                string absolutePath = Path.Combine(webRootPath, "images-recipe", file.FileName);
                using (var fileStream = new FileStream(absolutePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }
                return Created();
            }
            else
            {
                return BadRequest("No file found when uploading recipe image");
            }
        }
        catch
        {
            return StatusCode(500, "Server error when uploading recipe image");
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using MealAppAPI.DTOs;
using MealAppAPI.Services;

namespace MealAppAPI.Controllers;

[ApiController]
[Route("api/import")]
public class ImportController(IRecipeImportService _importService) : ControllerBase
{
    [HttpPost("url")]
    public async Task<IActionResult> ImportFromUrl([FromBody] ImportUrlRequest request)
    {
        try
        {
            var draft = await _importService.ImportFromUrlAsync(request.Url);
            return Ok(draft);
        }
        catch (RecipeNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (PageFetchException ex)
        {
            return UnprocessableEntity(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.ToString());
        }
    }
}

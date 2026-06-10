using Microsoft.AspNetCore.Mvc;

namespace MealAppAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecipeImageUploadController(IHttpClientFactory _httpClientFactory, IConfiguration _configuration) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Post(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file found when uploading recipe image");

        try
        {
            string supabaseUrl = _configuration["Supabase:Url"]!;
            string serviceRoleKey = _configuration["Supabase:ServiceRoleKey"]!;

            string extension = Path.GetExtension(file.FileName);
            string uniqueFileName = $"{Guid.NewGuid()}{extension}";

            var client = _httpClientFactory.CreateClient();

            using var content = new StreamContent(file.OpenReadStream());
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);

            var request = new HttpRequestMessage(HttpMethod.Post,
                $"{supabaseUrl}/storage/v1/object/recipe-images/{uniqueFileName}");
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", serviceRoleKey);
            request.Content = content;

            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                return StatusCode(500, "Failed to upload image to Supabase Storage");

            string publicUrl = $"{supabaseUrl}/storage/v1/object/public/recipe-images/{uniqueFileName}";
            return Ok(new { url = publicUrl });
        }
        catch
        {
            return StatusCode(500, "Server error when uploading recipe image");
        }
    }
}

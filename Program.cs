using Microsoft.EntityFrameworkCore;
using MealAppAPI.Context;
using MealAppAPI.Services;

var builder = WebApplication.CreateBuilder(args);

/* CONTEXT / DATABASE */
builder.Services.AddDbContext<MealAppContext>(
    options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddCors();
builder.Services.AddHttpClient();
builder.Services.AddHttpClient("import", client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
    client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; MealAppBot/1.0)");
});
builder.Services.AddScoped<IRecipeImportService, RecipeImportService>();

var app = builder.Build();

// Kjør migrasjonene ved oppstart — lager databasen og tabellene hvis de mangler.
// Lokalt gjør den ingenting hvis alt allerede er oppdatert.
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<MealAppContext>();
    context.Database.Migrate();
}

/* CORS CONFIG */
app.UseCors(policy => policy
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader()
);

DefaultFilesOptions defaultFilesOptions = new DefaultFilesOptions();
defaultFilesOptions.DefaultFileNames.Add("index.html");
app.UseDefaultFiles(defaultFilesOptions);
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
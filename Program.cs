using Microsoft.EntityFrameworkCore;
using MealAppAPI.Context;

var builder = WebApplication.CreateBuilder(args);

/* CONTEXT / DATABASE */
builder.Services.AddDbContext<MealAppContext>(
    options => options.UseSqlite("Data Source=Databases/MealApp.db")
);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddCors();

var app = builder.Build();

/* DATABASE SETUP */
// Sørg for at mappa til databasefila finnes (git sporer ikke tomme mapper,
// så på Render eksisterer ikke Databases/ før vi lager den)
Directory.CreateDirectory("Databases");

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
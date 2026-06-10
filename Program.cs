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
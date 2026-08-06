using Microsoft.EntityFrameworkCore;
using MealAppAPI.Context;
using MealAppAPI.Services;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

/* AUTHENTICATION */
var supabaseUrl = builder.Configuration["Supabase:Url"]
    ?? throw new InvalidOperationException("Supabase:Url mangler");

// Hent Supabase sine offentlige signeringsnøkler (JWKS) ved oppstart.
// Prosjektet signerer tokens asymmetrisk (ES256), så backend trenger
// bare de offentlige nøklene for å verifisere – ingen delt secret.
using var jwksClient = new HttpClient();
var jwksJson = await jwksClient.GetStringAsync(
    $"{supabaseUrl}/auth/v1/.well-known/jwks.json");
var jwks = new JsonWebKeySet(jwksJson);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = $"{supabaseUrl}/auth/v1",
            ValidateAudience = true,
            ValidAudience = "authenticated",
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKeys = jwks.GetSigningKeys()
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = ctx =>
            {
                Console.WriteLine($">>> JWT FEIL: {ctx.Exception.Message}");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

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
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
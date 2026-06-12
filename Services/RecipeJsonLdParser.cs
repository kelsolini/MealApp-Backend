using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using MealAppAPI.DTOs;

namespace MealAppAPI.Services;

public static class RecipeJsonLdParser
{
    private static readonly Regex IngredientRegex = new(
        @"^(\d+(?:[.,]\d+)?)\s*(g|kg|dl|l|ss|ts|stk|ms|bunter|bunt|båter|båt|kvister|kvist|fedd|bokser|boks|pakker|pakke|poser|pose|never|neve)\b\.?\s*(.+)$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled
    );

    private static readonly Regex PrefixRegex = new(
        @"^(?:ca\.|ca|cirka|omtrent|evt\.)\s+",
        RegexOptions.IgnoreCase | RegexOptions.Compiled
    );

    private static readonly Dictionary<string, string> UnitSingular = new(StringComparer.OrdinalIgnoreCase)
    {
        ["bunter"]  = "bunt",
        ["båter"]   = "båt",
        ["kvister"] = "kvist",
        ["bokser"]  = "boks",
        ["pakker"]  = "pakke",
        ["poser"]   = "pose",
        ["never"]   = "neve",
    };

    public static RecipeDraftDto? Parse(string html, string sourceUrl)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var scripts = doc.DocumentNode.SelectNodes("//script[@type='application/ld+json']");
        if (scripts is null) return null;

        foreach (var script in scripts)
        {
            var draft = TryParseBlock(script.InnerText, sourceUrl);
            if (draft is not null) return draft;
        }

        return null;
    }

    private static RecipeDraftDto? TryParseBlock(string json, string sourceUrl)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var recipeEl = FindRecipeElement(doc.RootElement);
            return recipeEl is null ? null : MapToDto(recipeEl.Value, sourceUrl);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static JsonElement? FindRecipeElement(JsonElement root)
    {
        if (root.ValueKind == JsonValueKind.Object)
        {
            if (IsRecipeType(root)) return root;

            if (root.TryGetProperty("@graph", out var graph) && graph.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in graph.EnumerateArray())
                {
                    if (IsRecipeType(item)) return item;
                }
            }
        }
        else if (root.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in root.EnumerateArray())
            {
                if (IsRecipeType(item)) return item;
            }
        }

        return null;
    }

    private static bool IsRecipeType(JsonElement el)
    {
        if (!el.TryGetProperty("@type", out var type)) return false;

        if (type.ValueKind == JsonValueKind.String)
            return type.GetString()?.Equals("Recipe", StringComparison.OrdinalIgnoreCase) == true;

        if (type.ValueKind == JsonValueKind.Array)
        {
            foreach (var t in type.EnumerateArray())
            {
                if (t.GetString()?.Equals("Recipe", StringComparison.OrdinalIgnoreCase) == true)
                    return true;
            }
        }

        return false;
    }

    private static RecipeDraftDto MapToDto(JsonElement recipe, string sourceUrl) => new()
    {
        Title = GetString(recipe, "name") ?? string.Empty,
        Source = sourceUrl,
        Cuisine = GetString(recipe, "recipeCuisine"),
        Description = GetString(recipe, "description"),
        Image = ParseImage(recipe),
        Portions = ParseYield(recipe),
        Ingredients = ParseIngredients(recipe),
        Method = ParseInstructions(recipe),
    };

    private static string? GetString(JsonElement el, string key) =>
        el.TryGetProperty(key, out var val) && val.ValueKind == JsonValueKind.String
            ? val.GetString()
            : null;

    private static string? ParseImage(JsonElement recipe)
    {
        if (!recipe.TryGetProperty("image", out var img)) return null;

        if (img.ValueKind == JsonValueKind.String) return img.GetString();

        if (img.ValueKind == JsonValueKind.Array)
        {
            var first = img.EnumerateArray().FirstOrDefault();
            if (first.ValueKind == JsonValueKind.String) return first.GetString();
            if (first.ValueKind == JsonValueKind.Object) return GetString(first, "url");
        }

        if (img.ValueKind == JsonValueKind.Object) return GetString(img, "url");

        return null;
    }

    private static int? ParseYield(JsonElement recipe)
    {
        if (!recipe.TryGetProperty("recipeYield", out var yield)) return null;

        if (yield.ValueKind == JsonValueKind.Number)
        {
            if (yield.TryGetInt32(out var n)) return n;
            if (yield.TryGetDouble(out var d)) return (int)Math.Round(d);
            return null;
        }

        string? text = yield.ValueKind switch
        {
            JsonValueKind.String => yield.GetString(),
            JsonValueKind.Array  => yield.EnumerateArray().FirstOrDefault().GetString(),
            _                    => null,
        };

        if (text is null) return null;
        var match = Regex.Match(text, @"\d+");
        return match.Success ? int.Parse(match.Value) : null;
    }

    private static List<IngredientDto> ParseIngredients(JsonElement recipe)
    {
        var result = new List<IngredientDto>();
        if (!recipe.TryGetProperty("recipeIngredient", out var ingredients)
            || ingredients.ValueKind != JsonValueKind.Array)
            return result;

        foreach (var ing in ingredients.EnumerateArray())
        {
            var text = ing.GetString();
            if (!string.IsNullOrWhiteSpace(text))
                result.Add(ParseIngredient(text));
        }

        return result;
    }

    public static IngredientDto ParseIngredient(string text)
    {
        var trimmed = PrefixRegex.Replace(text.Trim(), string.Empty);
        var match = IngredientRegex.Match(trimmed);
        if (match.Success)
        {
            var amountStr = match.Groups[1].Value.Replace(',', '.');
            var rawUnit = match.Groups[2].Value.ToLower();
            var rawName = match.Groups[3].Value.TrimStart('.', ' ', '\t');
            return new IngredientDto
            {
                Amount = double.TryParse(amountStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var amount)
                    ? amount : null,
                Unit = UnitSingular.TryGetValue(rawUnit, out var singular) ? singular : rawUnit,
                Name = rawName,
            };
        }

        return new IngredientDto { Name = trimmed };
    }

    private static List<string> ParseInstructions(JsonElement recipe)
    {
        var result = new List<string>();
        if (!recipe.TryGetProperty("recipeInstructions", out var instructions)) return result;

        if (instructions.ValueKind == JsonValueKind.String)
        {
            var text = instructions.GetString();
            if (!string.IsNullOrWhiteSpace(text)) result.Add(text);
            return result;
        }

        if (instructions.ValueKind != JsonValueKind.Array) return result;

        foreach (var step in instructions.EnumerateArray())
            ExtractSteps(step, result);

        return result;
    }

    private static void ExtractSteps(JsonElement element, List<string> result)
    {
        if (element.ValueKind == JsonValueKind.String)
        {
            var text = element.GetString();
            if (!string.IsNullOrWhiteSpace(text)) result.Add(text);
            return;
        }

        if (element.ValueKind != JsonValueKind.Object) return;

        // HowToStep — has "text" property
        if (element.TryGetProperty("text", out var textEl) && textEl.ValueKind == JsonValueKind.String)
        {
            var t = textEl.GetString();
            if (!string.IsNullOrWhiteSpace(t)) result.Add(t);
            return;
        }

        // HowToSection — has "itemListElement" array of steps
        if (element.TryGetProperty("itemListElement", out var items) && items.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in items.EnumerateArray())
                ExtractSteps(item, result);
        }
    }
}

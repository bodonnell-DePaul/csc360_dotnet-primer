using System.Linq;
using System.Text.Json;

using dotnet_primer;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options => 
{
    options.AddPolicy("AllowAllOrigins",
    builder =>
    {
        builder.AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAllOrigins");
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    Console.WriteLine("Executing weather forecast: " + DateTime.Now.ToShortTimeString());
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.MapGet("/initialize", () => {

    var options = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };
    string titleJson = File.ReadAllText("recipeTitleData.json");
    RecipeTitle[]? titles = JsonSerializer.Deserialize<RecipeTitle[]>(titleJson, options);

    string ingredientJson = File.ReadAllText("recipeIngredientsData.json");
    RecipeIngredients[]? ingredients = JsonSerializer.Deserialize<RecipeIngredients[]>(ingredientJson, options);

    //Adding new data to tables
    using (var context = new RecipeContext())
    {
        //Create Recipe Titles
        foreach (var title in titles)
        {
            context.RecipeTitles.Add(title);
        }

        context.SaveChanges();
        

        //Loop through titles and assign id to recipe review
        List<RecipeTitle> fromdb = context.RecipeTitles.ToList();
        foreach(var item in fromdb)
        {
            //updating recipeReview to have the recipeTItle id association
            foreach(var review in context.RecipeReviews.ToList())
            {
                if(review.Id == item.Id)
                {
                    review.RecipeId = item.Id;
                    item.Review = review;
                    break;
                }
                continue;
            }

            //updating ingredients to have recipeTitle id association
            //RecipeTitle r = context.RecipeTitles.FirstOrDefault(rt => rt.Title == item.Title);
            List<RecipeIngredients> matched = ingredients.Where(ingredient => ingredient.RecipeTitle.ToLower() == item.Title.ToLower()).ToList();
            foreach (var ingredient in matched)
            {
                ingredient.RecipeId = item.Id;
                context.RecipeIngredients.Add(ingredient);
            }

            
        }
        context.SaveChanges();
        context.Database.ExecuteSqlRaw("PRAGMA wal_checkpoint;");
    }
    
}).WithName("Init").WithOpenApi();


app.MapGet("/recipes", () => {
    Console.WriteLine("Executing Hello World: " + DateTime.Now.ToShortTimeString());
    return "Hello World!";
}).WithName("HelloWorld").WithOpenApi();

app.MapGet("/recipeTitles", () => {
    Console.WriteLine("Executing recipeTitles: " + DateTime.Now.ToShortTimeString());
    
    using(var context = new RecipeContext())
    {
        var titles = context.RecipeTitles.ToList();
        return titles;
    }

}).WithName("GetRecipeTitles").WithOpenApi();

app.MapGet("/recipeIngredients/{repcipeTitle}", (string recipeTitle) => {

    Console.WriteLine("Executing recipeIngredients: " + DateTime.Now.ToShortTimeString());

    using(var context = new RecipeContext())
    {
        var ingredients = context.RecipeIngredients.Where(item => item.RecipeTitle.ToLower() == recipeTitle.ToLower()).ToList();
        return Results.Ok(ingredients);
    }
    //List<RecipeIngredients> retVal = ingredients.Where(item => item.RecipeTitle.ToLower() == recipeTitle.ToLower()).ToList();

}).WithName("GetRecipeIngredients").WithOpenApi();

app.Run();
record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

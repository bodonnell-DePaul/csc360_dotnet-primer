using System.Text.Json;
using dotnet_primer;

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

app.MapGet("/recipes", () => {
    Console.WriteLine("Executing Hello World: " + DateTime.Now.ToShortTimeString());
    return "Hello World!";
}).WithName("HelloWorld").WithOpenApi();

app.MapGet("/recipeTitles", () => {
    Console.WriteLine("Executing recipeTitles: " + DateTime.Now.ToShortTimeString());
    var options = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };
    string jsonString = File.ReadAllText("recipeTitleData.json");
    RecipeTitle[]? titles = JsonSerializer.Deserialize<RecipeTitle[]>(jsonString, options);
    return titles;

}).WithName("GetRecipeTitles").WithOpenApi();

app.MapGet("/recipeIngredients/{repcipeTitle}", (string recipeTitle) => {

    Console.WriteLine("Executing recipeIngredients: " + DateTime.Now.ToShortTimeString());
    
    var options = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true

    };
    string jsonString = File.ReadAllText("recipeIngredientsData.json");
    RecipeIngredients[]? ingredients = JsonSerializer.Deserialize<RecipeIngredients[]>(jsonString, options);

    List<RecipeIngredients> retVal = ingredients.Where(item => item.RecipeTitle.ToLower() == recipeTitle.ToLower()).ToList();
    return Results.Ok(retVal);


}).WithName("GetRecipeIngredients").WithOpenApi();

app.Run();
record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

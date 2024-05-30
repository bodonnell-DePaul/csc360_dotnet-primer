using System.Linq;
using System.Text.Json;

using dotnet_primer;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;


var builder = WebApplication.CreateBuilder(args);

Action<ApplicationInsightsServiceOptions> configureAppInsights = (options) =>
{
    options.InstrumentationKey = builder.Configuration["APPINSIGHTS_INSTRUMENTATIONKEY"];
    options.EnableAdaptiveSampling = false;
    options.EnableQuickPulseMetricStream = true;
    options.EnableHeartbeat = true;
    options.EnablePerformanceCounterCollectionModule = true;
    options.EnableDependencyTrackingTelemetryModule = true;
    options.EnableAppServicesHeartbeatTelemetryModule = true;
    options.EnableRequestTrackingTelemetryModule = true;
    options.EnableEventCounterCollectionModule = true;
    options.EnableAzureInstanceMetadataTelemetryModule = true;
    options.EnableAppServicesHeartbeatTelemetryModule = true;
    options.EnablePerformanceCounterCollectionModule = true;
    options.EnableQuickPulseMetricStream = true;
    options.EnableAdaptiveSampling = true;
    options.EnableHeartbeat = true;
};
builder.Services.AddApplicationInsightsTelemetry(configureAppInsights);//(builder.Configuration["APPINSIGHTS_CONNECTIONSTRING"]);
//builder.Services.AddSingleton<TelemetryClient>();
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
var logger = LoggerFactory.Create(builder => builder.AddApplicationInsights()).CreateLogger<Program>();
var telemetryClient = builder.Services.BuildServiceProvider().GetService<TelemetryClient>();


builder.Services.AddAuthorization();
builder.Services.AddAuthentication("BasicAuthentication")
    .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("basic", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "basic",
        In = ParameterLocation.Header,
        Description = "Basic Authorization header using the Bearer scheme."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "basic"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();
app.UseCors("AllowAllOrigins");

telemetryClient.TrackTrace("Starting Application");
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

app.MapPost("/newUser", (Login newUser) =>{
    logger.LogTrace("Executing newUser: " + DateTime.Now.ToShortTimeString());
    telemetryClient.TrackTrace("Executing newUser: " + DateTime.Now.ToShortTimeString());
    try{
        using(var context = new LoginContext())
        {
            context.Logins.Add(newUser);
            context.SaveChanges();
            context.Database.ExecuteSqlRaw("PRAGMA wal_checkpoint;");
        }
    }
    catch(Exception e)
    {
        logger.LogError(e.Message);
        telemetryClient.TrackException(e);
        telemetryClient.Flush();
        return Results.BadRequest(e.Message);
        
    }
    logger.LogTrace("Finished newUser: " + DateTime.Now.ToShortTimeString());
    telemetryClient.Flush();
    return Results.Created($"/newUser/{newUser.Id}", newUser);
}).WithName("PostLogin").WithOpenApi();



app.MapGet("/initialize", () => {

    logger.LogTrace("Executing Initialize: " + DateTime.Now.ToShortTimeString());
    telemetryClient.TrackTrace("Executing Initialize: " + DateTime.Now.ToShortTimeString());
    try{
        using (var context = new RecipeContext())
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        }
    }
    catch(Exception e)
    {
        telemetryClient.TrackException(e);
        logger.LogError(e.Message);
    }


    var options = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    string titleJson = String.Empty;
    RecipeTitle[]? titles = null;
    try
    {
        logger.LogTrace("Reading recipeTitlesData.json: " + DateTime.Now.ToShortTimeString());
        telemetryClient.TrackTrace("Reading recipeTitleData.json: " + DateTime.Now.ToShortTimeString());
        titleJson = File.ReadAllText("recipeTitleData.json");
        titles = JsonSerializer.Deserialize<RecipeTitle[]>(titleJson, options);
    }
    catch (Exception e)
    {
        logger.LogError(e.Message);
        telemetryClient.TrackException(e);
    }

    try
    {
        logger.LogTrace("Reading recipeIngredientsData.json: " + DateTime.Now.ToShortTimeString());
        telemetryClient.TrackTrace("Reading recipeIngredientsData.json: " + DateTime.Now.ToShortTimeString());
        string ingredientJson = File.ReadAllText("recipeIngredientsData.json");
        RecipeIngredients[]? ingredients = JsonSerializer.Deserialize<RecipeIngredients[]>(ingredientJson, options);
        using (var context = new RecipeContext())
        {
            logger.LogTrace("Createing titles.json: " + DateTime.Now.ToShortTimeString());
            telemetryClient.TrackTrace("Creating titles.json: " + DateTime.Now.ToShortTimeString());
            foreach (var title in titles)
            {
                context.RecipeTitles.Add(title);
            }

            context.SaveChanges();
            //Loop through titles and assign id to recipe review
            List<RecipeTitle> fromdb = context.RecipeTitles.ToList();
            foreach(var item in fromdb)
            {
                logger.LogTrace("Attaching reviews to recipes " + DateTime.Now.ToShortTimeString());
                telemetryClient.TrackTrace("Attaching reviews to recipes " + DateTime.Now.ToShortTimeString());
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
                logger.LogTrace("Attaching ingredients to recipes " + DateTime.Now.ToShortTimeString());
                telemetryClient.TrackTrace("Attaching ingredients to recipes " + DateTime.Now.ToShortTimeString());
                foreach (var ingredient in matched)
                {
                    ingredient.RecipeId = item.Id;
                    context.RecipeIngredients.Add(ingredient);
                }

                
            }

            context.SaveChanges();
            context.Database.ExecuteSqlRaw("PRAGMA wal_checkpoint;");
            
        }
    }
    catch (Exception e)
    {
        logger.LogError(e.Message);
        telemetryClient.TrackException(e);
    }

    try{
        logger.LogTrace("Creating default user/pass: " + DateTime.Now.ToShortTimeString());
        telemetryClient.TrackTrace("Creating default user/pass: " + DateTime.Now.ToShortTimeString());
        using(var loginContext = new LoginContext())
        {
            Login starter = new Login("brian", "password");
            loginContext.Logins.Add(starter);
            loginContext.SaveChanges();
            loginContext.Database.ExecuteSqlRaw("PRAGMA wal_checkpoint;");
        }
    }
    catch(Exception e)
    {
        logger.LogError(e.Message);
        telemetryClient.TrackException(e);
    }
    telemetryClient.Flush();
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

app.MapPost("/recipeTitles", (RecipeTitle title) => {
    Console.WriteLine("Executing recipeTitles: " + DateTime.Now.ToShortTimeString());
    using(var context = new RecipeContext())
    {
        context.RecipeTitles.Add(title);
        context.SaveChanges();

        if(title.Review == null)
        {
            title.Review = new RecipeReview(title.Id);
        }
        else{
            RecipeReview n = context.RecipeReviews.ToList().Last();
            n.RecipeId = title.Id;
            n.Rating = title.Review.Rating;
            n.Reviews = title.Review.Reviews;
            title.Review = n;
        }
        context.RecipeReviews.Add(title.Review);
        context.SaveChanges();
        context.Database.ExecuteSqlRaw("PRAGMA wal_checkpoint;");
    }


    return Results.Created($"/recipeTitles/{title.Id}", title);
}).WithName("PostRecipeTitles").WithOpenApi().RequireAuthorization(new AuthorizeAttribute() {AuthenticationSchemes="BasicAuthentication"});

app.MapPost("/recipeIngredients", (RecipeIngredients ingredient) => {
    Console.WriteLine("Executing recipeIngredients: " + DateTime.Now.ToShortTimeString());
    using(var context = new RecipeContext())
    {
        context.RecipeIngredients.Add(ingredient);
        context.SaveChanges();
        context.Database.ExecuteSqlRaw("PRAGMA wal_checkpoint;");
    }
    return Results.Created($"/recipeIngredients/{ingredient.Id}", ingredient);
}).WithName("PostRecipeIngredients").WithOpenApi();

app.MapPost("/login", (Login authenticatedUser) => {
    Console.WriteLine("Executing login: " + DateTime.Now.ToShortTimeString());
    using(var context = new LoginContext())
    {
        var user = context.Logins.FirstOrDefault(l => l.Username == authenticatedUser.Username);
        if(user == null){
            return Results.NotFound();
        }
        return Results.Ok(user);
    }
}).WithName("Login").WithOpenApi().RequireAuthorization(new AuthorizeAttribute() {AuthenticationSchemes="BasicAuthentication"});

app.UseStaticFiles();

app.MapFallbackToFile("index.html");
app.Run();
record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

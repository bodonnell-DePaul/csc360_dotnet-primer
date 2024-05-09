namespace dotnet_primer;
using Microsoft.EntityFrameworkCore;



public class RecipeContext : DbContext
    {
        public DbSet<RecipeTitle> RecipeTitles { get; set; } // Replace with your actual model
        public DbSet<RecipeReview> RecipeReviews {get; set;}
        public DbSet<RecipeIngredients> RecipeIngredients {get;set;}
        public DbSet<RecipePrepSteps> RecipePrepSteps {get;set;}
        // Add more DbSets for your other models

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=RecipeDb.db");
        }
    }


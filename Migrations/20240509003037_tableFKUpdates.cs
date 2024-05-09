using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dotnet_primer.Migrations
{
    /// <inheritdoc />
    public partial class tableFKUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RecipeId",
                table: "RecipeReviews",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RecipeId",
                table: "RecipeReviews");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dotnet_primer.Migrations.Login
{
    /// <inheritdoc />
    public partial class logindb_hash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "base64Credentials",
                table: "Logins",
                type: "BLOB",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<string>(
                name: "hash",
                table: "Logins",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "base64Credentials",
                table: "Logins");

            migrationBuilder.DropColumn(
                name: "hash",
                table: "Logins");
        }
    }
}

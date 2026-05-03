using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalMenuApi.Migrations
{
    /// <inheritdoc />
    public partial class AddMealLogConsumedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MealLogs_UserId_CreatedAt",
                table: "MealLogs");

            migrationBuilder.AddColumn<DateTime>(
                name: "ConsumedAt",
                table: "MealLogs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MealLogs_UserId_ConsumedAt",
                table: "MealLogs",
                columns: new[] { "UserId", "ConsumedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MealLogs_UserId_ConsumedAt",
                table: "MealLogs");

            migrationBuilder.DropColumn(
                name: "ConsumedAt",
                table: "MealLogs");

            migrationBuilder.CreateIndex(
                name: "IX_MealLogs_UserId_CreatedAt",
                table: "MealLogs",
                columns: new[] { "UserId", "CreatedAt" });
        }
    }
}

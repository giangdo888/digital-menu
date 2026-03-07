using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalMenuApi.Migrations
{
    /// <inheritdoc />
    public partial class removeLoggedDateFromMealLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MealLogs_UserId_LoggedAt",
                table: "MealLogs");

            migrationBuilder.DropColumn(
                name: "LoggedAt",
                table: "MealLogs");

            migrationBuilder.CreateIndex(
                name: "IX_MealLogs_UserId_CreatedAt",
                table: "MealLogs",
                columns: new[] { "UserId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MealLogs_UserId_CreatedAt",
                table: "MealLogs");

            migrationBuilder.AddColumn<DateTime>(
                name: "LoggedAt",
                table: "MealLogs",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_MealLogs_UserId_LoggedAt",
                table: "MealLogs",
                columns: new[] { "UserId", "LoggedAt" });
        }
    }
}

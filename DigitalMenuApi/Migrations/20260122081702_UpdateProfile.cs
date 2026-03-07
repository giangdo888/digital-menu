using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalMenuApi.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DietaryGoal",
                table: "UserProfiles");

            migrationBuilder.RenameColumn(
                name: "GoalWeightKg",
                table: "UserProfiles",
                newName: "BmiGoal");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BmiGoal",
                table: "UserProfiles",
                newName: "GoalWeightKg");

            migrationBuilder.AddColumn<string>(
                name: "DietaryGoal",
                table: "UserProfiles",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }
    }
}

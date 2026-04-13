using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalMenuApi.Migrations
{
    /// <inheritdoc />
    public partial class ChangeBmiGoalToWeeklyWeightGoal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BmiGoal",
                table: "UserProfiles",
                newName: "WeeklyWeightGoal");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "WeeklyWeightGoal",
                table: "UserProfiles",
                newName: "BmiGoal");
        }
    }
}

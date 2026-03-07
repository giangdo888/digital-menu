using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalMenuApi.Migrations
{
    /// <inheritdoc />
    public partial class AddAFCDPublicFoodKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PublicFoodKey",
                table: "AFCDItems",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AFCDItems_PublicFoodKey",
                table: "AFCDItems",
                column: "PublicFoodKey",
                unique: true,
                filter: "[PublicFoodKey] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AFCDItems_PublicFoodKey",
                table: "AFCDItems");

            migrationBuilder.DropColumn(
                name: "PublicFoodKey",
                table: "AFCDItems");
        }
    }
}

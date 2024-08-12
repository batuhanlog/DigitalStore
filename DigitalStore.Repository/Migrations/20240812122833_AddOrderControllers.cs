using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalStore.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderControllers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "MaxPoints",
                table: "Products",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PointsPercentage",
                table: "Products",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxPoints",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "PointsPercentage",
                table: "Products");
        }
    }
}

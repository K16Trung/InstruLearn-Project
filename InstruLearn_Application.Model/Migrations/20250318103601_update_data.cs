using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstruLearn_Application.Model.Migrations
{
    /// <inheritdoc />
    public partial class update_data : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "Purchase_Items");

            migrationBuilder.AddColumn<decimal>(
                name: "TotalPrice",
                table: "Purchases",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalPrice",
                table: "Purchases");

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "Purchase_Items",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}

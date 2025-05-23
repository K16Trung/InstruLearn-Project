using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace InstruLearn_Application.Model.Migrations
{
    /// <inheritdoc />
    public partial class SeedRegistrationTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Learning_Registration_Types",
                columns: new[] { "RegisTypeId", "RegisPrice", "RegisTypeName" },
                values: new object[,]
                {
                    { 1, 0.00m, "Đăng kí học theo yêu cầu" },
                    { 2, 0.00m, "Center" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Learning_Registration_Types",
                keyColumn: "RegisTypeId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Learning_Registration_Types",
                keyColumn: "RegisTypeId",
                keyValue: 2);
        }
    }
}

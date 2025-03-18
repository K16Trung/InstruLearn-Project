using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstruLearn_Application.Model.Migrations
{
    /// <inheritdoc />
    public partial class regis_data : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Feedback",
                table: "Test_Results");

            migrationBuilder.DropColumn(
                name: "LevelAssigned",
                table: "Test_Results");

            migrationBuilder.DropColumn(
                name: "Score",
                table: "Test_Results");

            migrationBuilder.DropColumn(
                name: "VideoUrl",
                table: "Test_Results");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Test_Results",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Feedback",
                table: "Learning_Registrations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LevelAssigned",
                table: "Learning_Registrations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Learning_Registrations",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Score",
                table: "Learning_Registrations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VideoUrl",
                table: "Learning_Registrations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Test_Results");

            migrationBuilder.DropColumn(
                name: "Feedback",
                table: "Learning_Registrations");

            migrationBuilder.DropColumn(
                name: "LevelAssigned",
                table: "Learning_Registrations");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Learning_Registrations");

            migrationBuilder.DropColumn(
                name: "Score",
                table: "Learning_Registrations");

            migrationBuilder.DropColumn(
                name: "VideoUrl",
                table: "Learning_Registrations");

            migrationBuilder.AddColumn<string>(
                name: "Feedback",
                table: "Test_Results",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LevelAssigned",
                table: "Test_Results",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Score",
                table: "Test_Results",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VideoUrl",
                table: "Test_Results",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}

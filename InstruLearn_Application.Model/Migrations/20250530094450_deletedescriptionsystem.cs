using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstruLearn_Application.Model.Migrations
{
    /// <inheritdoc />
    public partial class deletedescriptionsystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "SystemConfigurations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "SystemConfigurations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}

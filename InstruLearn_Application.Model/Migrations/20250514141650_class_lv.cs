using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstruLearn_Application.Model.Migrations
{
    /// <inheritdoc />
    public partial class class_lv : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LevelId",
                table: "Classes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Classes_LevelId",
                table: "Classes",
                column: "LevelId");

            migrationBuilder.AddForeignKey(
                name: "FK_Classes_LevelAssigneds_LevelId",
                table: "Classes",
                column: "LevelId",
                principalTable: "LevelAssigneds",
                principalColumn: "LevelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Classes_LevelAssigneds_LevelId",
                table: "Classes");

            migrationBuilder.DropIndex(
                name: "IX_Classes_LevelId",
                table: "Classes");

            migrationBuilder.DropColumn(
                name: "LevelId",
                table: "Classes");
        }
    }
}

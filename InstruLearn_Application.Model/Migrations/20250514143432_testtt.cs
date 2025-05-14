using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstruLearn_Application.Model.Migrations
{
    /// <inheritdoc />
    public partial class testtt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Classes_LevelAssigneds_LevelId",
                table: "Classes");

            migrationBuilder.AddForeignKey(
                name: "FK_Classes_LevelAssigneds_LevelId",
                table: "Classes",
                column: "LevelId",
                principalTable: "LevelAssigneds",
                principalColumn: "LevelId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Classes_LevelAssigneds_LevelId",
                table: "Classes");

            migrationBuilder.AddForeignKey(
                name: "FK_Classes_LevelAssigneds_LevelId",
                table: "Classes",
                column: "LevelId",
                principalTable: "LevelAssigneds",
                principalColumn: "LevelId");
        }
    }
}

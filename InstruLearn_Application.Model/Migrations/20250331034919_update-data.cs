using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstruLearn_Application.Model.Migrations
{
    /// <inheritdoc />
    public partial class updatedata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LevelId",
                table: "Learning_Registrations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Learning_Registrations_LevelId",
                table: "Learning_Registrations",
                column: "LevelId");

            migrationBuilder.AddForeignKey(
                name: "FK_Learning_Registrations_LevelAssigneds_LevelId",
                table: "Learning_Registrations",
                column: "LevelId",
                principalTable: "LevelAssigneds",
                principalColumn: "LevelId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Learning_Registrations_LevelAssigneds_LevelId",
                table: "Learning_Registrations");

            migrationBuilder.DropIndex(
                name: "IX_Learning_Registrations_LevelId",
                table: "Learning_Registrations");

            migrationBuilder.DropColumn(
                name: "LevelId",
                table: "Learning_Registrations");
        }
    }
}

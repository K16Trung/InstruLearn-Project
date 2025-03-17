using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstruLearn_Application.Model.Migrations
{
    /// <inheritdoc />
    public partial class fix_db : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MajorId",
                table: "Learning_Registrations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Learning_Registrations_MajorId",
                table: "Learning_Registrations",
                column: "MajorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Learning_Registrations_Majors_MajorId",
                table: "Learning_Registrations",
                column: "MajorId",
                principalTable: "Majors",
                principalColumn: "MajorId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Learning_Registrations_Majors_MajorId",
                table: "Learning_Registrations");

            migrationBuilder.DropIndex(
                name: "IX_Learning_Registrations_MajorId",
                table: "Learning_Registrations");

            migrationBuilder.DropColumn(
                name: "MajorId",
                table: "Learning_Registrations");
        }
    }
}

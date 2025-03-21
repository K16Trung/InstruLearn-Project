using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstruLearn_Application.Model.Migrations
{
    /// <inheritdoc />
    public partial class fix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Test_Results_MajorTests_MajorTestId",
                table: "Test_Results");

            migrationBuilder.RenameColumn(
                name: "MajorTestId",
                table: "Test_Results",
                newName: "MajorId");

            migrationBuilder.RenameIndex(
                name: "IX_Test_Results_MajorTestId",
                table: "Test_Results",
                newName: "IX_Test_Results_MajorId");

            migrationBuilder.AddColumn<int>(
                name: "ResultType",
                table: "Test_Results",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_Test_Results_Majors_MajorId",
                table: "Test_Results",
                column: "MajorId",
                principalTable: "Majors",
                principalColumn: "MajorId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Test_Results_Majors_MajorId",
                table: "Test_Results");

            migrationBuilder.DropColumn(
                name: "ResultType",
                table: "Test_Results");

            migrationBuilder.RenameColumn(
                name: "MajorId",
                table: "Test_Results",
                newName: "MajorTestId");

            migrationBuilder.RenameIndex(
                name: "IX_Test_Results_MajorId",
                table: "Test_Results",
                newName: "IX_Test_Results_MajorTestId");

            migrationBuilder.AddForeignKey(
                name: "FK_Test_Results_MajorTests_MajorTestId",
                table: "Test_Results",
                column: "MajorTestId",
                principalTable: "MajorTests",
                principalColumn: "MajorTestId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

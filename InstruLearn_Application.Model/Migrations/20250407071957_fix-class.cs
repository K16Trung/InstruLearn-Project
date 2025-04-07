using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstruLearn_Application.Model.Migrations
{
    /// <inheritdoc />
    public partial class fixclass : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Classes_CoursePackages_CoursePackageId",
                table: "Classes");

            migrationBuilder.RenameColumn(
                name: "CoursePackageId",
                table: "Classes",
                newName: "MajorId");

            migrationBuilder.RenameIndex(
                name: "IX_Classes_CoursePackageId",
                table: "Classes",
                newName: "IX_Classes_MajorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Classes_Majors_MajorId",
                table: "Classes",
                column: "MajorId",
                principalTable: "Majors",
                principalColumn: "MajorId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Classes_Majors_MajorId",
                table: "Classes");

            migrationBuilder.RenameColumn(
                name: "MajorId",
                table: "Classes",
                newName: "CoursePackageId");

            migrationBuilder.RenameIndex(
                name: "IX_Classes_MajorId",
                table: "Classes",
                newName: "IX_Classes_CoursePackageId");

            migrationBuilder.AddForeignKey(
                name: "FK_Classes_CoursePackages_CoursePackageId",
                table: "Classes",
                column: "CoursePackageId",
                principalTable: "CoursePackages",
                principalColumn: "CoursePackageId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

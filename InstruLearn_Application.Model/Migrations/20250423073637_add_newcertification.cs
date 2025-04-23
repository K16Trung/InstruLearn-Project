using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstruLearn_Application.Model.Migrations
{
    /// <inheritdoc />
    public partial class add_newcertification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Certifications_CoursePackages_CoursePackageId",
                table: "Certifications");

            migrationBuilder.DropIndex(
                name: "IX_Certifications_CoursePackageId",
                table: "Certifications");

            migrationBuilder.RenameColumn(
                name: "CoursePackageId",
                table: "Certifications",
                newName: "CertificationType");

            migrationBuilder.AddColumn<DateTime>(
                name: "IssueDate",
                table: "Certifications",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "LearningMode",
                table: "Certifications",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LearningRegisId",
                table: "Certifications",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Subject",
                table: "Certifications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TeacherName",
                table: "Certifications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Certifications_LearningRegisId",
                table: "Certifications",
                column: "LearningRegisId");

            migrationBuilder.AddForeignKey(
                name: "FK_Certifications_Learning_Registrations_LearningRegisId",
                table: "Certifications",
                column: "LearningRegisId",
                principalTable: "Learning_Registrations",
                principalColumn: "LearningRegisId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Certifications_Learning_Registrations_LearningRegisId",
                table: "Certifications");

            migrationBuilder.DropIndex(
                name: "IX_Certifications_LearningRegisId",
                table: "Certifications");

            migrationBuilder.DropColumn(
                name: "IssueDate",
                table: "Certifications");

            migrationBuilder.DropColumn(
                name: "LearningMode",
                table: "Certifications");

            migrationBuilder.DropColumn(
                name: "LearningRegisId",
                table: "Certifications");

            migrationBuilder.DropColumn(
                name: "Subject",
                table: "Certifications");

            migrationBuilder.DropColumn(
                name: "TeacherName",
                table: "Certifications");

            migrationBuilder.RenameColumn(
                name: "CertificationType",
                table: "Certifications",
                newName: "CoursePackageId");

            migrationBuilder.CreateIndex(
                name: "IX_Certifications_CoursePackageId",
                table: "Certifications",
                column: "CoursePackageId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Certifications_CoursePackages_CoursePackageId",
                table: "Certifications",
                column: "CoursePackageId",
                principalTable: "CoursePackages",
                principalColumn: "CoursePackageId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

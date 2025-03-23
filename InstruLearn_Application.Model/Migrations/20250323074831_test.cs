using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstruLearn_Application.Model.Migrations
{
    /// <inheritdoc />
    public partial class test : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TeacherMajor_Majors_MajorId",
                table: "TeacherMajor");

            migrationBuilder.DropForeignKey(
                name: "FK_TeacherMajor_Teachers_TeacherId",
                table: "TeacherMajor");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TeacherMajor",
                table: "TeacherMajor");

            migrationBuilder.RenameTable(
                name: "TeacherMajor",
                newName: "TeacherMajors");

            migrationBuilder.RenameIndex(
                name: "IX_TeacherMajor_MajorId",
                table: "TeacherMajors",
                newName: "IX_TeacherMajors_MajorId");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "Accounts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Gender",
                table: "Accounts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "DateOfEmployment",
                table: "Accounts",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1),
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TeacherMajors",
                table: "TeacherMajors",
                columns: new[] { "TeacherId", "MajorId" });

            migrationBuilder.AddForeignKey(
                name: "FK_TeacherMajors_Majors_MajorId",
                table: "TeacherMajors",
                column: "MajorId",
                principalTable: "Majors",
                principalColumn: "MajorId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TeacherMajors_Teachers_TeacherId",
                table: "TeacherMajors",
                column: "TeacherId",
                principalTable: "Teachers",
                principalColumn: "TeacherId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TeacherMajors_Majors_MajorId",
                table: "TeacherMajors");

            migrationBuilder.DropForeignKey(
                name: "FK_TeacherMajors_Teachers_TeacherId",
                table: "TeacherMajors");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TeacherMajors",
                table: "TeacherMajors");

            migrationBuilder.RenameTable(
                name: "TeacherMajors",
                newName: "TeacherMajor");

            migrationBuilder.RenameIndex(
                name: "IX_TeacherMajors_MajorId",
                table: "TeacherMajor",
                newName: "IX_TeacherMajor_MajorId");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "Accounts",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Gender",
                table: "Accounts",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "DateOfEmployment",
                table: "Accounts",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TeacherMajor",
                table: "TeacherMajor",
                columns: new[] { "TeacherId", "MajorId" });

            migrationBuilder.AddForeignKey(
                name: "FK_TeacherMajor_Majors_MajorId",
                table: "TeacherMajor",
                column: "MajorId",
                principalTable: "Majors",
                principalColumn: "MajorId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TeacherMajor_Teachers_TeacherId",
                table: "TeacherMajor",
                column: "TeacherId",
                principalTable: "Teachers",
                principalColumn: "TeacherId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstruLearn_Application.Model.Migrations
{
    /// <inheritdoc />
    public partial class db_laerningregisfeedback_update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "LearningRegisFeedbackQuestions");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "LearningRegisFeedbackOptions");

            migrationBuilder.DropColumn(
                name: "Value",
                table: "LearningRegisFeedbackOptions");

            migrationBuilder.DropColumn(
                name: "Comment",
                table: "LearningRegisFeedbackAnswers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "LearningRegisFeedbackQuestions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "LearningRegisFeedbackOptions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Value",
                table: "LearningRegisFeedbackOptions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Comment",
                table: "LearningRegisFeedbackAnswers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}

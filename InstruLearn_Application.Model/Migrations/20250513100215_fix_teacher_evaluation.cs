using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstruLearn_Application.Model.Migrations
{
    /// <inheritdoc />
    public partial class fix_teacher_evaluation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GoalsAssessment",
                table: "TeacherEvaluationFeedbacks");

            migrationBuilder.DropColumn(
                name: "ProgressRating",
                table: "TeacherEvaluationFeedbacks");

            migrationBuilder.AddColumn<int>(
                name: "RatingValue",
                table: "TeacherEvaluationOptions",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RatingValue",
                table: "TeacherEvaluationOptions");

            migrationBuilder.AddColumn<string>(
                name: "GoalsAssessment",
                table: "TeacherEvaluationFeedbacks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProgressRating",
                table: "TeacherEvaluationFeedbacks",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}

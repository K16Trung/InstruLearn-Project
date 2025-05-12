using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstruLearn_Application.Model.Migrations
{
    /// <inheritdoc />
    public partial class teacherevaluation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TeacherEvaluationFeedbacks",
                columns: table => new
                {
                    EvaluationFeedbackId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LearningRegistrationId = table.Column<int>(type: "int", nullable: false),
                    TeacherId = table.Column<int>(type: "int", nullable: false),
                    LearnerId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GoalsAssessment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProgressRating = table.Column<int>(type: "int", nullable: false),
                    GoalsAchieved = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherEvaluationFeedbacks", x => x.EvaluationFeedbackId);
                    table.ForeignKey(
                        name: "FK_TeacherEvaluationFeedbacks_Learners_LearnerId",
                        column: x => x.LearnerId,
                        principalTable: "Learners",
                        principalColumn: "LearnerId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TeacherEvaluationFeedbacks_Learning_Registrations_LearningRegistrationId",
                        column: x => x.LearningRegistrationId,
                        principalTable: "Learning_Registrations",
                        principalColumn: "LearningRegisId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeacherEvaluationFeedbacks_Teachers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Teachers",
                        principalColumn: "TeacherId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TeacherEvaluationQuestions",
                columns: table => new
                {
                    EvaluationQuestionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuestionText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherEvaluationQuestions", x => x.EvaluationQuestionId);
                });

            migrationBuilder.CreateTable(
                name: "TeacherEvaluationOptions",
                columns: table => new
                {
                    EvaluationOptionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EvaluationQuestionId = table.Column<int>(type: "int", nullable: false),
                    OptionText = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherEvaluationOptions", x => x.EvaluationOptionId);
                    table.ForeignKey(
                        name: "FK_TeacherEvaluationOptions_TeacherEvaluationQuestions_EvaluationQuestionId",
                        column: x => x.EvaluationQuestionId,
                        principalTable: "TeacherEvaluationQuestions",
                        principalColumn: "EvaluationQuestionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeacherEvaluationAnswers",
                columns: table => new
                {
                    EvaluationAnswerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EvaluationFeedbackId = table.Column<int>(type: "int", nullable: false),
                    EvaluationQuestionId = table.Column<int>(type: "int", nullable: false),
                    SelectedOptionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherEvaluationAnswers", x => x.EvaluationAnswerId);
                    table.ForeignKey(
                        name: "FK_TeacherEvaluationAnswers_TeacherEvaluationFeedbacks_EvaluationFeedbackId",
                        column: x => x.EvaluationFeedbackId,
                        principalTable: "TeacherEvaluationFeedbacks",
                        principalColumn: "EvaluationFeedbackId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeacherEvaluationAnswers_TeacherEvaluationOptions_SelectedOptionId",
                        column: x => x.SelectedOptionId,
                        principalTable: "TeacherEvaluationOptions",
                        principalColumn: "EvaluationOptionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TeacherEvaluationAnswers_TeacherEvaluationQuestions_EvaluationQuestionId",
                        column: x => x.EvaluationQuestionId,
                        principalTable: "TeacherEvaluationQuestions",
                        principalColumn: "EvaluationQuestionId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TeacherEvaluationAnswers_EvaluationFeedbackId",
                table: "TeacherEvaluationAnswers",
                column: "EvaluationFeedbackId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherEvaluationAnswers_EvaluationQuestionId",
                table: "TeacherEvaluationAnswers",
                column: "EvaluationQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherEvaluationAnswers_SelectedOptionId",
                table: "TeacherEvaluationAnswers",
                column: "SelectedOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherEvaluationFeedbacks_LearnerId",
                table: "TeacherEvaluationFeedbacks",
                column: "LearnerId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherEvaluationFeedbacks_LearningRegistrationId",
                table: "TeacherEvaluationFeedbacks",
                column: "LearningRegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherEvaluationFeedbacks_TeacherId",
                table: "TeacherEvaluationFeedbacks",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherEvaluationOptions_EvaluationQuestionId",
                table: "TeacherEvaluationOptions",
                column: "EvaluationQuestionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TeacherEvaluationAnswers");

            migrationBuilder.DropTable(
                name: "TeacherEvaluationFeedbacks");

            migrationBuilder.DropTable(
                name: "TeacherEvaluationOptions");

            migrationBuilder.DropTable(
                name: "TeacherEvaluationQuestions");
        }
    }
}

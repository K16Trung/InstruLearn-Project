using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstruLearn_Application.Model.Migrations
{
    /// <inheritdoc />
    public partial class FeedbackLearningRegis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LearningRegisFeedbackQuestions",
                columns: table => new
                {
                    QuestionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuestionText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningRegisFeedbackQuestions", x => x.QuestionId);
                });

            migrationBuilder.CreateTable(
                name: "LearningRegisFeedbacks",
                columns: table => new
                {
                    FeedbackId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LearningRegistrationId = table.Column<int>(type: "int", nullable: false),
                    LearnerId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AdditionalComments = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningRegisFeedbacks", x => x.FeedbackId);
                    table.ForeignKey(
                        name: "FK_LearningRegisFeedbacks_Accounts_LearnerId",
                        column: x => x.LearnerId,
                        principalTable: "Accounts",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LearningRegisFeedbacks_Learning_Registrations_LearningRegistrationId",
                        column: x => x.LearningRegistrationId,
                        principalTable: "Learning_Registrations",
                        principalColumn: "LearningRegisId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LearningRegisFeedbackOptions",
                columns: table => new
                {
                    OptionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuestionId = table.Column<int>(type: "int", nullable: false),
                    OptionText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Value = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningRegisFeedbackOptions", x => x.OptionId);
                    table.ForeignKey(
                        name: "FK_LearningRegisFeedbackOptions_LearningRegisFeedbackQuestions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "LearningRegisFeedbackQuestions",
                        principalColumn: "QuestionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LearningRegisFeedbackAnswers",
                columns: table => new
                {
                    AnswerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FeedbackId = table.Column<int>(type: "int", nullable: false),
                    QuestionId = table.Column<int>(type: "int", nullable: false),
                    SelectedOptionId = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningRegisFeedbackAnswers", x => x.AnswerId);
                    table.ForeignKey(
                        name: "FK_LearningRegisFeedbackAnswers_LearningRegisFeedbackOptions_SelectedOptionId",
                        column: x => x.SelectedOptionId,
                        principalTable: "LearningRegisFeedbackOptions",
                        principalColumn: "OptionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LearningRegisFeedbackAnswers_LearningRegisFeedbackQuestions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "LearningRegisFeedbackQuestions",
                        principalColumn: "QuestionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LearningRegisFeedbackAnswers_LearningRegisFeedbacks_FeedbackId",
                        column: x => x.FeedbackId,
                        principalTable: "LearningRegisFeedbacks",
                        principalColumn: "FeedbackId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LearningRegisFeedbackAnswers_FeedbackId",
                table: "LearningRegisFeedbackAnswers",
                column: "FeedbackId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningRegisFeedbackAnswers_QuestionId",
                table: "LearningRegisFeedbackAnswers",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningRegisFeedbackAnswers_SelectedOptionId",
                table: "LearningRegisFeedbackAnswers",
                column: "SelectedOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningRegisFeedbackOptions_QuestionId",
                table: "LearningRegisFeedbackOptions",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningRegisFeedbacks_LearnerId",
                table: "LearningRegisFeedbacks",
                column: "LearnerId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningRegisFeedbacks_LearningRegistrationId",
                table: "LearningRegisFeedbacks",
                column: "LearningRegistrationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LearningRegisFeedbackAnswers");

            migrationBuilder.DropTable(
                name: "LearningRegisFeedbackOptions");

            migrationBuilder.DropTable(
                name: "LearningRegisFeedbacks");

            migrationBuilder.DropTable(
                name: "LearningRegisFeedbackQuestions");
        }
    }
}

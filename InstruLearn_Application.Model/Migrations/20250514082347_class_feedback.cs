using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstruLearn_Application.Model.Migrations
{
    /// <inheritdoc />
    public partial class class_feedback : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LevelFeedbackTemplates",
                columns: table => new
                {
                    TemplateId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LevelId = table.Column<int>(type: "int", nullable: false),
                    TemplateName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LevelFeedbackTemplates", x => x.TemplateId);
                    table.ForeignKey(
                        name: "FK_LevelFeedbackTemplates_LevelAssigneds_LevelId",
                        column: x => x.LevelId,
                        principalTable: "LevelAssigneds",
                        principalColumn: "LevelId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClassFeedbacks",
                columns: table => new
                {
                    FeedbackId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClassId = table.Column<int>(type: "int", nullable: false),
                    LearnerId = table.Column<int>(type: "int", nullable: false),
                    TemplateId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AdditionalComments = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassFeedbacks", x => x.FeedbackId);
                    table.ForeignKey(
                        name: "FK_ClassFeedbacks_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "ClassId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClassFeedbacks_Learners_LearnerId",
                        column: x => x.LearnerId,
                        principalTable: "Learners",
                        principalColumn: "LearnerId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClassFeedbacks_LevelFeedbackTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "LevelFeedbackTemplates",
                        principalColumn: "TemplateId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LevelFeedbackCriteria",
                columns: table => new
                {
                    CriterionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateId = table.Column<int>(type: "int", nullable: false),
                    GradeCategory = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LevelFeedbackCriteria", x => x.CriterionId);
                    table.ForeignKey(
                        name: "FK_LevelFeedbackCriteria_LevelFeedbackTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "LevelFeedbackTemplates",
                        principalColumn: "TemplateId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClassFeedbackEvaluations",
                columns: table => new
                {
                    EvaluationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FeedbackId = table.Column<int>(type: "int", nullable: false),
                    CriterionId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassFeedbackEvaluations", x => x.EvaluationId);
                    table.ForeignKey(
                        name: "FK_ClassFeedbackEvaluations_ClassFeedbacks_FeedbackId",
                        column: x => x.FeedbackId,
                        principalTable: "ClassFeedbacks",
                        principalColumn: "FeedbackId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassFeedbackEvaluations_LevelFeedbackCriteria_CriterionId",
                        column: x => x.CriterionId,
                        principalTable: "LevelFeedbackCriteria",
                        principalColumn: "CriterionId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClassFeedbackEvaluations_CriterionId",
                table: "ClassFeedbackEvaluations",
                column: "CriterionId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassFeedbackEvaluations_FeedbackId",
                table: "ClassFeedbackEvaluations",
                column: "FeedbackId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassFeedbacks_ClassId",
                table: "ClassFeedbacks",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassFeedbacks_LearnerId",
                table: "ClassFeedbacks",
                column: "LearnerId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassFeedbacks_TemplateId",
                table: "ClassFeedbacks",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_LevelFeedbackCriteria_TemplateId",
                table: "LevelFeedbackCriteria",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_LevelFeedbackTemplates_LevelId",
                table: "LevelFeedbackTemplates",
                column: "LevelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClassFeedbackEvaluations");

            migrationBuilder.DropTable(
                name: "ClassFeedbacks");

            migrationBuilder.DropTable(
                name: "LevelFeedbackCriteria");

            migrationBuilder.DropTable(
                name: "LevelFeedbackTemplates");
        }
    }
}

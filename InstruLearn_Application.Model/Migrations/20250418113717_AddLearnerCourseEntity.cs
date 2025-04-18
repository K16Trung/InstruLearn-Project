using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstruLearn_Application.Model.Migrations
{
    /// <inheritdoc />
    public partial class AddLearnerCourseEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LearnerCourses",
                columns: table => new
                {
                    LearnerCourseId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LearnerId = table.Column<int>(type: "int", nullable: false),
                    CoursePackageId = table.Column<int>(type: "int", nullable: false),
                    CompletionPercentage = table.Column<double>(type: "float", nullable: false),
                    EnrollmentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastAccessDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearnerCourses", x => x.LearnerCourseId);
                    table.ForeignKey(
                        name: "FK_LearnerCourses_CoursePackages_CoursePackageId",
                        column: x => x.CoursePackageId,
                        principalTable: "CoursePackages",
                        principalColumn: "CoursePackageId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LearnerCourses_Learners_LearnerId",
                        column: x => x.LearnerId,
                        principalTable: "Learners",
                        principalColumn: "LearnerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LearnerCourses_CoursePackageId",
                table: "LearnerCourses",
                column: "CoursePackageId");

            migrationBuilder.CreateIndex(
                name: "IX_LearnerCourses_LearnerId",
                table: "LearnerCourses",
                column: "LearnerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LearnerCourses");
        }
    }
}

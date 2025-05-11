using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstruLearn_Application.Model.Migrations
{
    /// <inheritdoc />
    public partial class delete_test_result : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Test_Results");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Test_Results",
                columns: table => new
                {
                    TestResultId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LearnerId = table.Column<int>(type: "int", nullable: false),
                    LearningRegisId = table.Column<int>(type: "int", nullable: true),
                    MajorId = table.Column<int>(type: "int", nullable: true),
                    TeacherId = table.Column<int>(type: "int", nullable: false),
                    ResultType = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Test_Results", x => x.TestResultId);
                    table.ForeignKey(
                        name: "FK_Test_Results_Learners_LearnerId",
                        column: x => x.LearnerId,
                        principalTable: "Learners",
                        principalColumn: "LearnerId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Test_Results_Learning_Registrations_LearningRegisId",
                        column: x => x.LearningRegisId,
                        principalTable: "Learning_Registrations",
                        principalColumn: "LearningRegisId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Test_Results_Majors_MajorId",
                        column: x => x.MajorId,
                        principalTable: "Majors",
                        principalColumn: "MajorId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Test_Results_Teachers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Teachers",
                        principalColumn: "TeacherId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Test_Results_LearnerId",
                table: "Test_Results",
                column: "LearnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Test_Results_LearningRegisId",
                table: "Test_Results",
                column: "LearningRegisId");

            migrationBuilder.CreateIndex(
                name: "IX_Test_Results_MajorId",
                table: "Test_Results",
                column: "MajorId");

            migrationBuilder.CreateIndex(
                name: "IX_Test_Results_TeacherId",
                table: "Test_Results",
                column: "TeacherId");
        }
    }
}

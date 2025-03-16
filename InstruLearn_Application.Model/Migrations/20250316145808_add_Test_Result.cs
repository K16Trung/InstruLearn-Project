using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstruLearn_Application.Model.Migrations
{
    /// <inheritdoc />
    public partial class add_Test_Result : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Test_Results",
                columns: table => new
                {
                    TestResultId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LearnerId = table.Column<int>(type: "int", nullable: false),
                    TeacherId = table.Column<int>(type: "int", nullable: false),
                    MajorTestId = table.Column<int>(type: "int", nullable: false),
                    LearningRegisId = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false),
                    LevelAssigned = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Feedback = table.Column<string>(type: "nvarchar(max)", nullable: false)
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
                        name: "FK_Test_Results_MajorTests_MajorTestId",
                        column: x => x.MajorTestId,
                        principalTable: "MajorTests",
                        principalColumn: "MajorTestId",
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
                name: "IX_Test_Results_MajorTestId",
                table: "Test_Results",
                column: "MajorTestId");

            migrationBuilder.CreateIndex(
                name: "IX_Test_Results_TeacherId",
                table: "Test_Results",
                column: "TeacherId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Test_Results");
        }
    }
}

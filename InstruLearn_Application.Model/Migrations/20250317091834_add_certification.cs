using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstruLearn_Application.Model.Migrations
{
    /// <inheritdoc />
    public partial class add_certification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Certifications",
                columns: table => new
                {
                    CertificationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LearnerId = table.Column<int>(type: "int", nullable: false),
                    CoursePackageId = table.Column<int>(type: "int", nullable: false),
                    CertificationName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Certifications", x => x.CertificationId);
                    table.ForeignKey(
                        name: "FK_Certifications_CoursePackages_CoursePackageId",
                        column: x => x.CoursePackageId,
                        principalTable: "CoursePackages",
                        principalColumn: "CoursePackageId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Certifications_Learners_LearnerId",
                        column: x => x.LearnerId,
                        principalTable: "Learners",
                        principalColumn: "LearnerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Certifications_CoursePackageId",
                table: "Certifications",
                column: "CoursePackageId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Certifications_LearnerId",
                table: "Certifications",
                column: "LearnerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Certifications");
        }
    }
}

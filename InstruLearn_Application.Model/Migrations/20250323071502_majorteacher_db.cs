using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstruLearn_Application.Model.Migrations
{
    /// <inheritdoc />
    public partial class majorteacher_db : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Teachers_Majors_MajorId",
                table: "Teachers");

            migrationBuilder.DropIndex(
                name: "IX_Teachers_MajorId",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "MajorId",
                table: "Teachers");

            migrationBuilder.CreateTable(
                name: "TeacherMajor",
                columns: table => new
                {
                    TeacherId = table.Column<int>(type: "int", nullable: false),
                    MajorId = table.Column<int>(type: "int", nullable: false),
                    TeacherMajorId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherMajor", x => new { x.TeacherId, x.MajorId });
                    table.ForeignKey(
                        name: "FK_TeacherMajor_Majors_MajorId",
                        column: x => x.MajorId,
                        principalTable: "Majors",
                        principalColumn: "MajorId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeacherMajor_Teachers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Teachers",
                        principalColumn: "TeacherId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TeacherMajor_MajorId",
                table: "TeacherMajor",
                column: "MajorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TeacherMajor");

            migrationBuilder.AddColumn<int>(
                name: "MajorId",
                table: "Teachers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Teachers_MajorId",
                table: "Teachers",
                column: "MajorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Teachers_Majors_MajorId",
                table: "Teachers",
                column: "MajorId",
                principalTable: "Majors",
                principalColumn: "MajorId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

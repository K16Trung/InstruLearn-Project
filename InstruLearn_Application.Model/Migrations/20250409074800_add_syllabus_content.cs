using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstruLearn_Application.Model.Migrations
{
    /// <inheritdoc />
    public partial class add_syllabus_content : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Syllabus_Contents",
                columns: table => new
                {
                    SyllabusContentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SyllabusId = table.Column<int>(type: "int", nullable: false),
                    ContentName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Syllabus_Contents", x => x.SyllabusContentId);
                    table.ForeignKey(
                        name: "FK_Syllabus_Contents_Syllabus_SyllabusId",
                        column: x => x.SyllabusId,
                        principalTable: "Syllabus",
                        principalColumn: "SyllabusId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Syllabus_Contents_SyllabusId",
                table: "Syllabus_Contents",
                column: "SyllabusId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Syllabus_Contents");
        }
    }
}

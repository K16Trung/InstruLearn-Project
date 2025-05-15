using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstruLearn_Application.Model.Migrations
{
    /// <inheritdoc />
    public partial class remove_syllabus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Classes_Syllabus_SyllabusId",
                table: "Classes");

            migrationBuilder.DropTable(
                name: "Syllabus_Contents");

            migrationBuilder.DropTable(
                name: "Syllabus");

            migrationBuilder.DropIndex(
                name: "IX_Classes_SyllabusId",
                table: "Classes");

            migrationBuilder.DropColumn(
                name: "SyllabusId",
                table: "Classes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SyllabusId",
                table: "Classes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Syllabus",
                columns: table => new
                {
                    SyllabusId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SyllabusDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SyllabusName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Syllabus", x => x.SyllabusId);
                });

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
                name: "IX_Classes_SyllabusId",
                table: "Classes",
                column: "SyllabusId");

            migrationBuilder.CreateIndex(
                name: "IX_Syllabus_Contents_SyllabusId",
                table: "Syllabus_Contents",
                column: "SyllabusId");

            migrationBuilder.AddForeignKey(
                name: "FK_Classes_Syllabus_SyllabusId",
                table: "Classes",
                column: "SyllabusId",
                principalTable: "Syllabus",
                principalColumn: "SyllabusId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

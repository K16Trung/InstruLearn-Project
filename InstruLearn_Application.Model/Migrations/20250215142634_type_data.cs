using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstruLearn_Application.Model.Migrations
{
    /// <inheritdoc />
    public partial class type_data : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Type",
                table: "Courses",
                newName: "TypeId");

            migrationBuilder.CreateTable(
                name: "CourseType",
                columns: table => new
                {
                    TypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TypeName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseType", x => x.TypeId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Courses_TypeId",
                table: "Courses",
                column: "TypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_CourseType_TypeId",
                table: "Courses",
                column: "TypeId",
                principalTable: "CourseType",
                principalColumn: "TypeId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_CourseType_TypeId",
                table: "Courses");

            migrationBuilder.DropTable(
                name: "CourseType");

            migrationBuilder.DropIndex(
                name: "IX_Courses_TypeId",
                table: "Courses");

            migrationBuilder.RenameColumn(
                name: "TypeId",
                table: "Courses",
                newName: "Type");
        }
    }
}

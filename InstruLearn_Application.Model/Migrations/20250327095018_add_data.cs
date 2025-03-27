using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstruLearn_Application.Model.Migrations
{
    /// <inheritdoc />
    public partial class add_data : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Feedback",
                table: "Learning_Registrations");

            migrationBuilder.DropColumn(
                name: "LevelAssigned",
                table: "Learning_Registrations");

            migrationBuilder.DropColumn(
                name: "Score",
                table: "Learning_Registrations");

            migrationBuilder.AddColumn<int>(
                name: "ResponseId",
                table: "Learning_Registrations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "LevelAssigneds",
                columns: table => new
                {
                    LevelId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MajorId = table.Column<int>(type: "int", nullable: false),
                    LevelName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LevelPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LevelAssigneds", x => x.LevelId);
                    table.ForeignKey(
                        name: "FK_LevelAssigneds_Majors_MajorId",
                        column: x => x.MajorId,
                        principalTable: "Majors",
                        principalColumn: "MajorId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ResponseTypes",
                columns: table => new
                {
                    ResponseTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ResponseTypeName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResponseTypes", x => x.ResponseTypeId);
                });

            migrationBuilder.CreateTable(
                name: "Responses",
                columns: table => new
                {
                    ResponseId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ResponseTypeId = table.Column<int>(type: "int", nullable: false),
                    ResponseName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Responses", x => x.ResponseId);
                    table.ForeignKey(
                        name: "FK_Responses_ResponseTypes_ResponseTypeId",
                        column: x => x.ResponseTypeId,
                        principalTable: "ResponseTypes",
                        principalColumn: "ResponseTypeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Learning_Registrations_ResponseId",
                table: "Learning_Registrations",
                column: "ResponseId");

            migrationBuilder.CreateIndex(
                name: "IX_LevelAssigneds_MajorId",
                table: "LevelAssigneds",
                column: "MajorId");

            migrationBuilder.CreateIndex(
                name: "IX_Responses_ResponseTypeId",
                table: "Responses",
                column: "ResponseTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Learning_Registrations_Responses_ResponseId",
                table: "Learning_Registrations",
                column: "ResponseId",
                principalTable: "Responses",
                principalColumn: "ResponseId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Learning_Registrations_Responses_ResponseId",
                table: "Learning_Registrations");

            migrationBuilder.DropTable(
                name: "LevelAssigneds");

            migrationBuilder.DropTable(
                name: "Responses");

            migrationBuilder.DropTable(
                name: "ResponseTypes");

            migrationBuilder.DropIndex(
                name: "IX_Learning_Registrations_ResponseId",
                table: "Learning_Registrations");

            migrationBuilder.DropColumn(
                name: "ResponseId",
                table: "Learning_Registrations");

            migrationBuilder.AddColumn<string>(
                name: "Feedback",
                table: "Learning_Registrations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LevelAssigned",
                table: "Learning_Registrations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Score",
                table: "Learning_Registrations",
                type: "int",
                nullable: true);
        }
    }
}

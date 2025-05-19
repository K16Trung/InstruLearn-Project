using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstruLearn_Application.Model.Migrations
{
    /// <inheritdoc />
    public partial class SelfAssessment_db : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SelfAssessment",
                table: "Learning_Registrations");

            migrationBuilder.AddColumn<int>(
                name: "SelfAssessmentId",
                table: "Learning_Registrations",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SelfAssessments",
                columns: table => new
                {
                    SelfAssessmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SelfAssessments", x => x.SelfAssessmentId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Learning_Registrations_SelfAssessmentId",
                table: "Learning_Registrations",
                column: "SelfAssessmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Learning_Registrations_SelfAssessments_SelfAssessmentId",
                table: "Learning_Registrations",
                column: "SelfAssessmentId",
                principalTable: "SelfAssessments",
                principalColumn: "SelfAssessmentId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Learning_Registrations_SelfAssessments_SelfAssessmentId",
                table: "Learning_Registrations");

            migrationBuilder.DropTable(
                name: "SelfAssessments");

            migrationBuilder.DropIndex(
                name: "IX_Learning_Registrations_SelfAssessmentId",
                table: "Learning_Registrations");

            migrationBuilder.DropColumn(
                name: "SelfAssessmentId",
                table: "Learning_Registrations");

            migrationBuilder.AddColumn<string>(
                name: "SelfAssessment",
                table: "Learning_Registrations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstruLearn_Application.Model.Migrations
{
    /// <inheritdoc />
    public partial class removecertification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Certifications_Learning_Registrations_LearningRegisId",
                table: "Certifications");

            migrationBuilder.DropIndex(
                name: "IX_Certifications_LearningRegisId",
                table: "Certifications");

            migrationBuilder.DropColumn(
                name: "LearningRegisId",
                table: "Certifications");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LearningRegisId",
                table: "Certifications",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Certifications_LearningRegisId",
                table: "Certifications",
                column: "LearningRegisId");

            migrationBuilder.AddForeignKey(
                name: "FK_Certifications_Learning_Registrations_LearningRegisId",
                table: "Certifications",
                column: "LearningRegisId",
                principalTable: "Learning_Registrations",
                principalColumn: "LearningRegisId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

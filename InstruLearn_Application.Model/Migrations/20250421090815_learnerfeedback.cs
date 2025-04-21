using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstruLearn_Application.Model.Migrations
{
    /// <inheritdoc />
    public partial class learnerfeedback : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LearningRegisFeedbacks_Accounts_LearnerId",
                table: "LearningRegisFeedbacks");

            migrationBuilder.AlterColumn<int>(
                name: "LearnerId",
                table: "LearningRegisFeedbacks",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_LearningRegisFeedbacks_Learners_LearnerId",
                table: "LearningRegisFeedbacks",
                column: "LearnerId",
                principalTable: "Learners",
                principalColumn: "LearnerId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LearningRegisFeedbacks_Learners_LearnerId",
                table: "LearningRegisFeedbacks");

            migrationBuilder.AlterColumn<string>(
                name: "LearnerId",
                table: "LearningRegisFeedbacks",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_LearningRegisFeedbacks_Accounts_LearnerId",
                table: "LearningRegisFeedbacks",
                column: "LearnerId",
                principalTable: "Accounts",
                principalColumn: "AccountId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

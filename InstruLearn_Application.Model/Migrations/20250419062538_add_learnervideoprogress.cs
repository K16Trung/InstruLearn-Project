using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstruLearn_Application.Model.Migrations
{
    /// <inheritdoc />
    public partial class add_learnervideoprogress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "DurationInSeconds",
                table: "Course_Content_Items",
                type: "float",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LearnerContentProgresses",
                columns: table => new
                {
                    ProgressId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LearnerId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    WatchTimeInSeconds = table.Column<double>(type: "float", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    LastAccessDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearnerContentProgresses", x => x.ProgressId);
                    table.ForeignKey(
                        name: "FK_LearnerContentProgresses_Course_Content_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Course_Content_Items",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LearnerContentProgresses_Learners_LearnerId",
                        column: x => x.LearnerId,
                        principalTable: "Learners",
                        principalColumn: "LearnerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LearnerContentProgresses_ItemId",
                table: "LearnerContentProgresses",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_LearnerContentProgresses_LearnerId",
                table: "LearnerContentProgresses",
                column: "LearnerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LearnerContentProgresses");

            migrationBuilder.DropColumn(
                name: "DurationInSeconds",
                table: "Course_Content_Items");
        }
    }
}

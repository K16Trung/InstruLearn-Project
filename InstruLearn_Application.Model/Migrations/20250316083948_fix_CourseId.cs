using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstruLearn_Application.Model.Migrations
{
    /// <inheritdoc />
    public partial class fix_CourseId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Course_Contents_CoursePackages_CourseId",
                table: "Course_Contents");

            migrationBuilder.DropForeignKey(
                name: "FK_FeedBacks_CoursePackages_CourseId",
                table: "FeedBacks");

            migrationBuilder.DropForeignKey(
                name: "FK_QnA_CoursePackages_CourseId",
                table: "QnA");

            migrationBuilder.RenameColumn(
                name: "CourseId",
                table: "QnA",
                newName: "CoursePackageId");

            migrationBuilder.RenameIndex(
                name: "IX_QnA_CourseId",
                table: "QnA",
                newName: "IX_QnA_CoursePackageId");

            migrationBuilder.RenameColumn(
                name: "CourseId",
                table: "FeedBacks",
                newName: "CoursePackageId");

            migrationBuilder.RenameIndex(
                name: "IX_FeedBacks_CourseId",
                table: "FeedBacks",
                newName: "IX_FeedBacks_CoursePackageId");

            migrationBuilder.RenameColumn(
                name: "CourseId",
                table: "Course_Contents",
                newName: "CoursePackageId");

            migrationBuilder.RenameIndex(
                name: "IX_Course_Contents_CourseId",
                table: "Course_Contents",
                newName: "IX_Course_Contents_CoursePackageId");

            migrationBuilder.AddForeignKey(
                name: "FK_Course_Contents_CoursePackages_CoursePackageId",
                table: "Course_Contents",
                column: "CoursePackageId",
                principalTable: "CoursePackages",
                principalColumn: "CoursePackageId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FeedBacks_CoursePackages_CoursePackageId",
                table: "FeedBacks",
                column: "CoursePackageId",
                principalTable: "CoursePackages",
                principalColumn: "CoursePackageId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_QnA_CoursePackages_CoursePackageId",
                table: "QnA",
                column: "CoursePackageId",
                principalTable: "CoursePackages",
                principalColumn: "CoursePackageId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Course_Contents_CoursePackages_CoursePackageId",
                table: "Course_Contents");

            migrationBuilder.DropForeignKey(
                name: "FK_FeedBacks_CoursePackages_CoursePackageId",
                table: "FeedBacks");

            migrationBuilder.DropForeignKey(
                name: "FK_QnA_CoursePackages_CoursePackageId",
                table: "QnA");

            migrationBuilder.RenameColumn(
                name: "CoursePackageId",
                table: "QnA",
                newName: "CourseId");

            migrationBuilder.RenameIndex(
                name: "IX_QnA_CoursePackageId",
                table: "QnA",
                newName: "IX_QnA_CourseId");

            migrationBuilder.RenameColumn(
                name: "CoursePackageId",
                table: "FeedBacks",
                newName: "CourseId");

            migrationBuilder.RenameIndex(
                name: "IX_FeedBacks_CoursePackageId",
                table: "FeedBacks",
                newName: "IX_FeedBacks_CourseId");

            migrationBuilder.RenameColumn(
                name: "CoursePackageId",
                table: "Course_Contents",
                newName: "CourseId");

            migrationBuilder.RenameIndex(
                name: "IX_Course_Contents_CoursePackageId",
                table: "Course_Contents",
                newName: "IX_Course_Contents_CourseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Course_Contents_CoursePackages_CourseId",
                table: "Course_Contents",
                column: "CourseId",
                principalTable: "CoursePackages",
                principalColumn: "CoursePackageId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FeedBacks_CoursePackages_CourseId",
                table: "FeedBacks",
                column: "CourseId",
                principalTable: "CoursePackages",
                principalColumn: "CoursePackageId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_QnA_CoursePackages_CourseId",
                table: "QnA",
                column: "CourseId",
                principalTable: "CoursePackages",
                principalColumn: "CoursePackageId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

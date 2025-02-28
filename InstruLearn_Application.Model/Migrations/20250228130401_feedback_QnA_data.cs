using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstruLearn_Application.Model.Migrations
{
    /// <inheritdoc />
    public partial class feedback_QnA_data : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FeedBacks",
                columns: table => new
                {
                    FeedbackId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CourseId = table.Column<int>(type: "int", nullable: false),
                    FeedbackContent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedBacks", x => x.FeedbackId);
                    table.ForeignKey(
                        name: "FK_FeedBacks_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FeedBacks_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "CourseId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QnA",
                columns: table => new
                {
                    QuestionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CourseId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QuestionContent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QnA", x => x.QuestionId);
                    table.ForeignKey(
                        name: "FK_QnA_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QnA_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "CourseId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FeedbackReplies",
                columns: table => new
                {
                    FeedbackRepliesId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FeedbackId = table.Column<int>(type: "int", nullable: false),
                    AccountId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RepliesContent = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedbackReplies", x => x.FeedbackRepliesId);
                    table.ForeignKey(
                        name: "FK_FeedbackReplies_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FeedbackReplies_FeedBacks_FeedbackId",
                        column: x => x.FeedbackId,
                        principalTable: "FeedBacks",
                        principalColumn: "FeedbackId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QnAReplies",
                columns: table => new
                {
                    QnARepliesId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    QuestionId = table.Column<int>(type: "int", nullable: false),
                    QnAContent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QnAReplies", x => x.QnARepliesId);
                    table.ForeignKey(
                        name: "FK_QnAReplies_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QnAReplies_QnA_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "QnA",
                        principalColumn: "QuestionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackReplies_AccountId",
                table: "FeedbackReplies",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackReplies_FeedbackId",
                table: "FeedbackReplies",
                column: "FeedbackId");

            migrationBuilder.CreateIndex(
                name: "IX_FeedBacks_AccountId",
                table: "FeedBacks",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_FeedBacks_CourseId",
                table: "FeedBacks",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_QnA_AccountId",
                table: "QnA",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_QnA_CourseId",
                table: "QnA",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_QnAReplies_AccountId",
                table: "QnAReplies",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_QnAReplies_QuestionId",
                table: "QnAReplies",
                column: "QuestionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FeedbackReplies");

            migrationBuilder.DropTable(
                name: "QnAReplies");

            migrationBuilder.DropTable(
                name: "FeedBacks");

            migrationBuilder.DropTable(
                name: "QnA");
        }
    }
}

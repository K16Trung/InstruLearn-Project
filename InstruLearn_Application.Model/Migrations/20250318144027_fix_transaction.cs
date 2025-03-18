using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstruLearn_Application.Model.Migrations
{
    /// <inheritdoc />
    public partial class fix_transaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_WalletTransactions_WalletTransactionId",
                table: "Payments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WalletTransactions",
                table: "WalletTransactions");

            migrationBuilder.DropIndex(
                name: "IX_Payments_WalletTransactionId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "WalletTransactionId",
                table: "WalletTransactions");

            migrationBuilder.DropColumn(
                name: "WalletTransactionId",
                table: "Payments");

            migrationBuilder.AlterColumn<string>(
                name: "TransactionId",
                table: "WalletTransactions",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "TransactionId",
                table: "Payments",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_WalletTransactions",
                table: "WalletTransactions",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_TransactionId",
                table: "Payments",
                column: "TransactionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_WalletTransactions_TransactionId",
                table: "Payments",
                column: "TransactionId",
                principalTable: "WalletTransactions",
                principalColumn: "TransactionId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_WalletTransactions_TransactionId",
                table: "Payments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WalletTransactions",
                table: "WalletTransactions");

            migrationBuilder.DropIndex(
                name: "IX_Payments_TransactionId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "TransactionId",
                table: "Payments");

            migrationBuilder.AlterColumn<string>(
                name: "TransactionId",
                table: "WalletTransactions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<int>(
                name: "WalletTransactionId",
                table: "WalletTransactions",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<int>(
                name: "WalletTransactionId",
                table: "Payments",
                type: "int",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_WalletTransactions",
                table: "WalletTransactions",
                column: "WalletTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_WalletTransactionId",
                table: "Payments",
                column: "WalletTransactionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_WalletTransactions_WalletTransactionId",
                table: "Payments",
                column: "WalletTransactionId",
                principalTable: "WalletTransactions",
                principalColumn: "WalletTransactionId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

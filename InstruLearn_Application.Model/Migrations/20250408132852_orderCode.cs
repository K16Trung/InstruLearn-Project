using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstruLearn_Application.Model.Migrations
{
    /// <inheritdoc />
    public partial class orderCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "OrderCode",
                table: "WalletTransactions",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrderCode",
                table: "WalletTransactions");
        }
    }
}

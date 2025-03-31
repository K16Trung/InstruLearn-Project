using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstruLearn_Application.Model.Migrations
{
    /// <inheritdoc />
    public partial class test1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SchedulesScheduleId",
                table: "ClassDays",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClassDays_SchedulesScheduleId",
                table: "ClassDays",
                column: "SchedulesScheduleId");

            migrationBuilder.AddForeignKey(
                name: "FK_ClassDays_Schedules_SchedulesScheduleId",
                table: "ClassDays",
                column: "SchedulesScheduleId",
                principalTable: "Schedules",
                principalColumn: "ScheduleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClassDays_Schedules_SchedulesScheduleId",
                table: "ClassDays");

            migrationBuilder.DropIndex(
                name: "IX_ClassDays_SchedulesScheduleId",
                table: "ClassDays");

            migrationBuilder.DropColumn(
                name: "SchedulesScheduleId",
                table: "ClassDays");
        }
    }
}

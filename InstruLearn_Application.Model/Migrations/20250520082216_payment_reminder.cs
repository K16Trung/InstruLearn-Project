using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstruLearn_Application.Model.Migrations
{
    /// <inheritdoc />
    public partial class payment_reminder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ChangeTeacherRequested",
                table: "Learning_Registrations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastReminderSent",
                table: "Learning_Registrations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReminderCount",
                table: "Learning_Registrations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "SentTeacherChangeReminder",
                table: "Learning_Registrations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "TeacherChangeProcessed",
                table: "Learning_Registrations",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChangeTeacherRequested",
                table: "Learning_Registrations");

            migrationBuilder.DropColumn(
                name: "LastReminderSent",
                table: "Learning_Registrations");

            migrationBuilder.DropColumn(
                name: "ReminderCount",
                table: "Learning_Registrations");

            migrationBuilder.DropColumn(
                name: "SentTeacherChangeReminder",
                table: "Learning_Registrations");

            migrationBuilder.DropColumn(
                name: "TeacherChangeProcessed",
                table: "Learning_Registrations");
        }
    }
}

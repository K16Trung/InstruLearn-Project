using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstruLearn_Application.Model.Migrations
{
    /// <inheritdoc />
    public partial class fix_type : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CoursePackages_CourseType_TypeId",
                table: "CoursePackages");

            migrationBuilder.DropForeignKey(
                name: "FK_Learning_Registrations_Learning_Registration_Types_TypeId",
                table: "Learning_Registrations");

            migrationBuilder.RenameColumn(
                name: "TypeId",
                table: "Learning_Registrations",
                newName: "RegisTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_Learning_Registrations_TypeId",
                table: "Learning_Registrations",
                newName: "IX_Learning_Registrations_RegisTypeId");

            migrationBuilder.RenameColumn(
                name: "TypeName",
                table: "Learning_Registration_Types",
                newName: "RegisTypeName");

            migrationBuilder.RenameColumn(
                name: "TypeId",
                table: "Learning_Registration_Types",
                newName: "RegisTypeId");

            migrationBuilder.RenameColumn(
                name: "TypeName",
                table: "CourseType",
                newName: "CourseTypeName");

            migrationBuilder.RenameColumn(
                name: "TypeId",
                table: "CourseType",
                newName: "CourseTypeId");

            migrationBuilder.RenameColumn(
                name: "TypeId",
                table: "CoursePackages",
                newName: "CourseTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_CoursePackages_TypeId",
                table: "CoursePackages",
                newName: "IX_CoursePackages_CourseTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_CoursePackages_CourseType_CourseTypeId",
                table: "CoursePackages",
                column: "CourseTypeId",
                principalTable: "CourseType",
                principalColumn: "CourseTypeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Learning_Registrations_Learning_Registration_Types_RegisTypeId",
                table: "Learning_Registrations",
                column: "RegisTypeId",
                principalTable: "Learning_Registration_Types",
                principalColumn: "RegisTypeId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CoursePackages_CourseType_CourseTypeId",
                table: "CoursePackages");

            migrationBuilder.DropForeignKey(
                name: "FK_Learning_Registrations_Learning_Registration_Types_RegisTypeId",
                table: "Learning_Registrations");

            migrationBuilder.RenameColumn(
                name: "RegisTypeId",
                table: "Learning_Registrations",
                newName: "TypeId");

            migrationBuilder.RenameIndex(
                name: "IX_Learning_Registrations_RegisTypeId",
                table: "Learning_Registrations",
                newName: "IX_Learning_Registrations_TypeId");

            migrationBuilder.RenameColumn(
                name: "RegisTypeName",
                table: "Learning_Registration_Types",
                newName: "TypeName");

            migrationBuilder.RenameColumn(
                name: "RegisTypeId",
                table: "Learning_Registration_Types",
                newName: "TypeId");

            migrationBuilder.RenameColumn(
                name: "CourseTypeName",
                table: "CourseType",
                newName: "TypeName");

            migrationBuilder.RenameColumn(
                name: "CourseTypeId",
                table: "CourseType",
                newName: "TypeId");

            migrationBuilder.RenameColumn(
                name: "CourseTypeId",
                table: "CoursePackages",
                newName: "TypeId");

            migrationBuilder.RenameIndex(
                name: "IX_CoursePackages_CourseTypeId",
                table: "CoursePackages",
                newName: "IX_CoursePackages_TypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_CoursePackages_CourseType_TypeId",
                table: "CoursePackages",
                column: "TypeId",
                principalTable: "CourseType",
                principalColumn: "TypeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Learning_Registrations_Learning_Registration_Types_TypeId",
                table: "Learning_Registrations",
                column: "TypeId",
                principalTable: "Learning_Registration_Types",
                principalColumn: "TypeId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

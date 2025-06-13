using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ErpPortal.Infrastructure.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class ExplicitUserWorkCenterJoinTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserWorkCenters_WorkCenters_WorkCenterId",
                table: "UserWorkCenters");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "WorkCenters",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AddForeignKey(
                name: "FK_UserWorkCenters_WorkCenters_WorkCenterId",
                table: "UserWorkCenters",
                column: "WorkCenterId",
                principalTable: "WorkCenters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserWorkCenters_WorkCenters_WorkCenterId",
                table: "UserWorkCenters");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "WorkCenters",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UserWorkCenters_WorkCenters_WorkCenterId",
                table: "UserWorkCenters",
                column: "WorkCenterId",
                principalTable: "WorkCenters",
                principalColumn: "Id");
        }
    }
}

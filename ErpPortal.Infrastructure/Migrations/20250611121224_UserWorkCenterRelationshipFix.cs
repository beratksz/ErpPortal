using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ErpPortal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UserWorkCenterRelationshipFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ShopOrders",
                columns: table => new
                {
                    OrderNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PartNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PartDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PlannedStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PlannedFinishDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopOrders", x => x.OrderNo);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsAdmin = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkCenters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkCenters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OperationNo = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Duration = table.Column<TimeSpan>(type: "time", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShopOrderOperations",
                columns: table => new
                {
                    OrderNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OperationNo = table.Column<int>(type: "int", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false),
                    ReleaseNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SequenceNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OpSequenceNo = table.Column<int>(type: "int", nullable: false),
                    EfficiencyFactor = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MachRunFactor = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MachSetupTime = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MoveTime = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    QueueTime = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LaborRunFactor = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LaborSetupTime = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OperationDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WorkCenterNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PartNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PartDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OperStatusCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RevisedQtyDue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    QtyComplete = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    QtyScrapped = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OpStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OpFinishDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SchedDirection = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RunTimeCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Contract = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ParallelOperation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OperationSchedStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OutsideOpComplete = table.Column<bool>(type: "bit", nullable: false),
                    OutsideOpBackflush = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WorkCenterCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AssignedTo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ReportedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    QuantityCompleted = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    QuantityScrapped = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsSyncPending = table.Column<bool>(type: "bit", nullable: false),
                    LastSyncTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastSyncError = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ETag = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastInterruptionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    InterruptionReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TotalInterruptionDuration = table.Column<TimeSpan>(type: "time", nullable: false),
                    ActualStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualFinishDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    WorkCenterId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopOrderOperations", x => new { x.OrderNo, x.OperationNo });
                    table.ForeignKey(
                        name: "FK_ShopOrderOperations_ShopOrders_OrderNo",
                        column: x => x.OrderNo,
                        principalTable: "ShopOrders",
                        principalColumn: "OrderNo",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ShopOrderOperations_WorkCenters_WorkCenterId",
                        column: x => x.WorkCenterId,
                        principalTable: "WorkCenters",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserWorkCenters",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    WorkCenterId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserWorkCenters", x => new { x.UserId, x.WorkCenterId });
                    table.ForeignKey(
                        name: "FK_UserWorkCenters_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserWorkCenters_WorkCenters_WorkCenterId",
                        column: x => x.WorkCenterId,
                        principalTable: "WorkCenters",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShopOrderOperations_WorkCenterId",
                table: "ShopOrderOperations",
                column: "WorkCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserWorkCenters_WorkCenterId",
                table: "UserWorkCenters",
                column: "WorkCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkCenters_Code",
                table: "WorkCenters",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkLogs_OrderNo_OperationNo_UserId_StartTime",
                table: "WorkLogs",
                columns: new[] { "OrderNo", "OperationNo", "UserId", "StartTime" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkLogs_UserId",
                table: "WorkLogs",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShopOrderOperations");

            migrationBuilder.DropTable(
                name: "UserWorkCenters");

            migrationBuilder.DropTable(
                name: "WorkLogs");

            migrationBuilder.DropTable(
                name: "ShopOrders");

            migrationBuilder.DropTable(
                name: "WorkCenters");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PralPer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddManagerDashboard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeptPerformances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    Department = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Completed = table.Column<int>(type: "int", nullable: false),
                    Pending = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeptPerformances", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ManagerApprovals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    EmployeeName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ApprovalType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AgoText = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManagerApprovals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ManagerDashboardStats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeamEvaluationProgress = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    PendingApprovals = table.Column<int>(type: "int", nullable: false),
                    AverageTeamScore = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    EmployeesAwaitingReview = table.Column<int>(type: "int", nullable: false),
                    TotalEvaluationsAssigned = table.Column<int>(type: "int", nullable: false),
                    PendingEvaluations = table.Column<int>(type: "int", nullable: false),
                    SubmittedEvaluations = table.Column<int>(type: "int", nullable: false),
                    DaysUntilDeadline = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManagerDashboardStats", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ManagerRaters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsActiveRator = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManagerRaters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PeerEvaluationSnapshots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RaterId = table.Column<int>(type: "int", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    RateeName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RateeDepartment = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RateeJobTitle = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    PeriodName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AverageRating = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    CompetenciesRated = table.Column<int>(type: "int", nullable: false),
                    AssessmentScorePercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeerEvaluationSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PeerEvaluationSnapshots_ManagerRaters_RaterId",
                        column: x => x.RaterId,
                        principalTable: "ManagerRaters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PeerEvaluationLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SnapshotId = table.Column<int>(type: "int", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    Competency = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    AttributeName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Score = table.Column<int>(type: "int", nullable: false),
                    MaxScore = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeerEvaluationLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PeerEvaluationLines_PeerEvaluationSnapshots_SnapshotId",
                        column: x => x.SnapshotId,
                        principalTable: "PeerEvaluationSnapshots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PeerEvaluationLines_SnapshotId",
                table: "PeerEvaluationLines",
                column: "SnapshotId");

            migrationBuilder.CreateIndex(
                name: "IX_PeerEvaluationSnapshots_RaterId",
                table: "PeerEvaluationSnapshots",
                column: "RaterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeptPerformances");

            migrationBuilder.DropTable(
                name: "ManagerApprovals");

            migrationBuilder.DropTable(
                name: "ManagerDashboardStats");

            migrationBuilder.DropTable(
                name: "PeerEvaluationLines");

            migrationBuilder.DropTable(
                name: "PeerEvaluationSnapshots");

            migrationBuilder.DropTable(
                name: "ManagerRaters");
        }
    }
}

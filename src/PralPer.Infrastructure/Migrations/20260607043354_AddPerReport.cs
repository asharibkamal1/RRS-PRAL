using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PralPer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPerReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PerReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EvaluationPeriodId = table.Column<int>(type: "int", nullable: false),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    GoalScorePercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    CompetencyScorePercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    GoalWeightPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    CompetencyWeightPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    FinalPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    Band = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Approved = table.Column<bool>(type: "bit", nullable: false),
                    ManagerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Strengths = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DevelopmentAreas = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    OverallComments = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ApprovedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ApprovedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GoalsSubmittedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Evaluation360On = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ManagerReviewOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FinalApprovalOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PerReports_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PerReports_EvaluationPeriods_EvaluationPeriodId",
                        column: x => x.EvaluationPeriodId,
                        principalTable: "EvaluationPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PerReportCompetencyLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PerReportId = table.Column<int>(type: "int", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    Competency = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false),
                    MaxScore = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerReportCompetencyLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PerReportCompetencyLines_PerReports_PerReportId",
                        column: x => x.PerReportId,
                        principalTable: "PerReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PerReportGoalLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PerReportId = table.Column<int>(type: "int", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    WeightPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    ProgressPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    MaxRating = table.Column<int>(type: "int", nullable: false),
                    ContributionPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerReportGoalLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PerReportGoalLines_PerReports_PerReportId",
                        column: x => x.PerReportId,
                        principalTable: "PerReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PerReportCompetencyLines_PerReportId",
                table: "PerReportCompetencyLines",
                column: "PerReportId");

            migrationBuilder.CreateIndex(
                name: "IX_PerReportGoalLines_PerReportId",
                table: "PerReportGoalLines",
                column: "PerReportId");

            migrationBuilder.CreateIndex(
                name: "IX_PerReports_EmployeeId",
                table: "PerReports",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_PerReports_EvaluationPeriodId_EmployeeId",
                table: "PerReports",
                columns: new[] { "EvaluationPeriodId", "EmployeeId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PerReportCompetencyLines");

            migrationBuilder.DropTable(
                name: "PerReportGoalLines");

            migrationBuilder.DropTable(
                name: "PerReports");
        }
    }
}

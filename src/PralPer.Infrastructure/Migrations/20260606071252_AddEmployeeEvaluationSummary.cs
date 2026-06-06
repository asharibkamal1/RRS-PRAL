using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PralPer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeEvaluationSummary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmployeeEvaluationSummaries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EvaluationPeriodId = table.Column<int>(type: "int", nullable: false),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    GoalCompletionPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    EvaluationStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FinalPerScore = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    PendingActions = table.Column<int>(type: "int", nullable: false),
                    PendingEvaluations = table.Column<int>(type: "int", nullable: false),
                    SubmittedEvaluations = table.Column<int>(type: "int", nullable: false),
                    DaysUntilDeadline = table.Column<int>(type: "int", nullable: false),
                    EvaluationDeadline = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ManagerStrengths = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ManagerDevelopmentAreas = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ManagerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    FeedbackUpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeEvaluationSummaries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeEvaluationSummaries_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeeEvaluationSummaries_EvaluationPeriods_EvaluationPeriodId",
                        column: x => x.EvaluationPeriodId,
                        principalTable: "EvaluationPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeEvaluationSummaries_EmployeeId",
                table: "EmployeeEvaluationSummaries",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeEvaluationSummaries_EvaluationPeriodId_EmployeeId",
                table: "EmployeeEvaluationSummaries",
                columns: new[] { "EvaluationPeriodId", "EmployeeId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeEvaluationSummaries");
        }
    }
}

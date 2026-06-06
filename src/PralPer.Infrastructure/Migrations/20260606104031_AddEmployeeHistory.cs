using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PralPer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "JobTitle",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BonusHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    BonusType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Quarter = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BonusHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BonusHistories_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IncrementHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Percentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncrementHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncrementHistories_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PromotionHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    EffectiveDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FromTitle = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    ToTitle = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Note = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromotionHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PromotionHistories_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BonusHistories_EmployeeId",
                table: "BonusHistories",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_IncrementHistories_EmployeeId",
                table: "IncrementHistories",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_PromotionHistories_EmployeeId",
                table: "PromotionHistories",
                column: "EmployeeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BonusHistories");

            migrationBuilder.DropTable(
                name: "IncrementHistories");

            migrationBuilder.DropTable(
                name: "PromotionHistories");

            migrationBuilder.DropColumn(
                name: "JobTitle",
                table: "Employees");
        }
    }
}

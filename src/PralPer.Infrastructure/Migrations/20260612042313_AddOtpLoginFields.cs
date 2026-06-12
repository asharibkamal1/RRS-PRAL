using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PralPer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOtpLoginFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "WorkEmail",
                table: "Employees",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OtpCodeHash",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "OtpExpiresAtUtc",
                table: "AspNetUsers",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OtpFailedAttempts",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "OtpVerifiedAtUtc",
                table: "AspNetUsers",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_WorkEmail",
                table: "Employees",
                column: "WorkEmail",
                unique: true,
                filter: "[WorkEmail] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Employees_WorkEmail",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "OtpCodeHash",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "OtpExpiresAtUtc",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "OtpFailedAttempts",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "OtpVerifiedAtUtc",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<string>(
                name: "WorkEmail",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }
    }
}

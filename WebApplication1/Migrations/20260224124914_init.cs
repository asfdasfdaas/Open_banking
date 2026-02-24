using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AccountLists",
                table: "AccountLists");

            migrationBuilder.RenameColumn(
                name: "AccountName",
                table: "AccountLists",
                newName: "CurrencyCode");

            migrationBuilder.AlterColumn<string>(
                name: "AccountNumber",
                table: "AccountLists",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "AccountLists",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<string>(
                name: "AccountStatus",
                table: "AccountLists",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "AccountType",
                table: "AccountLists",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastTransactionDate",
                table: "AccountLists",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "RemainingBalance",
                table: "AccountLists",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddPrimaryKey(
                name: "PK_AccountLists",
                table: "AccountLists",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AccountLists",
                table: "AccountLists");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "AccountLists");

            migrationBuilder.DropColumn(
                name: "AccountStatus",
                table: "AccountLists");

            migrationBuilder.DropColumn(
                name: "AccountType",
                table: "AccountLists");

            migrationBuilder.DropColumn(
                name: "LastTransactionDate",
                table: "AccountLists");

            migrationBuilder.DropColumn(
                name: "RemainingBalance",
                table: "AccountLists");

            migrationBuilder.RenameColumn(
                name: "CurrencyCode",
                table: "AccountLists",
                newName: "AccountName");

            migrationBuilder.AlterColumn<string>(
                name: "AccountNumber",
                table: "AccountLists",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AccountLists",
                table: "AccountLists",
                column: "AccountNumber");
        }
    }
}

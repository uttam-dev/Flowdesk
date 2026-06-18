using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlowDesk.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class changeUpdateOn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedOn",
                table: "Users",
                type: "datetime(6)",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValueSql: "UTC_TIMESTAMP()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedOn",
                table: "Requests",
                type: "datetime(6)",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValueSql: "UTC_TIMESTAMP()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ChangedOn",
                table: "RequestHistories",
                type: "datetime(6)",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValueSql: "UTC_TIMESTAMP()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedOn",
                table: "EscalationHistories",
                type: "datetime(6)",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValueSql: "UTC_TIMESTAMP()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedOn",
                table: "Comments",
                type: "datetime(6)",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValueSql: "UTC_TIMESTAMP()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedOn",
                table: "Categories",
                type: "datetime(6)",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValueSql: "UTC_TIMESTAMP()");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedOn",
                table: "Users",
                type: "datetime(6)",
                nullable: false,
                defaultValueSql: "UTC_TIMESTAMP()",
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedOn",
                table: "Requests",
                type: "datetime(6)",
                nullable: false,
                defaultValueSql: "UTC_TIMESTAMP()",
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ChangedOn",
                table: "RequestHistories",
                type: "datetime(6)",
                nullable: false,
                defaultValueSql: "UTC_TIMESTAMP()",
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedOn",
                table: "EscalationHistories",
                type: "datetime(6)",
                nullable: false,
                defaultValueSql: "UTC_TIMESTAMP()",
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedOn",
                table: "Comments",
                type: "datetime(6)",
                nullable: false,
                defaultValueSql: "UTC_TIMESTAMP()",
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedOn",
                table: "Categories",
                type: "datetime(6)",
                nullable: false,
                defaultValueSql: "UTC_TIMESTAMP()",
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");
        }
    }
}

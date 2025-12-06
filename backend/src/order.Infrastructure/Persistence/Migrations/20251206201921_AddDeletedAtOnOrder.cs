using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace order.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDeletedAtOnOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "orders",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "orders");
        }
    }
}

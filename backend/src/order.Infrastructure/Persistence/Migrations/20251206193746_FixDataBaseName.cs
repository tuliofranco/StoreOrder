using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace order.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixDataBaseName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Orders_OrderId",
                table: "OrderItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Orders",
                table: "Orders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrderItems",
                table: "OrderItems");

            migrationBuilder.RenameTable(
                name: "Orders",
                newName: "orders");

            migrationBuilder.RenameTable(
                name: "OrderItems",
                newName: "orderitems");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_OrderNumber",
                table: "orders",
                newName: "IX_orders_OrderNumber");

            migrationBuilder.RenameIndex(
                name: "IX_OrderItems_OrderId",
                table: "orderitems",
                newName: "IX_orderitems_OrderId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_orders",
                table: "orders",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_orderitems",
                table: "orderitems",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_orderitems_orders_OrderId",
                table: "orderitems",
                column: "OrderId",
                principalTable: "orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_orderitems_orders_OrderId",
                table: "orderitems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_orders",
                table: "orders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_orderitems",
                table: "orderitems");

            migrationBuilder.RenameTable(
                name: "orders",
                newName: "Orders");

            migrationBuilder.RenameTable(
                name: "orderitems",
                newName: "OrderItems");

            migrationBuilder.RenameIndex(
                name: "IX_orders_OrderNumber",
                table: "Orders",
                newName: "IX_Orders_OrderNumber");

            migrationBuilder.RenameIndex(
                name: "IX_orderitems_OrderId",
                table: "OrderItems",
                newName: "IX_OrderItems_OrderId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Orders",
                table: "Orders",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrderItems",
                table: "OrderItems",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Orders_OrderId",
                table: "OrderItems",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace order.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ProductId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProductId",
                table: "order_items",
                type: "character varying(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_order_items_ProductId",
                table: "order_items",
                column: "ProductId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_order_items_ProductId",
                table: "order_items");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "order_items");
        }
    }
}

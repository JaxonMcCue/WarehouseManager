using Microsoft.EntityFrameworkCore.Migrations;

namespace WarehouseManager.Data.Migrations
{
    public partial class orderAndOrderItem : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    ItemID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemName = table.Column<string>(nullable: true),
                    ItemDescription = table.Column<string>(nullable: true),
                    Price = table.Column<double>(nullable: false),
                    ItemAmount = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.ItemID);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    OrderID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerID = table.Column<int>(nullable: false),
                    OrderCost = table.Column<decimal>(nullable: false),
                    ItemCount = table.Column<int>(nullable: false),
                    Completed = table.Column<bool>(nullable: false),
                    Cancelled = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.OrderID);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemID = table.Column<int>(nullable: false),
                    OrderID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.ID);
                    table.ForeignKey(
                        name: "FK_OrderItems_Items_ItemID",
                        column: x => x.ItemID,
                        principalTable: "Items",
                        principalColumn: "ItemID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_ItemID",
                table: "OrderItems",
                column: "ItemID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Items");
        }
    }
}

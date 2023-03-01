using Microsoft.EntityFrameworkCore.Migrations;

namespace WarehouseManager.Data.Migrations
{
    public partial class addTestData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ItemName",
                table: "Items",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.InsertData(
                table: "Items",
                columns: new[] { "ItemID", "ItemAmount", "ItemDescription", "ItemName", "Price" },
                values: new object[] { 1, 1, "This is a test item", "Test Item", 1.5 });

            migrationBuilder.InsertData(
                table: "Orders",
                columns: new[] { "OrderID", "Cancelled", "Completed", "CustomerID", "ItemCount", "OrderCost" },
                values: new object[] { 1, false, false, 1, 0, 0m });

            migrationBuilder.InsertData(
                table: "OrderItems",
                columns: new[] { "ID", "ItemID", "OrderID" },
                values: new object[] { 1, 1, 1 });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "ID",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "OrderID",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Items",
                keyColumn: "ItemID",
                keyValue: 1);

            migrationBuilder.AlterColumn<string>(
                name: "ItemName",
                table: "Items",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string));
        }
    }
}

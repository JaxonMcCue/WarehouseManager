using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WarehouseManager.Migrations
{
    public partial class Damaged : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Damaged",
                columns: table => new
                {
                    DamagedID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemID = table.Column<int>(nullable: false),
                    Count = table.Column<int>(nullable: false),
                    Desc = table.Column<string>(nullable: true),
                    Date = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Damaged", x => x.DamagedID);
                    table.ForeignKey(
                        name: "FK_Damaged_Items_ItemID",
                        column: x => x.ItemID,
                        principalTable: "Items",
                        principalColumn: "ItemID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Damaged_ItemID",
                table: "Damaged",
                column: "ItemID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Damaged");
        }
    }
}

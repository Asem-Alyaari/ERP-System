using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ItemBatches_ItemId_BatchNumber",
                table: "ItemBatches");

            migrationBuilder.AddColumn<Guid>(
                name: "ExpenseAccountId",
                table: "StockGroups",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AverageCost",
                table: "Items",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "StandardCost",
                table: "Items",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "WarehouseId",
                table: "ItemBatches",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPosted",
                table: "InventoryTransactionMasters",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "ToWarehouseId",
                table: "InventoryTransactionMasters",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "WarehouseId",
                table: "InventoryTransactionMasters",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Warehouses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Warehouses", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StockGroups_ExpenseAccountId",
                table: "StockGroups",
                column: "ExpenseAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemBatches_ItemId_WarehouseId_BatchNumber",
                table: "ItemBatches",
                columns: new[] { "ItemId", "WarehouseId", "BatchNumber" },
                unique: true,
                filter: "WarehouseId IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ItemBatches_WarehouseId",
                table: "ItemBatches",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactionMasters_ToWarehouseId",
                table: "InventoryTransactionMasters",
                column: "ToWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactionMasters_WarehouseId",
                table: "InventoryTransactionMasters",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_Warehouses_Code",
                table: "Warehouses",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryTransactionMasters_Warehouses_ToWarehouseId",
                table: "InventoryTransactionMasters",
                column: "ToWarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryTransactionMasters_Warehouses_WarehouseId",
                table: "InventoryTransactionMasters",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemBatches_Warehouses_WarehouseId",
                table: "ItemBatches",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StockGroups_Accounts_ExpenseAccountId",
                table: "StockGroups",
                column: "ExpenseAccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryTransactionMasters_Warehouses_ToWarehouseId",
                table: "InventoryTransactionMasters");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryTransactionMasters_Warehouses_WarehouseId",
                table: "InventoryTransactionMasters");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemBatches_Warehouses_WarehouseId",
                table: "ItemBatches");

            migrationBuilder.DropForeignKey(
                name: "FK_StockGroups_Accounts_ExpenseAccountId",
                table: "StockGroups");

            migrationBuilder.DropTable(
                name: "Warehouses");

            migrationBuilder.DropIndex(
                name: "IX_StockGroups_ExpenseAccountId",
                table: "StockGroups");

            migrationBuilder.DropIndex(
                name: "IX_ItemBatches_ItemId_WarehouseId_BatchNumber",
                table: "ItemBatches");

            migrationBuilder.DropIndex(
                name: "IX_ItemBatches_WarehouseId",
                table: "ItemBatches");

            migrationBuilder.DropIndex(
                name: "IX_InventoryTransactionMasters_ToWarehouseId",
                table: "InventoryTransactionMasters");

            migrationBuilder.DropIndex(
                name: "IX_InventoryTransactionMasters_WarehouseId",
                table: "InventoryTransactionMasters");

            migrationBuilder.DropColumn(
                name: "ExpenseAccountId",
                table: "StockGroups");

            migrationBuilder.DropColumn(
                name: "AverageCost",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "StandardCost",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "WarehouseId",
                table: "ItemBatches");

            migrationBuilder.DropColumn(
                name: "IsPosted",
                table: "InventoryTransactionMasters");

            migrationBuilder.DropColumn(
                name: "ToWarehouseId",
                table: "InventoryTransactionMasters");

            migrationBuilder.DropColumn(
                name: "WarehouseId",
                table: "InventoryTransactionMasters");

            migrationBuilder.CreateIndex(
                name: "IX_ItemBatches_ItemId_BatchNumber",
                table: "ItemBatches",
                columns: new[] { "ItemId", "BatchNumber" },
                unique: true);
        }
    }
}

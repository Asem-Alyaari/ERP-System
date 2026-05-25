using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVendorIdAndCostCenterIdToReceiptVoucher : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CostCenterId",
                table: "ReceiptVouchers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "VendorId",
                table: "ReceiptVouchers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptVouchers_CostCenterId",
                table: "ReceiptVouchers",
                column: "CostCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptVouchers_VendorId",
                table: "ReceiptVouchers",
                column: "VendorId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReceiptVouchers_CostCenters_CostCenterId",
                table: "ReceiptVouchers",
                column: "CostCenterId",
                principalTable: "CostCenters",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ReceiptVouchers_Vendors_VendorId",
                table: "ReceiptVouchers",
                column: "VendorId",
                principalTable: "Vendors",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReceiptVouchers_CostCenters_CostCenterId",
                table: "ReceiptVouchers");

            migrationBuilder.DropForeignKey(
                name: "FK_ReceiptVouchers_Vendors_VendorId",
                table: "ReceiptVouchers");

            migrationBuilder.DropIndex(
                name: "IX_ReceiptVouchers_CostCenterId",
                table: "ReceiptVouchers");

            migrationBuilder.DropIndex(
                name: "IX_ReceiptVouchers_VendorId",
                table: "ReceiptVouchers");

            migrationBuilder.DropColumn(
                name: "CostCenterId",
                table: "ReceiptVouchers");

            migrationBuilder.DropColumn(
                name: "VendorId",
                table: "ReceiptVouchers");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingRelationshipsToReceiptVoucher : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReceiptVouchers_CostCenters_CostCenterId",
                table: "ReceiptVouchers");

            migrationBuilder.DropForeignKey(
                name: "FK_ReceiptVouchers_Vendors_VendorId",
                table: "ReceiptVouchers");

            migrationBuilder.AddForeignKey(
                name: "FK_ReceiptVouchers_CostCenters_CostCenterId",
                table: "ReceiptVouchers",
                column: "CostCenterId",
                principalTable: "CostCenters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ReceiptVouchers_Vendors_VendorId",
                table: "ReceiptVouchers",
                column: "VendorId",
                principalTable: "Vendors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
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
    }
}

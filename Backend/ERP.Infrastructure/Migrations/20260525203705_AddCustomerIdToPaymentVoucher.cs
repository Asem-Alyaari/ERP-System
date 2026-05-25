using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerIdToPaymentVoucher : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CostCenterId",
                table: "PaymentVouchers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "PaymentVouchers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentVouchers_CostCenterId",
                table: "PaymentVouchers",
                column: "CostCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentVouchers_CustomerId",
                table: "PaymentVouchers",
                column: "CustomerId");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentVouchers_CostCenters_CostCenterId",
                table: "PaymentVouchers",
                column: "CostCenterId",
                principalTable: "CostCenters",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentVouchers_Customers_CustomerId",
                table: "PaymentVouchers",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentVouchers_CostCenters_CostCenterId",
                table: "PaymentVouchers");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentVouchers_Customers_CustomerId",
                table: "PaymentVouchers");

            migrationBuilder.DropIndex(
                name: "IX_PaymentVouchers_CostCenterId",
                table: "PaymentVouchers");

            migrationBuilder.DropIndex(
                name: "IX_PaymentVouchers_CustomerId",
                table: "PaymentVouchers");

            migrationBuilder.DropColumn(
                name: "CostCenterId",
                table: "PaymentVouchers");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "PaymentVouchers");
        }
    }
}

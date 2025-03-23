using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShareCare.Data.Migrations
{
    /// <inheritdoc />
    public partial class Purchase_Debt_Cascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Debt_Purchase_PurchaseId",
                table: "Debt");

            migrationBuilder.AddForeignKey(
                name: "FK_Debt_Purchase_PurchaseId",
                table: "Debt",
                column: "PurchaseId",
                principalTable: "Purchase",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Debt_Purchase_PurchaseId",
                table: "Debt");

            migrationBuilder.AddForeignKey(
                name: "FK_Debt_Purchase_PurchaseId",
                table: "Debt",
                column: "PurchaseId",
                principalTable: "Purchase",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

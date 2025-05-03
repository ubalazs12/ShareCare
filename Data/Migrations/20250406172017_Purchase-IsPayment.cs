using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShareCare.Data.Migrations
{
    /// <inheritdoc />
    public partial class PurchaseIsPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPayment",
                table: "Purchase",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPayment",
                table: "Purchase");
        }
    }
}

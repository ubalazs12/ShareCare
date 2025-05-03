using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShareCare.Data.Migrations
{
    /// <inheritdoc />
    public partial class Connected_Debts_To_Group : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GroupId",
                table: "Debt",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Debt_GroupId",
                table: "Debt",
                column: "GroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Debt_Groups_GroupId",
                table: "Debt",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Debt_Groups_GroupId",
                table: "Debt");

            migrationBuilder.DropIndex(
                name: "IX_Debt_GroupId",
                table: "Debt");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "Debt");
        }
    }
}

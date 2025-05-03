using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShareCare.Data.Migrations
{
    /// <inheritdoc />
    public partial class Added_Debt_To_Model : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Debt",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PurchaseId = table.Column<int>(type: "int", nullable: true),
                    UploaderUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    OwerUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Amount = table.Column<double>(type: "float", nullable: false),
                    ApprovalState = table.Column<int>(type: "int", nullable: false),
                    PaymentState = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Debt", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Debt_AspNetUsers_OwerUserId",
                        column: x => x.OwerUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Debt_AspNetUsers_UploaderUserId",
                        column: x => x.UploaderUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Debt_Purchase_PurchaseId",
                        column: x => x.PurchaseId,
                        principalTable: "Purchase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Debt_OwerUserId",
                table: "Debt",
                column: "OwerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Debt_PurchaseId",
                table: "Debt",
                column: "PurchaseId");

            migrationBuilder.CreateIndex(
                name: "IX_Debt_UploaderUserId",
                table: "Debt",
                column: "UploaderUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Debt");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HeftyHub.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddForeignKeyForCategoryProductRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "tblProduct",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "tblProduct",
                keyColumn: "ProductId",
                keyValue: 1,
                column: "CategoryId",
                value: 2);

            migrationBuilder.UpdateData(
                table: "tblProduct",
                keyColumn: "ProductId",
                keyValue: 2,
                column: "CategoryId",
                value: 3);

            migrationBuilder.UpdateData(
                table: "tblProduct",
                keyColumn: "ProductId",
                keyValue: 3,
                column: "CategoryId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "tblProduct",
                keyColumn: "ProductId",
                keyValue: 4,
                column: "CategoryId",
                value: 5);

            migrationBuilder.UpdateData(
                table: "tblProduct",
                keyColumn: "ProductId",
                keyValue: 5,
                column: "CategoryId",
                value: 6);

            migrationBuilder.UpdateData(
                table: "tblProduct",
                keyColumn: "ProductId",
                keyValue: 6,
                column: "CategoryId",
                value: 4);

            migrationBuilder.CreateIndex(
                name: "IX_tblProduct_CategoryId",
                table: "tblProduct",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_tblProduct_tblCategory_CategoryId",
                table: "tblProduct",
                column: "CategoryId",
                principalTable: "tblCategory",
                principalColumn: "CategoryId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tblProduct_tblCategory_CategoryId",
                table: "tblProduct");

            migrationBuilder.DropIndex(
                name: "IX_tblProduct_CategoryId",
                table: "tblProduct");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "tblProduct");
        }
    }
}

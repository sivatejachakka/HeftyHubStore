using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HeftyHub.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddImageUrlToTblProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "tblProduct",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "tblProduct",
                keyColumn: "ProductId",
                keyValue: 1,
                column: "ImageUrl",
                value: "");

            migrationBuilder.UpdateData(
                table: "tblProduct",
                keyColumn: "ProductId",
                keyValue: 2,
                column: "ImageUrl",
                value: "");

            migrationBuilder.UpdateData(
                table: "tblProduct",
                keyColumn: "ProductId",
                keyValue: 3,
                column: "ImageUrl",
                value: "");

            migrationBuilder.UpdateData(
                table: "tblProduct",
                keyColumn: "ProductId",
                keyValue: 4,
                column: "ImageUrl",
                value: "");

            migrationBuilder.UpdateData(
                table: "tblProduct",
                keyColumn: "ProductId",
                keyValue: 5,
                column: "ImageUrl",
                value: "");

            migrationBuilder.UpdateData(
                table: "tblProduct",
                keyColumn: "ProductId",
                keyValue: 6,
                column: "ImageUrl",
                value: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "tblProduct");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shortener.Migrations
{
    /// <inheritdoc />
    public partial class RenameColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Urls_Users_UserId",
                table: "Urls");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Urls",
                newName: "OwnerId");

            migrationBuilder.RenameIndex(
                name: "IX_Urls_UserId",
                table: "Urls",
                newName: "IX_Urls_OwnerId");

            migrationBuilder.RenameColumn(
                name: "Timestamp",
                table: "Clicks",
                newName: "RedirectAt");

            migrationBuilder.AddForeignKey(
                name: "FK_Urls_Users_OwnerId",
                table: "Urls",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Urls_Users_OwnerId",
                table: "Urls");

            migrationBuilder.RenameColumn(
                name: "OwnerId",
                table: "Urls",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Urls_OwnerId",
                table: "Urls",
                newName: "IX_Urls_UserId");

            migrationBuilder.RenameColumn(
                name: "RedirectAt",
                table: "Clicks",
                newName: "Timestamp");

            migrationBuilder.AddForeignKey(
                name: "FK_Urls_Users_UserId",
                table: "Urls",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}

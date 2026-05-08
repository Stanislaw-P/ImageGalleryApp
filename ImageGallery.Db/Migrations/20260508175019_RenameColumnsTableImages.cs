using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ImageGallery.Db.Migrations
{
    /// <inheritdoc />
    public partial class RenameColumnsTableImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Url",
                table: "Images",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Images",
                newName: "FilePath");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Title",
                table: "Images",
                newName: "Url");

            migrationBuilder.RenameColumn(
                name: "FilePath",
                table: "Images",
                newName: "Name");
        }
    }
}

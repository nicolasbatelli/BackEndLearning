using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialChat.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProfilePictureThumbnail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProfilePictureContentType",
                table: "Users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "ProfilePictureThumbnail",
                table: "Users",
                type: "varbinary(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfilePictureContentType",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ProfilePictureThumbnail",
                table: "Users");
        }
    }
}

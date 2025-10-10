using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChorePlay.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddOauthEmailConfirmedToAppUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "OAuthEmailConfirmed",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OAuthEmailConfirmed",
                table: "AspNetUsers");
        }
    }
}

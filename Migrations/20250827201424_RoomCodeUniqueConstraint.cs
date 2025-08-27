using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chariot.Migrations
{
    /// <inheritdoc />
    public partial class RoomCodeUniqueConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Chatrooms_Code",
                table: "Chatrooms",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chatrooms_Code",
                table: "Chatrooms");
        }
    }
}

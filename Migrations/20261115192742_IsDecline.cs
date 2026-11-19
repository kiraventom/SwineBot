using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SwineBot.Migrations
{
    /// <inheritdoc />
    public partial class IsDecline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDecline",
                table: "DuelResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDecline",
                table: "DuelResults");
        }
    }
}

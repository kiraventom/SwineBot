using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SwineBot.Migrations
{
    /// <inheritdoc />
    public partial class RenameStatstoInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StatsId",
                table: "Swines");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StatsId",
                table: "Swines",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }
    }
}

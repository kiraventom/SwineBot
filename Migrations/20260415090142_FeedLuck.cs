using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SwineBot.Migrations
{
    /// <inheritdoc />
    public partial class FeedLuck : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Luck",
                table: "WeightLosses",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Luck",
                table: "Feeds",
                type: "REAL",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Luck",
                table: "WeightLosses");

            migrationBuilder.DropColumn(
                name: "Luck",
                table: "Feeds");
        }
    }
}

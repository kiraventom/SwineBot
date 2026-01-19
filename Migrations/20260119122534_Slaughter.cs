using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SwineBot.Migrations
{
    /// <inheritdoc />
    public partial class Slaughter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TotalWeightSlaughtered",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalWeightSlaughtered",
                table: "Users");
        }
    }
}

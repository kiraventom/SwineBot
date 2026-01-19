using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SwineBot.Migrations
{
    /// <inheritdoc />
    public partial class SlaughtersSet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalWeightSlaughtered",
                table: "Users");

            migrationBuilder.CreateTable(
                name: "Slaughters",
                columns: table => new
                {
                    SlaughterId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    SwineName = table.Column<string>(type: "TEXT", nullable: true),
                    SwineWeight = table.Column<int>(type: "INTEGER", nullable: false),
                    DateTime = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Slaughters", x => x.SlaughterId);
                    table.ForeignKey(
                        name: "FK_Slaughters_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Slaughters_UserId",
                table: "Slaughters",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Slaughters");

            migrationBuilder.AddColumn<int>(
                name: "TotalWeightSlaughtered",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }
    }
}

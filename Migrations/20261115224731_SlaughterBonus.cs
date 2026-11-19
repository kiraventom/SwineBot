using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SwineBot.Migrations
{
    /// <inheritdoc />
    public partial class SlaughterBonus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SlaughterBonuses",
                columns: table => new
                {
                    SlaughterBonusId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    GroupId = table.Column<int>(type: "INTEGER", nullable: false),
                    Weight = table.Column<int>(type: "INTEGER", nullable: false),
                    DateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Reason = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SlaughterBonuses", x => x.SlaughterBonusId);
                    table.ForeignKey(
                        name: "FK_SlaughterBonuses_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "GroupId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SlaughterBonuses_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SlaughterBonuses_GroupId",
                table: "SlaughterBonuses",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_SlaughterBonuses_UserId",
                table: "SlaughterBonuses",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SlaughterBonuses");
        }
    }
}

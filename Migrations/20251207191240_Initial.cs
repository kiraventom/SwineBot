using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SwineBot.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TelegramId = table.Column<long>(type: "INTEGER", nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", nullable: false),
                    Tag = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "Swines",
                columns: table => new
                {
                    SwineId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OwnerId = table.Column<int>(type: "INTEGER", nullable: false),
                    StatsId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Weight = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Swines", x => x.SwineId);
                    table.ForeignKey(
                        name: "FK_Swines_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DuelRequests",
                columns: table => new
                {
                    RequestId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AttackerId = table.Column<int>(type: "INTEGER", nullable: false),
                    DefenderId = table.Column<int>(type: "INTEGER", nullable: false),
                    DateTime = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DuelRequests", x => x.RequestId);
                    table.ForeignKey(
                        name: "FK_DuelRequests_Swines_AttackerId",
                        column: x => x.AttackerId,
                        principalTable: "Swines",
                        principalColumn: "SwineId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DuelRequests_Swines_DefenderId",
                        column: x => x.DefenderId,
                        principalTable: "Swines",
                        principalColumn: "SwineId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DuelResults",
                columns: table => new
                {
                    DuelResultId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AttackerId = table.Column<int>(type: "INTEGER", nullable: false),
                    DefenderId = table.Column<int>(type: "INTEGER", nullable: false),
                    AttackerWon = table.Column<bool>(type: "INTEGER", nullable: false),
                    WinnerSwineId = table.Column<int>(type: "INTEGER", nullable: true),
                    DateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    WinnerWeightBefore = table.Column<int>(type: "INTEGER", nullable: false),
                    WinnerWeightAfter = table.Column<int>(type: "INTEGER", nullable: false),
                    LoserWeightBefore = table.Column<int>(type: "INTEGER", nullable: false),
                    LoserWeightAfter = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DuelResults", x => x.DuelResultId);
                    table.ForeignKey(
                        name: "FK_DuelResults_Swines_AttackerId",
                        column: x => x.AttackerId,
                        principalTable: "Swines",
                        principalColumn: "SwineId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DuelResults_Swines_DefenderId",
                        column: x => x.DefenderId,
                        principalTable: "Swines",
                        principalColumn: "SwineId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DuelResults_Swines_WinnerSwineId",
                        column: x => x.WinnerSwineId,
                        principalTable: "Swines",
                        principalColumn: "SwineId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Feeds",
                columns: table => new
                {
                    FeedId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SwineId = table.Column<int>(type: "INTEGER", nullable: false),
                    DateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Amount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Feeds", x => x.FeedId);
                    table.ForeignKey(
                        name: "FK_Feeds_Swines_SwineId",
                        column: x => x.SwineId,
                        principalTable: "Swines",
                        principalColumn: "SwineId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Infos",
                columns: table => new
                {
                    InfoId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SwineId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Infos", x => x.InfoId);
                    table.ForeignKey(
                        name: "FK_Infos_Swines_SwineId",
                        column: x => x.SwineId,
                        principalTable: "Swines",
                        principalColumn: "SwineId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WeightLosses",
                columns: table => new
                {
                    LossId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SwineId = table.Column<int>(type: "INTEGER", nullable: false),
                    DateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Amount = table.Column<int>(type: "INTEGER", nullable: false),
                    IsThrowUp = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeightLosses", x => x.LossId);
                    table.ForeignKey(
                        name: "FK_WeightLosses_Swines_SwineId",
                        column: x => x.SwineId,
                        principalTable: "Swines",
                        principalColumn: "SwineId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DuelRequests_AttackerId",
                table: "DuelRequests",
                column: "AttackerId");

            migrationBuilder.CreateIndex(
                name: "IX_DuelRequests_DefenderId",
                table: "DuelRequests",
                column: "DefenderId");

            migrationBuilder.CreateIndex(
                name: "IX_DuelResults_AttackerId",
                table: "DuelResults",
                column: "AttackerId");

            migrationBuilder.CreateIndex(
                name: "IX_DuelResults_DefenderId",
                table: "DuelResults",
                column: "DefenderId");

            migrationBuilder.CreateIndex(
                name: "IX_DuelResults_WinnerSwineId",
                table: "DuelResults",
                column: "WinnerSwineId");

            migrationBuilder.CreateIndex(
                name: "IX_Feeds_SwineId",
                table: "Feeds",
                column: "SwineId");

            migrationBuilder.CreateIndex(
                name: "IX_Infos_SwineId",
                table: "Infos",
                column: "SwineId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Swines_OwnerId",
                table: "Swines",
                column: "OwnerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_TelegramId",
                table: "Users",
                column: "TelegramId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WeightLosses_SwineId",
                table: "WeightLosses",
                column: "SwineId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DuelRequests");

            migrationBuilder.DropTable(
                name: "DuelResults");

            migrationBuilder.DropTable(
                name: "Feeds");

            migrationBuilder.DropTable(
                name: "Infos");

            migrationBuilder.DropTable(
                name: "WeightLosses");

            migrationBuilder.DropTable(
                name: "Swines");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}

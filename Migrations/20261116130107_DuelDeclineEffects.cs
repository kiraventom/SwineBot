using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SwineBot.Migrations
{
    /// <inheritdoc />
    public partial class DuelDeclineEffects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Declines",
                table: "Swines",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FeedStealSwineId",
                table: "Swines",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FeedStealUntil",
                table: "Swines",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Glory",
                table: "Swines",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Declines",
                table: "Swines");

            migrationBuilder.DropColumn(
                name: "FeedStealSwineId",
                table: "Swines");

            migrationBuilder.DropColumn(
                name: "FeedStealUntil",
                table: "Swines");

            migrationBuilder.DropColumn(
                name: "Glory",
                table: "Swines");
        }
    }
}

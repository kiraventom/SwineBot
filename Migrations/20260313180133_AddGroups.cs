using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SwineBot.Migrations
{
    /// <inheritdoc />
    public partial class AddGroups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Swines_OwnerId",
                table: "Swines");

            migrationBuilder.AddColumn<int>(
                name: "GroupId",
                table: "Swines",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GroupId",
                table: "Slaughters",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Groups",
                columns: table => new
                {
                    GroupId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TelegramId = table.Column<long>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groups", x => x.GroupId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Swines_GroupId",
                table: "Swines",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Swines_OwnerId",
                table: "Swines",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Slaughters_GroupId",
                table: "Slaughters",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Groups_TelegramId",
                table: "Groups",
                column: "TelegramId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Slaughters_Groups_GroupId",
                table: "Slaughters",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "GroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Swines_Groups_GroupId",
                table: "Swines",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "GroupId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Slaughters_Groups_GroupId",
                table: "Slaughters");

            migrationBuilder.DropForeignKey(
                name: "FK_Swines_Groups_GroupId",
                table: "Swines");

            migrationBuilder.DropTable(
                name: "Groups");

            migrationBuilder.DropIndex(
                name: "IX_Swines_GroupId",
                table: "Swines");

            migrationBuilder.DropIndex(
                name: "IX_Swines_OwnerId",
                table: "Swines");

            migrationBuilder.DropIndex(
                name: "IX_Slaughters_GroupId",
                table: "Slaughters");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "Swines");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "Slaughters");

            migrationBuilder.CreateIndex(
                name: "IX_Swines_OwnerId",
                table: "Swines",
                column: "OwnerId",
                unique: true);
        }
    }
}

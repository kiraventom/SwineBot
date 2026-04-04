using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SwineBot.Migrations
{
    /// <inheritdoc />
    public partial class private_fluent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DuelResults_Swines_AttackerId",
                table: "DuelResults");

            migrationBuilder.DropForeignKey(
                name: "FK_DuelResults_Swines_DefenderId",
                table: "DuelResults");

            migrationBuilder.DropForeignKey(
                name: "FK_DuelResults_Swines_WinnerSwineId",
                table: "DuelResults");

            migrationBuilder.DropForeignKey(
                name: "FK_Slaughters_Groups_GroupId",
                table: "Slaughters");

            migrationBuilder.DropIndex(
                name: "IX_DuelResults_WinnerSwineId",
                table: "DuelResults");

            migrationBuilder.DropColumn(
                name: "WinnerSwineId",
                table: "DuelResults");

            migrationBuilder.AddColumn<int>(
                name: "PrivateSwineId",
                table: "Users",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "DefenderId",
                table: "DuelResults",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<int>(
                name: "AttackerId",
                table: "DuelResults",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.CreateIndex(
                name: "IX_Users_PrivateSwineId",
                table: "Users",
                column: "PrivateSwineId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DuelResults_Swines_AttackerId",
                table: "DuelResults",
                column: "AttackerId",
                principalTable: "Swines",
                principalColumn: "SwineId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_DuelResults_Swines_DefenderId",
                table: "DuelResults",
                column: "DefenderId",
                principalTable: "Swines",
                principalColumn: "SwineId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Slaughters_Groups_GroupId",
                table: "Slaughters",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "GroupId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Swines_PrivateSwineId",
                table: "Users",
                column: "PrivateSwineId",
                principalTable: "Swines",
                principalColumn: "SwineId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DuelResults_Swines_AttackerId",
                table: "DuelResults");

            migrationBuilder.DropForeignKey(
                name: "FK_DuelResults_Swines_DefenderId",
                table: "DuelResults");

            migrationBuilder.DropForeignKey(
                name: "FK_Slaughters_Groups_GroupId",
                table: "Slaughters");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Swines_PrivateSwineId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_PrivateSwineId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PrivateSwineId",
                table: "Users");

            migrationBuilder.AlterColumn<int>(
                name: "DefenderId",
                table: "DuelResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "AttackerId",
                table: "DuelResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WinnerSwineId",
                table: "DuelResults",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DuelResults_WinnerSwineId",
                table: "DuelResults",
                column: "WinnerSwineId");

            migrationBuilder.AddForeignKey(
                name: "FK_DuelResults_Swines_AttackerId",
                table: "DuelResults",
                column: "AttackerId",
                principalTable: "Swines",
                principalColumn: "SwineId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DuelResults_Swines_DefenderId",
                table: "DuelResults",
                column: "DefenderId",
                principalTable: "Swines",
                principalColumn: "SwineId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DuelResults_Swines_WinnerSwineId",
                table: "DuelResults",
                column: "WinnerSwineId",
                principalTable: "Swines",
                principalColumn: "SwineId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Slaughters_Groups_GroupId",
                table: "Slaughters",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "GroupId");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddParejas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Lista_Por_Pareja",
                table: "Torneo",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Ejercito_Companero1",
                table: "Partida_Torneo",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Ejercito_Companero2",
                table: "Partida_Torneo",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ID_Companero1",
                table: "Partida_Torneo",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ID_Companero2",
                table: "Partida_Torneo",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Partida_Torneo_ID_Companero1",
                table: "Partida_Torneo",
                column: "ID_Companero1");

            migrationBuilder.CreateIndex(
                name: "IX_Partida_Torneo_ID_Companero2",
                table: "Partida_Torneo",
                column: "ID_Companero2");

            migrationBuilder.AddForeignKey(
                name: "FK_Partida_Torneo_Companero1",
                table: "Partida_Torneo",
                column: "ID_Companero1",
                principalTable: "Usuario",
                principalColumn: "ID_Usuario",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Partida_Torneo_Companero2",
                table: "Partida_Torneo",
                column: "ID_Companero2",
                principalTable: "Usuario",
                principalColumn: "ID_Usuario",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Partida_Torneo_Companero1",
                table: "Partida_Torneo");

            migrationBuilder.DropForeignKey(
                name: "FK_Partida_Torneo_Companero2",
                table: "Partida_Torneo");

            migrationBuilder.DropIndex(
                name: "IX_Partida_Torneo_ID_Companero1",
                table: "Partida_Torneo");

            migrationBuilder.DropIndex(
                name: "IX_Partida_Torneo_ID_Companero2",
                table: "Partida_Torneo");

            migrationBuilder.DropColumn(
                name: "Lista_Por_Pareja",
                table: "Torneo");

            migrationBuilder.DropColumn(
                name: "Ejercito_Companero1",
                table: "Partida_Torneo");

            migrationBuilder.DropColumn(
                name: "Ejercito_Companero2",
                table: "Partida_Torneo");

            migrationBuilder.DropColumn(
                name: "ID_Companero1",
                table: "Partida_Torneo");

            migrationBuilder.DropColumn(
                name: "ID_Companero2",
                table: "Partida_Torneo");
        }
    }
}

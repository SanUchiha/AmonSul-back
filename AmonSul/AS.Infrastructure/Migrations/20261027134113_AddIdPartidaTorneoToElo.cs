using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIdPartidaTorneoToElo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ID_Partida_Torneo",
                table: "Elo",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Elo_ID_Partida_Torneo",
                table: "Elo",
                column: "ID_Partida_Torneo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Elo_ID_Partida_Torneo",
                table: "Elo");

            migrationBuilder.DropColumn(
                name: "ID_Partida_Torneo",
                table: "Elo");
        }
    }
}

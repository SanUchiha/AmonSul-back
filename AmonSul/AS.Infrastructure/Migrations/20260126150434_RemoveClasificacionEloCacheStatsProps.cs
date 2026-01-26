using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveClasificacionEloCacheStatsProps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Empatadas",
                table: "Clasificacion_Elo_Cache");

            migrationBuilder.DropColumn(
                name: "Ganadas",
                table: "Clasificacion_Elo_Cache");

            migrationBuilder.DropColumn(
                name: "Numero_Partidas_Jugadas",
                table: "Clasificacion_Elo_Cache");

            migrationBuilder.DropColumn(
                name: "Partidas",
                table: "Clasificacion_Elo_Cache");

            migrationBuilder.DropColumn(
                name: "Perdidas",
                table: "Clasificacion_Elo_Cache");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Empatadas",
                table: "Clasificacion_Elo_Cache",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Ganadas",
                table: "Clasificacion_Elo_Cache",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Numero_Partidas_Jugadas",
                table: "Clasificacion_Elo_Cache",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Partidas",
                table: "Clasificacion_Elo_Cache",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Perdidas",
                table: "Clasificacion_Elo_Cache",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}

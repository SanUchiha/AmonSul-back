using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AS.Infrastructure.Migrations
{
    /// <summary>
    /// Formato Copa: sus opciones en el torneo, el grupo de cada inscripción y, en cada partida, la fase, el grupo, el
    /// cruce del cuadro y quién pasa en un empate de la eliminatoria
    /// </summary>
    public partial class AddFormatoCopa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Clasificados_Por_Grupo",
                table: "Torneo",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Desempate_Eliminatoria",
                table: "Torneo",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Numero_Grupos",
                table: "Torneo",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Reparto_Grupos",
                table: "Torneo",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Tercer_Puesto",
                table: "Torneo",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Cruce",
                table: "Partida_Torneo",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Fase",
                table: "Partida_Torneo",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Grupo",
                table: "Partida_Torneo",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ID_Usuario_Pasa",
                table: "Partida_Torneo",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Grupo",
                table: "Inscripcion_Torneo",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Clasificados_Por_Grupo",
                table: "Torneo");

            migrationBuilder.DropColumn(
                name: "Desempate_Eliminatoria",
                table: "Torneo");

            migrationBuilder.DropColumn(
                name: "Numero_Grupos",
                table: "Torneo");

            migrationBuilder.DropColumn(
                name: "Reparto_Grupos",
                table: "Torneo");

            migrationBuilder.DropColumn(
                name: "Tercer_Puesto",
                table: "Torneo");

            migrationBuilder.DropColumn(
                name: "Cruce",
                table: "Partida_Torneo");

            migrationBuilder.DropColumn(
                name: "Fase",
                table: "Partida_Torneo");

            migrationBuilder.DropColumn(
                name: "Grupo",
                table: "Partida_Torneo");

            migrationBuilder.DropColumn(
                name: "ID_Usuario_Pasa",
                table: "Partida_Torneo");

            migrationBuilder.DropColumn(
                name: "Grupo",
                table: "Inscripcion_Torneo");
        }
    }
}

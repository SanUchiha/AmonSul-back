using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AS.Infrastructure.Migrations
{
    /// <summary>
    /// Formato del torneo (Suizo o Liga), opciones de la Liga y ejército que se elige al inscribirse en ella
    /// </summary>
    public partial class AddFormatoLiga : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Cuenta_Elo",
                table: "Torneo",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Formato_Torneo",
                table: "Torneo",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: false,
                // Todos los torneos que ya existen son suizos
                defaultValue: "Suizo");

            migrationBuilder.AddColumn<bool>(
                name: "Ida_Y_Vuelta",
                table: "Torneo",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Ejercito_Elegido",
                table: "Inscripcion_Torneo",
                type: "varchar(100)",
                unicode: false,
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cuenta_Elo",
                table: "Torneo");

            migrationBuilder.DropColumn(
                name: "Formato_Torneo",
                table: "Torneo");

            migrationBuilder.DropColumn(
                name: "Ida_Y_Vuelta",
                table: "Torneo");

            migrationBuilder.DropColumn(
                name: "Ejercito_Elegido",
                table: "Inscripcion_Torneo");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AS.Infrastructure.Migrations
{
    /// <summary>
    /// Lista con la que se jugó cada lado de una partida de liga, guardada al registrar el resultado
    /// </summary>
    public partial class AddListaPartidaLiga : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Lista_Usuario1",
                table: "Partida_Torneo",
                type: "varchar(500)",
                unicode: false,
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Lista_Usuario2",
                table: "Partida_Torneo",
                type: "varchar(500)",
                unicode: false,
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Lista_Usuario1",
                table: "Partida_Torneo");

            migrationBuilder.DropColumn(
                name: "Lista_Usuario2",
                table: "Partida_Torneo");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AS.Infrastructure.Migrations
{
    /// <summary>
    /// Copa: el hueco de cada clasificado en la primera ronda del cuadro, del que sale toda la eliminatoria
    /// </summary>
    public partial class AddPosicionCuadroCopa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Posicion_Cuadro",
                table: "Inscripcion_Torneo",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Posicion_Cuadro",
                table: "Inscripcion_Torneo");
        }
    }
}

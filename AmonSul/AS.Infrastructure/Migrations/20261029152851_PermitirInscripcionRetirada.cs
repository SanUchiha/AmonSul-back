using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AS.Infrastructure.Migrations
{
    /// <summary>
    /// La restricción de la base de datos solo admitía EN PROCESO y COMPLETADA; las ligas marcan RETIRADO al retirar
    /// a un jugador
    /// </summary>
    public partial class PermitirInscripcionRetirada : Migration
    {
        private const string Restriccion = "CK__Inscripcion_Torneo_CHECK_Estado_Inscripcion";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"ALTER TABLE Inscripcion_Torneo DROP CONSTRAINT {Restriccion};");
            migrationBuilder.Sql(
                $"ALTER TABLE Inscripcion_Torneo ADD CONSTRAINT {Restriccion} CHECK "
                    + "(Estado_Inscripcion = 'EN PROCESO' OR Estado_Inscripcion = 'COMPLETADA' OR Estado_Inscripcion = 'RETIRADO');"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Falla si queda algún retirado, para no perder ese estado sin querer
            migrationBuilder.Sql($"ALTER TABLE Inscripcion_Torneo DROP CONSTRAINT {Restriccion};");
            migrationBuilder.Sql(
                $"ALTER TABLE Inscripcion_Torneo ADD CONSTRAINT {Restriccion} CHECK "
                    + "(Estado_Inscripcion = 'EN PROCESO' OR Estado_Inscripcion = 'COMPLETADA');"
            );
        }
    }
}

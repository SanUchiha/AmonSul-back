using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AS.Infrastructure.Migrations
{
    /// <summary>
    /// Si el jugador deja que el organizador vea su correo, en cada inscripción. Las que ya existían empiezan
    /// en null: el organizador deja de verlo hasta que el jugador lo comparta
    /// </summary>
    public partial class AddCompartirEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Compartir_Email",
                table: "Inscripcion_Torneo",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Compartir_Email",
                table: "Inscripcion_Torneo");
        }
    }
}

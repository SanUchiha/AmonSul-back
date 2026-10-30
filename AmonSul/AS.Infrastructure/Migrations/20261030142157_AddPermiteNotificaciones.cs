using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AS.Infrastructure.Migrations
{
    /// <summary>
    /// Si el usuario quiere recibir correos de avisos. Todos los que ya existían, sí: como hasta ahora
    /// </summary>
    public partial class AddPermiteNotificaciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Permite_Notificaciones",
                table: "Usuario",
                type: "bit",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Permite_Notificaciones",
                table: "Usuario");
        }
    }
}

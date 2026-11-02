using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AS.Infrastructure.Migrations
{
    /// <summary>
    /// El enlace para recuperar la contraseña: se guarda su hash y su caducidad, nunca el enlace.
    /// Todos los usuarios que ya existían empiezan sin enlace
    /// </summary>
    public partial class AddEnlaceContrasena : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Caducidad_Enlace_Contrasena",
                table: "Usuario",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Hash_Enlace_Contrasena",
                table: "Usuario",
                type: "varchar(64)",
                unicode: false,
                maxLength: 64,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Caducidad_Enlace_Contrasena",
                table: "Usuario");

            migrationBuilder.DropColumn(
                name: "Hash_Enlace_Contrasena",
                table: "Usuario");
        }
    }
}

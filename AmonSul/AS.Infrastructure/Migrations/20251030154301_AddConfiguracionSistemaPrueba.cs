using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddConfiguracionSistemaPrueba : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Configuracion_Sistema",
                columns: table => new
                {
                    ID_Configuracion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre_Configuracion = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    Valor_Configuracion = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: false),
                    Descripcion = table.Column<string>(type: "varchar(1000)", unicode: false, maxLength: 1000, nullable: true),
                    Fecha_Creacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    Fecha_Modificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ConfiguracionSistema__IdConfiguracion", x => x.ID_Configuracion);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionSistema_NombreConfiguracion",
                table: "Configuracion_Sistema",
                column: "Nombre_Configuracion",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Configuracion_Sistema");
        }
    }
}

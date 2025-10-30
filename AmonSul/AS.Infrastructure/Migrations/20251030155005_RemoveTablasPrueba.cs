using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTablasPrueba : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Configuracion_Sistema");

            migrationBuilder.DropTable(
                name: "Tabla_Prueba");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Configuracion_Sistema",
                columns: table => new
                {
                    ID_Configuracion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Descripcion = table.Column<string>(type: "varchar(1000)", unicode: false, maxLength: 1000, nullable: true),
                    Fecha_Creacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    Fecha_Modificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Nombre_Configuracion = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    Valor_Configuracion = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ConfiguracionSistema__IdConfiguracion", x => x.ID_Configuracion);
                });

            migrationBuilder.CreateTable(
                name: "Tabla_Prueba",
                columns: table => new
                {
                    ID_Prueba = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Fecha_Creacion_Prueba = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    Nombre_Prueba = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__TablaPrueba__IdPrueba", x => x.ID_Prueba);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionSistema_NombreConfiguracion",
                table: "Configuracion_Sistema",
                column: "Nombre_Configuracion",
                unique: true);
        }
    }
}

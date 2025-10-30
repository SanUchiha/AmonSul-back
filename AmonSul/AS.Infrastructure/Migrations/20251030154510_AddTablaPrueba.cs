using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTablaPrueba : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tabla_Prueba",
                columns: table => new
                {
                    ID_Prueba = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre_Prueba = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Fecha_Creacion_Prueba = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__TablaPrueba__IdPrueba", x => x.ID_Prueba);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tabla_Prueba");
        }
    }
}

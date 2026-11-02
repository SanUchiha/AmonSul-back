using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AS.Infrastructure.Migrations
{
    /// <summary>
    /// Cuándo se creó cada circuito, para filtrarlos por año. Los que ya existían: el 1 de enero del año de su
    /// nombre; sin año en el nombre, el de su primer evento; y sin eventos, hoy
    /// </summary>
    public partial class AddFechaCreacionCircuito : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "Fecha_Creacion",
                table: "Circuito",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.Sql(@"
                UPDATE Circuito
                SET Fecha_Creacion = DATEFROMPARTS(
                    CAST(SUBSTRING(Nombre_Circuito, PATINDEX('%20[0-9][0-9]%', Nombre_Circuito), 4) AS int), 1, 1)
                WHERE PATINDEX('%20[0-9][0-9]%', Nombre_Circuito) > 0;");

            migrationBuilder.Sql(@"
                UPDATE c
                SET Fecha_Creacion = DATEFROMPARTS(YEAR(p.Primero), 1, 1)
                FROM Circuito c
                CROSS APPLY (
                    SELECT MIN(t.Fecha_Inicio_Torneo) AS Primero
                    FROM CircuitoTorneo ct
                    JOIN Torneo t ON t.ID_Torneo = ct.Id_Torneo
                    WHERE ct.Id_Circuito = c.Id_Circuito) p
                WHERE c.Fecha_Creacion = '0001-01-01' AND p.Primero IS NOT NULL;");

            migrationBuilder.Sql(@"
                UPDATE Circuito SET Fecha_Creacion = CAST(GETDATE() AS date) WHERE Fecha_Creacion = '0001-01-01';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Fecha_Creacion",
                table: "Circuito");
        }
    }
}

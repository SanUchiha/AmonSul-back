using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddClasificacionEloCacheTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clasificacion_Elo_Cache",
                columns: table => new
                {
                    ID_Clasificacion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ID_Usuario = table.Column<int>(type: "int", nullable: false),
                    Email = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    Nick = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    ID_Faccion = table.Column<int>(type: "int", nullable: true),
                    Elo = table.Column<int>(type: "int", nullable: false),
                    Partidas = table.Column<int>(type: "int", nullable: false),
                    Ganadas = table.Column<int>(type: "int", nullable: false),
                    Empatadas = table.Column<int>(type: "int", nullable: false),
                    Perdidas = table.Column<int>(type: "int", nullable: false),
                    Numero_Partidas_Jugadas = table.Column<int>(type: "int", nullable: false),
                    Anio_Clasificacion = table.Column<int>(type: "int", nullable: false),
                    Fecha_Actualizacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ClasificacionEloCache__IdClasificacion", x => x.ID_Clasificacion);
                    table.ForeignKey(
                        name: "FK_ClasificacionEloCache_Faccion",
                        column: x => x.ID_Faccion,
                        principalTable: "Faccion",
                        principalColumn: "ID_Faccion",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ClasificacionEloCache_Usuario",
                        column: x => x.ID_Usuario,
                        principalTable: "Usuario",
                        principalColumn: "ID_Usuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Clasificacion_Elo_Cache_ID_Faccion",
                table: "Clasificacion_Elo_Cache",
                column: "ID_Faccion");

            migrationBuilder.CreateIndex(
                name: "IX_ClasificacionEloCache_Anio",
                table: "Clasificacion_Elo_Cache",
                column: "Anio_Clasificacion");

            migrationBuilder.CreateIndex(
                name: "IX_ClasificacionEloCache_FechaActualizacion",
                table: "Clasificacion_Elo_Cache",
                column: "Fecha_Actualizacion");

            migrationBuilder.CreateIndex(
                name: "IX_ClasificacionEloCache_Usuario_Anio",
                table: "Clasificacion_Elo_Cache",
                columns: new[] { "ID_Usuario", "Anio_Clasificacion" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Clasificacion_Elo_Cache");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateClasificacionEloCacheSimplified : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClasificacionEloCache_Faccion",
                table: "Clasificacion_Elo_Cache");

            migrationBuilder.DropForeignKey(
                name: "FK_ClasificacionEloCache_Usuario",
                table: "Clasificacion_Elo_Cache");

            migrationBuilder.DropIndex(
                name: "IX_Clasificacion_Elo_Cache_ID_Faccion",
                table: "Clasificacion_Elo_Cache");

            migrationBuilder.DropIndex(
                name: "IX_ClasificacionEloCache_Anio",
                table: "Clasificacion_Elo_Cache");

            migrationBuilder.DropIndex(
                name: "IX_ClasificacionEloCache_FechaActualizacion",
                table: "Clasificacion_Elo_Cache");

            migrationBuilder.DropIndex(
                name: "IX_ClasificacionEloCache_Usuario_Anio",
                table: "Clasificacion_Elo_Cache");

            migrationBuilder.DropColumn(
                name: "Activo",
                table: "Clasificacion_Elo_Cache");

            migrationBuilder.DropColumn(
                name: "Anio_Clasificacion",
                table: "Clasificacion_Elo_Cache");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Clasificacion_Elo_Cache");

            migrationBuilder.DropColumn(
                name: "Fecha_Actualizacion",
                table: "Clasificacion_Elo_Cache");

            migrationBuilder.AlterColumn<int>(
                name: "ID_Faccion",
                table: "Clasificacion_Elo_Cache",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClasificacionEloCache_Elo",
                table: "Clasificacion_Elo_Cache",
                column: "Elo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ClasificacionEloCache_Elo",
                table: "Clasificacion_Elo_Cache");

            migrationBuilder.AlterColumn<int>(
                name: "ID_Faccion",
                table: "Clasificacion_Elo_Cache",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<bool>(
                name: "Activo",
                table: "Clasificacion_Elo_Cache",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "Anio_Clasificacion",
                table: "Clasificacion_Elo_Cache",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Clasificacion_Elo_Cache",
                type: "varchar(100)",
                unicode: false,
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "Fecha_Actualizacion",
                table: "Clasificacion_Elo_Cache",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()");

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

            migrationBuilder.AddForeignKey(
                name: "FK_ClasificacionEloCache_Faccion",
                table: "Clasificacion_Elo_Cache",
                column: "ID_Faccion",
                principalTable: "Faccion",
                principalColumn: "ID_Faccion",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ClasificacionEloCache_Usuario",
                table: "Clasificacion_Elo_Cache",
                column: "ID_Usuario",
                principalTable: "Usuario",
                principalColumn: "ID_Usuario",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDispositivoPush : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Dispositivo_Push",
                columns: table => new
                {
                    ID_Dispositivo_Push = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ID_Usuario = table.Column<int>(type: "int", nullable: false),
                    Token = table.Column<string>(type: "varchar(512)", unicode: false, maxLength: 512, nullable: false),
                    Plataforma = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    Fecha_Creacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Dispositivo_Push__IdDispositivoPush", x => x.ID_Dispositivo_Push);
                    table.ForeignKey(
                        name: "FK_Dispositivo_Push_Usuario_ID_Usuario",
                        column: x => x.ID_Usuario,
                        principalTable: "Usuario",
                        principalColumn: "ID_Usuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Dispositivo_Push_ID_Usuario",
                table: "Dispositivo_Push",
                column: "ID_Usuario");

            migrationBuilder.CreateIndex(
                name: "IX_Dispositivo_Push_Token",
                table: "Dispositivo_Push",
                column: "Token",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Dispositivo_Push");
        }
    }
}

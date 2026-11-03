using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRefreshToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Refresh_Token",
                columns: table => new
                {
                    ID_Refresh_Token = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ID_Usuario = table.Column<int>(type: "int", nullable: false),
                    Hash = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                    Caducidad = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Fecha_Creacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Refresh_Token__IdRefreshToken", x => x.ID_Refresh_Token);
                    table.ForeignKey(
                        name: "FK_Refresh_Token_Usuario_ID_Usuario",
                        column: x => x.ID_Usuario,
                        principalTable: "Usuario",
                        principalColumn: "ID_Usuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Refresh_Token_Hash",
                table: "Refresh_Token",
                column: "Hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Refresh_Token_ID_Usuario",
                table: "Refresh_Token",
                column: "ID_Usuario");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Refresh_Token");
        }
    }
}

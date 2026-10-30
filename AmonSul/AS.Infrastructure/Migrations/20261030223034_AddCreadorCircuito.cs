using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AS.Infrastructure.Migrations
{
    /// <summary>
    /// Quién creó cada circuito desde la web, el único que lo renombra o lo borra. Los que ya existían, sin creador
    /// </summary>
    public partial class AddCreadorCircuito : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Id_Usuario",
                table: "Circuito",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Circuito_Id_Usuario",
                table: "Circuito",
                column: "Id_Usuario");

            migrationBuilder.AddForeignKey(
                name: "FK_Circuito_Usuario",
                table: "Circuito",
                column: "Id_Usuario",
                principalTable: "Usuario",
                principalColumn: "ID_Usuario",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Circuito_Usuario",
                table: "Circuito");

            migrationBuilder.DropIndex(
                name: "IX_Circuito_Id_Usuario",
                table: "Circuito");

            migrationBuilder.DropColumn(
                name: "Id_Usuario",
                table: "Circuito");
        }
    }
}

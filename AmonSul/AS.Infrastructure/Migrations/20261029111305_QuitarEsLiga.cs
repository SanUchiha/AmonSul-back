using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AS.Infrastructure.Migrations
{
    /// <summary>
    /// Borra Torneo.Es_Liga, que no usaba nadie. Se aplica cuando el back que ya no la conoce está desplegado
    /// </summary>
    public partial class QuitarEsLiga : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "IF EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = N'DF_Torneo_Es_Liga') "
                    + "ALTER TABLE [Torneo] DROP CONSTRAINT [DF_Torneo_Es_Liga];"
            );
            migrationBuilder.DropColumn(name: "Es_Liga", table: "Torneo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Es_Liga",
                table: "Torneo",
                type: "bit",
                nullable: false,
                defaultValue: false
            );
        }
    }
}

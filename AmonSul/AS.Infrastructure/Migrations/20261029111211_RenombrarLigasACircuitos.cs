using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AS.Infrastructure.Migrations
{
    /// <summary>
    /// Lo que se llamaba "Ligas" (circuitos que agrupan torneos) pasa a "Circuitos" para dejar "Liga" al formato
    /// todos contra todos. Renombra sin copiar datos: los circuitos y sus torneos se quedan como están.
    /// </summary>
    public partial class RenombrarLigasACircuitos : Migration
    {
        // En producción las claves las creó un script con nombres automáticos: pasan a los que da EF
        private static string RenombrarRestriccion(string actual, string nuevo) =>
            $"IF OBJECT_ID(N'dbo.{actual}') IS NOT NULL EXEC sp_rename N'dbo.{actual}', N'{nuevo}', N'OBJECT';";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(name: "Liga", newName: "Circuito");
            migrationBuilder.RenameColumn(name: "Id_Liga", table: "Circuito", newName: "Id_Circuito");
            migrationBuilder.RenameColumn(name: "Nombre_Liga", table: "Circuito", newName: "Nombre_Circuito");

            migrationBuilder.RenameTable(name: "LigaTorneo", newName: "CircuitoTorneo");
            migrationBuilder.RenameColumn(name: "Id_Liga", table: "CircuitoTorneo", newName: "Id_Circuito");

            migrationBuilder.Sql(RenombrarRestriccion("PK__Liga__0CCA9DE1B35035A6", "PK_Circuito"));
            migrationBuilder.Sql(RenombrarRestriccion("PK__LigaTorn__9EA9C56F6C8E62D0", "PK_CircuitoTorneo"));
            migrationBuilder.Sql(RenombrarRestriccion("FK__LigaTorne__Id_Li__1D7B6025", "FK_CircuitoTorneo_Circuito_Id_Circuito"));
            migrationBuilder.Sql(RenombrarRestriccion("FK__LigaTorne__Id_To__1E6F845E", "FK_CircuitoTorneo_Torneo_Id_Torneo"));

            // El modelo cuenta con un índice por torneo que en producción no existía
            migrationBuilder.Sql(
                "IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CircuitoTorneo_Id_Torneo') "
                    + "CREATE INDEX [IX_CircuitoTorneo_Id_Torneo] ON [CircuitoTorneo] ([Id_Torneo]);"
            );

            // Es_Liga no la usa nadie: se vacía y se le pone un valor por defecto para que el back nuevo,
            // que ya no la conoce, pueda crear torneos. Se borra en QuitarEsLiga, cuando ya esté desplegado
            migrationBuilder.Sql("UPDATE [Torneo] SET [Es_Liga] = 0;");
            migrationBuilder.Sql(
                "IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = N'DF_Torneo_Es_Liga') "
                    + "ALTER TABLE [Torneo] ADD CONSTRAINT [DF_Torneo_Es_Liga] DEFAULT 0 FOR [Es_Liga];"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "IF EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = N'DF_Torneo_Es_Liga') "
                    + "ALTER TABLE [Torneo] DROP CONSTRAINT [DF_Torneo_Es_Liga];"
            );
            migrationBuilder.Sql(
                "IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CircuitoTorneo_Id_Torneo') "
                    + "DROP INDEX [IX_CircuitoTorneo_Id_Torneo] ON [CircuitoTorneo];"
            );

            migrationBuilder.Sql(RenombrarRestriccion("FK_CircuitoTorneo_Torneo_Id_Torneo", "FK__LigaTorne__Id_To__1E6F845E"));
            migrationBuilder.Sql(RenombrarRestriccion("FK_CircuitoTorneo_Circuito_Id_Circuito", "FK__LigaTorne__Id_Li__1D7B6025"));
            migrationBuilder.Sql(RenombrarRestriccion("PK_CircuitoTorneo", "PK__LigaTorn__9EA9C56F6C8E62D0"));
            migrationBuilder.Sql(RenombrarRestriccion("PK_Circuito", "PK__Liga__0CCA9DE1B35035A6"));

            migrationBuilder.RenameColumn(name: "Id_Circuito", table: "CircuitoTorneo", newName: "Id_Liga");
            migrationBuilder.RenameTable(name: "CircuitoTorneo", newName: "LigaTorneo");

            migrationBuilder.RenameColumn(name: "Nombre_Circuito", table: "Circuito", newName: "Nombre_Liga");
            migrationBuilder.RenameColumn(name: "Id_Circuito", table: "Circuito", newName: "Id_Liga");
            migrationBuilder.RenameTable(name: "Circuito", newName: "Liga");
        }
    }
}

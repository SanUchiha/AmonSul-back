BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028135729_GanadorUnicoPorTorneoYUsuario'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Ganador_Torneo_Usuario] ON [Ganador] ([Id_Torneo], [Id_Usuario]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028135729_GanadorUnicoPorTorneoYUsuario'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261028135729_GanadorUnicoPorTorneoYUsuario', N'8.0.5');
END;
GO

COMMIT;
GO


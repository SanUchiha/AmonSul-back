BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261029111305_QuitarEsLiga'
)
BEGIN
    IF EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = N'DF_Torneo_Es_Liga') ALTER TABLE [Torneo] DROP CONSTRAINT [DF_Torneo_Es_Liga];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261029111305_QuitarEsLiga'
)
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Torneo]') AND [c].[name] = N'Es_Liga');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Torneo] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [Torneo] DROP COLUMN [Es_Liga];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261029111305_QuitarEsLiga'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261029111305_QuitarEsLiga', N'8.0.5');
END;
GO

COMMIT;
GO


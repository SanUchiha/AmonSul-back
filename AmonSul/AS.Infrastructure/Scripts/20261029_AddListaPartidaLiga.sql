BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261029121953_AddListaPartidaLiga'
)
BEGIN
    ALTER TABLE [Partida_Torneo] ADD [Lista_Usuario1] varchar(500) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261029121953_AddListaPartidaLiga'
)
BEGIN
    ALTER TABLE [Partida_Torneo] ADD [Lista_Usuario2] varchar(500) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261029121953_AddListaPartidaLiga'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261029121953_AddListaPartidaLiga', N'8.0.5');
END;
GO

COMMIT;
GO


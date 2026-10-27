BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261027134113_AddIdPartidaTorneoToElo'
)
BEGIN
    ALTER TABLE [Elo] ADD [ID_Partida_Torneo] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261027134113_AddIdPartidaTorneoToElo'
)
BEGIN
    CREATE INDEX [IX_Elo_ID_Partida_Torneo] ON [Elo] ([ID_Partida_Torneo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261027134113_AddIdPartidaTorneoToElo'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261027134113_AddIdPartidaTorneoToElo', N'8.0.5');
END;
GO

COMMIT;
GO


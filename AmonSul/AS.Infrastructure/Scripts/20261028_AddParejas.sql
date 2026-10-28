BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028102020_AddParejas'
)
BEGIN
    ALTER TABLE [Torneo] ADD [Lista_Por_Pareja] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028102020_AddParejas'
)
BEGIN
    ALTER TABLE [Partida_Torneo] ADD [Ejercito_Companero1] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028102020_AddParejas'
)
BEGIN
    ALTER TABLE [Partida_Torneo] ADD [Ejercito_Companero2] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028102020_AddParejas'
)
BEGIN
    ALTER TABLE [Partida_Torneo] ADD [ID_Companero1] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028102020_AddParejas'
)
BEGIN
    ALTER TABLE [Partida_Torneo] ADD [ID_Companero2] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028102020_AddParejas'
)
BEGIN
    CREATE INDEX [IX_Partida_Torneo_ID_Companero1] ON [Partida_Torneo] ([ID_Companero1]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028102020_AddParejas'
)
BEGIN
    CREATE INDEX [IX_Partida_Torneo_ID_Companero2] ON [Partida_Torneo] ([ID_Companero2]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028102020_AddParejas'
)
BEGIN
    ALTER TABLE [Partida_Torneo] ADD CONSTRAINT [FK_Partida_Torneo_Companero1] FOREIGN KEY ([ID_Companero1]) REFERENCES [Usuario] ([ID_Usuario]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028102020_AddParejas'
)
BEGIN
    ALTER TABLE [Partida_Torneo] ADD CONSTRAINT [FK_Partida_Torneo_Companero2] FOREIGN KEY ([ID_Companero2]) REFERENCES [Usuario] ([ID_Usuario]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028102020_AddParejas'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261028102020_AddParejas', N'8.0.5');
END;
GO

COMMIT;
GO


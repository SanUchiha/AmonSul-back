BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261029115121_AddFormatoLiga'
)
BEGIN
    ALTER TABLE [Torneo] ADD [Cuenta_Elo] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261029115121_AddFormatoLiga'
)
BEGIN
    ALTER TABLE [Torneo] ADD [Formato_Torneo] varchar(20) NOT NULL DEFAULT 'Suizo';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261029115121_AddFormatoLiga'
)
BEGIN
    ALTER TABLE [Torneo] ADD [Ida_Y_Vuelta] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261029115121_AddFormatoLiga'
)
BEGIN
    ALTER TABLE [Inscripcion_Torneo] ADD [Ejercito_Elegido] varchar(100) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261029115121_AddFormatoLiga'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261029115121_AddFormatoLiga', N'8.0.5');
END;
GO

COMMIT;
GO


BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261030101157_AddFormatoCopa'
)
BEGIN
    ALTER TABLE [Torneo] ADD [Clasificados_Por_Grupo] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261030101157_AddFormatoCopa'
)
BEGIN
    ALTER TABLE [Torneo] ADD [Desempate_Eliminatoria] varchar(20) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261030101157_AddFormatoCopa'
)
BEGIN
    ALTER TABLE [Torneo] ADD [Numero_Grupos] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261030101157_AddFormatoCopa'
)
BEGIN
    ALTER TABLE [Torneo] ADD [Reparto_Grupos] varchar(20) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261030101157_AddFormatoCopa'
)
BEGIN
    ALTER TABLE [Torneo] ADD [Tercer_Puesto] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261030101157_AddFormatoCopa'
)
BEGIN
    ALTER TABLE [Partida_Torneo] ADD [Cruce] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261030101157_AddFormatoCopa'
)
BEGIN
    ALTER TABLE [Partida_Torneo] ADD [Fase] varchar(20) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261030101157_AddFormatoCopa'
)
BEGIN
    ALTER TABLE [Partida_Torneo] ADD [Grupo] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261030101157_AddFormatoCopa'
)
BEGIN
    ALTER TABLE [Partida_Torneo] ADD [ID_Usuario_Pasa] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261030101157_AddFormatoCopa'
)
BEGIN
    ALTER TABLE [Inscripcion_Torneo] ADD [Grupo] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261030101157_AddFormatoCopa'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261030101157_AddFormatoCopa', N'8.0.5');
END;
GO

COMMIT;
GO


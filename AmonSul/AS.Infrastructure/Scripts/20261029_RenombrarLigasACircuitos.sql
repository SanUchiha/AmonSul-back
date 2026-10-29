BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261029111211_RenombrarLigasACircuitos'
)
BEGIN
    EXEC sp_rename N'[Liga]', N'Circuito';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261029111211_RenombrarLigasACircuitos'
)
BEGIN
    EXEC sp_rename N'[Circuito].[Id_Liga]', N'Id_Circuito', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261029111211_RenombrarLigasACircuitos'
)
BEGIN
    EXEC sp_rename N'[Circuito].[Nombre_Liga]', N'Nombre_Circuito', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261029111211_RenombrarLigasACircuitos'
)
BEGIN
    EXEC sp_rename N'[LigaTorneo]', N'CircuitoTorneo';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261029111211_RenombrarLigasACircuitos'
)
BEGIN
    EXEC sp_rename N'[CircuitoTorneo].[Id_Liga]', N'Id_Circuito', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261029111211_RenombrarLigasACircuitos'
)
BEGIN
    IF OBJECT_ID(N'dbo.PK__Liga__0CCA9DE1B35035A6') IS NOT NULL EXEC sp_rename N'dbo.PK__Liga__0CCA9DE1B35035A6', N'PK_Circuito', N'OBJECT';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261029111211_RenombrarLigasACircuitos'
)
BEGIN
    IF OBJECT_ID(N'dbo.PK__LigaTorn__9EA9C56F6C8E62D0') IS NOT NULL EXEC sp_rename N'dbo.PK__LigaTorn__9EA9C56F6C8E62D0', N'PK_CircuitoTorneo', N'OBJECT';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261029111211_RenombrarLigasACircuitos'
)
BEGIN
    IF OBJECT_ID(N'dbo.FK__LigaTorne__Id_Li__1D7B6025') IS NOT NULL EXEC sp_rename N'dbo.FK__LigaTorne__Id_Li__1D7B6025', N'FK_CircuitoTorneo_Circuito_Id_Circuito', N'OBJECT';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261029111211_RenombrarLigasACircuitos'
)
BEGIN
    IF OBJECT_ID(N'dbo.FK__LigaTorne__Id_To__1E6F845E') IS NOT NULL EXEC sp_rename N'dbo.FK__LigaTorne__Id_To__1E6F845E', N'FK_CircuitoTorneo_Torneo_Id_Torneo', N'OBJECT';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261029111211_RenombrarLigasACircuitos'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CircuitoTorneo_Id_Torneo') CREATE INDEX [IX_CircuitoTorneo_Id_Torneo] ON [CircuitoTorneo] ([Id_Torneo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261029111211_RenombrarLigasACircuitos'
)
BEGIN
    UPDATE [Torneo] SET [Es_Liga] = 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261029111211_RenombrarLigasACircuitos'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = N'DF_Torneo_Es_Liga') ALTER TABLE [Torneo] ADD CONSTRAINT [DF_Torneo_Es_Liga] DEFAULT 0 FOR [Es_Liga];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261029111211_RenombrarLigasACircuitos'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261029111211_RenombrarLigasACircuitos', N'8.0.5');
END;
GO

COMMIT;
GO


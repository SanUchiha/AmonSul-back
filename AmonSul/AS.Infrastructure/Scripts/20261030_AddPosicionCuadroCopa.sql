BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261030105857_AddPosicionCuadroCopa'
)
BEGIN
    ALTER TABLE [Inscripcion_Torneo] ADD [Posicion_Cuadro] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261030105857_AddPosicionCuadroCopa'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261030105857_AddPosicionCuadroCopa', N'8.0.5');
END;
GO

COMMIT;
GO


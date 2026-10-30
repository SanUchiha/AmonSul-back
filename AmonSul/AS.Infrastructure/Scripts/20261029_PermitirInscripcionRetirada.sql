BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261029152851_PermitirInscripcionRetirada'
)
BEGIN
    ALTER TABLE Inscripcion_Torneo DROP CONSTRAINT CK__Inscripcion_Torneo_CHECK_Estado_Inscripcion;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261029152851_PermitirInscripcionRetirada'
)
BEGIN
    ALTER TABLE Inscripcion_Torneo ADD CONSTRAINT CK__Inscripcion_Torneo_CHECK_Estado_Inscripcion CHECK (Estado_Inscripcion = 'EN PROCESO' OR Estado_Inscripcion = 'COMPLETADA' OR Estado_Inscripcion = 'RETIRADO');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261029152851_PermitirInscripcionRetirada'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261029152851_PermitirInscripcionRetirada', N'8.0.5');
END;
GO

COMMIT;
GO


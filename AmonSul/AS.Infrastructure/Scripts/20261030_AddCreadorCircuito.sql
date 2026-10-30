BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261030223034_AddCreadorCircuito'
)
BEGIN
    ALTER TABLE [Circuito] ADD [Id_Usuario] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261030223034_AddCreadorCircuito'
)
BEGIN
    CREATE INDEX [IX_Circuito_Id_Usuario] ON [Circuito] ([Id_Usuario]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261030223034_AddCreadorCircuito'
)
BEGIN
    ALTER TABLE [Circuito] ADD CONSTRAINT [FK_Circuito_Usuario] FOREIGN KEY ([Id_Usuario]) REFERENCES [Usuario] ([ID_Usuario]) ON DELETE SET NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261030223034_AddCreadorCircuito'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261030223034_AddCreadorCircuito', N'8.0.5');
END;
GO

COMMIT;
GO


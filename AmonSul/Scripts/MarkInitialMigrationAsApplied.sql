-- Script para marcar la migración inicial como aplicada sin ejecutarla
-- Ejecutar este script en la base de datos DBAMONSUL

USE DBAMONSUL;
GO

-- Verificar si la tabla __EFMigrationsHistory existe, si no, crearla
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='__EFMigrationsHistory' AND xtype='U')
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
    PRINT 'Tabla __EFMigrationsHistory creada';
END
ELSE
BEGIN
    PRINT 'Tabla __EFMigrationsHistory ya existe';
END

-- Insertar el registro de la migración inicial si no existe
IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20251030153035_InitialCreate')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) 
    VALUES ('20251030153035_InitialCreate', '8.0.5');
    PRINT 'Migración inicial marcada como aplicada';
END
ELSE
BEGIN
    PRINT 'La migración inicial ya está marcada como aplicada';
END

-- Verificar el estado actual
SELECT * FROM [__EFMigrationsHistory] ORDER BY [MigrationId];
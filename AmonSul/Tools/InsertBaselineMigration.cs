using System;
using Microsoft.Data.SqlClient;

Console.WriteLine("Insertando registro de migración baseline...");

string connectionString = "Data Source=SQL6032.site4now.net;Initial Catalog=db_aa5e12_amonsul;User Id=db_aa5e12_amonsul_admin;Password=29101988aA..;";

try
{
    using var connection = new SqlConnection(connectionString);
    await connection.OpenAsync();
    
    // Verificar si la tabla existe
    var checkTableCommand = new SqlCommand(@"
        SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES 
        WHERE TABLE_NAME = '__EFMigrationsHistory'", connection);
    
    var tableExists = (int)await checkTableCommand.ExecuteScalarAsync() > 0;
    
    if (!tableExists)
    {
        Console.WriteLine("Creando tabla __EFMigrationsHistory...");
        var createTableCommand = new SqlCommand(@"
            CREATE TABLE [__EFMigrationsHistory] (
                [MigrationId] nvarchar(150) NOT NULL,
                [ProductVersion] nvarchar(32) NOT NULL,
                CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
            );", connection);
        
        await createTableCommand.ExecuteNonQueryAsync();
        Console.WriteLine("Tabla __EFMigrationsHistory creada exitosamente.");
    }
    
    // Crear migración baseline vacía
    Console.WriteLine("Creando migración baseline...");
    var migrationId = DateTime.Now.ToString("yyyyMMddHHmmss") + "_BaselineMigration";
    
    var insertCommand = new SqlCommand(@"
        IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = @MigrationId)
        BEGIN
            INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) 
            VALUES (@MigrationId, '8.0.5');
            SELECT 'INSERTED' as Result;
        END
        ELSE
        BEGIN
            SELECT 'EXISTS' as Result;
        END", connection);
    
    insertCommand.Parameters.AddWithValue("@MigrationId", migrationId);
    
    var result = await insertCommand.ExecuteScalarAsync();
    Console.WriteLine($"Resultado: {result}");
    Console.WriteLine($"Migración baseline '{migrationId}' lista.");
    Console.WriteLine($"Ahora puedes usar: dotnet ef migrations add [NuevaMigracion]");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}

Console.WriteLine("Presiona cualquier tecla para continuar...");
Console.ReadKey();
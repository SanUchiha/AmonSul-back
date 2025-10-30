# Guía de Migraciones Entity Framework - Proyecto AmonSul

## Configuración Completada ✅

El proyecto **AmonSul-back** ha sido configurado para usar **Code First** con migraciones de Entity Framework. 

### Estructura Actual
- **DbContext**: `AS.Domain.Models.DbamonsulContext`
- **Migraciones**: `AS.Infrastructure/Migrations/`
- **Configuración**: `AS.Infrastructure.Configurations.DataBaseContextConfigurations`

## Estado Actual de Migraciones

### Migración Inicial (20251030153035_InitialCreate)
- **Estado**: Pendiente de marcar como aplicada
- **Propósito**: Representa el estado actual de la BD (Database First → Code First)
- **Contenido**: Todas las tablas existentes

### Nueva Funcionalidad (20251030153353_AddConfiguracionSistema)
- **Estado**: Lista para aplicar
- **Propósito**: Demuestra el flujo Code First
- **Contenido**: Nueva tabla `Configuracion_Sistema`

## Pasos Necesarios para Completar la Migración

### 1. Marcar Migración Inicial como Aplicada
Ejecutar el script: `Scripts/MarkInitialMigrationAsApplied.sql` en la base de datos.

```sql
-- Opción manual si no puedes ejecutar el script
INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) 
VALUES ('20251030153035_InitialCreate', '8.0.5');
```

### 2. Aplicar Nueva Migración
```bash
dotnet ef database update --project AS.Infrastructure --startup-project AS.API
```

## Comandos Esenciales de Migraciones

### Crear Nueva Migración
```bash
dotnet ef migrations add NombreMigracion --project AS.Infrastructure --startup-project AS.API
```

### Aplicar Migraciones
```bash
# Aplicar todas las migraciones pendientes
dotnet ef database update --project AS.Infrastructure --startup-project AS.API

# Aplicar hasta una migración específica
dotnet ef database update NombreMigracion --project AS.Infrastructure --startup-project AS.API
```

### Ver Estado de Migraciones
```bash
dotnet ef migrations list --project AS.Infrastructure --startup-project AS.API
```

### Revertir Migración
```bash
# Revertir a la migración anterior
dotnet ef migrations remove --project AS.Infrastructure --startup-project AS.API

# Revertir BD a una migración específica
dotnet ef database update MigracionAnterior --project AS.Infrastructure --startup-project AS.API
```

### Generar Script SQL
```bash
# Script para todas las migraciones pendientes
dotnet ef migrations script --project AS.Infrastructure --startup-project AS.API

# Script desde una migración específica
dotnet ef migrations script MigracionOrigen MigracionDestino --project AS.Infrastructure --startup-project AS.API
```

## Flujo de Desarrollo Code First

### 1. Modificar Entidades
- Agregar nuevas entidades en `AS.Domain/Models/`
- Modificar entidades existentes
- Actualizar configuraciones en `DbamonsulContext.OnModelCreating`

### 2. Generar Migración
```bash
dotnet ef migrations add DescripcionDelCambio --project AS.Infrastructure --startup-project AS.API
```

### 3. Revisar Migración Generada
- Verificar el archivo generado en `AS.Infrastructure/Migrations/`
- Asegurar que los cambios son correctos
- Agregar lógica personalizada si es necesario

### 4. Aplicar a Desarrollo
```bash
dotnet ef database update --project AS.Infrastructure --startup-project AS.API
```

### 5. Aplicar a Producción
- Generar script SQL: `dotnet ef migrations script`
- Revisar script antes de aplicar
- Aplicar en horario de mantenimiento

## Mejores Prácticas

### ✅ Hacer
- **Revisar siempre** las migraciones generadas antes de aplicarlas
- **Usar nombres descriptivos** para las migraciones
- **Hacer backup** de la BD antes de aplicar migraciones en producción
- **Probar migraciones** en ambiente de desarrollo primero
- **Versionar** los archivos de migración en el control de código
- **Generar scripts SQL** para producción en lugar de aplicar directamente

### ❌ Evitar
- **Modificar** migraciones ya aplicadas en producción
- **Eliminar** archivos de migración sin revertir primero
- **Aplicar migraciones** directamente en producción sin testing
- **Ignorar** warnings del generador de migraciones
- **Mezclar** cambios de esquema con cambios de datos en la misma migración

## Estructura de Archivos Generados

```
AS.Infrastructure/Migrations/
├── 20251030153035_InitialCreate.cs              # Migración inicial
├── 20251030153035_InitialCreate.Designer.cs     # Metadata de migración
├── 20251030153353_AddConfiguracionSistema.cs    # Nueva funcionalidad
├── 20251030153353_AddConfiguracionSistema.Designer.cs
└── DbamonsulContextModelSnapshot.cs             # Estado actual del modelo
```

## Ejemplo: Nueva Entidad Creada

```csharp
public class ConfiguracionSistema
{
    public int IdConfiguracion { get; set; }
    public string NombreConfiguracion { get; set; } = string.Empty;
    public string ValorConfiguracion { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
    public bool Activo { get; set; } = true;
}
```

Esta entidad será mapeada a la tabla `Configuracion_Sistema` con:
- Clave primaria autoincrementable
- Índice único en `NombreConfiguracion`
- Valores por defecto para `FechaCreacion` y `Activo`
- Restricciones de longitud en campos de texto

## Soporte y Troubleshooting

### Error: "There is already an object named 'X' in the database"
- La migración inicial intenta crear tablas existentes
- Solución: Marcar migración inicial como aplicada (ver paso 1)

### Error: "Migration X has already been applied"
- Verificar estado con: `dotnet ef migrations list`
- La migración ya fue aplicada anteriormente

### Error: "No migrations configuration found"
- Verificar que `MigrationsAssembly` esté configurado correctamente
- Verificar que el proyecto correcto esté especificado en los comandos

---

**Nota**: Este documento debe actualizarse conforme evolucionen las necesidades del proyecto.
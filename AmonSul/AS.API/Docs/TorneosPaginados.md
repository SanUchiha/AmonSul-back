# Documentación de Endpoints: Torneos Próximos y Pasados (Paginados)

## Resumen
Se han creado dos endpoints REST para obtener torneos próximos y pasados, optimizados para grandes volúmenes de datos mediante paginación. Esto permite al frontend mostrar los torneos de forma eficiente y escalable.

---

## Endpoints

### 1. Obtener Torneos Próximos
- **URL:** `/api/torneo/proximos`
- **Método:** `GET`
- **Query Params:**
  - `pageNumber` (opcional, default: 1): número de página
  - `pageSize` (opcional, default: 10): cantidad de torneos por página
- **Descripción:** Devuelve los torneos cuya fecha de inicio es igual o posterior al día de hoy, ordenados por fecha de inicio ascendente.

### 2. Obtener Torneos Pasados
- **URL:** `/api/torneo/pasados`
- **Método:** `GET`
- **Query Params:**
  - `pageNumber` (opcional, default: 1): número de página
  - `pageSize` (opcional, default: 10): cantidad de torneos por página
- **Descripción:** Devuelve los torneos cuya fecha de inicio es anterior al día de hoy, ordenados por fecha de inicio descendente.

---

## Respuesta (Ejemplo)
La respuesta de ambos endpoints es un objeto paginado:

```json
{
  "items": [
    {
      "idTorneo": 123,
      "nombreTorneo": "Torneo de Navidad",
      "fechaInicioTorneo": "2025-12-24",
      "fechaFinTorneo": "2025-12-26",
      "descripcionTorneo": "...",
      // ...otros campos del torneo
    }
    // ...más torneos
  ],
  "totalCount": 42,
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 5
}
```

- `items`: array de torneos para la página solicitada
- `totalCount`: total de torneos encontrados
- `pageNumber`: número de página actual
- `pageSize`: cantidad de torneos por página
- `totalPages`: total de páginas disponibles

---

## Consideraciones para el Frontend
- Utilizar los parámetros `pageNumber` y `pageSize` para implementar paginación (botones siguiente/anterior, scroll infinito, etc).
- Mostrar el total de páginas o torneos si es necesario.
- El endpoint de próximos y pasados tienen la misma estructura de respuesta.
- Si no hay torneos, `items` será un array vacío y `totalCount` será 0.

---

## Ejemplo de llamada

```
GET /api/torneo/proximos?pageNumber=2&pageSize=5
```

---

Cualquier duda sobre los campos del objeto torneo, consultar el DTO `TorneoDTO` en backend.

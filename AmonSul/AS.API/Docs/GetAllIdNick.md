# GET /api/Usuario/IdNick

Devuelve el listado de todos los usuarios con únicamente su `idUsuario` y su `nick`. Endpoint optimizado para lookups rápidos en el frontend (autocompletados, listados, etc.).

## Autenticación

Requiere token JWT válido (`Authorization: Bearer <token>`).

## Request

**Método:** `GET`  
**URL:** `/api/Usuario/IdNick`  
**Body:** ninguno  
**Query params:** ninguno  

## Response

### 200 OK

Array de objetos con `idUsuario` y `nick`.

```json
[
  {
    "idUsuario": 1,
    "nick": "Aragorn"
  },
  {
    "idUsuario": 2,
    "nick": "Legolas"
  }
]
```

| Campo      | Tipo   | Descripción             |
|------------|--------|-------------------------|
| `idUsuario`| int    | Identificador del usuario |
| `nick`     | string | Nombre visible del usuario |

### 204 No Content

No hay usuarios en la base de datos.

### 401 Unauthorized

El token JWT no es válido o no se ha proporcionado.

### 500 Internal Server Error

```json
{ "message": "Ocurrió un error en el servidor." }
```

## Notas

- Este endpoint proyecta solo las dos columnas necesarias a nivel de base de datos, por lo que es más eficiente que `/api/Usuario` o `/api/Usuario/All`.
- Usar este endpoint cuando solo se necesite mostrar/seleccionar usuarios por nombre (p. ej. buscadores, dropdowns, emparejamientos).

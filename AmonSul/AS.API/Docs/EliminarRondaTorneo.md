# DELETE /api/Torneo/Gestion/{idTorneo}/Ronda/{idRonda}

Elimina todas las partidas de una ronda de un torneo. Solo se puede eliminar la última ronda generada.

## Autenticación

Requiere token JWT (`Authorization: Bearer <token>`) y que el usuario sea administrador del torneo.

## Parámetros de ruta

| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| `idTorneo` | `int` | ID del torneo |
| `idRonda`  | `int` | Número de la ronda a eliminar |

---

## Respuestas

### `200 OK`

La ronda ha sido eliminada correctamente.

```
"La ronda 3 del torneo ha sido eliminada con éxito"
```

### `400 Bad Request` — Ronda no existe

```
"La ronda 2 no existe en el torneo."
```

### `400 Bad Request` — No es la última ronda

```
"Solo se puede eliminar la última ronda. La ronda 1 no es la última (ronda actual: 3)."
```

### `401 Unauthorized`

Token JWT ausente o inválido.

### `403 Forbidden`

El usuario autenticado no es administrador del torneo.

---

## Implementación frontend (Angular / TypeScript)

### 1. Servicio

```typescript
deleteRonda(idTorneo: number, idRonda: number): Observable<string> {
  return this.http.delete(
    `${this.apiUrl}/Torneo/Gestion/${idTorneo}/Ronda/${idRonda}`,
    { responseType: 'text' }
  );
}
```

> `responseType: 'text'` es necesario porque el backend devuelve un string plano, no JSON.

### 2. Llamada en el componente

```typescript
eliminarRonda(idTorneo: number, idRonda: number): void {
  this.torneoService.deleteRonda(idTorneo, idRonda).subscribe({
    next: (msg) => {
      // Notificar al usuario y recargar el estado del torneo
      this.toastService.success(msg);
      this.cargarPartidas();
    },
    error: (err) => {
      // El backend devuelve el mensaje de error en el body
      const mensaje = err.error ?? 'No se ha podido eliminar la ronda';
      this.toastService.error(mensaje);
    }
  });
}
```

### 3. Manejo de errores HTTP

| Código | Causa                                      | Acción sugerida                                              |
|--------|--------------------------------------------|--------------------------------------------------------------|
| `400`  | Ronda inexistente o no es la última        | Mostrar `err.error` directamente como mensaje al usuario     |
| `401`  | Token expirado / no autenticado            | Redirigir al login                                           |
| `403`  | No es administrador del torneo             | Mostrar mensaje de permiso denegado                          |

---

## Notas

- Solo se puede eliminar la ronda con el número más alto del torneo. Si la ronda 3 existe, no se puede eliminar la 1 ni la 2.
- La operación es irreversible: elimina físicamente todas las partidas de esa ronda.
- Tras eliminar, el torneo queda en estado de ronda anterior lista para volver a generar emparejamientos.

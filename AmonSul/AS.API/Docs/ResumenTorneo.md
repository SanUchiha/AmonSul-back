# GET /api/Torneo/resumen/{idTorneo}

Devuelve el estado actual de un torneo para pintarlo en la vista de resumen.

## Autenticación

Requiere token JWT (`Authorization: Bearer <token>`).

## Parámetros de ruta

| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| `idTorneo` | `int` | ID del torneo |

## Respuesta `200 OK`

```json
{
  "fecha": "2025-11-15",
  "listasVisibles": true,
  "numeroRondas": 4,
  "rondas": [true, true, false, false],
  "clasificacionVisible": false,
  "ganadores": ["NickJugador1", "NickJugador2", "NickJugador3"]
}
```

### Campos

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `fecha` | `string (DateOnly)` | Fecha de inicio del torneo (`YYYY-MM-DD`) |
| `listasVisibles` | `bool` | Si las listas de ejércitos son visibles para los jugadores |
| `numeroRondas` | `int` | Número total de rondas configuradas en el torneo |
| `rondas` | `bool[]` | Array de longitud `numeroRondas`. Cada posición indica si esa ronda ha sido generada (`true`) o no (`false`). El índice 0 corresponde a la Ronda 1 |
| `clasificacionVisible` | `bool` | Si la clasificación es visible para los jugadores |
| `ganadores` | `string[]` | Nicks de los 3 primeros clasificados, ordenados por posición (1º, 2º, 3º). En torneos por equipos (Parejas / Equipos_4 / Equipos_6) son los nombres de equipo. Array vacío si el torneo no ha finalizado aún |

## Casos de uso frontend

- **Rondas generadas:** iterar `rondas` para mostrar el estado de cada ronda. Ejemplo: `rondas[0]` → Ronda 1.
- **Ganadores pendientes:** si `ganadores` es un array vacío (`[]`) el torneo no tiene resultados guardados todavía.
- **Tipo de torneo:** el campo `ganadores` ya devuelve el nombre correcto según el tipo (nick individual o nombre de equipo), el frontal no necesita distinguir.

## Errores

| Código | Descripción |
|--------|-------------|
| `401` | Token no válido o ausente |
| `404` | Torneo no encontrado |

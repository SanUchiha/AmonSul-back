/*
    Rellena Elo.ID_Partida_Torneo en los ELO de torneos calculados antes de que se guardara la partida.

    REQUISITO: aplicar antes la migración AddIdPartidaTorneoToElo (crea la columna).

    CÓMO FUNCIONA
    GanadorApplication.ActualizarEloAsync, al guardar los resultados de un torneo, recorre sus partidas
    por ronda (y dentro de la ronda por ID_Partida_Torneo) e inserta seguidas una fila de ELO para el
    jugador 1 y otra para el jugador 2 de cada partida (nunca para el usuario 568, el bye).
    Así que cada torneo dejó en la tabla Elo un bloque de filas consecutivas cuyo orden de jugadores
    es conocido. El script busca, para cada torneo con resultados guardados, ese bloque exacto:
      - mismo orden de jugadores, fila a fila, sin huecos (por ID_Elo),
      - todas las filas sin ID_Partida_Torneo,
      - insertado en menos de @VentanaSegundos.
    Solo asigna los torneos con UN único bloque. El resto se lista y no se toca:
      - Sin bloque: no contó para el ELO (7 partidas o menos en la ronda 1), se calculó con otra regla
        (antes del 31/03/2025), otra inserción se coló en medio, o ya está asignado.
      - Varios bloques: los resultados se guardaron más de una vez.

    USO
      1. Ejecuta el script tal cual. Hace la asignación dentro de una transacción y al final ROLLBACK.
         Revisa los resultados:
           a) Resumen por torneo.
           b) Detalle de las filas que se asignarían.
           c) Comprobación: filas en las que el ganador pierde ELO o el perdedor lo gana. Debe salir vacía.
      2. Si todo cuadra, cambia el ROLLBACK final por COMMIT y vuelve a ejecutarlo.
    Es idempotente: solo rellena filas con ID_Partida_Torneo a NULL y nunca reasigna.
*/
SET NOCOUNT ON;

DECLARE @IdUsuarioBye int = 568;
DECLARE @VentanaSegundos int = 3600;

-- 1. ELO en orden de inserción
IF OBJECT_ID('tempdb..#EloOrden') IS NOT NULL DROP TABLE #EloOrden;
SELECT
    ROW_NUMBER() OVER (ORDER BY e.ID_Elo) AS G,
    e.ID_Elo,
    e.ID_Usuario,
    e.Puntuacion_Elo,
    e.Fecha_Elo,
    e.ID_Partida_Torneo
INTO #EloOrden
FROM Elo e;
CREATE UNIQUE CLUSTERED INDEX IX_EloOrden_G ON #EloOrden (G);
CREATE INDEX IX_EloOrden_Usuario ON #EloOrden (ID_Usuario) INCLUDE (ID_Partida_Torneo);

-- 2. Secuencia esperada por torneo, en el mismo orden que ActualizarEloAsync
IF OBJECT_ID('tempdb..#Esperado') IS NOT NULL DROP TABLE #Esperado;
WITH Partidas AS (
    SELECT
        p.ID_Torneo,
        p.ID_Partida_Torneo,
        p.ID_Usuario1,
        p.ID_Usuario2,
        ROW_NUMBER() OVER (
            PARTITION BY p.ID_Torneo
            ORDER BY p.Numero_Ronda, p.ID_Partida_Torneo
        ) AS OrdenPartida
    FROM Partida_Torneo p
    WHERE p.ID_Torneo IN (SELECT DISTINCT g.Id_Torneo FROM Ganador g)
),
Filas AS (
    SELECT ID_Torneo, ID_Partida_Torneo, OrdenPartida, 1 AS Lado, ID_Usuario1 AS ID_Usuario FROM Partidas
    UNION ALL
    SELECT ID_Torneo, ID_Partida_Torneo, OrdenPartida, 2 AS Lado, ID_Usuario2 AS ID_Usuario FROM Partidas
)
SELECT
    ID_Torneo,
    ID_Partida_Torneo,
    ID_Usuario,
    ROW_NUMBER() OVER (PARTITION BY ID_Torneo ORDER BY OrdenPartida, Lado) AS Pos
INTO #Esperado
FROM Filas
WHERE ID_Usuario <> @IdUsuarioBye;
CREATE UNIQUE CLUSTERED INDEX IX_Esperado ON #Esperado (ID_Torneo, Pos);

IF OBJECT_ID('tempdb..#Tamano') IS NOT NULL DROP TABLE #Tamano;
SELECT ID_Torneo, COUNT(*) AS R
INTO #Tamano
FROM #Esperado
GROUP BY ID_Torneo;

-- 3. Bloques: empiezan por el primer jugador esperado y coinciden fila a fila
IF OBJECT_ID('tempdb..#Bloques') IS NOT NULL DROP TABLE #Bloques;
SELECT c.ID_Torneo, c.G AS GInicio
INTO #Bloques
FROM (
    SELECT e1.ID_Torneo, o.G
    FROM #Esperado e1
    JOIN #EloOrden o
        ON o.ID_Usuario = e1.ID_Usuario
        AND o.ID_Partida_Torneo IS NULL
    WHERE e1.Pos = 1
) c
JOIN #Tamano t ON t.ID_Torneo = c.ID_Torneo
JOIN #EloOrden primera ON primera.G = c.G
JOIN #EloOrden ultima ON ultima.G = c.G + t.R - 1
WHERE
    (
        SELECT COUNT(*)
        FROM #Esperado e
        JOIN #EloOrden o ON o.G = c.G + e.Pos - 1
        WHERE e.ID_Torneo = c.ID_Torneo
            AND o.ID_Usuario = e.ID_Usuario
            AND o.ID_Partida_Torneo IS NULL
    ) = t.R
    AND DATEDIFF(SECOND, primera.Fecha_Elo, ultima.Fecha_Elo) BETWEEN 0 AND @VentanaSegundos;

-- 4. Resumen por torneo
IF OBJECT_ID('tempdb..#Resumen') IS NOT NULL DROP TABLE #Resumen;
SELECT
    t.ID_Torneo,
    t.R AS FilasEsperadas,
    COUNT(b.GInicio) AS BloquesEncontrados,
    -- varchar(50): luego puede pasar a 'Conflicto con otro torneo', más largo que estos textos
    CAST(
        CASE COUNT(b.GInicio)
            WHEN 1 THEN 'OK'
            WHEN 0 THEN 'Sin bloque'
            ELSE 'Varios bloques'
        END AS varchar(50)
    ) AS Estado,
    MIN(b.GInicio) AS GInicio
INTO #Resumen
FROM #Tamano t
LEFT JOIN #Bloques b ON b.ID_Torneo = t.ID_Torneo
GROUP BY t.ID_Torneo, t.R;

-- 5. Filas a asignar (solo torneos OK)
IF OBJECT_ID('tempdb..#Asignacion') IS NOT NULL DROP TABLE #Asignacion;
SELECT r.ID_Torneo, o.ID_Elo, o.ID_Usuario, o.Puntuacion_Elo, o.Fecha_Elo, e.ID_Partida_Torneo
INTO #Asignacion
FROM #Resumen r
JOIN #Esperado e ON e.ID_Torneo = r.ID_Torneo
JOIN #EloOrden o ON o.G = r.GInicio + e.Pos - 1
WHERE r.Estado = 'OK';

-- Por seguridad: si una fila de ELO encajara en dos torneos, no se asigna ninguno de los dos
UPDATE r SET Estado = 'Conflicto con otro torneo'
FROM #Resumen r
WHERE r.ID_Torneo IN (
    SELECT a.ID_Torneo
    FROM #Asignacion a
    WHERE a.ID_Elo IN (SELECT ID_Elo FROM #Asignacion GROUP BY ID_Elo HAVING COUNT(*) > 1)
);
DELETE a
FROM #Asignacion a
JOIN #Resumen r ON r.ID_Torneo = a.ID_Torneo
WHERE r.Estado <> 'OK';

-- a) Resumen por torneo
SELECT r.ID_Torneo, tn.Nombre_Torneo, r.FilasEsperadas, r.BloquesEncontrados, r.Estado
FROM #Resumen r
JOIN Torneo tn ON tn.ID_Torneo = r.ID_Torneo
ORDER BY r.Estado, r.ID_Torneo;

-- b) Detalle de lo que se asignaría
SELECT a.ID_Torneo, a.ID_Elo, a.ID_Usuario, a.Fecha_Elo, a.ID_Partida_Torneo
FROM #Asignacion a
ORDER BY a.ID_Elo;

-- c) Comprobación: el ganador no puede perder ELO ni el perdedor ganarlo (debe salir vacía)
SELECT a.ID_Torneo, a.ID_Elo, a.ID_Usuario, a.ID_Partida_Torneo, p.Ganador_Partida_Torneo,
    a.Puntuacion_Elo - ISNULL(anterior.Puntuacion_Elo, 800) AS Variacion
FROM #Asignacion a
JOIN Partida_Torneo p ON p.ID_Partida_Torneo = a.ID_Partida_Torneo
OUTER APPLY (
    SELECT TOP 1 prev.Puntuacion_Elo
    FROM Elo prev
    WHERE prev.ID_Usuario = a.ID_Usuario AND prev.ID_Elo < a.ID_Elo
    ORDER BY prev.ID_Elo DESC
) anterior
WHERE p.Ganador_Partida_Torneo IS NOT NULL
    AND (
        (p.Ganador_Partida_Torneo = a.ID_Usuario AND a.Puntuacion_Elo < ISNULL(anterior.Puntuacion_Elo, 800))
        OR (p.Ganador_Partida_Torneo <> a.ID_Usuario AND a.Puntuacion_Elo > ISNULL(anterior.Puntuacion_Elo, 800))
    );

-- 6. Asignación
BEGIN TRANSACTION;

UPDATE e
SET e.ID_Partida_Torneo = a.ID_Partida_Torneo
FROM Elo e
JOIN #Asignacion a ON a.ID_Elo = e.ID_Elo
WHERE e.ID_Partida_Torneo IS NULL;

DECLARE @FilasActualizadas int = @@ROWCOUNT;
PRINT CONCAT('Filas de ELO actualizadas: ', @FilasActualizadas);

-- Revisa los resultados. Si todo cuadra, cambia ROLLBACK por COMMIT y vuelve a ejecutar el script.
ROLLBACK TRANSACTION;
-- COMMIT TRANSACTION;

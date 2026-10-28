/*
    Corrige el ejército guardado en las partidas de la ronda 1 de los torneos 115 y 180.

    GenerarPrimeraRondaNoPermiteMismaComunidadAsync ponía a los dos jugadores de cada partida el ejército
    de la última inscripción con lista, por un if sin llaves. En esos dos torneos casi todas las partidas de
    la ronda 1 tienen el mismo ejército en los dos lados. Las demás rondas no usan ese código.

    A cada jugador se le pone el ejército de su lista, solo si todas sus listas del torneo son del mismo
    ejército: con varios no se sabe con cuál jugó y se deja como está.

    USO
      1. Ejecútalo tal cual: muestra el antes y el después, hace los UPDATE y al final ROLLBACK.
      2. Si cuadra, cambia el ROLLBACK final por COMMIT y vuelve a ejecutarlo.
    Es idempotente: una segunda ejecución no encuentra nada que cambiar.
*/
SET NOCOUNT ON;

-- Ejército de cada jugador en el torneo, cuando es uno solo
SELECT i.ID_Torneo, i.ID_Usuario, MIN(l.Ejercito) AS Ejercito
INTO #EjercitoUnico
FROM Inscripcion_Torneo i
JOIN Lista l ON l.ID_Inscripcion = i.ID_Inscripcion
WHERE i.ID_Torneo IN (115, 180)
GROUP BY i.ID_Torneo, i.ID_Usuario
HAVING COUNT(DISTINCT l.Ejercito) = 1;

-- Partidas que cambian
SELECT pt.ID_Torneo, pt.ID_Partida_Torneo,
       pt.Ejercito_Usuario1 AS Antes1, ISNULL(e1.Ejercito, pt.Ejercito_Usuario1) AS Despues1,
       pt.Ejercito_Usuario2 AS Antes2, ISNULL(e2.Ejercito, pt.Ejercito_Usuario2) AS Despues2
FROM Partida_Torneo pt
LEFT JOIN #EjercitoUnico e1 ON e1.ID_Torneo = pt.ID_Torneo AND e1.ID_Usuario = pt.ID_Usuario1
LEFT JOIN #EjercitoUnico e2 ON e2.ID_Torneo = pt.ID_Torneo AND e2.ID_Usuario = pt.ID_Usuario2
WHERE pt.ID_Torneo IN (115, 180) AND pt.Numero_Ronda = 1
  AND (ISNULL(pt.Ejercito_Usuario1, '') <> ISNULL(e1.Ejercito, ISNULL(pt.Ejercito_Usuario1, ''))
    OR ISNULL(pt.Ejercito_Usuario2, '') <> ISNULL(e2.Ejercito, ISNULL(pt.Ejercito_Usuario2, '')))
ORDER BY pt.ID_Torneo, pt.ID_Partida_Torneo;

BEGIN TRANSACTION;

UPDATE pt
SET Ejercito_Usuario1 = e.Ejercito
FROM Partida_Torneo pt
JOIN #EjercitoUnico e ON e.ID_Torneo = pt.ID_Torneo AND e.ID_Usuario = pt.ID_Usuario1
WHERE pt.ID_Torneo IN (115, 180) AND pt.Numero_Ronda = 1
  AND ISNULL(pt.Ejercito_Usuario1, '') <> e.Ejercito;

PRINT CONCAT('Ejércitos del jugador 1 corregidos: ', @@ROWCOUNT);

UPDATE pt
SET Ejercito_Usuario2 = e.Ejercito
FROM Partida_Torneo pt
JOIN #EjercitoUnico e ON e.ID_Torneo = pt.ID_Torneo AND e.ID_Usuario = pt.ID_Usuario2
WHERE pt.ID_Torneo IN (115, 180) AND pt.Numero_Ronda = 1
  AND ISNULL(pt.Ejercito_Usuario2, '') <> e.Ejercito;

PRINT CONCAT('Ejércitos del jugador 2 corregidos: ', @@ROWCOUNT);

-- Comprobación: partidas de la ronda 1 que siguen con el mismo ejército en los dos lados
SELECT ID_Torneo, COUNT(*) AS Partidas,
       SUM(CASE WHEN Ejercito_Usuario1 = Ejercito_Usuario2 THEN 1 ELSE 0 END) AS MismoEjercito
FROM Partida_Torneo
WHERE ID_Torneo IN (115, 180) AND Numero_Ronda = 1
GROUP BY ID_Torneo;

-- Revisa los resultados. Si todo cuadra, cambia ROLLBACK por COMMIT y vuelve a ejecutar el script.
ROLLBACK TRANSACTION;
-- COMMIT TRANSACTION;

DROP TABLE #EjercitoUnico;

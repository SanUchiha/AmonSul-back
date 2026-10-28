/*
    Rellena el ejército vacío de las partidas de los torneos 200, 206 y 218.

    En esos torneos las listas se subieron después de generar las rondas y las partidas quedaron sin
    ejército. Solo se rellenan los vacíos, en cualquier ronda, con el ejército de la lista del jugador
    y solo si todas sus listas del torneo son del mismo ejército. Un ejército ya guardado no se toca.

    USO
      1. Ejecútalo tal cual: muestra lo que cambia, hace los UPDATE y al final ROLLBACK.
      2. Si cuadra, cambia el ROLLBACK final por COMMIT y vuelve a ejecutarlo.
    Es idempotente: una segunda ejecución no encuentra nada que rellenar.
*/
SET NOCOUNT ON;

-- Ejército de cada jugador en el torneo, cuando es uno solo
SELECT i.ID_Torneo, i.ID_Usuario, MIN(l.Ejercito) AS Ejercito
INTO #EjercitoUnico
FROM Inscripcion_Torneo i
JOIN Lista l ON l.ID_Inscripcion = i.ID_Inscripcion
WHERE i.ID_Torneo IN (200, 206, 218)
GROUP BY i.ID_Torneo, i.ID_Usuario
HAVING COUNT(DISTINCT l.Ejercito) = 1;

-- Partidas que se rellenan, por torneo y ronda
SELECT pt.ID_Torneo, pt.Numero_Ronda,
       SUM(CASE WHEN ISNULL(pt.Ejercito_Usuario1, '') = '' AND e1.Ejercito IS NOT NULL THEN 1 ELSE 0 END) AS Jugador1,
       SUM(CASE WHEN ISNULL(pt.Ejercito_Usuario2, '') = '' AND e2.Ejercito IS NOT NULL THEN 1 ELSE 0 END) AS Jugador2
FROM Partida_Torneo pt
LEFT JOIN #EjercitoUnico e1 ON e1.ID_Torneo = pt.ID_Torneo AND e1.ID_Usuario = pt.ID_Usuario1
LEFT JOIN #EjercitoUnico e2 ON e2.ID_Torneo = pt.ID_Torneo AND e2.ID_Usuario = pt.ID_Usuario2
WHERE pt.ID_Torneo IN (200, 206, 218)
GROUP BY pt.ID_Torneo, pt.Numero_Ronda
ORDER BY pt.ID_Torneo, pt.Numero_Ronda;

BEGIN TRANSACTION;

UPDATE pt
SET Ejercito_Usuario1 = e.Ejercito
FROM Partida_Torneo pt
JOIN #EjercitoUnico e ON e.ID_Torneo = pt.ID_Torneo AND e.ID_Usuario = pt.ID_Usuario1
WHERE pt.ID_Torneo IN (200, 206, 218) AND ISNULL(pt.Ejercito_Usuario1, '') = '';

PRINT CONCAT('Ejércitos del jugador 1 rellenados: ', @@ROWCOUNT);

UPDATE pt
SET Ejercito_Usuario2 = e.Ejercito
FROM Partida_Torneo pt
JOIN #EjercitoUnico e ON e.ID_Torneo = pt.ID_Torneo AND e.ID_Usuario = pt.ID_Usuario2
WHERE pt.ID_Torneo IN (200, 206, 218) AND ISNULL(pt.Ejercito_Usuario2, '') = '';

PRINT CONCAT('Ejércitos del jugador 2 rellenados: ', @@ROWCOUNT);

-- Comprobación: vacíos que quedan, de jugadores sin lista o con listas de varios ejércitos
SELECT ID_Torneo,
       SUM(CASE WHEN ISNULL(Ejercito_Usuario1, '') = '' THEN 1 ELSE 0 END)
     + SUM(CASE WHEN ISNULL(Ejercito_Usuario2, '') = '' THEN 1 ELSE 0 END) AS SiguenVacios
FROM Partida_Torneo
WHERE ID_Torneo IN (200, 206, 218)
GROUP BY ID_Torneo;

-- Revisa los resultados. Si todo cuadra, cambia ROLLBACK por COMMIT y vuelve a ejecutar el script.
ROLLBACK TRANSACTION;
-- COMMIT TRANSACTION;

DROP TABLE #EjercitoUnico;

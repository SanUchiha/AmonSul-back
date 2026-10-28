/*
    Renombra el tipo de torneo 'Parejas' a 'Equipos_2'.

    'Parejas' eran en realidad equipos de 2: cada enfrentamiento genera una partida individual por jugador.
    El nombre queda libre para el tipo Parejas de verdad (2 contra 2 en la misma mesa).

    REQUISITO: ejecutarlo DESPUÉS de desplegar el back y el front que entienden 'Equipos_2'.
    Ese código acepta también 'Parejas', así que el orden no rompe nada mientras tanto.

    USO
      1. Ejecútalo tal cual: muestra los torneos afectados, hace el UPDATE y al final ROLLBACK.
      2. Si la lista cuadra, cambia el ROLLBACK final por COMMIT y vuelve a ejecutarlo.
    Es idempotente: una segunda ejecución no encuentra ningún torneo 'Parejas'.
*/
SET NOCOUNT ON;

-- Torneos que se van a renombrar
SELECT ID_Torneo, Nombre_Torneo, Tipo_Torneo, Fecha_Inicio_Torneo
FROM Torneo
WHERE Tipo_Torneo = 'Parejas'
ORDER BY ID_Torneo;

BEGIN TRANSACTION;

UPDATE Torneo
SET Tipo_Torneo = 'Equipos_2'
WHERE Tipo_Torneo = 'Parejas';

PRINT CONCAT('Torneos renombrados: ', @@ROWCOUNT);

-- Revisa los resultados. Si todo cuadra, cambia ROLLBACK por COMMIT y vuelve a ejecutar el script.
ROLLBACK TRANSACTION;
-- COMMIT TRANSACTION;

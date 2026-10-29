namespace AS.Utils.Statics;

/// <summary>
/// Calendario de todos contra todos por el método del círculo. No depende de la base de datos ni del tipo de
/// participante: sirve para una liga y, más adelante, para cada grupo de un mundial
/// </summary>
public static class RoundRobin
{
    /// <summary>
    /// Jornadas en las que cada participante se enfrenta una vez a cada uno de los demás. Con un número impar,
    /// en cada jornada descansa uno. Con ida y vuelta se repiten las jornadas con los lados cambiados
    /// </summary>
    public static List<List<(T Local, T Visitante)>> GenerarJornadas<T>(
        IReadOnlyList<T> participantes,
        bool idaYVuelta = false
    )
    {
        if (participantes.Count < 2)
            return [];

        int n = participantes.Count;
        bool impares = n % 2 == 1;

        // Con impares se añade un hueco: a quien le toca contra él, descansa esa jornada
        List<int?> posiciones = [.. Enumerable.Range(0, n).Select(i => (int?)i)];
        if (impares)
            posiciones.Add(null);
        int total = posiciones.Count;

        List<List<(T Local, T Visitante)>> ida = [];
        for (int jornada = 0; jornada < total - 1; jornada++)
        {
            List<(T Local, T Visitante)> partidas = [];
            for (int k = 0; k < total / 2; k++)
            {
                int? a = posiciones[k];
                int? b = posiciones[total - 1 - k];
                if (a is not int i || b is not int j)
                    continue;

                // Con impares cada uno es local justo la mitad de veces: i lo es si j está en la mitad siguiente del círculo.
                // Con pares no se puede cuadrar del todo: el fijo alterna cada jornada y el resto por mesa
                bool cambiarLados = impares
                    ? (j - i + n) % n > n / 2
                    : k == 0 ? jornada % 2 == 1 : k % 2 == 1;
                partidas.Add(
                    cambiarLados ? (participantes[j], participantes[i]) : (participantes[i], participantes[j])
                );
            }
            ida.Add(partidas);

            // Gira todos menos el primero
            int? ultimo = posiciones[total - 1];
            posiciones.RemoveAt(total - 1);
            posiciones.Insert(1, ultimo);
        }

        if (!idaYVuelta)
            return ida;

        return [.. ida, .. ida.Select(j => j.Select(p => (Local: p.Visitante, Visitante: p.Local)).ToList())];
    }
}

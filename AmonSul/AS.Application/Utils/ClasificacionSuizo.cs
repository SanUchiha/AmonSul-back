using AS.Domain.Models;
using AS.Domain.Types.Torneo;

namespace AS.Application.Utils;

/// <summary>
/// Clasificación final de un torneo suizo, la misma que guarda el organizador desde la gestión del torneo en el front:
/// solo cuentan las partidas que han validado los dos; en el tipo normal manda el ganador de la partida y en los otros
/// dos, el resultado. Desempate: puntos, diferencia, puntos a favor y generales matados; si todo empata, el id de
/// usuario más bajo, que es el orden en que el front los recorre
/// </summary>
internal static class ClasificacionSuizo
{
    // El bye no es un jugador: no sale en la clasificación
    private const int IdUsuarioBye = 568;

    private class Fila
    {
        public required int IdUsuario { get; init; }
        public int Puntos { get; set; }
        public int PuntosFavor { get; set; }
        public int PuntosContra { get; set; }
        public int Generales { get; set; }
        public int Diferencia => PuntosFavor - PuntosContra;
    }

    private static readonly IComparer<Fila> Comparador = Comparer<Fila>.Create(
        (a, b) =>
        {
            int[] diferencias =
            [
                b.Puntos.CompareTo(a.Puntos),
                b.Diferencia.CompareTo(a.Diferencia),
                b.PuntosFavor.CompareTo(a.PuntosFavor),
                b.Generales.CompareTo(a.Generales),
                a.IdUsuario.CompareTo(b.IdUsuario),
            ];
            return diferencias.FirstOrDefault(d => d != 0);
        }
    );

    /// <summary>
    /// Los jugadores del primero al último. Por equipos cada jugador puntúa sus partidas, con la escala de equipos en
    /// el tipo extendido. Los puntos extra van por jugador
    /// </summary>
    internal static List<int> Clasificar(
        IEnumerable<PartidaTorneo> partidas,
        ClassificationType tipo,
        bool porEquipos,
        IReadOnlyDictionary<int, int> puntosExtra
    )
    {
        Dictionary<int, Fila> filas = [];
        Fila FilaDe(int idUsuario) =>
            filas.TryGetValue(idUsuario, out Fila? fila) ? fila : filas[idUsuario] = new Fila { IdUsuario = idUsuario };

        foreach (PartidaTorneo partida in partidas.Where(ClasificacionJornadas.Jugada))
        {
            Fila fila1 = FilaDe(partida.IdUsuario1);
            Fila fila2 = FilaDe(partida.IdUsuario2);
            int resultado1 = partida.ResultadoUsuario1 ?? 0;
            int resultado2 = partida.ResultadoUsuario2 ?? 0;
            (int puntos1, int puntos2) = Puntos(partida, resultado1, resultado2, tipo, porEquipos);

            fila1.Puntos += puntos1;
            fila1.PuntosFavor += resultado1;
            fila1.PuntosContra += resultado2;
            if (partida.LiderMuertoUsuario1 == true)
                fila1.Generales++;

            fila2.Puntos += puntos2;
            fila2.PuntosFavor += resultado2;
            fila2.PuntosContra += resultado1;
            if (partida.LiderMuertoUsuario2 == true)
                fila2.Generales++;
        }

        foreach (Fila fila in filas.Values)
            fila.Puntos += puntosExtra.GetValueOrDefault(fila.IdUsuario);

        return [.. filas.Values.Where(f => f.IdUsuario != IdUsuarioBye).Order(Comparador).Select(f => f.IdUsuario)];
    }

    private static (int, int) Puntos(
        PartidaTorneo partida,
        int resultado1,
        int resultado2,
        ClassificationType tipo,
        bool porEquipos
    )
    {
        if (tipo == ClassificationType.ALEMAN)
        {
            // La suma de los dos es 20: diez cada uno y medio punto por cada punto de diferencia, redondeando arriba
            if (resultado1 == resultado2)
                return (10, 10);
            int puntos = (int)Math.Ceiling(Math.Abs(resultado1 - resultado2) / 2.0);
            return resultado1 > resultado2 ? (10 + puntos, 10 - puntos) : (10 - puntos, 10 + puntos);
        }
        if (tipo == ClassificationType.EXTENDED)
        {
            // Victoria amplia: al menos el doble que el rival y como mínimo 6
            if (resultado1 == resultado2)
                return (2, 2);
            int ganador = Math.Max(resultado1, resultado2);
            int perdedor = Math.Min(resultado1, resultado2);
            bool amplia = ganador >= 2 * perdedor && ganador >= 6;
            (int gana, int pierde) = (amplia, porEquipos) switch
            {
                (true, false) => (6, 0),
                (false, false) => (5, 1),
                (true, true) => (4, 0),
                (false, true) => (3, 1),
            };
            return resultado1 > resultado2 ? (gana, pierde) : (pierde, gana);
        }
        // Normal: 3 al ganador y, si no lo hay, 1 a cada uno
        if (partida.GanadorPartidaTorneo == partida.IdUsuario1)
            return (3, 0);
        if (partida.GanadorPartidaTorneo == partida.IdUsuario2)
            return (0, 3);
        return (1, 1);
    }
}

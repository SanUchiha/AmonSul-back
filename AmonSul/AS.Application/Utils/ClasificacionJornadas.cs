using AS.Domain.Models;
using AS.Domain.Types.Torneo;

namespace AS.Application.Utils;

/// <summary>
/// Clasificación de un todos contra todos (un grupo de la Copa), con la misma regla que la liga en el front: solo
/// cuentan las partidas que han validado los dos, los puntos salen del tipo de clasificación del torneo y el desempate
/// es puntos, diferencia, puntos a favor y generales matados
/// </summary>
internal static class ClasificacionJornadas
{
    internal class Fila
    {
        public required InscripcionTorneo Inscripcion { get; init; }
        public int Jugadas { get; set; }
        public int Victorias { get; set; }
        public int Empates { get; set; }
        public int Derrotas { get; set; }
        public int Puntos { get; set; }
        public int PuntosFavor { get; set; }
        public int PuntosContra { get; set; }
        public int Generales { get; set; }
        public int Diferencia => PuntosFavor - PuntosContra;
        // En su grupo, empezando en 1
        public int Posicion { get; set; }
    }

    // El mejor primero; si todo empata, la inscripción más antigua, para que el orden no cambie entre llamadas
    internal static readonly IComparer<Fila> Comparador = Comparer<Fila>.Create(
        (a, b) =>
        {
            int[] diferencias =
            [
                b.Puntos.CompareTo(a.Puntos),
                b.Diferencia.CompareTo(a.Diferencia),
                b.PuntosFavor.CompareTo(a.PuntosFavor),
                b.Generales.CompareTo(a.Generales),
                a.Inscripcion.IdInscripcion.CompareTo(b.Inscripcion.IdInscripcion),
            ];
            return diferencias.FirstOrDefault(d => d != 0);
        }
    );

    internal static bool Jugada(PartidaTorneo partida) =>
        partida.PartidaValidadaUsuario1 == true && partida.PartidaValidadaUsuario2 == true;

    internal static List<Fila> Clasificar(
        IEnumerable<InscripcionTorneo> jugadores,
        IEnumerable<PartidaTorneo> partidas,
        ClassificationType tipo
    )
    {
        Dictionary<int, Fila> filas = jugadores.ToDictionary(j => j.IdUsuario, j => new Fila { Inscripcion = j });
        foreach (PartidaTorneo partida in partidas.Where(Jugada))
        {
            if (!filas.TryGetValue(partida.IdUsuario1, out Fila? fila1) || !filas.TryGetValue(partida.IdUsuario2, out Fila? fila2))
                continue;
            int resultado1 = partida.ResultadoUsuario1 ?? 0;
            int resultado2 = partida.ResultadoUsuario2 ?? 0;
            (int puntos1, int puntos2) = Puntos(resultado1, resultado2, tipo);
            Sumar(fila1, resultado1, resultado2, puntos1, partida.LiderMuertoUsuario1 == true);
            Sumar(fila2, resultado2, resultado1, puntos2, partida.LiderMuertoUsuario2 == true);
        }

        // Puntos extra que da el organizador
        foreach (Fila fila in filas.Values)
            fila.Puntos += fila.Inscripcion.PuntosExtra;

        List<Fila> orden = [.. filas.Values.Order(Comparador)];
        for (int i = 0; i < orden.Count; i++)
            orden[i].Posicion = i + 1;
        return orden;
    }

    private static void Sumar(Fila fila, int aFavor, int enContra, int puntos, bool generalMatado)
    {
        fila.Jugadas++;
        fila.Puntos += puntos;
        fila.PuntosFavor += aFavor;
        fila.PuntosContra += enContra;
        if (aFavor > enContra)
            fila.Victorias++;
        else if (aFavor < enContra)
            fila.Derrotas++;
        else
            fila.Empates++;
        if (generalMatado)
            fila.Generales++;
    }

    private static (int, int) Puntos(int resultado1, int resultado2, ClassificationType tipo)
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
            if (resultado1 == resultado2)
                return (2, 2);
            int ganador = Math.Max(resultado1, resultado2);
            int perdedor = Math.Min(resultado1, resultado2);
            bool amplia = ganador >= 2 * perdedor && ganador >= 6;
            (int gana, int pierde) = amplia ? (6, 0) : (5, 1);
            return resultado1 > resultado2 ? (gana, pierde) : (pierde, gana);
        }
        if (resultado1 == resultado2)
            return (1, 1);
        return resultado1 > resultado2 ? (3, 0) : (0, 3);
    }
}

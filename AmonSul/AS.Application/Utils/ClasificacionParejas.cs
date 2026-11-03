using AS.Domain.DTOs.Equipo;
using AS.Domain.Models;
using AS.Domain.Types.Torneo;

namespace AS.Application.Utils;

/// <summary>
/// Clasificación final de un torneo a Parejas, la misma que guarda el organizador desde la gestión en el front: cada
/// partida es un enfrentamiento entero entre dos parejas y puntúa por el resultado, con la escala individual en el tipo
/// extendido. Cada pareja empieza con los puntos extra de sus dos miembros. Desempate: puntos, diferencia, puntos a
/// favor y generales matados; si todo empata, la pareja que se apuntó antes
/// </summary>
internal static class ClasificacionParejas
{
    private class Fila
    {
        public required EquipoDTO Pareja { get; init; }
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
                a.Pareja.IdEquipo.CompareTo(b.Pareja.IdEquipo),
            ];
            return diferencias.FirstOrDefault(d => d != 0);
        }
    );

    /// <summary>
    /// Las parejas de la primera a la última. El bye no es una pareja: ganarle suma, pero no sale en la clasificación
    /// </summary>
    internal static List<EquipoDTO> Clasificar(
        IEnumerable<EquipoDTO> parejas,
        IEnumerable<PartidaTorneo> partidas,
        ClassificationType tipo
    )
    {
        Dictionary<int, Fila> filas = parejas.ToDictionary(
            p => p.IdEquipo,
            p => new Fila { Pareja = p, Puntos = p.Inscripciones.Sum(i => i.PuntosExtra) }
        );

        foreach (PartidaTorneo partida in partidas.Where(ClasificacionJornadas.Jugada))
        {
            if (partida.IdEquipo1 is null || partida.IdEquipo2 is null)
                continue;
            Fila? fila1 = filas.GetValueOrDefault(partida.IdEquipo1.Value);
            Fila? fila2 = filas.GetValueOrDefault(partida.IdEquipo2.Value);
            int resultado1 = partida.ResultadoUsuario1 ?? 0;
            int resultado2 = partida.ResultadoUsuario2 ?? 0;
            (int puntos1, int puntos2) = Puntos(resultado1, resultado2, tipo);
            Sumar(fila1, resultado1, resultado2, puntos1, partida.LiderMuertoUsuario1 == true);
            Sumar(fila2, resultado2, resultado1, puntos2, partida.LiderMuertoUsuario2 == true);
        }

        return [.. filas.Values.Order(Comparador).Select(f => f.Pareja)];
    }

    private static void Sumar(Fila? fila, int aFavor, int enContra, int puntos, bool generalMatado)
    {
        if (fila is null)
            return;
        fila.Puntos += puntos;
        fila.PuntosFavor += aFavor;
        fila.PuntosContra += enContra;
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
            // Victoria amplia: al menos el doble que el rival y como mínimo 6
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

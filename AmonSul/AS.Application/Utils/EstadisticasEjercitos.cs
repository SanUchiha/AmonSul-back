using System.Text;
using AS.Application.DTOs.Estadisticas;
using AS.Domain.DTOs.Estadisticas;

namespace AS.Application.Utils;

/// <summary>
/// Estadísticas de ejércitos a partir de las partidas validadas. Cada partida cuenta para los dos ejércitos, salvo las
/// espejo (el mismo ejército en los dos lados), que no dicen nada de él. El porcentaje cuenta el empate como media
/// victoria. «Rinde» es lo que se ha ganado de más o de menos frente a lo que esperaba el ELO de los dos jugadores al
/// empezar el día de la partida, en puntos de porcentaje: separa lo que es del ejército de lo que es de quien lo lleva
/// </summary>
internal static class EstadisticasEjercitos
{
    internal const string BIEN = "good";
    internal const string MAL = "evil";

    // Casi todo se juega a 800, 700, 600-650 o 500-550 puntos
    internal static readonly string[] Tramos = ["hasta-550", "600-650", "700-750", "800+"];

    internal static string? TramoDe(int? puntos) => puntos switch
    {
        null or <= 0 => null,
        <= 550 => "hasta-550",
        < 700 => "600-650",
        < 800 => "700-750",
        _ => "800+",
    };

    /// <summary>
    /// Une las formas de escribir un mismo ejército: sin mayúsculas, tildes, apóstrofos ni «The» delante. Vacía si no
    /// es un ejército, como «N/A»
    /// </summary>
    internal static string Clave(string nombre)
    {
        string texto = nombre.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        if (texto.StartsWith("the "))
            texto = texto[4..];
        StringBuilder clave = new();
        foreach (char c in texto)
            if (char.IsLetterOrDigit(c))
                clave.Append(c);
        return clave.ToString() == "na" ? "" : clave.ToString();
    }

    /// <summary>
    /// El nombre y el bando de cada ejército. El nombre es el más entregado en las listas o, si no está en ninguna, el
    /// más usado en las partidas; el bando, el de la mayoría de sus listas
    /// </summary>
    internal class Catalogo
    {
        private readonly Dictionary<string, string> _nombres = [];
        private readonly Dictionary<string, string> _bandos = [];

        internal Catalogo(IEnumerable<EjercitoBandoDTO> listas, IEnumerable<string> usados)
        {
            foreach (var ejercito in listas.GroupBy(l => Clave(l.Ejercito)).Where(g => g.Key != ""))
            {
                _nombres[ejercito.Key] = ejercito
                    .GroupBy(l => l.Ejercito.Trim())
                    .OrderByDescending(g => g.Sum(l => l.Listas))
                    .First().Key;
                string bando = ejercito
                    .GroupBy(l => l.Bando.Trim().ToLowerInvariant())
                    .OrderByDescending(g => g.Sum(l => l.Listas))
                    .First().Key;
                if (bando is BIEN or MAL)
                    _bandos[ejercito.Key] = bando;
            }
            foreach (var ejercito in usados.GroupBy(Clave).Where(g => g.Key != "" && !_nombres.ContainsKey(g.Key)))
                _nombres[ejercito.Key] = ejercito.GroupBy(n => n.Trim()).OrderByDescending(g => g.Count()).First().Key;
        }

        internal string? Nombre(string ejercito) => _nombres.GetValueOrDefault(Clave(ejercito));

        internal string? Bando(string ejercito) => _bandos.GetValueOrDefault(Clave(ejercito));
    }

    /// <summary>
    /// Una partida lista para contar: con los ejércitos unificados, su tramo de puntos y lo que esperaba el ELO
    /// </summary>
    internal class Partida
    {
        public required PartidaEstadisticaDTO Origen { get; init; }
        public required string Ejercito1 { get; init; }
        public required string Ejercito2 { get; init; }
        public string? Tramo { get; init; }
        // Lo que esperaba el ELO del jugador 1, entre 0 y 1
        public double Esperado1 { get; init; }
        public int Temporada => Origen.Fecha.Year;

        public double Puntuacion1 =>
            Origen.Ganador == Origen.IdUsuario1 ? 1
            : Origen.Ganador == Origen.IdUsuario2 ? 0
            : 0.5;
    }

    /// <summary>
    /// Una partida vista desde uno de los dos jugadores
    /// </summary>
    internal readonly record struct Lado(Partida Partida, bool EsPrimero)
    {
        public string Ejercito => EsPrimero ? Partida.Ejercito1 : Partida.Ejercito2;
        public string Rival => EsPrimero ? Partida.Ejercito2 : Partida.Ejercito1;
        public int IdUsuario => EsPrimero ? Partida.Origen.IdUsuario1 : Partida.Origen.IdUsuario2;
        public int IdUsuarioRival => EsPrimero ? Partida.Origen.IdUsuario2 : Partida.Origen.IdUsuario1;
        public double Puntuacion => EsPrimero ? Partida.Puntuacion1 : 1 - Partida.Puntuacion1;
        public double Esperado => EsPrimero ? Partida.Esperado1 : 1 - Partida.Esperado1;
        public int Favor => EsPrimero ? Partida.Origen.Resultado1 : Partida.Origen.Resultado2;
        public int Contra => EsPrimero ? Partida.Origen.Resultado2 : Partida.Origen.Resultado1;
        public bool General => EsPrimero ? Partida.Origen.General1 : Partida.Origen.General2;
    }

    internal static List<Partida> Preparar(
        IEnumerable<PartidaEstadisticaDTO> partidas,
        Catalogo catalogo,
        IEnumerable<EloFechaDTO> historial
    )
    {
        Dictionary<int, List<EloFechaDTO>> elos = historial
            .GroupBy(e => e.IdUsuario)
            .ToDictionary(g => g.Key, g => g.OrderBy(e => e.Fecha).ThenBy(e => e.IdElo).ToList());

        List<Partida> preparadas = [];
        foreach (PartidaEstadisticaDTO partida in partidas)
        {
            string? ejercito1 = catalogo.Nombre(partida.Ejercito1);
            string? ejercito2 = catalogo.Nombre(partida.Ejercito2);
            if (ejercito1 is null || ejercito2 is null)
                continue;

            int elo1 = EloAlEmpezarElDia(elos, partida.IdUsuario1, partida.Fecha);
            int elo2 = EloAlEmpezarElDia(elos, partida.IdUsuario2, partida.Fecha);
            preparadas.Add(new Partida
            {
                Origen = partida,
                Ejercito1 = ejercito1,
                Ejercito2 = ejercito2,
                Tramo = TramoDe(partida.Puntos),
                Esperado1 = 1.0 / (1 + Math.Pow(10, (elo2 - elo1) / 400.0)),
            });
        }
        return preparadas;
    }

    // La última fila de antes de ese día; si no tiene ninguna de antes, la primera que tenga, y sin historial, 800
    private static int EloAlEmpezarElDia(Dictionary<int, List<EloFechaDTO>> elos, int idUsuario, DateOnly fecha)
    {
        if (!elos.TryGetValue(idUsuario, out List<EloFechaDTO>? filas) || filas.Count == 0)
            return 800;

        DateTime inicio = fecha.ToDateTime(TimeOnly.MinValue);
        int desde = 0, hasta = filas.Count - 1, anterior = -1;
        while (desde <= hasta)
        {
            int medio = (desde + hasta) / 2;
            if (filas[medio].Fecha < inicio)
            {
                anterior = medio;
                desde = medio + 1;
            }
            else
                hasta = medio - 1;
        }
        return filas[Math.Max(anterior, 0)].Elo;
    }

    internal static IEnumerable<Partida> Filtrar(IEnumerable<Partida> partidas, FiltroEstadisticasDTO filtro) =>
        partidas.Where(p =>
            (filtro.Amistosas || !p.Origen.EsAmistosa)
            && (filtro.Temporada is null || p.Temporada == filtro.Temporada)
            && (string.IsNullOrEmpty(filtro.Tramo) || p.Tramo == filtro.Tramo)
        );

    // Los dos lados de cada partida, sin las espejo
    private static IEnumerable<Lado> Lados(IEnumerable<Partida> partidas) =>
        partidas.Where(p => p.Ejercito1 != p.Ejercito2).SelectMany(p => new[] { new Lado(p, true), new Lado(p, false) });

    private class Acumulado
    {
        public int Partidas, Victorias, Empates, Derrotas, Favor, Contra, DeTorneo, Generales;
        public double Puntuacion, Esperado;
        public HashSet<int> Jugadores { get; } = [];

        public void Sumar(Lado lado)
        {
            Partidas++;
            Puntuacion += lado.Puntuacion;
            Esperado += lado.Esperado;
            Favor += lado.Favor;
            Contra += lado.Contra;
            if (lado.Puntuacion == 1)
                Victorias++;
            else if (lado.Puntuacion == 0)
                Derrotas++;
            else
                Empates++;
            if (!lado.Partida.Origen.EsAmistosa)
            {
                DeTorneo++;
                if (lado.General)
                    Generales++;
            }
            Jugadores.Add(lado.IdUsuario);
        }

        public T Rellenar<T>(T balance)
            where T : BalanceDTO
        {
            balance.Partidas = Partidas;
            balance.Victorias = Victorias;
            balance.Empates = Empates;
            balance.Derrotas = Derrotas;
            balance.Porcentaje = Media(100 * Puntuacion);
            balance.Rinde = Media(100 * (Puntuacion - Esperado));
            balance.PuntosFavor = Media(Favor);
            balance.PuntosContra = Media(Contra);
            return balance;
        }

        private double Media(double total) => Partidas == 0 ? 0 : Math.Round(total / Partidas, 1);
    }

    private static Acumulado Acumular(IEnumerable<Lado> lados)
    {
        Acumulado acumulado = new();
        foreach (Lado lado in lados)
            acumulado.Sumar(lado);
        return acumulado;
    }

    private static EjercitoBalanceDTO FilaEjercito(string ejercito, Acumulado acumulado, Catalogo catalogo) =>
        acumulado.Rellenar(new EjercitoBalanceDTO
        {
            Ejercito = ejercito,
            Bando = catalogo.Bando(ejercito),
            Jugadores = acumulado.Jugadores.Count,
        });

    private static List<TramoBalanceDTO> PorTramo(List<Lado> lados) =>
        [.. Tramos.Select(t => Acumular(lados.Where(l => l.Partida.Tramo == t)).Rellenar(new TramoBalanceDTO { Tramo = t }))];

    /// <summary>
    /// Los ejércitos con al menos «minimo» partidas, del mejor porcentaje al peor. Con bando, solo los de ese bando
    /// </summary>
    internal static List<EjercitoBalanceDTO> Ranking(IEnumerable<Partida> partidas, Catalogo catalogo, string? bando, int minimo) =>
        [
            .. Lados(partidas)
                .GroupBy(l => l.Ejercito)
                .Select(g => FilaEjercito(g.Key, Acumular(g), catalogo))
                .Where(f => f.Partidas >= minimo && (string.IsNullOrEmpty(bando) || f.Bando == bando))
                .OrderByDescending(f => f.Porcentaje)
                .ThenByDescending(f => f.Partidas),
        ];

    internal static FichaEjercitoDTO Ficha(
        string ejercito,
        IEnumerable<Partida> partidas,
        Catalogo catalogo,
        IReadOnlyDictionary<int, string> nicks,
        int minimoRival,
        int minimoJugador
    )
    {
        List<Lado> suyos = [.. Lados(partidas).Where(l => l.Ejercito == ejercito)];
        Acumulado total = Acumular(suyos);
        return new FichaEjercitoDTO
        {
            Resumen = FilaEjercito(ejercito, total, catalogo),
            GeneralesMatados = total.DeTorneo == 0 ? null : Math.Round(100.0 * total.Generales / total.DeTorneo, 1),
            PorTramo = PorTramo(suyos),
            PorTemporada =
            [
                .. suyos
                    .GroupBy(l => l.Partida.Temporada)
                    .OrderByDescending(g => g.Key)
                    .Select(g => Acumular(g).Rellenar(new TemporadaBalanceDTO { Temporada = g.Key })),
            ],
            Rivales =
            [
                .. suyos
                    .GroupBy(l => l.Rival)
                    .Select(g => FilaEjercito(g.Key, Acumular(g), catalogo))
                    .Where(f => f.Partidas >= minimoRival)
                    .OrderByDescending(f => f.Porcentaje)
                    .ThenByDescending(f => f.Partidas),
            ],
            Jugadores =
            [
                .. suyos
                    .GroupBy(l => l.IdUsuario)
                    .Select(g => Acumular(g).Rellenar(new JugadorBalanceDTO
                    {
                        IdUsuario = g.Key,
                        Nick = nicks.GetValueOrDefault(g.Key, ""),
                    }))
                    .Where(j => j.Partidas >= minimoJugador)
                    .OrderByDescending(j => j.Rinde)
                    .ThenByDescending(j => j.Partidas),
            ],
        };
    }

    /// <summary>
    /// Un ejército contra otro o, sin rival, contra todos los de un bando
    /// </summary>
    internal static CruceEjercitosDTO Cruce(
        string ejercito,
        string? rival,
        string? bandoRival,
        IEnumerable<Partida> partidas,
        Catalogo catalogo,
        IReadOnlyDictionary<int, string> nicks,
        int maximoPartidas
    )
    {
        List<Lado> lados =
        [
            .. Lados(partidas).Where(l =>
                l.Ejercito == ejercito && (rival is not null ? l.Rival == rival : catalogo.Bando(l.Rival) == bandoRival)
            ),
        ];
        return new CruceEjercitosDTO
        {
            Ejercito = ejercito,
            Rival = rival ?? bandoRival ?? "",
            Resumen = Acumular(lados).Rellenar(new BalanceDTO()),
            PorTramo = PorTramo(lados),
            Partidas =
            [
                .. lados
                    .OrderByDescending(l => l.Partida.Origen.Fecha)
                    .ThenByDescending(l => l.Partida.Origen.IdPartida)
                    .Take(maximoPartidas)
                    .Select(l => new PartidaCruceDTO
                    {
                        IdPartida = l.Partida.Origen.IdPartida,
                        EsAmistosa = l.Partida.Origen.EsAmistosa,
                        IdTorneo = l.Partida.Origen.IdTorneo,
                        NombreTorneo = l.Partida.Origen.NombreTorneo,
                        Ronda = l.Partida.Origen.Ronda,
                        Fecha = l.Partida.Origen.Fecha,
                        Puntos = l.Partida.Origen.Puntos,
                        IdUsuario = l.IdUsuario,
                        Nick = nicks.GetValueOrDefault(l.IdUsuario, ""),
                        Ejercito = l.Ejercito,
                        Resultado = l.Favor,
                        IdUsuarioRival = l.IdUsuarioRival,
                        NickRival = nicks.GetValueOrDefault(l.IdUsuarioRival, ""),
                        EjercitoRival = l.Rival,
                        ResultadoRival = l.Contra,
                        Puntuacion = l.Puntuacion,
                    }),
            ],
        };
    }

    /// <summary>
    /// Los cruces más jugados y, entre los que tienen al menos «minimo» partidas, los más desequilibrados. Cada uno
    /// visto desde el ejército que se impone
    /// </summary>
    internal static CrucesDestacadosDTO Destacados(IEnumerable<Partida> partidas, int cuantos, int minimo)
    {
        List<CruceResumenDTO> cruces =
        [
            .. Lados(partidas)
                .Where(l => string.CompareOrdinal(l.Ejercito, l.Rival) < 0)
                .GroupBy(l => (l.Ejercito, l.Rival))
                .Select(g => DesdeElQueSeImpone(Acumular(g).Rellenar(new CruceResumenDTO
                {
                    Ejercito = g.Key.Ejercito,
                    Rival = g.Key.Rival,
                }))),
        ];
        return new CrucesDestacadosDTO
        {
            MasJugados = [.. cruces.OrderByDescending(c => c.Partidas).ThenByDescending(c => c.Porcentaje).Take(cuantos)],
            MasDesequilibrados =
            [
                .. cruces
                    .Where(c => c.Partidas >= minimo)
                    .OrderByDescending(c => c.Porcentaje)
                    .ThenByDescending(c => c.Partidas)
                    .Take(cuantos),
            ],
        };
    }

    private static CruceResumenDTO DesdeElQueSeImpone(CruceResumenDTO cruce) =>
        cruce.Porcentaje >= 50
            ? cruce
            : new CruceResumenDTO
            {
                Ejercito = cruce.Rival,
                Rival = cruce.Ejercito,
                Partidas = cruce.Partidas,
                Victorias = cruce.Derrotas,
                Empates = cruce.Empates,
                Derrotas = cruce.Victorias,
                Porcentaje = Math.Round(100 - cruce.Porcentaje, 1),
                Rinde = -cruce.Rinde,
                PuntosFavor = cruce.PuntosContra,
                PuntosContra = cruce.PuntosFavor,
            };

    internal static EjercitosJugadorDTO Jugador(
        int idUsuario,
        IEnumerable<Partida> partidas,
        Catalogo catalogo,
        IReadOnlyDictionary<int, string> nicks,
        int minimoRival,
        int cuantosRivales
    )
    {
        List<Lado> suyos = [.. Lados(partidas).Where(l => l.IdUsuario == idUsuario)];
        return new EjercitosJugadorDTO
        {
            IdUsuario = idUsuario,
            Nick = nicks.GetValueOrDefault(idUsuario, ""),
            Resumen = Acumular(suyos).Rellenar(new BalanceDTO()),
            Ejercitos =
            [
                .. suyos
                    .GroupBy(l => l.Ejercito)
                    .Select(g => FilaEjercito(g.Key, Acumular(g), catalogo))
                    .OrderByDescending(f => f.Partidas)
                    .ThenByDescending(f => f.Porcentaje),
            ],
            RivalesDificiles =
            [
                .. suyos
                    .GroupBy(l => l.Rival)
                    .Select(g => FilaEjercito(g.Key, Acumular(g), catalogo))
                    .Where(f => f.Partidas >= minimoRival && f.Porcentaje < 50)
                    .OrderBy(f => f.Porcentaje)
                    .ThenByDescending(f => f.Partidas)
                    .Take(cuantosRivales),
            ],
        };
    }
}

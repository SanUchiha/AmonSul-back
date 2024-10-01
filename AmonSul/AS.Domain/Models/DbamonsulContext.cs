using Microsoft.EntityFrameworkCore;

namespace AS.Domain.Models;

public partial class DbamonsulContext : DbContext
{
    public DbamonsulContext()
    {
    }

    public DbamonsulContext(DbContextOptions<DbamonsulContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ClasificacionGeneral> ClasificacionGenerals { get; set; }

    public virtual DbSet<ClasificacionTorneo> ClasificacionTorneos { get; set; }

    public virtual DbSet<Comentario> Comentarios { get; set; }

    public virtual DbSet<Elo> Elos { get; set; }

    public virtual DbSet<Faccion> Facciones { get; set; }

    public virtual DbSet<InscripcionTorneo> InscripcionTorneos { get; set; }

    public virtual DbSet<Lista> Listas { get; set; }

    public virtual DbSet<PartidaAmistosa> PartidaAmistosas { get; set; }

    public virtual DbSet<PartidaTorneo> PartidaTorneos { get; set; }

    public virtual DbSet<RangoTorneo> RangoTorneos { get; set; }

    public virtual DbSet<Ronda> Ronda { get; set; }

    public virtual DbSet<Torneo> Torneos { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<Ganador> Ganador { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //Clasificacion general
        modelBuilder.Entity<ClasificacionGeneral>(entity =>
        {
            entity.HasKey(e => e.IdClasificacion).HasName("PK__Clasific__0D78096BD848289A");

            entity.ToTable("Clasificacion_General");

            entity.Property(e => e.IdClasificacion).HasColumnName("ID_Clasificacion");
            entity.Property(e => e.IdPuntuacionElo).HasColumnName("ID_Puntuacion_Elo");
            entity.Property(e => e.IdUsuario).HasColumnName("ID_Usuario");
            entity.Property(e => e.PuntuacionTotal).HasColumnName("Puntuacion_Total");

            entity.HasOne(d => d.IdPuntuacionEloNavigation).WithMany(p => p.ClasificacionGenerals)
                .HasForeignKey(d => d.IdPuntuacionElo)
                .HasConstraintName("FK_Clasificacion_General_Elo");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.ClasificacionGenerals)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("FK_Clasificacion_General_Usuario");
        });

        //Clasifiacion torneo
        modelBuilder.Entity<ClasificacionTorneo>(entity =>
        {
            entity.HasKey(e => e.IdClasificacionTorneo).HasName("PK__Clasific__CDB980300F5DB99A");

            entity.ToTable("Clasificacion_Torneo");

            entity.Property(e => e.IdClasificacionTorneo).HasColumnName("ID_Clasificacion_Torneo");
            entity.Property(e => e.IdTorneo).HasColumnName("ID_Torneo");
            entity.Property(e => e.IdUsuario).HasColumnName("ID_Usuario");
            entity.Property(e => e.PosicionFinal).HasColumnName("Posicion_Final");
            entity.Property(e => e.PuntosContra).HasColumnName("Puntos_contra");
            entity.Property(e => e.PuntosFavor).HasColumnName("Puntos_Favor");
            entity.Property(e => e.PuntosGeneral).HasColumnName("Puntos_General");
            entity.Property(e => e.PuntosTorneo).HasColumnName("Puntos_Torneo");

            entity.HasOne(d => d.IdTorneoNavigation).WithMany(p => p.ClasificacionTorneos)
                .HasForeignKey(d => d.IdTorneo)
                .HasConstraintName("FK_Clasificacion_Torneo_Torneo");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.ClasificacionTorneos)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("FK_Clasificacion_Torneo_Usuario");
        });

        //Comentarios
        modelBuilder.Entity<Comentario>(entity =>
        {
            entity.HasKey(e => e.IdComentario).HasName("PK__Comentar__E9AA9973A0E38D7D");

            entity.ToTable("Comentario");

            entity.Property(e => e.IdComentario).HasColumnName("ID_Comentario");
            entity.Property(e => e.Comentario1)
                .HasColumnType("text")
                .HasColumnName("Comentario");
            entity.Property(e => e.FechaComentario).HasColumnName("Fecha_Comentario");
            entity.Property(e => e.IdTorneo).HasColumnName("ID_Torneo");
            entity.Property(e => e.IdUsuario).HasColumnName("ID_Usuario");
            entity.Property(e => e.PuntuarTorneo).HasColumnName("Puntuar_Torneo");

            entity.HasOne(d => d.IdTorneoNavigation).WithMany(p => p.Comentarios)
                .HasForeignKey(d => d.IdTorneo)
                .HasConstraintName("FK_Comentario_Torneo");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Comentarios)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("FK_Comentario_Usuario");
        });

        //ELO
        modelBuilder.Entity<Elo>(entity =>
        {
            entity.HasKey(e => e.IdElo).HasName("PK__Elo__2D4D22C31CCC8245");

            entity.ToTable("Elo");

            entity.Property(e => e.IdElo).HasColumnName("ID_Elo");
            entity.Property(e => e.IdUsuario).HasColumnName("ID_Usuario");
            entity.Property(e => e.PuntuacionElo).HasColumnName("Puntuacion_Elo");
            entity.Property(e => e.FechaElo).HasColumnName("Fecha_Elo");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Elos)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("FK_Elo_Usuario");
        });

        //Faccion
        modelBuilder.Entity<Faccion>(entity =>
        {
            entity.HasKey(e => e.IdFaccion).HasName("PK__Faccion__8743EEA90139EE02");

            entity.ToTable("Faccion");

            entity.Property(e => e.IdFaccion).HasColumnName("ID_Faccion");
            entity.Property(e => e.NombreFaccion)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("Nombre_Faccion");
        });

        //Inscripciones
        modelBuilder.Entity<InscripcionTorneo>(entity =>
        {
            entity.HasKey(e => e.IdInscripcion).HasName("PK__Inscripc__B84666E0A08239E2");

            entity.ToTable("Inscripcion_Torneo");

            entity.Property(e => e.IdInscripcion).HasColumnName("ID_Inscripcion");
            entity.Property(e => e.EsPago)
                .HasColumnName("Es_Pago")
                .HasMaxLength(2)
                .IsUnicode(false);
            entity.Property(e => e.EstadoInscripcion)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("Estado_Inscripcion");
            entity.Property(e => e.EstadoLista)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("Estado_Lista");
            entity.Property(e => e.FechaEntregaLista).HasColumnName("Fecha_Entrega_Lista");
            entity.Property(e => e.FechaInscripcion).HasColumnName("Fecha_Inscripcion");
            entity.Property(e => e.IdTorneo).HasColumnName("ID_Torneo");
            entity.Property(e => e.IdUsuario).HasColumnName("ID_Usuario");

            entity.HasOne(d => d.IdTorneoNavigation).WithMany(p => p.InscripcionTorneos)
                .HasForeignKey(d => d.IdTorneo)
                .HasConstraintName("FK_Inscripcion_Torneo_Torneo");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.InscripcionTorneos)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("FK_Inscripcion_Torneo_Usuario");

            entity.HasMany(e => e.Lista)
               .WithOne(e => e.IdInscripcionNavigation)
               .HasForeignKey(e => e.IdInscripcion)
               .OnDelete(DeleteBehavior.Cascade);
        });

        //Listas
        modelBuilder.Entity<Lista>(entity =>
        {
            entity.HasKey(e => e.IdLista).HasName("PK__Lista__B5A70F121CFB4DDE");

            entity.ToTable("Lista");

            entity.Property(e => e.IdLista).HasColumnName("ID_Lista");
            entity.Property(e => e.FechaEntrega).HasColumnName("Fecha_Entrega");
            entity.Property(e => e.IdInscripcion).HasColumnName("ID_Inscripcion");
            entity.Property(e => e.ListaData).HasColumnName("Lista_Data");
            entity.Property(e => e.Ejercito).HasColumnName("Ejercito");

            entity.HasOne(d => d.IdInscripcionNavigation).WithMany(p => p.Lista)
                .HasForeignKey(d => d.IdInscripcion)
                .HasConstraintName("FK_Lista_Inscripcion");
        });

        //Partidas amistosas
        modelBuilder.Entity<PartidaAmistosa>(entity =>
        {
            entity.HasKey(e => e.IdPartidaAmistosa).HasName("PK__Partida___55DEAFB728B99DE3");

            entity.ToTable("Partida_Amistosa");

            entity.Property(e => e.IdPartidaAmistosa).HasColumnName("ID_Partida_Amistosa");
            entity.Property(e => e.EsElo).HasColumnName("Es_Elo");
            entity.Property(e => e.EsMatchedPlayPartida).HasColumnName("Es_Matched_Play_Partida");
            entity.Property(e => e.EscenarioPartida)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("Escenario_Partida");
            entity.Property(e => e.FechaPartida).HasColumnName("Fecha_Partida");
            entity.Property(e => e.GanadorPartida).HasColumnName("Ganador_Partida");
            entity.Property(e => e.IdUsuario1).HasColumnName("ID_Usuario1");
            entity.Property(e => e.IdUsuario2).HasColumnName("ID_Usuario2");
            entity.Property(e => e.PuntosPartida).HasColumnName("Puntos_Partida");
            entity.Property(e => e.ResultadoUsuario1).HasColumnName("Resultado_Usuario1");
            entity.Property(e => e.ResultadoUsuario2).HasColumnName("Resultado_Usuario2");

            entity.HasOne(d => d.GanadorPartidaNavigation).WithMany(p => p.PartidaAmistosaGanadorPartidaNavigations)
                .HasForeignKey(d => d.GanadorPartida)
                .HasConstraintName("FK_Partida_Amistosa_Ganador");

            entity.HasOne(d => d.IdUsuario1Navigation).WithMany(p => p.PartidaAmistosaIdUsuario1Navigations)
                .HasForeignKey(d => d.IdUsuario1)
                .HasConstraintName("FK_Partida_Amistosa_Usuario1");

            entity.HasOne(d => d.IdUsuario2Navigation).WithMany(p => p.PartidaAmistosaIdUsuario2Navigations)
                .HasForeignKey(d => d.IdUsuario2)
                .HasConstraintName("FK_Partida_Amistosa_Usuario2");
            entity.Property(e => e.PartidaValidadaUsuario1).HasColumnName("Partida_Validada_Usuario1");
            entity.Property(e => e.PartidaValidadaUsuario2).HasColumnName("Partida_Validada_Usuario2");
            entity.Property(e => e.EjercitoUsuario1).HasColumnName("Ejercito_Usuario1");
            entity.Property(e => e.EjercitoUsuario2).HasColumnName("Ejercito_Usuario2");
            entity.Property(e => e.EsTorneo).HasColumnName("Es_Torneo");

        });

        //Partidas torneo
        modelBuilder.Entity<PartidaTorneo>(entity =>
        {
            entity.HasKey(e => e.IdPartidaTorneo).HasName("PK__Partida___4AA741CABAA82AD2");

            entity.ToTable("Partida_Torneo");

            entity.Property(e => e.IdPartidaTorneo).HasColumnName("ID_Partida_Torneo");
            entity.Property(e => e.EsMatchedPlayPartida).HasColumnName("Es_Matched_Play_Partida");
            entity.Property(e => e.EscenarioPartida)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("Escenario_Partida");
            entity.Property(e => e.FechaPartida).HasColumnName("Fecha_Partida");
            entity.Property(e => e.GanadorPartidaTorneo).HasColumnName("Ganador_Partida_Torneo");
            entity.Property(e => e.IdTorneo).HasColumnName("ID_Torneo");
            entity.Property(e => e.IdUsuario1).HasColumnName("ID_Usuario1");
            entity.Property(e => e.IdUsuario2).HasColumnName("ID_Usuario2");
            entity.Property(e => e.PuntosPartida).HasColumnName("Puntos_Partida");
            entity.Property(e => e.ResultadoUsuario1).HasColumnName("Resultado_Usuario1");
            entity.Property(e => e.ResultadoUsuario2).HasColumnName("Resultado_Usuario2");
            entity.Property(e => e.PartidaValidadaUsuario1).HasColumnName("Partida_Validada_Usuario1");
            entity.Property(e => e.PartidaValidadaUsuario2).HasColumnName("Partida_Validada_Usuario2");
            entity.Property(e => e.EjercitoUsuario1).HasColumnName("Ejercito_Usuario1");
            entity.Property(e => e.EjercitoUsuario2).HasColumnName("Ejercito_Usuario2");
            entity.Property(e => e.EsElo).HasColumnName("Es_Elo");
            entity.Property(e => e.NumeroRonda).HasColumnName("Numero_Ronda");
            entity.Property(e => e.LiderMuertoUsuario1).HasColumnName("Lider_Muerto_Usuario1");
            entity.Property(e => e.LiderMuertoUsuario2).HasColumnName("Lider_Muerto_Usuario2");

            entity.HasOne(d => d.IdTorneoNavigation).WithMany(p => p.PartidaTorneos)
                .HasForeignKey(d => d.IdTorneo)
                .HasConstraintName("FK_Partida_Torneo_Torneo");

            entity.HasOne(d => d.IdUsuario1Navigation).WithMany(p => p.PartidaTorneoIdUsuario1Navigations)
                .HasForeignKey(d => d.IdUsuario1)
                .HasConstraintName("FK_Partida_Torneo_Usuario1");

            entity.HasOne(d => d.IdUsuario2Navigation).WithMany(p => p.PartidaTorneoIdUsuario2Navigations)
                .HasForeignKey(d => d.IdUsuario2)
                .HasConstraintName("FK_Partida_Torneo_Usuario2");

            entity.HasOne(d => d.GanadorPartidaTorneoNavigation).WithMany(p => p.PartidaTorneoGanadorPartidaNavigations)
                .HasForeignKey(d => d.GanadorPartidaTorneo)
                .HasConstraintName("FK_Partida_Torneo_Ganador");
        });

        //Rango torneos
        modelBuilder.Entity<RangoTorneo>(entity =>
        {
            entity.HasKey(e => e.IdRango).HasName("PK__Rango_To__B4DBE7A05469851E");

            entity.ToTable("Rango_Torneo");

            entity.Property(e => e.IdRango).HasColumnName("ID_Rango");
            entity.Property(e => e.NombreRango)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("Nombre_Rango");
            entity.Property(e => e.Puntos1).HasColumnName("Puntos_1");
            entity.Property(e => e.Puntos10).HasColumnName("Puntos_10");
            entity.Property(e => e.Puntos2).HasColumnName("Puntos_2");
            entity.Property(e => e.Puntos3).HasColumnName("Puntos_3");
            entity.Property(e => e.Puntos4).HasColumnName("Puntos_4");
            entity.Property(e => e.Puntos5).HasColumnName("Puntos_5");
            entity.Property(e => e.Puntos6).HasColumnName("Puntos_6");
            entity.Property(e => e.Puntos7).HasColumnName("Puntos_7");
            entity.Property(e => e.Puntos8).HasColumnName("Puntos_8");
            entity.Property(e => e.Puntos9).HasColumnName("Puntos_9");
        });

        //Rondas
        modelBuilder.Entity<Ronda>(entity =>
        {
            entity.HasKey(e => e.IdRonda).HasName("PK__Ronda__2E6F61713267485D");

            entity.Property(e => e.IdRonda).HasColumnName("ID_Ronda");
            entity.Property(e => e.DuracionRonda).HasColumnName("Duracion_Ronda");
            entity.Property(e => e.EscenarioRonda)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("Escenario_Ronda");
            entity.Property(e => e.EstadoRonda)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("Estado_Ronda");
            entity.Property(e => e.FechaFinRonda).HasColumnName("Fecha_Fin_Ronda");
            entity.Property(e => e.FechaInicioRonda).HasColumnName("Fecha_Inicio_Ronda");
            entity.Property(e => e.HoraInicioRonda).HasColumnName("Hora_Inicio_Ronda");
            entity.Property(e => e.IdTorneo).HasColumnName("ID_Torneo");
            entity.Property(e => e.NumeroRonda).HasColumnName("Numero_Ronda");

            entity.HasOne(d => d.IdTorneoNavigation).WithMany(p => p.Ronda)
                .HasForeignKey(d => d.IdTorneo)
                .HasConstraintName("FK_Ronda_Torneo");
        });

        //Torneos
        modelBuilder.Entity<Torneo>(entity =>
        {
            entity.HasKey(e => e.IdTorneo).HasName("PK__Torneo__F5E859A023B6CC5E");

            entity.ToTable("Torneo");

            entity.Property(e => e.IdTorneo).HasColumnName("ID_Torneo");
            entity.Property(e => e.BasesTorneo).HasColumnName("Bases_Torneo");
            entity.Property(e => e.CartelTorneo)
                .HasColumnType("text")
                .HasColumnName("Cartel_Torneo");
            entity.Property(e => e.DescripcionTorneo)
                .HasColumnType("text")
                .HasColumnName("Descripcion_Torneo");
            entity.Property(e => e.EsLiga).HasColumnName("Es_Liga");
            entity.Property(e => e.EsMatchedPlayTorneo).HasColumnName("Es_Matched_Play_Torneo");
            entity.Property(e => e.EsPrivadoTorneo).HasColumnName("Es_Privado_Torneo");
            entity.Property(e => e.EstadoTorneo)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("Estado_Torneo");
            entity.Property(e => e.FechaEntregaListas).HasColumnName("Fecha_Entrega_Listas");
            entity.Property(e => e.FechaFinInscripcion).HasColumnName("Fecha_Fin_Inscripcion");
            entity.Property(e => e.FechaFinTorneo).HasColumnName("Fecha_Fin_Torneo");
            entity.Property(e => e.FechaInicioTorneo).HasColumnName("Fecha_Inicio_Torneo");
            entity.Property(e => e.HoraFinTorneo).HasColumnName("Hora_Fin_Torneo");
            entity.Property(e => e.HoraInicioTorneo).HasColumnName("Hora_Inicio_Torneo");
            entity.Property(e => e.IdRangoTorneo).HasColumnName("ID_Rango_Torneo");
            entity.Property(e => e.IdUsuario).HasColumnName("ID_Usuario");
            entity.Property(e => e.LimiteParticipantes).HasColumnName("Limite_Participantes");
            entity.Property(e => e.LugarTorneo)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("Lugar_Torneo");
            entity.Property(e => e.MetodosPago)
                .HasColumnType("text")
                .HasColumnName("Metodos_Pago");
            entity.Property(e => e.NombreTorneo)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("Nombre_Torneo");
            entity.Property(e => e.NumeroPartidas).HasColumnName("Numero_Partidas");
            entity.Property(e => e.PrecioTorneo).HasColumnName("Precio_Torneo");
            entity.Property(e => e.PuntosTorneo).HasColumnName("Puntos_Torneo");
            entity.Property(e => e.TipoTorneo)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Tipo_Torneo");

            entity.HasOne(d => d.IdRangoTorneoNavigation).WithMany(p => p.Torneos)
                .HasForeignKey(d => d.IdRangoTorneo)
                .HasConstraintName("FK_Torneo_Rango_Torneo");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Torneos)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Torneo_Usuario");
        });

        //Usuarios
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("PK__Usuario__DE4431C52F8B3BC9");

            entity.ToTable("Usuario");

            entity.HasIndex(e => e.Nick, "UQ__Usuario__7D3471B6867373C0").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Usuario__A9D105344761A97B").IsUnique();

            entity.Property(e => e.IdUsuario).HasColumnName("ID_Usuario");
            entity.Property(e => e.Ciudad)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Contraseña)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FechaNacimiento).HasColumnName("Fecha_Nacimiento");
            entity.Property(e => e.FechaRegistro).HasColumnName("Fecha_Registro");
            entity.Property(e => e.IdFaccion).HasColumnName("ID_Faccion");
            entity.Property(e => e.Nick)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NombreUsuario)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Nombre_Usuario");
            entity.Property(e => e.PrimerApellido)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Primer_Apellido");
            entity.Property(e => e.SegundoApellido)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Segundo_Apellido");
            entity.Property(e => e.Telefono)
                .HasMaxLength(9)
                .IsUnicode(false);
            entity.Property(e => e.ProteccionDatos)
                .HasColumnName("Proteccion_Datos");

            entity.HasOne(d => d.IdFaccionNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdFaccion)
                .HasConstraintName("FK_Usuario_Faccion");
        });

        //Ganadores
        modelBuilder.Entity<Ganador>(entity =>
        {
            entity.HasKey(e => e.IdGanador).HasName("PK__Ganador__160354F0762D9D53");

            entity.ToTable("Ganador");

            entity.Property(e => e.IdGanador).HasColumnName("Id_Ganador");
            entity.Property(e => e.IdUsuario)
                .HasColumnName("Id_Usuario")
                .IsRequired();
            entity.Property(e => e.IdTorneo)
                .HasColumnName("Id_Torneo")
                .IsRequired();
            entity.Property(e => e.Resultado)
                .HasColumnName("Resultado")
                .IsRequired();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

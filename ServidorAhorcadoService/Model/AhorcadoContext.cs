using ServidorAhorcadoService.DTO;
using ServidorAhorcadoService.Model;
using System.Collections.Generic;
using System.Data.Entity;
using System.Runtime.Remoting.Contexts;


namespace ServidorAhorcadoService.Model
{
    public class AhorcadoContext : DbContext
    {
        static AhorcadoContext()
        {
            Database.SetInitializer<AhorcadoContext>(null);
        }

        public AhorcadoContext() : base("name=AhorcadoDB") { }

        public DbSet<Idioma> Idiomas { get; set; }
        public DbSet<Jugador> Jugadores { get; set; }
        public DbSet<Estado> Estados { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Palabra> Palabras { get; set; }
        public DbSet<Partida> Partidas { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Jugador>().ToTable("Jugador");
            modelBuilder.Entity<Jugador>().HasKey(j => j.IDJugador);
            modelBuilder.Entity<Jugador>().Property(j => j.Nombre).HasColumnName("Nombre");
            modelBuilder.Entity<Jugador>().Property(j => j.Contraseña).HasColumnName("Contraseña");

            modelBuilder.Entity<Idioma>().ToTable("Idioma");
            modelBuilder.Entity<Idioma>().HasKey(i => i.CodigoIdioma);

            modelBuilder.Entity<Estado>().ToTable("Estado");
            modelBuilder.Entity<Estado>().HasKey(e => e.IDEstado);

            modelBuilder.Entity<Categoria>().ToTable("Categoria");
            modelBuilder.Entity<Categoria>().HasKey(c => c.IDCategoria);
            modelBuilder.Entity<Categoria>().HasRequired(c => c.Idioma)
                                            .WithMany()
                                            .HasForeignKey(c => c.CodigoIdioma);

            modelBuilder.Entity<Palabra>().ToTable("Palabra");
            modelBuilder.Entity<Palabra>().HasKey(p => p.IDPalabra);
            modelBuilder.Entity<Palabra>().HasRequired(p => p.Categoria)
                                          .WithMany()
                                          .HasForeignKey(p => p.IDCategoria);

            modelBuilder.Entity<Partida>().ToTable("Partida");
            modelBuilder.Entity<Partida>().HasKey(p => p.IDPartida);
            modelBuilder.Entity<Partida>().Property(p => p.LetrasUsadas).HasMaxLength(200);


            // Relaciones múltiples con Jugador
            modelBuilder.Entity<Partida>()
                .HasRequired(p => p.Creador)
                .WithMany()
                .HasForeignKey(p => p.IDJugadorCreador)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Partida>()
                .HasRequired(p => p.Retador)
                .WithMany()
                .HasForeignKey(p => p.IDJugadorRetador)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Partida>()
                .HasOptional(p => p.GanadorJugador)
                .WithMany()
                .HasForeignKey(p => p.Ganador)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Partida>()
                .HasOptional(p => p.Cancelador)
                .WithMany()
                .HasForeignKey(p => p.IDCancelador)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Partida>()
                .HasRequired(p => p.Estado)
                .WithMany()
                .HasForeignKey(p => p.IDEstado);

            modelBuilder.Entity<Partida>()
                .HasRequired(p => p.Palabra)
                .WithMany()
                .HasForeignKey(p => p.IDPalabra);
        }
    }
}
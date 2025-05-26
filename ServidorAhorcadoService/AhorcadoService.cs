using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using ServidorAhorcadoService.DTO;
using ServidorAhorcadoService.Model;
using ServidorAhorcadoService;
using ServidorAhorcadoService.DTO;


namespace ServidorAhorcadoService
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class AhorcadoService : IAhorcadoService
    {
        private readonly Dictionary<int, IAhorcadoCallback> clientesConectados = new Dictionary<int, IAhorcadoCallback>();

        // --- AUTENTICACIÓN Y USUARIO ---

        public JugadorDTO IniciarSesion(string correo, string password)
        {
            using (var db = new AhorcadoContext())
            {
                var jugador = db.Jugadores.FirstOrDefault(j => j.Correo == correo && j.Contraseña == password);
                if (jugador == null) return null;

                return new JugadorDTO
                {
                    IDJugador = jugador.IDJugador,
                    Nombre = jugador.Nombre,
                    Correo = jugador.Correo,
                    Telefono = jugador.Telefono,
                    FechaNacimiento = jugador.FechaNacimiento,
                    PuntajeGlobal = jugador.PuntajeGlobal,
                    Contraseña = jugador.Contraseña
                };
            }
        }

        public bool RegistrarJugador(JugadorDTO jugador)
        {
            using (var db = new AhorcadoContext())
            {
                if (db.Jugadores.Any(j => j.Correo == jugador.Correo))
                    return false;

                db.Jugadores.Add(new Jugador
                {
                    Nombre = jugador.Nombre,
                    Correo = jugador.Correo,
                    Contraseña = jugador.Contraseña,
                    FechaNacimiento = jugador.FechaNacimiento,
                    Telefono = jugador.Telefono,
                    PuntajeGlobal = 0
                });

                db.SaveChanges();
                return true;
            }
        }

        public JugadorDTO ConsultarPerfil(int idJugador)
        {
            using (var db = new AhorcadoContext())
            {
                var jugador = db.Jugadores.FirstOrDefault(j => j.IDJugador == idJugador);
                if (jugador == null) return null;

                return new JugadorDTO
                {
                    IDJugador = jugador.IDJugador,
                    Nombre = jugador.Nombre,
                    Correo = jugador.Correo,
                    Telefono = jugador.Telefono,
                    FechaNacimiento = jugador.FechaNacimiento,
                    PuntajeGlobal = jugador.PuntajeGlobal
                };
            }
        }

        public bool ModificarPerfil(JugadorDTO jugadorModificado)
        {
            using (var db = new AhorcadoContext())
            {
                var jugador = db.Jugadores.FirstOrDefault(j => j.IDJugador == jugadorModificado.IDJugador);
                if (jugador == null) return false;

                jugador.Nombre = jugadorModificado.Nombre;
                jugador.Telefono = jugadorModificado.Telefono;
                jugador.FechaNacimiento = jugadorModificado.FechaNacimiento;
                jugador.Contraseña = jugadorModificado.Contraseña;

                db.SaveChanges();
                return true;
            }
        }

        public int ObtenerPuntajeGlobal(int idJugador)
        {
            using (var db = new AhorcadoContext())
            {
                var jugador = db.Jugadores.FirstOrDefault(j => j.IDJugador == idJugador);
                return jugador?.PuntajeGlobal ?? 0;
            }
        }

        public List<PartidaDTO> ConsultarPartidasJugadas(int idJugador)
        {
            using (var db = new AhorcadoContext())
            {
                return db.Partidas
                    .Where(p => p.IDJugadorCreador == idJugador || p.IDJugadorRetador == idJugador)
                    .OrderByDescending(p => p.Fecha)
                    .Select(p => new PartidaDTO
                    {
                        IDPartida = p.IDPartida,
                        CreadorNombre = p.Creador.Nombre,
                        RetadorNombre = p.Retador != null ? p.Retador.Nombre : null,
                        Estado = p.Estado.Nombre,
                        Fecha = p.Fecha.ToString("yyyy-MM-dd HH:mm:ss"),
                        PalabraTexto = p.Palabra.PalabraTexto
                    }).ToList();
            }
        }

        // Agrega este método en la clase AhorcadoService

        public List<JugadorDTO> ObtenerJugadoresMarcadores()
        {
            using (var db = new AhorcadoContext())
            {
                return db.Jugadores
                    .Select(j => new JugadorDTO
                    {
                        IDJugador = j.IDJugador,
                        Nombre = j.Nombre,
                        PuntajeGlobal = j.PuntajeGlobal
                    })
                    .OrderByDescending(j => j.PuntajeGlobal)
                    .ToList();
            }
        }


        // --- PALABRAS Y CATEGORÍAS ---

        public List<CategoriaDTO> ObtenerCategoriasPorIdioma(string codigoIdioma)
        {
            using (var db = new AhorcadoContext())
            {
                var idIdioma = db.Idiomas.Where(i => i.Nombre == codigoIdioma).Select(i => i.CodigoIdioma).FirstOrDefault();

                return db.Categorias
                    .Where(c => c.CodigoIdioma == idIdioma)
                    .Select(c => new CategoriaDTO
                    {
                        IDCategoria = c.IDCategoria,
                        Nombre = c.Nombre
                    }).ToList();
            }
        }

        public PalabraDTO ObtenerPalabraConDescripcion(int idPalabra, string idioma)
        {
            using (var db = new AhorcadoContext())
            {
                var palabra = db.Palabras.FirstOrDefault(p => p.IDPalabra == idPalabra);
                if (palabra == null) return null;

                return new PalabraDTO
                {
                    IDPalabra = palabra.IDPalabra,
                    Texto = palabra.PalabraTexto,
                    Definicion = palabra.Definicion,
                    Dificultad = palabra.Dificultad,
                    IDCategoria = palabra.Categoria.Nombre
                };
            }
        }

        public List<PalabraDTO> ObtenerPalabrasPorCategoria(int idCategoria, string idioma)
        {
            using (var db = new AhorcadoContext())
            {
                return db.Palabras
                    .Where(p => p.IDCategoria == idCategoria)
                    .Select(p => new PalabraDTO
                    {
                        IDPalabra = p.IDPalabra,
                        Texto = p.PalabraTexto,
                        Definicion = p.Definicion,
                        Dificultad = p.Dificultad,
                        IDCategoria = p.Categoria.Nombre
                    }).ToList();
            }
        }

        // --- PARTIDAS Y JUEGO ---

        public bool CrearPartida(int idCreador, int idPalabra)
        {
            using (var db = new AhorcadoContext())
            {
                var nueva = new Partida
                {
                    IDJugadorCreador = idCreador,
                    IDPalabra = idPalabra,
                    IDEstado = 1,
                    Fecha = DateTime.Now,
                    Puntaje = 0
                };

                db.Partidas.Add(nueva);
                db.SaveChanges();
                return true;
            }
        }

        public List<PartidaDTO> ObtenerPartidasDisponibles()
        {
            using (var db = new AhorcadoContext())
            {
                return db.Partidas
                    .Where(p => p.IDEstado == 1)
                    .AsEnumerable() // Cambia a LINQ-to-Objects
                    .Select(p => new PartidaDTO
                    {
                        IDPartida = p.IDPartida,
                        CreadorNombre = p.Creador.Nombre,
                        RetadorNombre = p.Retador != null ? p.Retador.Nombre : null,
                        Estado = p.Estado.Nombre,
                        Fecha = p.Fecha.ToString("yyyy-MM-dd HH:mm:ss"),
                        PalabraTexto = p.Palabra.PalabraTexto
                    }).ToList();
            }
        }

        public bool UnirseAPartida(int idPartida, int idJugador)
        {
            using (var db = new AhorcadoContext())
            {
                var partida = db.Partidas.FirstOrDefault(p => p.IDPartida == idPartida && p.IDEstado == 1);
                if (partida == null) return false;

                partida.IDJugadorRetador = idJugador;
                partida.IDEstado = 2;
                db.SaveChanges();
                return true;
            }
        }

        public bool AbandonarPartida(int idPartida, int idJugador)
        {
            using (var db = new AhorcadoContext())
            {
                var partida = db.Partidas.Find(idPartida);
                if (partida == null) return false;

                partida.IDEstado = 3;
                partida.IDCancelador = idJugador;
                db.SaveChanges();
                return true;
            }
        }

        public PartidaEstadoDTO ObtenerEstadoPartida(int idPartida)
        {
            using (var db = new AhorcadoContext())
            {
                var partida = db.Partidas.FirstOrDefault(p => p.IDPartida == idPartida);
                if (partida == null) return null;

                return new PartidaEstadoDTO
                {
                    PalabraConGuiones = "____", // Simulación
                    IntentosRestantes = 6,
                    LetrasUsadas = new List<char>(),
                    TurnoActual = partida.IDJugadorRetador != null ? db.Jugadores.Find(partida.IDJugadorRetador)?.Nombre : "Esperando"
                };
            }
        }


        // --- JUGABILIDAD ---

        private bool TodasLetrasAdivinadas(string palabra, List<string> letrasUsadas)
        {
            var letrasPalabra = palabra.ToLower().Distinct().Where(c => Char.IsLetter(c)).Select(c => c.ToString());
            return letrasPalabra.All(l => letrasUsadas.Contains(l));
        }

        public bool EnviarLetra(int idPartida, int idJugador, char letra)
        {
            using (var db = new AhorcadoContext())
            {
                var partida = db.Partidas.FirstOrDefault(p => p.IDPartida == idPartida);
                if (partida == null || partida.IDEstado != 2)
                    return false;

                string letraStr = letra.ToString().ToLower();
                var palabra = db.Palabras.FirstOrDefault(p => p.IDPalabra == partida.IDPalabra);
                if (palabra == null) return false;

                if (partida.LetrasUsadas == null)
                    partida.LetrasUsadas = "";

                var letrasUsadas = partida.LetrasUsadas.Split(',').Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
                if (letrasUsadas.Contains(letraStr))
                    return false; // Ya usada

                letrasUsadas.Add(letraStr);
                partida.LetrasUsadas = string.Join(",", letrasUsadas);

                if (!palabra.PalabraTexto.ToLower().Contains(letraStr))
                    partida.IntentosRestantes--;

                if (partida.IntentosRestantes <= 0)
                {
                    partida.IDEstado = 4; // Perdida
                    partida.Ganador = partida.IDJugadorCreador == idJugador ? partida.IDJugadorRetador : partida.IDJugadorCreador;
                }
                else if (TodasLetrasAdivinadas(palabra.PalabraTexto, letrasUsadas))
                {
                    partida.IDEstado = 5; // Ganada
                    partida.Ganador = idJugador;
                }

                db.SaveChanges();
                return true;
            }
        }

        //testing

        // --- CHAT ---

        public void EnviarMensajeChat(int idPartida, string nombreJugador, string mensaje)
        {
            foreach (var callback in clientesConectados.Values)
            {
                callback.RecibirMensajeChat(nombreJugador, mensaje);
            }
        }
    }
}

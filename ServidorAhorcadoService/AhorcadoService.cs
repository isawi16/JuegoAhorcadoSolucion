using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using ServidorAhorcadoService.DTO;
using ServidorAhorcadoService.Model;
using ServidorAhorcadoService;


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
                // Registrar al jugador en clientes conectados
                var callback = OperationContext.Current.GetCallbackChannel<IAhorcadoCallback>();
                if (!clientesConectados.ContainsKey(jugador.IDJugador))
                {
                    clientesConectados.Add(jugador.IDJugador, callback);
                }

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


        public List<IdiomaDTO> ObtenerIdiomas()
        {
            using (var db = new AhorcadoContext())
            {
                return db.Idiomas
                         .Select(i => new IdiomaDTO
                         {
                             CodigoIdioma = i.CodigoIdioma,
                             Nombre = i.Nombre
                         }).ToList();
            }
        }

        public List<PalabraDTO> ObtenerPalabrasPorIdiomaYCategoria(string idioma, int idCategoria)
        {
            using (var db = new AhorcadoContext())
            {
                // Obtener el Código de Idioma (CodigoIdioma) basado en el nombre del idioma
                var idIdioma = db.Idiomas
                                 .Where(i => i.Nombre == idioma)
                                 .Select(i => i.CodigoIdioma)
                                 .FirstOrDefault();

                // Consultar las palabras asociadas al idioma y la categoría
                return db.Palabras
                    .Where(p => p.IDCategoria == idCategoria && p.Categoria.CodigoIdioma == idIdioma) // Relación con Categoria y CodigoIdioma
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


        /* public List<PalabraDTO> ObtenerPalabrasPorIdioma(string idioma)
         {
             using (var db = new AhorcadoContext())
             {
                 var idIdioma = db.Idiomas.Where(i => i.Nombre == idioma).Select(i => i.CodigoIdioma).FirstOrDefault();

                 return db.Palabras
                     .Where(p => p.CodigoIdioma == idIdioma)
                     .Select(p => new PalabraDTO
                     {
                         IDPalabra = p.IDPalabra,
                         Texto = p.PalabraTexto,
                         Definicion = p.Definicion,
                         Dificultad = p.Dificultad,
                         IDCategoria = p.Categoria.Nombre
                     }).ToList();
             }
         } */

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

        public List<CategoriaDTO> ObtenerCategoriasPorIdioma(int idiomaId)
        {
            using (var db = new AhorcadoContext())
            {
                return db.Categorias
                         .Where(c => c.CodigoIdioma == idiomaId)
                         .Select(c => new CategoriaDTO
                         {
                             IDCategoria = c.IDCategoria,
                             Nombre = c.Nombre
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
        private string ObtenerPalabraConGuiones(string palabra, List<string> letrasUsadas)
        {
            return string.Join(" ", palabra.Select(c => letrasUsadas.Contains(c.ToString().ToLower()) ? c : '_'));
        }


        private string GenerarPalabraConGuiones(string palabra, List<string> letrasUsadas)
        {
            return string.Join("", palabra.Select(c =>
                letrasUsadas.Contains(c.ToString().ToLower()) ? c.ToString() : "_"));
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

                var letrasUsadas = partida.LetrasUsadas.Split(',')
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ToList();

                if (letrasUsadas.Contains(letraStr))
                    return false;

                letrasUsadas.Add(letraStr);
                partida.LetrasUsadas = string.Join(",", letrasUsadas);

                if (!palabra.PalabraTexto.ToLower().Contains(letraStr))
                    partida.IntentosRestantes--;

                if (partida.IntentosRestantes <= 0)
                {
                    partida.IDEstado = 4;
                    partida.Ganador = partida.IDJugadorCreador == idJugador ? partida.IDJugadorRetador : partida.IDJugadorCreador;
                }
                else if (TodasLetrasAdivinadas(palabra.PalabraTexto, letrasUsadas))
                {
                    partida.IDEstado = 5;
                    partida.Ganador = idJugador;
                }

                var estadoDTO = new PartidaEstadoDTO
                {
                    PalabraConGuiones = GenerarPalabraConGuiones(palabra.PalabraTexto, letrasUsadas),
                    IntentosRestantes = partida.IntentosRestantes,
                    LetrasUsadas = letrasUsadas.Select(s => s[0]).ToList(),
                    TurnoActual = db.Jugadores.Find(partida.IDJugadorRetador)?.Nombre
                };

                if (clientesConectados.TryGetValue(partida.IDJugadorCreador, out var callbackCreador))
                {
                    callbackCreador.ActualizarEstadoPartida(estadoDTO);
                }

                if (partida.IDJugadorRetador != null &&
                    clientesConectados.TryGetValue(partida.IDJugadorRetador.Value, out var callbackRetador))
                {
                    callbackRetador.ActualizarEstadoPartida(estadoDTO);
                }

                db.SaveChanges();
                return true;
            }
        }


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

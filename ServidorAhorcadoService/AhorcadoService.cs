using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.ServiceModel;
using BibliotecaClasesNetFramework.Contratos;
using BibliotecaClasesNetFramework.DTO;
using ServidorAhorcadoService.Model;
using System.IO;


namespace ServidorAhorcadoService
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class AhorcadoService : IAhorcadoService
    {
        private static readonly ConcurrentDictionary<int, IAhorcadoCallback> clientesConectados = new ConcurrentDictionary<int, IAhorcadoCallback>();
        private static readonly ConcurrentDictionary<int, int> jugadorAPartida = new ConcurrentDictionary<int, int>();

        // --- AUTENTICACIÓN Y USUARIO ---

        public JugadorDTO IniciarSesion(string correo, string password)
        {
            try
            {
                using (var db = new AhorcadoContext())
                {
                    var jugador = db.Jugadores.FirstOrDefault(j => j.Correo == correo && j.Contraseña == password);
                    if (jugador == null) return null;

                    var callback = OperationContext.Current.GetCallbackChannel<IAhorcadoCallback>();
                    clientesConectados[jugador.IDJugador] = callback;

                    return new JugadorDTO
                    {
                        IDJugador = jugador.IDJugador,
                        Nombre = jugador.Nombre,
                        Correo = jugador.Correo,
                        Telefono = jugador.Telefono,
                        FechaNacimiento = jugador.FechaNacimiento,
                        PuntajeGlobal = jugador.PuntajeGlobal,
                        Contraseña = jugador.Contraseña,
                        FotoPerfil = jugador.FotoPerfil
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al iniciar sesión: {ex.Message}");
                //return null;
                throw;
            }
            
        }

        public bool RegistrarJugador(JugadorDTO jugador)
        {
            using (var db = new AhorcadoContext())
            {
                try
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
                        FotoPerfil = jugador.FotoPerfil ?? File.ReadAllBytes("Imagenes/iconoDefault.png"),
                        PuntajeGlobal = 0
                    });

                    db.SaveChanges();
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al registrar jugador: {ex.Message}");
                    return false;
                }
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
                    PuntajeGlobal = jugador.PuntajeGlobal,
                    FotoPerfil = jugador.FotoPerfil
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
                jugador.FotoPerfil = jugadorModificado.FotoPerfil;
                jugador.Contraseña = jugadorModificado.Contraseña;

                db.SaveChanges();
                return true;
            }
        }

        public int ObtenerPuntajeGlobal(int idJugador)
        {
            using (var db = new AhorcadoContext())
            {
                var jugador = db.Jugadores.Find(idJugador);
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
                    .Select(p => new
                    {
                        IDPartida = p.IDPartida,
                        IDJugadorCreador = p.IDJugadorCreador,
                        IDJugadorRetador = p.IDJugadorRetador ?? 0,
                        CreadorNombre = p.Creador.Nombre,
                        RetadorNombre = p.Retador != null ? p.Retador.Nombre : null,
                        Estado = p.Estado.Nombre,
                        Fecha = p.Fecha,
                        PalabraTexto = p.Palabra.PalabraTexto,
                        GanadorNombre = p.GanadorJugador != null ? p.GanadorJugador.Nombre : null,
                        Puntaje = p.Puntaje,
                    })
                    .AsEnumerable()
                    .Select(p => new PartidaDTO
                    {
                        IDPartida = p.IDPartida,
                        CreadorNombre = p.CreadorNombre,
                        RetadorNombre = p.RetadorNombre,
                        Estado = p.Estado,
                        Fecha = p.Fecha.ToString("yyyy-MM-dd HH:mm:ss"),
                        PalabraTexto = p.PalabraTexto,
                        GanadorNombre = p.GanadorNombre,
                        Puntaje = p.Puntaje ?? 0,
                        RivalNombre = (p.IDJugadorCreador == idJugador) ? p.RetadorNombre : p.CreadorNombre
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

        public List<IdiomaDTO> ObtenerIdiomas()
        {
            using (var db = new AhorcadoContext())
            {
                return db.Idiomas
                    .Select(i => new IdiomaDTO
                    {
                        CodigoIdioma = i.CodigoIdioma,
                        Nombre = i.Nombre
                    })
                    .ToList();
            }
        }


        public List<PalabraDTO> ObtenerPalabrasPorIdiomaYCategoria(int codigoIdioma, int idCategoria)
        {
            using (var contexto = new AhorcadoContext())
            {
                var palabras = contexto.Palabras
                    .Where(p => p.IDCategoria == idCategoria &&
                                p.Categoria.CodigoIdioma == codigoIdioma)
                    .Select(p => new PalabraDTO
                    {
                        IDPalabra = p.IDPalabra,
                        Texto = p.PalabraTexto,
                        Dificultad = p.Dificultad,
                        IDCategoria = p.IDCategoria,
                        CodigoIdioma = p.Categoria.CodigoIdioma
                    })
                    .ToList();

                return palabras;
            }
        }

        public PalabraDTO ObtenerPalabraConDescripcion(int idPalabra)
        {
            using (var db = new AhorcadoContext())
            {
                var palabra = db.Palabras
                    .Where(p => p.IDPalabra == idPalabra)
                    .Select(p => new PalabraDTO
                    {
                        IDPalabra = p.IDPalabra,
                        Texto = p.PalabraTexto,
                        Definicion = p.Definicion
                    })
                    .FirstOrDefault();

                return palabra;
            }
        }

        public List<PalabraDTO> ObtenerPalabrasPorCategoria(int idCategoria, string idioma)
        {
            using (var db = new AhorcadoContext())
            {
                return db.Palabras
                    .Where(p => p.IDCategoria == idCategoria && p.Categoria.Idioma.Nombre == idioma)
                    .Select(p => new PalabraDTO
                    {
                        IDPalabra = p.IDPalabra,
                        Texto = p.PalabraTexto,
                        Definicion = p.Definicion,
                        Dificultad = p.Dificultad,
                        IDCategoria = p.IDCategoria,
                        CodigoIdioma = p.Categoria.CodigoIdioma
                    }).ToList();
            }
        }

        // --- PARTIDAS Y JUEGO ---

        public int CrearPartida(int idCreador, int idPalabra)
        {
            using (var db = new AhorcadoContext())
            {
                var nueva = new Partida
                {
                    IDJugadorCreador = idCreador,
                    IDPalabra = idPalabra,
                    IDEstado = 1,
                    Fecha = DateTime.Now,
                    Puntaje = 0,
                    IntentosRestantes = 6
                };

                db.Partidas.Add(nueva);
                db.SaveChanges();

                // Asociar creador a la partida
                jugadorAPartida[idCreador] = nueva.IDPartida;

                return nueva.IDPartida;
            }
        }

        public List<PartidaCategoriaDTO> ObtenerPartidasDisponibles()
        {
            using (var db = new AhorcadoContext())
            {
                var partidas = db.Partidas
                    .Where(p => p.IDEstado == 1)
                    .Select(p => new PartidaCategoriaDTO
                    {
                        IDPartida = p.IDPartida,
                        CategoriaNombre = p.Palabra.Categoria.Nombre,
                        IDPalabra = p.IDPalabra,
                    })
                    .ToList();

                Console.WriteLine($">> [ObtenerPartidasDisponibles] Partidas encontradas: {partidas.Count}");

                foreach (var partida in partidas)
                {
                    Console.WriteLine($"IDPartida: {partida.IDPartida}, Categoria: {partida.CategoriaNombre}");
                }

                return partidas;
            }
        }

        public bool UnirseAPartida(int idPartida, int idJugador)
        {
            using (var db = new AhorcadoContext())
            {
                var partida = db.Partidas.FirstOrDefault(p => p.IDPartida == idPartida && p.IDEstado == 1);
                if (partida == null) return false;

                partida.IDJugadorRetador = idJugador;
                partida.IDEstado = 3; // Cambiar a "En curso"
                db.SaveChanges();

                // Asociar retador a la partida
                jugadorAPartida[idJugador] = idPartida;

                return true;
            }
        }

        public bool AbandonarPartida(int idPartida, int idJugador)
        {
            using (var db = new AhorcadoContext())
            {
                var partida = db.Partidas.Find(idPartida);
                if (partida == null) return false;

                partida.IDEstado = 2; // Cancelada
                partida.IDCancelador = idJugador;

                var cancelador = db.Jugadores.FirstOrDefault(j => j.IDJugador == idJugador);
                if (cancelador != null)
                    cancelador.PuntajeGlobal = Math.Max(0, cancelador.PuntajeGlobal - 3);

                db.SaveChanges();

                // Eliminar la asociación del jugador con la partida para evitar errores en callback
                jugadorAPartida.TryRemove(partida.IDJugadorCreador, out _);
                if (partida.IDJugadorRetador.HasValue)
                    jugadorAPartida.TryRemove(partida.IDJugadorRetador.Value, out _);


                // Notificar a ambos jugadores
                string mensaje = "¡Partida cancelada!";
                string palabra = partida.Palabra.PalabraTexto;
                NotificarFinPartida(partida.IDJugadorCreador, partida.IDJugadorRetador, mensaje, palabra, idPartida);
                return true;
            }
        }

        public bool EnviarLetra(int idPartida, int idJugador, char letra)
        {
            Console.WriteLine($"Entrando al método EnviarLetra");

            using (var db = new AhorcadoContext())
            {
                var partida = db.Partidas.FirstOrDefault(p => p.IDPartida == idPartida);
                if (partida == null || partida.IDEstado != 3)
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

                string palabraConGuiones = GenerarPalabraConGuiones(palabra.PalabraTexto, letrasUsadas);

                var estadoDTO = new PartidaEstadoDTO
                {
                    PalabraConGuiones = palabraConGuiones,
                    IntentosRestantes = partida.IntentosRestantes,
                    LetrasUsadas = letrasUsadas.Select(s => s[0]).ToList(),
                    IDPartida = partida.IDPartida,
                };

                Console.WriteLine("Antes de NotificarEstadoPartida");
                NotificarEstadoPartida(partida.IDJugadorCreador, partida.IDJugadorRetador, estadoDTO, partida.IDPartida);
                Console.WriteLine("después de NotificarEstadoPartida");

                bool adivinada = TodasLetrasAdivinadas(palabra.PalabraTexto, letrasUsadas);
                bool sinIntentos = partida.IntentosRestantes <= 0;

                if (adivinada || sinIntentos)
                {
                    partida.IDEstado = 4; // Concluida

                    if (adivinada)
                    {
                        partida.Ganador = idJugador;
                        var ganador = db.Jugadores.FirstOrDefault(j => j.IDJugador == idJugador);
                        if (ganador != null)
                            ganador.PuntajeGlobal += 10;
                    }
                    else // sinIntentos
                    {
                        partida.Ganador = partida.IDJugadorCreador;
                        var creador = db.Jugadores.FirstOrDefault(j => j.IDJugador == partida.IDJugadorCreador);
                        if (creador != null)
                            creador.PuntajeGlobal += 5;
                    }

                    db.SaveChanges();

                    // Eliminar asociación de ambos jugadores a la partida
                    jugadorAPartida.TryRemove(partida.IDJugadorCreador, out _);
                    if (partida.IDJugadorRetador.HasValue)
                        jugadorAPartida.TryRemove(partida.IDJugadorRetador.Value, out _);


                    NotificarFinPartida(partida.IDJugadorCreador, partida.IDJugadorRetador, "¡Juego terminado!", palabra.PalabraTexto, idPartida);
                }
                else
                {
                    db.SaveChanges();
                }
                return true;
            }
        }

        public PartidaEstadoDTO ObtenerEstadoPartida(int idPartida)
        {
            using (var db = new AhorcadoContext())
            {
                var partida = db.Partidas.FirstOrDefault(p => p.IDPartida == idPartida);
                if (partida == null) return null;

                var palabra = db.Palabras.FirstOrDefault(p => p.IDPalabra == partida.IDPalabra);
                var letrasUsadas = (partida.LetrasUsadas ?? "")
                    .Split(',')
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ToList();

                return new PartidaEstadoDTO
                {
                    PalabraConGuiones = GenerarPalabraConGuiones(palabra.PalabraTexto, letrasUsadas),
                    IntentosRestantes = partida.IntentosRestantes,
                    LetrasUsadas = letrasUsadas.Select(s => s[0]).ToList(),
                    IDPartida = partida.IDPartida,
                };
            }
        }

        private void NotificarEstadoPartida(int idCreador, int? idRetador, PartidaEstadoDTO estado, int idPartida)
        {
            if (jugadorAPartida.TryGetValue(idCreador, out int partidaCreador) && partidaCreador == idPartida)
            {
                if (clientesConectados.TryGetValue(idCreador, out var callbackCreador))
                {
                    try
                    {
                        callbackCreador.ActualizarEstadoPartida(estado);
                    }
                    catch
                    {
                        clientesConectados.TryRemove(idCreador, out _);
                    }
                }
            }

            if (idRetador != null && jugadorAPartida.TryGetValue(idRetador.Value, out int partidaRetador) && partidaRetador == idPartida)
            {
                if (clientesConectados.TryGetValue(idRetador.Value, out var callbackRetador))
                {
                    try
                    {
                        callbackRetador.ActualizarEstadoPartida(estado);
                    }
                    catch
                    {
                        clientesConectados.TryRemove(idRetador.Value, out _);
                    }
                }
            }
        }

        private void NotificarFinPartida(int idCreador, int? idRetador, string resultado, string palabra, int idPartida)
        {
            if (jugadorAPartida.TryGetValue(idCreador, out int partidaCreador) && partidaCreador == idPartida)
            {
                if (clientesConectados.TryGetValue(idCreador, out var callbackCreador))
                {
                    try
                    {
                        callbackCreador.NotificarFinPartida(resultado, palabra, idPartida);
                    }
                    catch
                    {
                        clientesConectados.TryRemove(idCreador, out _);
                    }
                }

                if (idRetador != null && jugadorAPartida.TryGetValue(idRetador.Value, out int partidaRetador) && partidaRetador == idPartida)
                {
                    if (clientesConectados.TryGetValue(idRetador.Value, out var callbackRetador))
                    {
                        try
                        {
                            callbackRetador.NotificarFinPartida(resultado, palabra, idPartida);
                        }
                        catch
                        {
                            clientesConectados.TryRemove(idRetador.Value, out _);
                        }
                    }
                }
            }
        }
        



      /*  public void EnviarMensajeChat(int idPartida, string nombreJugador, string mensaje)
        {
            foreach (var callback in clientesConectados.Values)
            {
                callback.RecibirMensajeChat(nombreJugador, mensaje);
            }
        }*/

        private bool TodasLetrasAdivinadas(string palabra, List<string> letrasUsadas)
        {
            var letrasPalabra = palabra.ToLower().Distinct().Where(c => Char.IsLetter(c)).Select(c => c.ToString());
            return letrasPalabra.All(l => letrasUsadas.Contains(l));
        }

       
        private string GenerarPalabraConGuiones(string palabra, List<string> letrasUsadas)
        {
            return string.Join("", palabra.Select(c =>
                c == ' ' ? " " :
                letrasUsadas.Contains(c.ToString().ToLower()) ? c.ToString() : "_"
            ));
        }

     
        public void ActualizarCallback(int idJugador)
        {
            if (clientesConectados.TryGetValue(idJugador, out var callback))
            {
                // Logic to update the callback for the specified player
                Console.WriteLine($"Notificando a creador {idJugador} - callback hash: {callback.GetHashCode()}");
                Console.WriteLine($"Callback updated for player with ID: {idJugador}");
            }
            else
            {
                Console.WriteLine($"Notificando a creador {idJugador} - callback hash: {callback.GetHashCode()}");
                Console.WriteLine($"No callback found for player with ID: {idJugador}");
            }
        }

        public string Ping() => "Holi <3";
    }
}
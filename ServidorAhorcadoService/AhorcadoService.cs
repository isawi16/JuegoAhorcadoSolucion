using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.ServiceModel;
using BibliotecaClasesNetFramework.Contratos;
using BibliotecaClasesNetFramework.DTO;
using ServidorAhorcadoService.Model;
using System.IO;
using System.Data.Entity; 

namespace ServidorAhorcadoService
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class AhorcadoService : IAhorcadoService
    {
        private static readonly ConcurrentDictionary<int, IAhorcadoCallback> clientesConectados = new ConcurrentDictionary<int, IAhorcadoCallback>();
        private static readonly ConcurrentDictionary<int, int> jugadorAPartida = new ConcurrentDictionary<int, int>();

        private void LogServidor(string mensaje)
        {
            try
            {
                File.AppendAllText("logServidor.txt", DateTime.Now + " | " + mensaje + Environment.NewLine);
            }
            catch
            {
                
            }
        }

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

                    LogServidor($"IniciarSesion: Registrado callback para IDJugador {jugador.IDJugador}, hash {callback.GetHashCode()}");

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
                LogServidor($"Error al iniciar sesión: {ex.Message}");
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
                    LogServidor($"RegistrarJugador: Se registró nuevo jugador {jugador.Correo}");
                    return true;
                }
                catch (Exception ex)
                {
                    LogServidor($"Error al registrar jugador: {ex.Message}");
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
                LogServidor($"ModificarPerfil: Se modificó el perfil de {jugadorModificado.IDJugador}");
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



        public PalabraDTO ObtenerPalabraPorId(int idPalabra)
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
                    IDCategoria = palabra.IDCategoria
                };
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

        public string ObtenerDefinicionPorIdPalabra(int idPalabra)
        {
            using (var db = new AhorcadoContext())
            {
                var palabra = db.Palabras.FirstOrDefault(p => p.IDPalabra == idPalabra);
                return palabra?.Definicion ?? "No hay pista disponible para esta palabra :c";
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

                jugadorAPartida[idCreador] = nueva.IDPartida;

                LogServidor($"CrearPartida: IDPartida {nueva.IDPartida} creada por jugador {idCreador}");

                return nueva.IDPartida;
            }
        }

        public List<PartidaCategoriaDTO> ObtenerPartidasDisponibles()
        {
            using (var db = new AhorcadoContext())
            {
                var partidas = db.Partidas
                    .Where(p => p.IDEstado == 1)
                    .Select(p => new
                    {
                        p.IDPartida,
                        p.IDJugadorCreador,
                        p.IDPalabra,
                        CategoriaNombre = p.Palabra.Categoria.Nombre
                    })
                    .ToList();

                var partidasDisponibles = new List<PartidaCategoriaDTO>();

                foreach (var p in partidas)
                {
                    try
                    {
                        if (clientesConectados.ContainsKey(p.IDJugadorCreador))
                        {
                            var callback = clientesConectados[p.IDJugadorCreador];
                            var commObj = callback as ICommunicationObject;
                            if (commObj != null && commObj.State == CommunicationState.Opened)
                            {
                                partidasDisponibles.Add(new PartidaCategoriaDTO
                                {
                                    IDPartida = p.IDPartida,
                                    CategoriaNombre = p.CategoriaNombre,
                                    IDPalabra = p.IDPalabra
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogServidor($"Error verificando callback para jugador {p.IDJugadorCreador}: {ex.Message}");
                    }
                }

                LogServidor($">> [ObtenerPartidasDisponibles] Partidas encontradas: {partidasDisponibles.Count}");
                foreach (var partida in partidasDisponibles)
                {
                    LogServidor($"IDPartida: {partida.IDPartida}, Categoria: {partida.CategoriaNombre}");
                }

                return partidasDisponibles;
            }
        }

        public bool UnirseAPartida(int idPartida, int idJugador)
        {
            using (var db = new AhorcadoContext())
            {
                var partida = db.Partidas.FirstOrDefault(p => p.IDPartida == idPartida && p.IDEstado == 1);
                if (partida == null) return false;

                if (!clientesConectados.TryGetValue(partida.IDJugadorCreador, out var callbackCreador) ||
                    ((ICommunicationObject)callbackCreador).State != CommunicationState.Opened)
                {
                    partida.IDEstado = 4; // "Concluida"
                    db.SaveChanges();
                    clientesConectados.TryRemove(partida.IDJugadorCreador, out _); // Limpia callback
                    LogServidor($"UnirseAPartida: callback de creador no válido o cerrado para partida {idPartida}");
                    return false;
                }

                partida.IDJugadorRetador = idJugador;
                partida.IDEstado = 3; // "En curso"
                db.SaveChanges();

                jugadorAPartida[idJugador] = idPartida;

                LogServidor($"UnirseAPartida: Jugador {idJugador} se unió a partida {idPartida}");

                return true;
            }
        }

        public bool AbandonarPartida(int idPartida, int idJugador)
        {
            using (var db = new AhorcadoContext())
            {
                var partida = db.Partidas.Find(idPartida);
                if (partida == null)
                {
                    LogServidor($"[AbandonarPartida] Partida {idPartida} no encontrada, NO SE NOTIFICA FIN PARTIDA");
                    return false;
                }

                partida.IDEstado = 2; // Cancelada
                partida.IDCancelador = idJugador;

                var cancelador = db.Jugadores.FirstOrDefault(j => j.IDJugador == idJugador);
                if (cancelador != null)
                    cancelador.PuntajeGlobal = Math.Max(0, cancelador.PuntajeGlobal - 3);

                db.SaveChanges();

                jugadorAPartida.TryRemove(partida.IDJugadorCreador, out _);
                if (partida.IDJugadorRetador.HasValue)
                    jugadorAPartida.TryRemove(partida.IDJugadorRetador.Value, out _);

                LogServidor($"[AbandonarPartida] Llamando NotificarFinPartida: creador={partida.IDJugadorCreador}, retador={partida.IDJugadorRetador}, palabra={(partida.Palabra != null ? partida.Palabra.PalabraTexto : "(NULL)")}, idPartida={idPartida}, ganador=null");
                NotificarFinPartida(partida.IDJugadorCreador, partida.IDJugadorRetador, partida.Palabra.PalabraTexto, idPartida, null);
                return true;
            }
        }


        public bool EnviarLetra(int idPartida, int idJugador, char letra)
        {
            LogServidor($"Entrando al método EnviarLetra para partida {idPartida}, jugador {idJugador}, letra '{letra}'");

            using (var db = new AhorcadoContext())
            {
                var partida = db.Partidas.FirstOrDefault(p => p.IDPartida == idPartida);
                if (partida == null || partida.IDEstado != 3)
                {
                    LogServidor($"EnviarLetra: Partida {idPartida} no encontrada o no en curso");
                    return false;
                }

                string letraStr = letra.ToString().ToLower();
                var palabra = db.Palabras.FirstOrDefault(p => p.IDPalabra == partida.IDPalabra);
                if (palabra == null)
                {
                    LogServidor($"EnviarLetra: Palabra no encontrada para partida {idPartida}");
                    return false;
                }

                if (partida.LetrasUsadas == null)
                    partida.LetrasUsadas = "";

                var letrasUsadas = partida.LetrasUsadas.Split(',')
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ToList();

                if (letrasUsadas.Contains(letraStr))
                {
                    LogServidor($"EnviarLetra: Letra '{letraStr}' ya usada en partida {idPartida}");
                    return false;
                }

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

                LogServidor("Antes de NotificarEstadoPartida");
                NotificarEstadoPartida(partida.IDJugadorCreador, partida.IDJugadorRetador, estadoDTO, partida.IDPartida);
                LogServidor("Después de NotificarEstadoPartida");

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
                        LogServidor($"EnviarLetra: Partida {idPartida} adivinada. Ganador: {idJugador}");
                    }
                    else
                    {
                        partida.Ganador = partida.IDJugadorCreador;
                        var creador = db.Jugadores.FirstOrDefault(j => j.IDJugador == partida.IDJugadorCreador);
                        if (creador != null)
                            creador.PuntajeGlobal += 5;
                        LogServidor($"EnviarLetra: Partida {idPartida} sin intentos. Ganador: {partida.IDJugadorCreador}");
                    }

                    db.SaveChanges();

                    LogServidor($"Llamando a NotificarFinPartida con idCreador={partida.IDJugadorCreador}, idRetador={partida.IDJugadorRetador}, palabra={partida.Palabra.PalabraTexto}, idPartida={idPartida}, ganador=null");
                    NotificarFinPartida(partida.IDJugadorCreador, partida.IDJugadorRetador, partida.Palabra.PalabraTexto, idPartida, null);

                    jugadorAPartida.TryRemove(partida.IDJugadorCreador, out _);
                    if (partida.IDJugadorRetador.HasValue)
                        jugadorAPartida.TryRemove(partida.IDJugadorRetador.Value, out _);
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
            void Notificar(int idJugador)
            {
                if (jugadorAPartida.TryGetValue(idJugador, out int partidaJugador) && partidaJugador == idPartida)
                {
                    if (clientesConectados.TryGetValue(idJugador, out var callback))
                    {
                        var commObj = (ICommunicationObject)callback;
                        LogServidor($"NotificarEstadoPartida: A jugador {idJugador} partida {idPartida}, canal: {commObj.State}");

                        if (commObj.State == CommunicationState.Opened)
                        {
                            try
                            {
                                callback.ActualizarEstadoPartida(estado);
                                LogServidor($"NotificarEstadoPartida: Éxito para jugador {idJugador}");
                            }
                            catch (Exception ex)
                            {
                                LogServidor($"Error enviando ActualizarEstadoPartida a {idJugador}: {ex.Message}");
                                clientesConectados.TryRemove(idJugador, out _);
                                jugadorAPartida.TryRemove(idJugador, out _);
                            }
                        }
                        else
                        {
                            LogServidor($"NotificarEstadoPartida: Canal cerrado para jugador {idJugador}, removiendo callback");
                            clientesConectados.TryRemove(idJugador, out _);
                            jugadorAPartida.TryRemove(idJugador, out _);
                        }
                    }
                    else
                    {
                        LogServidor($"NotificarEstadoPartida: No se encontró callback para jugador {idJugador}");
                    }
                }
                else
                {
                    LogServidor($"NotificarEstadoPartida: Jugador {idJugador} no está asociado a partida {idPartida}");
                }
            }

            Notificar(idCreador);
            if (idRetador.HasValue)
                Notificar(idRetador.Value);
        }

        private void NotificarFinPartida(int idCreador, int? idRetador, string palabra, int idPartida, int? idGanador)
        {
            LogServidor($"[NotificarFinPartida] Entrando. idCreador={idCreador}, idRetador={idRetador}, palabra={palabra}, idPartida={idPartida}, idGanador={idGanador}");

            void Notificar(int idJugador)
            {
                if (clientesConectados.TryGetValue(idJugador, out var callback))
                {
                    var commObj = (ICommunicationObject)callback;
                    LogServidor($"NotificarFinPartida: A jugador {idJugador} partida {idPartida}, canal: {commObj.State}");

                    if (commObj.State == CommunicationState.Opened)
                    {
                        string resultado;
                        if (idGanador.HasValue)
                        {
                            if (idJugador == idGanador.Value)
                            {
                                resultado = "¡Ganaste!";
                            }
                            else if (idRetador.HasValue && idJugador == idRetador.Value)
                            {
                                resultado = "¡Perdiste!";
                            }
                            else if (idJugador == idCreador)
                            {
                                resultado = "La palabra ha sido adivinada";
                            }
                            else
                            {
                                resultado = "¡Juego terminado!"; 
                            }
                        }
                        else
                        {
                            resultado = "¡Juego terminado!"; // cancelada
                        }
                        try
                        {
                            callback.NotificarFinPartida(resultado, palabra, idPartida, idJugador);
                            LogServidor($"NotificarFinPartida: Éxito para jugador {idJugador} - {resultado}");
                        }
                        catch (Exception ex)
                        {
                            LogServidor($"Error enviando NotificarFinPartida a {idJugador}: {ex.Message}");
                            clientesConectados.TryRemove(idJugador, out _);
                            jugadorAPartida.TryRemove(idJugador, out _);
                        }
                    }
                    else
                    {
                        LogServidor($"NotificarFinPartida: Canal cerrado para jugador {idJugador}, removiendo callback");
                        clientesConectados.TryRemove(idJugador, out _);
                        jugadorAPartida.TryRemove(idJugador, out _);
                    }
                }
                else
                {
                    LogServidor($"NotificarFinPartida: No se encontró callback para jugador {idJugador}");
                }
            }


            Notificar(idCreador);
            if (idRetador.HasValue)
                Notificar(idRetador.Value);
        }

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
                LogServidor($"Notificando a creador {idJugador} - callback hash: {callback.GetHashCode()}");
                LogServidor($"Callback updated for player with ID: {idJugador}");
            }
            else
            {
                LogServidor($"No callback found for player with ID: {idJugador}");
            }
        }


    }
}

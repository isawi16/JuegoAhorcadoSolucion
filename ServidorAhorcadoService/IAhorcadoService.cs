using System;
using System.Collections.Generic;
using System.ServiceModel;
using ServidorAhorcadoService.DTO;

namespace ServidorAhorcadoService
{
    [ServiceContract(CallbackContract = typeof(IAhorcadoCallback))]
    public interface IAhorcadoService
    {
        // --- AUTENTICACIÓN Y USUARIO ---
        [OperationContract]
        JugadorDTO IniciarSesion(string correo, string password);

        [OperationContract]
        bool RegistrarJugador(JugadorDTO jugador);

        [OperationContract]
        JugadorDTO ConsultarPerfil(int idJugador);

        [OperationContract]
        bool ModificarPerfil(JugadorDTO jugadorModificado);

        [OperationContract]
        int ObtenerPuntajeGlobal(int idJugador);

        [OperationContract]
        List<PartidaDTO> ConsultarPartidasJugadas(int idJugador);

        // --- PALABRAS Y CATEGORÍAS ---
        [OperationContract]
        List<CategoriaDTO> ObtenerCategoriasPorIdioma(string codigoIdioma);

        [OperationContract]
        PalabraDTO ObtenerPalabraConDescripcion(int idPalabra, string idioma);

        [OperationContract]
        List<PalabraDTO> ObtenerPalabrasPorCategoria(int idCategoria, string idioma);

        // --- PARTIDAS Y JUEGO ---
        [OperationContract]
        bool CrearPartida(int idCreador, int idPalabra);

        [OperationContract]
        List<PartidaDTO> ObtenerPartidasDisponibles();

        [OperationContract]
        bool UnirseAPartida(int idPartida, int idJugador);

        [OperationContract]
        bool AbandonarPartida(int idPartida, int idJugador);

        [OperationContract]
        bool EnviarLetra(int idPartida, int idJugador, char letra);

        [OperationContract]
        PartidaEstadoDTO ObtenerEstadoPartida(int idPartida);

        // --- CHAT Y COMUNICACIÓN ---
        [OperationContract]
        void EnviarMensajeChat(int idPartida, string nombreJugador, string mensaje);
    }
}
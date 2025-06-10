using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using ServidorAhorcadoService.DTO;

namespace ServidorAhorcadoService
{

    [ServiceContract]
    public interface IAhorcadoCallback
    {
        [OperationContract(IsOneWay = true)]
        void RecibirMensajeChat(string nombreJugador, string mensaje);

        [OperationContract(IsOneWay = true)]
        void NotificarFinPartida(string resultado, string palabra);

        [OperationContract(IsOneWay = true)]
        void ActualizarEstadoPartida(PartidaEstadoDTO estadoActual);


        // NUEVOS EVENTOS OPCIONALES

        /*[OperationContract(IsOneWay = true)]
        void JugadorSeUnio(string nombreJugador);

        [OperationContract(IsOneWay = true)]
        void JugadorAbandono(string nombreJugador);

        [OperationContract(IsOneWay = true)]
        void CambiarTurno(string nombreJugadorActual);

        [OperationContract(IsOneWay = true)]
        void ActualizarCantidadJugadores(int cantidadConectados);
        */
    }
}


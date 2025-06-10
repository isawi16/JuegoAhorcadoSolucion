using ClienteAhorcado;
using ServidorAhorcadoService;
using ServidorAhorcadoService.DTO;
using System.Windows;

namespace ClienteAhorcado
{
    public class AhorcadoCallbackCliente : IAhorcadoCallback
    {
        private readonly MainWindow mainWindow;

        public AhorcadoCallbackCliente(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
        }

        public void RecibirMensajeChat(string nombreJugador, string mensaje)
        {
            mainWindow.MostrarMensajeChat(nombreJugador, mensaje);
        }

        public void NotificarFinPartida(string resultado, string palabra)
        {
            MessageBox.Show($"Fin de partida: {resultado}. La palabra era: {palabra}");
        }

        public void ActualizarEstadoPartida(PartidaEstadoDTO estadoActual)
        {
            mainWindow.ActualizarEstadoPartida(estadoActual);
        }
    }
}


using ClienteAhorcado.Vistas;
using BibliotecaClasesNetFramework.Contratos;
using BibliotecaClasesNetFramework.DTO;
using System.Windows;

namespace ClienteAhorcado.Utilidades
{
    public class AhorcadoCallbackCliente : IAhorcadoCallback
    {
        private readonly MainWindow mainWindow;

        public AhorcadoCallbackCliente(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
        }

        public void ActualizarEstadoPartida(PartidaEstadoDTO estadoActual)
        {
            mainWindow.Dispatcher.Invoke(() =>
            {
                if (mainWindow.MainContent.Content is JuegoAhorcadoUserControl1 juegoControl)
                    juegoControl.ActualizarDesdeCallback(estadoActual);
            });
        }

       /* public void RecibirMensajeChat(string nombreJugador, string mensaje)
        {
            mainWindow.Dispatcher.Invoke(() =>
            {
                if (mainWindow.MainContent.Content is JuegoAhorcadoUserControl1 juegoControl)
                    juegoControl.AgregarMensajeChat(nombreJugador, mensaje);
            });
        }*/

        public void NotificarFinPartida(string resultado, string palabra)
        {
            mainWindow.Dispatcher.Invoke(() =>
            {
                mainWindow.NotificarFinPartida(resultado, palabra);
            });
        }
    }
}

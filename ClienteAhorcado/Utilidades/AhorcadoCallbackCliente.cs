using BibliotecaClasesNetFramework.Contratos;
using BibliotecaClasesNetFramework.DTO;
using ClienteAhorcado.Vistas;
using System;
using System.Diagnostics;
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
            Debug.WriteLine("Entrando a actualizar estado partida");
            try
            {
                
                if (mainWindow.MainContent.Content is JuegoAhorcadoUserControl1 juegoControl)
                    juegoControl.ActualizarDesdeCallback(estadoActual);
            }
            catch (Exception ex)
            {
               Console.WriteLine($"Error en ActualizarEstadoPartida: {ex.Message}");
            }
        }

        /*
        public void RecibirMensajeChat(string nombreJugador, string mensaje)
        {
            mainWindow.Dispatcher.Invoke(() =>
            {
                if (mainWindow.MainContent.Content is JuegoAhorcadoUserControl1 juegoControl)
                    juegoControl.AgregarMensajeChat(nombreJugador, mensaje);
            });
        }
        */

        public void NotificarFinPartida(string resultado, string palabra, int IDPartida)
        {
            Debug.WriteLine("Entrando a actualizar estado partida");
            mainWindow.Dispatcher.Invoke(() =>
            {
                Console.WriteLine("Recibido Callback notificar fin partida: ");
                mainWindow.NotificarFinPartida(resultado, palabra, IDPartida);
            });
        }
    }
}

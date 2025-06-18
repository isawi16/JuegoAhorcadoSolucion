using System;
using System.Windows;
using ServidorAhorcadoService;
using ServidorAhorcadoService.DTO;

namespace ClienteAhorcado.Vistas
{
    // Implementa la interfaz del callback WCF
    public class AhorcadoCallback : IAhorcadoCallback
    {
        private readonly JuegoAhorcadoUserControl1 _control;

        public AhorcadoCallback(JuegoAhorcadoUserControl1 control)
        {
            _control = control;
        }

        // Actualiza la partida automáticamente en el cliente
        public void ActualizarEstadoPartida(PartidaEstadoDTO estadoActual)
        {
            // Importante: actualizar la UI en el hilo de interfaz
            Application.Current.Dispatcher.Invoke(() =>
            {
                _control.ActualizarDesdeCallback(estadoActual);
            });
        }

        public void RecibirMensajeChat(string nombreJugador, string mensaje)
        {
            // (Puedes implementar tu lógica de chat aquí si quieres)
        }

        public void NotificarFinPartida(string resultado, string palabra)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show($"{resultado}. La palabra era: {palabra}", "Fin de partida");
            });
        }
    }
}

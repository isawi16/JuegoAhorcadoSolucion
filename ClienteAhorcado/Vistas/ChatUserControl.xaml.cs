using ServidorAhorcadoService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ClienteAhorcado.Vistas
{
    public partial class ChatUserControl : UserControl
    {
        private IAhorcadoService _ahorcadoService;
        private int _idPartida; // ID de la partida actual

        public ChatUserControl(int idPartida)
        {
            InitializeComponent();
            _idPartida = idPartida;
            _ahorcadoService = new AhorcadoService(); // O usa la instancia correspondiente del servicio
            CargarMensajes();
        }

        // Cargar los mensajes del chat (si es necesario, los mensajes pueden venir del servidor)
        private void CargarMensajes()
        {
            // Simulación de carga de mensajes (puedes obtenerlos desde el servidor)
            lstMensajes.Items.Add("Jugador 1: ¡Vamos a ganar!");
            lstMensajes.Items.Add("Jugador 2: ¡Sí, estamos listos!");
        }

        // Enviar un mensaje al servidor
        private void EnviarMensaje(object sender, RoutedEventArgs e)
        {
            var mensaje = txtMensaje.Text;
            if (!string.IsNullOrEmpty(mensaje))
            {
                _ahorcadoService.EnviarMensajeChat(_idPartida, "Jugador 1", mensaje); // Nombre del jugador y mensaje
                lstMensajes.Items.Add("Jugador 1: " + mensaje);
                txtMensaje.Clear();
            }
        }
    }
}


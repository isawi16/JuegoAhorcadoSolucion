using ClienteAhorcado;
using ServidorAhorcadoService;
using ServidorAhorcadoService.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ClienteAhorcado.Vistas
{
    public partial class JuegoAhorcadoUserControl1 : UserControl
    {
        private string palabraSecreta;
        private HashSet<char> letrasCorrectas;
        private HashSet<char> letrasUsadas;
        private int intentosRestantes;
        private TcpClient clienteSocket;
        private NetworkStream stream;
        private string nombreJugador;
        private int idPartida;
        private JugadorDTO jugador;
        private PartidaDTO partida;
        private bool esCreador;
        private IAhorcadoService proxy;


        public JuegoAhorcadoUserControl1(string palabra, string jugador, int partidaID)
        {
            InitializeComponent();
            palabraSecreta = palabra.ToUpper();
            nombreJugador = jugador;
            idPartida = partidaID;
            letrasCorrectas = new HashSet<char>();
            letrasUsadas = new HashSet<char>();
            intentosRestantes = 6;

            InicializarPalabra();
            GenerarBotonesLetras();
            ActualizarEstado();
            ConectarChat();
            ConfigurarRol();
        }

        public JuegoAhorcadoUserControl1(JugadorDTO jugador, PartidaDTO partida, bool esCreador)
        {
            InitializeComponent();
            this.jugador = jugador;
            this.partida = partida;
            this.esCreador = esCreador;
            this.palabraSecreta = partida.PalabraTexto.ToUpper();

            InicializarPalabra();
            GenerarBotonesLetras();
            ActualizarEstado();
            ConectarChat();
            ConfigurarRol();
        }

        private void ConfigurarRol()
        {
            if (esCreador)
            {
                wrapLetras.IsEnabled = false;
                foreach (Button btn in wrapLetras.Children)
                {
                    btn.IsEnabled = false;
                }

                // También podrías resaltar visualmente el rol de observador
                this.Background = new SolidColorBrush(Colors.DarkSlateGray);
            }
        }

        private void InicializarPalabra()
        {
            stackPalabra.Children.Clear();
            foreach (char c in palabraSecreta)
            {
                TextBlock letra = new TextBlock
                {
                    Text = "_",
                    FontSize = 24,
                    Margin = new Thickness(5)
                };
                stackPalabra.Children.Add(letra);
            }
        }

        private void GenerarBotonesLetras()
        {
            wrapLetras.Children.Clear();
            for (char c = 'A'; c <= 'Z'; c++)
            {
                Button btn = new Button
                {
                    Content = c.ToString(),
                    Width = 30,
                    Height = 30,
                    Margin = new Thickness(2)
                };
                btn.Click += BtnLetra_Click;
                wrapLetras.Children.Add(btn);
            }
        }

        private void BtnLetra_Click(object sender, RoutedEventArgs e)
        {
            if (esCreador) return; // El creador no juega

            Button btn = sender as Button;
            btn.IsEnabled = false;
            char letra = btn.Content.ToString()[0];

            try
            {
                proxy.EnviarLetra(partida.IDPartida, jugador.IDJugador, letra);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al enviar la letra: " + ex.Message);
            }
        }

        private void MostrarLetrasCorrectas()
        {
            for (int i = 0; i < palabraSecreta.Length; i++)
            {
                char c = palabraSecreta[i];
                TextBlock tb = (TextBlock)stackPalabra.Children[i];
                if (letrasCorrectas.Contains(c))
                {
                    tb.Text = c.ToString();
                }
            }
        }

        private void ActualizarImagen()
        {
            imgAhorcado.Source = new BitmapImage(new Uri($"/Images/ahorcado{intentosRestantes}.png", UriKind.Relative));
        }

        private void ActualizarEstado()
        {
            txtIntentosRestantes.Text = intentosRestantes.ToString();
            txtLetrasUsadas.Text = string.Join(", ", letrasUsadas);
        }

        private void ConectarChat()
        {
            try
            {
                clienteSocket = new TcpClient("127.0.0.1", 5000); // Puerto del servidor de chat
                stream = clienteSocket.GetStream();

                Task.Run(() => LeerMensajes());
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo conectar al chat: " + ex.Message);
            }
        }

        private void LeerMensajes()
        {
            byte[] buffer = new byte[1024];
            int bytesRead;

            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                string mensaje = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Dispatcher.Invoke(() =>
                {
                    MostrarMensajeChat(mensaje);
                });
            }
        }

        private void BtnEnviar_Click(object sender, RoutedEventArgs e)
        {
            string mensaje = txtMensaje.Text.Trim();
            if (!string.IsNullOrEmpty(mensaje))
            {
                // Enviar el mensaje al servidor
                proxy.EnviarMensajeChat(idPartida, nombreJugador, mensaje);

                // Mostrar el mensaje en el chat local del cliente
                string mensajeCompleto = $"{nombreJugador}: {mensaje}";
                MostrarMensajeChat(mensajeCompleto);

                // Limpiar el campo de texto después de enviar el mensaje
                txtMensaje.Clear();
            }
        }

        public void MostrarMensajeChat(string mensaje)
        {
            // Agregar el mensaje al TextBox de chat y desplazar hacia abajo
            txtChat.Text += $"{mensaje}\n";
            txtChat.ScrollToEnd(); // Esto asegura que el último mensaje siempre sea visible
        }

        public void ActualizarEstadoDesdeCallback(PartidaEstadoDTO estado)
        {
            txtIntentosRestantes.Text = estado.IntentosRestantes.ToString();
            txtLetrasUsadas.Text = string.Join(", ", estado.LetrasUsadas);

            // Actualiza la palabra en pantalla con las letras descubiertas
            for (int i = 0; i < estado.PalabraConGuiones.Length; i++)
            {
                var letra = estado.PalabraConGuiones[i].ToString();
                if (i < stackPalabra.Children.Count && stackPalabra.Children[i] is TextBlock tb)
                {
                    tb.Text = letra == "_" ? "_" : letra;
                }
            }

            // Cambia la imagen del ahorcado según los intentos restantes
            imgAhorcado.Source = new BitmapImage(new Uri($"/Images/ahorcado{estado.IntentosRestantes}.png", UriKind.Relative));
        }
    }
}


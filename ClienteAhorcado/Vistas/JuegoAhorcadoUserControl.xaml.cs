using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Net.Sockets;
using System.Text;


namespace ClienteAhorcado.Vistas
{
    public partial class JuegoAhorcadoUserControl : UserControl
    {
        private string _palabraSecreta = "EXAMPLE"; // Debe venir del servidor
        private int _intentosRestantes = 7;
        private List<string> _letrasUsadas = new List<string>();
        private TcpClient _cliente;
        private NetworkStream _stream;
        private const string _ipServidor = "127.0.0.1"; // IP del servidor (puedes cambiarla)
        private const int _puerto = 12345; // Puerto que usará la conexión

        private void ConectarServidor()
        {
            try
            {
                _cliente = new TcpClient(_ipServidor, _puerto);
                _stream = _cliente.GetStream();
                EscucharMensajes(); // Comenzar a escuchar mensajes del servidor
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al conectar al servidor: {ex.Message}");
            }
        }
        public JuegoAhorcadoUserControl()
        {
            InitializeComponent();
            InicializarJuego();
        }

        private void InicializarJuego()
        {
            CargarImagen();
            stackPalabra.Children.Clear();

            foreach (char letra in _palabraSecreta)
            {
                stackPalabra.Children.Add(new TextBlock
                {
                    Text = "_",
                    FontSize = 24,
                    Margin = new Thickness(5)
                });
            }
        }
        private void CargarImagen()
        {
            imgAhorcado.Source = new BitmapImage(new Uri($"pack://application:,,,/Images/{_intentosRestantes}.png"));
        }

        private void CargarPalabra()
        {
            stackPalabra.Children.Clear();
            foreach (char letra in _palabraSecreta)
            {
                stackPalabra.Children.Add(new TextBlock
                {
                    Text = "_",
                    FontSize = 24,
                    Margin = new Thickness(5)
                });
            }
        }

       /* private void CargarBotonesLetras()
        {
            string alfabeto = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            foreach (char letra in alfabeto)
            {
                Button btnLetra = new Button
                {
                    Content = letra.ToString(),
                    Width = 40,
                    Height = 40,
                    Margin = new Thickness(5)
                };
                btnLetra.Click += LetraSeleccionada;
                wrapPanelLetras.Children.Add(btnLetra);
            }
        }*/


        private void LetraSeleccionada(object sender, RoutedEventArgs e)
        {
            Button boton = sender as Button;
            if (boton == null) return;

            string letra = boton.Content.ToString();

            if (_letrasUsadas.Contains(letra)) return;

            boton.IsEnabled = false;
            boton.Background = Brushes.Gray;
            _letrasUsadas.Add(letra);

            string mensaje = $"LETRA:{letra}";
            EnviarMensajeServidor(mensaje);

        bool acierto = false;
            for (int i = 0; i < _palabraSecreta.Length; i++)
            {
                if (_palabraSecreta[i].ToString().Equals(letra, StringComparison.OrdinalIgnoreCase))
                {
                    acierto = true;
                    (stackPalabra.Children[i] as TextBlock).Text = letra;
                }
            }

            if (!acierto)
            {
                _intentosRestantes--;
                CargarImagen();

                if (_intentosRestantes == 0)
                    MessageBox.Show($"¡Has perdido! La palabra era: {_palabraSecreta}");
            }
        }

        private void EnviarMensaje(object sender, RoutedEventArgs e)

        {
            if (!string.IsNullOrWhiteSpace(txtMensaje.Text))
            {
                lstChat.Items.Add($"Jugador: {txtMensaje.Text}");
                txtMensaje.Clear();
            }
        }

        private void EnviarMensajeServidor(string mensaje)
        {
            try
            {
                if (_stream != null)
                {
                    byte[] datos = Encoding.UTF8.GetBytes(mensaje);
                    _stream.Write(datos, 0, datos.Length);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al enviar mensaje al servidor: {ex.Message}");
            }
        }

        private async void EscucharMensajes()
        {
            try
            {
                byte[] buffer = new byte[1024];

                while (true)
                {
                    int bytesLeidos = await _stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesLeidos > 0)
                    {
                        string mensajeRecibido = Encoding.UTF8.GetString(buffer, 0, bytesLeidos);
                        Dispatcher.Invoke(() => ProcesarMensaje(mensajeRecibido));
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Se perdió la conexión con el servidor.");
            }
        }

        private void ProcesarMensaje(string mensaje)
        {
            if (mensaje.StartsWith("LETRA:"))
            {
                string letraRecibida = mensaje.Split(':')[1];

                // Buscar el botón correspondiente y deshabilitarlo
                foreach (Button btn in wrapPanelLetras.Children)
                {
                    if (btn.Content.ToString() == letraRecibida)
                    {
                        btn.IsEnabled = false;
                        btn.Background = Brushes.Gray;
                    }
                }
            }
        }


        private void TxtMensaje_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.Text = "Escribe un mensaje...";
                textBox.Foreground = Brushes.Gray;
                textBox.CaretIndex = 0; // Para evitar que el cursor salte al final
            }
            else
            {
                textBox.Foreground = Brushes.Black;
            }
        }

    }
}

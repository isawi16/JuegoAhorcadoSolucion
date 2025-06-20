using BibliotecaClasesNetFramework.Contratos;
using BibliotecaClasesNetFramework.DTO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ClienteAhorcado.Vistas
{
    public partial class JuegoAhorcadoUserControl1 : UserControl
    {
        private string palabraSecreta;
        private int intentosRestantes;
        private List<char> letrasUsadas = new List<char>();
        private bool esCreador;
        private MainWindow mainWindow;
        private JugadorDTO jugador;
        private IAhorcadoService proxy;
        private int idPartida;
        private PalabraDTO palabra;

        // Para logs
        private bool debugLogs = true;
        private string rutaLog = "logCliente.txt";
        private void LogCliente(string mensaje)
        {
            if (!debugLogs) return;
            try { File.AppendAllText(rutaLog, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {mensaje}{Environment.NewLine}"); }
            catch { }
        }

        public JuegoAhorcadoUserControl1(JugadorDTO jugador, PalabraDTO palabra, int idPartida, bool esCreador, IAhorcadoService proxy)
        {
            InitializeComponent();
            txtIntentosRestantes.Foreground = new SolidColorBrush(Colors.White);
            txtLetrasUsadas.Foreground = new SolidColorBrush(Colors.White);

            this.proxy = proxy;
            this.jugador = jugador;
            this.palabraSecreta = palabra.Texto.ToUpper();
            this.intentosRestantes = 6;
            this.esCreador = esCreador;
            this.palabra = palabra;
            this.idPartida = idPartida;
            this.mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();

            LogCliente($"INICIO JuegoAhorcadoUserControl1 | Jugador {jugador.IDJugador} | Partida {idPartida} | {(esCreador ? "Creador" : "Retador")}");

            InicializarPalabra();
            GenerarBotonesLetras();
            ActualizarEstado();
            ConfigurarRol();
            txtFinPartidaNotificacion.Visibility = Visibility.Collapsed;
            btnVolverMenu.Visibility = Visibility.Collapsed;
        }

        private void ConfigurarRol()
        {
            if (esCreador)
            {
                wrapLetras.IsEnabled = false;
                foreach (Button btn in wrapLetras.Children)
                    btn.IsEnabled = false;
                this.Background = new SolidColorBrush(Colors.DarkSlateGray);
            }
            else
            {
                wrapLetras.IsEnabled = true;
                foreach (Button btn in wrapLetras.Children)
                    btn.IsEnabled = true;
                this.Background = new SolidColorBrush(Colors.White);
            }
        }

        private void InicializarPalabra()
        {
            stackPalabra.Children.Clear();
            foreach (char c in palabraSecreta)
            {
                string mostrar = c == ' ' ? " " : "_";
                TextBlock letra = new TextBlock
                {
                    Text = mostrar,
                    FontSize = 24,
                    Margin = new Thickness(5),
                    Foreground = new SolidColorBrush(Colors.White)
                };
                stackPalabra.Children.Add(letra);
            }
        }

        private void GenerarBotonesLetras()
        {
            wrapLetras.Children.Clear();
            for (char c = 'A'; c <= 'Z'; c++)
            {
                var btn = new Button
                {
                    Content = c.ToString(),
                    Width = 30,
                    Height = 30,
                    Margin = new Thickness(2)
                };

                if (esCreador)
                {
                    btn.IsEnabled = false;
                    btn.Background = new SolidColorBrush(Colors.LightGray);
                    btn.Foreground = new SolidColorBrush(Colors.Black);
                }
                else
                {
                    btn.IsEnabled = true;
                    btn.Background = new SolidColorBrush(Colors.SteelBlue);
                    btn.Foreground = new SolidColorBrush(Colors.White);

                    btn.Click += BtnLetra_Click;
                }

                wrapLetras.Children.Add(btn);
            }
        }

        private async void BtnLetra_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn)) return;
            btn.IsEnabled = false;
            char letra = btn.Content.ToString()[0];
            try
            {
                LogCliente($"BtnLetra_Click | Jugador {jugador.IDJugador} | Partida {idPartida} | Letra '{letra}' enviada");
                await Task.Run(() => proxy.EnviarLetra(idPartida, jugador.IDJugador, letra));
            }
            catch (Exception ex)
            {
                LogCliente($"ERROR BtnLetra_Click | Jugador {jugador.IDJugador} | Partida {idPartida} | {ex.Message}");
            }
        }

        private async void BtnObtenerIdea_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int idPalabra = this.palabra.IDPalabra;
                string definicion = await Task.Run(() => proxy.ObtenerDefinicionPorIdPalabra(idPalabra));
                tbIdea.Text = !string.IsNullOrWhiteSpace(definicion)
                    ? definicion
                    : "No hay pista disponible para esta palabra :c";
            }
            catch (Exception ex)
            {
                tbIdea.Text = $"Error al obtener la pista: {ex.Message}";
            }
        }

        private void ActualizarEstado()
        {
            txtIntentosRestantes.Text = intentosRestantes.ToString();
            txtLetrasUsadas.Text = string.Join(", ", letrasUsadas);
        }

        private void ActualizarImagenAhorcado()
        {
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(200));
            fadeOut.Completed += (s, e) =>
            {
                imgAhorcado.Source = new BitmapImage(new Uri($"/Images/{intentosRestantes}.png", UriKind.Relative));
                var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200));
                imgAhorcado.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            };
            imgAhorcado.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }

        private void LanzarConfeti()
        {
            var rand = new Random();
            for (int i = 0; i < 30; i++)
            {
                double size = rand.Next(20, 31);
                var estrella = new Ellipse
                {
                    Width = size,
                    Height = size,
                    Fill = new SolidColorBrush(Color.FromRgb(
                        (byte)rand.Next(100, 256),
                        (byte)rand.Next(100, 256),
                        (byte)rand.Next(100, 256))),
                    Opacity = 0.85
                };
                Canvas.SetLeft(estrella, rand.Next((int)this.ActualWidth));
                Canvas.SetTop(estrella, -size);
                // Asegúrate de tener canvasEfectos en tu XAML si usas esto
            }
        }

        private void BtnVolverMenu_Click(object sender, RoutedEventArgs e)
        {
            LogCliente($"BtnVolverMenu_Click | Jugador {jugador.IDJugador} | Partida {idPartida} | Volver al menú");
            mainWindow.CambiarVista(new MenuPrincipalUserControl(mainWindow, jugador, proxy));
        }

        public void ActualizarDesdeCallback(PartidaEstadoDTO estado)
        {
            LogCliente($"ActualizarDesdeCallback | Jugador {jugador.IDJugador} | Partida local {idPartida} | Callback con IDPartida {estado.IDPartida}");

            if (estado.IDPartida != this.idPartida)
            {
                LogCliente($"ActualizarDesdeCallback | Jugador {jugador.IDJugador} | Se ignoró callback por IDPartida distinto");
                return;
            }

            this.intentosRestantes = estado.IntentosRestantes;
            this.letrasUsadas = estado.LetrasUsadas ?? new List<char>();

            txtLetrasUsadas.Text = string.Join(", ", letrasUsadas);
            txtIntentosRestantes.Text = intentosRestantes.ToString();

            for (int i = 0; i < estado.PalabraConGuiones.Length && i < stackPalabra.Children.Count; i++)
            {
                if (stackPalabra.Children[i] is TextBlock tb)
                {
                    tb.Text = estado.PalabraConGuiones[i].ToString();
                    tb.Opacity = 0;
                    var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300));
                    tb.BeginAnimation(UIElement.OpacityProperty, fadeIn);
                }
            }

            ActualizarImagenAhorcado();

            if (esCreador)
            {
                foreach (Button btn in wrapLetras.Children)
                    btn.IsEnabled = false;
            }
            else
            {
                foreach (Button btn in wrapLetras.Children)
                {
                    if (btn.Content is string letra)
                    {
                        char letraBtn = letra.ToUpper()[0];
                        bool usada = letrasUsadas.Any(l => char.ToUpper(l) == letraBtn);
                        btn.IsEnabled = !usada && intentosRestantes > 0 && estado.PalabraConGuiones.Contains('_');
                    }
                }
            }

            if (intentosRestantes <= 0 || !estado.PalabraConGuiones.Contains('_'))
            {
                //LanzarConfeti();
                foreach (Button btn in wrapLetras.Children)
                    btn.IsEnabled = false;
                btnVolverMenu.Visibility = Visibility.Visible;
                LogCliente($"ActualizarDesdeCallback | Jugador {jugador.IDJugador} | Partida {idPartida} terminada");
            }
        }

        public void NotificarFinPartida(string mensaje, string palabra, int idPartida)
        {
            Dispatcher.Invoke(() =>
            {
                try
                {
                    // Logging en archivo
                    LogCliente($"[NotificarFinPartida] IDPartida local={this.idPartida} | IDPartida callback={idPartida} | mensaje='{mensaje}' | palabra='{palabra}'");

                    // Ignora si el IDPartida no corresponde a la partida actual
                    if (idPartida != this.idPartida)
                    {
                        LogCliente("[NotificarFinPartida] Se ignoró notificación por IDPartida distinto");
                        return;
                    }
                    LanzarConfeti();
                    // Muestra la notificación de fin de partida
                    txtFinPartidaNotificacion.Text = $"{mensaje}. La palabra era: {palabra}";
                    txtFinPartidaNotificacion.Visibility = Visibility.Visible;
                    btnVolverMenu.Visibility = Visibility.Visible;

                    LogCliente("[NotificarFinPartida] Notificación mostrada al usuario y botón volver visible.");
                }
                catch (Exception ex)
                {
                    LogCliente($"[NotificarFinPartida] ERROR: {ex.Message}");
                }
            });
        }


        private void BtnEnviar_Click(object sender, RoutedEventArgs e)
        {
            // Implementa el chat si lo necesitas
        }

        private async void BtnCancelarPartida_Click(object sender, RoutedEventArgs e)
        {
            foreach (Button btn in wrapLetras.Children)
                btn.IsEnabled = false;

            try
            {
                LogCliente($"BtnCancelarPartida_Click | Jugador {jugador.IDJugador} | Partida {idPartida} | Cancelando partida");
                await Task.Run(() => proxy.AbandonarPartida(idPartida, jugador.IDJugador));
            }
            catch (Exception ex)
            {
                LogCliente($"ERROR BtnCancelarPartida_Click | Jugador {jugador.IDJugador} | Partida {idPartida} | {ex.Message}");
                foreach (Button btn in wrapLetras.Children)
                    btn.IsEnabled = !esCreador;
            }
        }
    }
}

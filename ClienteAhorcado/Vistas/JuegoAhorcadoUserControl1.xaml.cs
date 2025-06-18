using BibliotecaClasesNetFramework.Contratos;
using BibliotecaClasesNetFramework.DTO;
using System;
using System.Collections.Generic;
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

        public JuegoAhorcadoUserControl1(JugadorDTO jugador, PalabraDTO palabra, int idPartida, bool esCreador, IAhorcadoService proxy)
        {
            InitializeComponent();
            txtIntentosRestantes.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);
            txtLetrasUsadas.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);

            this.proxy = proxy;
            this.jugador = jugador;
            this.palabraSecreta = palabra.Texto.ToUpper();
            this.intentosRestantes = 6;
            this.esCreador = esCreador;
            this.palabra = palabra;
            this.idPartida = idPartida;
            this.mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();

            InicializarPalabra();
            GenerarBotonesLetras();
            ActualizarEstado();
            ConfigurarRol();
        }


        private void ConfigurarRol()
        {
            if (esCreador)
            {
                wrapLetras.IsEnabled = false;
                foreach (Button btn in wrapLetras.Children)
                    btn.IsEnabled = false;
                this.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.DarkSlateGray);
                btnVolverMenu.Visibility = Visibility.Visible;

            }
            else
            {
                wrapLetras.IsEnabled = true;
                foreach (Button btn in wrapLetras.Children)
                    btn.IsEnabled = true;
                this.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);
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
                    Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White) 
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
                await Task.Run(() => proxy.EnviarLetra(idPartida, jugador.IDJugador, letra));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al llamar EnviarLetra: " + ex.Message);
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
                canvasEfectos.Children.Add(estrella);

                var animacionY = new DoubleAnimation
                {
                    From = -size,
                    To = this.ActualHeight + size,
                    Duration = TimeSpan.FromSeconds(rand.NextDouble() * 2 + 1),
                    AccelerationRatio = 0.2,
                    DecelerationRatio = 0.2
                };

                var animacionRotacion = new DoubleAnimation
                {
                    From = 0,
                    To = 360,
                    Duration = TimeSpan.FromSeconds(2),
                    RepeatBehavior = RepeatBehavior.Forever
                };

                estrella.RenderTransform = new RotateTransform();
                estrella.RenderTransformOrigin = new Point(0.5, 0.5);
                estrella.RenderTransform.BeginAnimation(RotateTransform.AngleProperty, animacionRotacion);

                estrella.BeginAnimation(Canvas.TopProperty, animacionY);
            }
        }



        private void BtnVolverMenu_Click(object sender, RoutedEventArgs e)
        {
            
            
                mainWindow.CambiarVista(new MenuPrincipalUserControl(mainWindow, jugador, proxy));
            
        }

        public void ActualizarDesdeCallback(PartidaEstadoDTO estado)
        {
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
                LanzarConfeti();
                foreach (Button btn in wrapLetras.Children)
                    btn.IsEnabled = false;
                btnVolverMenu.Visibility = Visibility.Visible;
            }
        }



        private void BtnEnviar_Click(object sender, RoutedEventArgs e)
        {
            // No hace nada por ahora
        }

        private void BtnCancelarPartida_Click(object sender, RoutedEventArgs e)
        {
            foreach (Button btn in wrapLetras.Children)
                btn.IsEnabled = false;

            try
            {
                proxy.AbandonarPartida(idPartida, jugador.IDJugador);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cancelar la partida: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                foreach (Button btn in wrapLetras.Children)
                    btn.IsEnabled = !esCreador;
            }
        }

    }
}

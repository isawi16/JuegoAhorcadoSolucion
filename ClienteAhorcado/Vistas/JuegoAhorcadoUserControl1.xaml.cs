using ClienteAhorcado;
using ServidorAhorcadoService;
using ServidorAhorcadoService.DTO;
using ServidorAhorcadoService.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

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

        public JuegoAhorcadoUserControl1(JugadorDTO jugador, PalabraDTO palabra, int idPartida, bool esCreador)
        {
            InitializeComponent();
            this.jugador = jugador;
            this.palabraSecreta = palabra.Texto.ToUpper();
            this.intentosRestantes = 6;
            this.esCreador = esCreador;

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
            if (!(sender is Button btn)) return;
            btn.IsEnabled = false;

            char letra = btn.Content.ToString()[0];
            letrasUsadas.Add(letra);

            bool acierto = false;
            for (int i = 0; i < palabraSecreta.Length; i++)
            {
                if (palabraSecreta[i] == letra)
                {
                    acierto = true;
                    if (stackPalabra.Children[i] is TextBlock tb)
                        tb.Text = letra.ToString();
                }
            }

            if (!acierto)
            {
                intentosRestantes--;
                ActualizarImagenAhorcado();
            }

            ActualizarEstado();

            if (VerificarVictoria())
                FinDeJuego(true);
            else if (intentosRestantes <= 0)
                FinDeJuego(false);
        }

        private void ActualizarEstado()
        {
            txtIntentosRestantes.Text = intentosRestantes.ToString();
            txtLetrasUsadas.Text = string.Join(", ", letrasUsadas);
        }

        private void ActualizarImagenAhorcado()
        {
            imgAhorcado.Source = new BitmapImage(new Uri($"/Images/{intentosRestantes}.png", UriKind.Relative));
        }

        private bool VerificarVictoria()
        {
            for (int i = 0; i < palabraSecreta.Length; i++)
            {
                if (palabraSecreta[i] != ' ' && (stackPalabra.Children[i] as TextBlock)?.Text == "_")
                    return false;
            }
            return true;
        }

        private void BtnVolverMenu_Click(object sender, RoutedEventArgs e)
        {
            
            
                mainWindow.CambiarVista(new MenuPrincipalUserControl(mainWindow, jugador));
            
        }

        private void FinDeJuego(bool gano)
        {
            foreach (Button btn in wrapLetras.Children)
                btn.IsEnabled = false;

            string msg = gano ? "¡Felicidades, ganaste!" : $"Perdiste, la palabra era: {palabraSecreta}";
            MessageBox.Show(msg, "Juego terminado");

            btnVolverMenu.Visibility = Visibility.Visible;
           
        }

        // Método para el botón "Enviar" del chat (aunque no haga nada)
        private void BtnEnviar_Click(object sender, RoutedEventArgs e)
        {
            // No hace nada por ahora
        }

        // Método para el botón "Cancelar Partida" (aunque no haga nada)
        private void BtnCancelarPartida_Click(object sender, RoutedEventArgs e)
        {
            // No hace nada por ahora
        }
    }
}

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
    public partial class JuegoAhorcadoUserControl : UserControl
    {
        private IAhorcadoService _ahorcadoService;
        private int _idPartida; // ID de la partida actual
        private string _palabraSecreta; // Palabra secreta que se está adivinando
        private List<string> _letrasUsadas = new List<string>(); // Letras usadas
        private int _intentosRestantes = 7; // Intentos restantes
        private List<Button> _letraButtons = new List<Button>(); // Botones de letras

        public JuegoAhorcadoUserControl(int idPartida)
        {
            InitializeComponent();
            _idPartida = idPartida;
            _ahorcadoService = new AhorcadoService(); // Instancia del servicio
            CargarLetras();
            CargarPalabra();
            ActualizarImagenAhorcado();
        }

        // Cargar los botones de las letras en orden QWERTY
        // Cargar los botones de las letras
        private void CargarLetras()
        {
            string alfabeto = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"; // Alfabeto QWERTY
            foreach (var letra in alfabeto)
            {
                var button = new Button
                {
                    Content = letra.ToString(),
                    Width = 40,
                    Height = 40,
                    Margin = new Thickness(5)
                };
                button.Click += LetraButton_Click;
                _letraButtons.Add(button);
                WrapPanel.Children.Add(button); // Añadir los botones al WrapPanel, no a 'this.Content'
            }
        }


        // Evento al hacer clic en una letra
        private void LetraButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            string letra = button.Content.ToString();

            if (_letrasUsadas.Contains(letra))
                return; // Si la letra ya fue usada, no hacer nada

            _letrasUsadas.Add(letra); // Marcar la letra como usada
            button.Background = Brushes.Gray; // Cambiar color para indicar que fue usada

            ComprobarLetra(letra);
        }

        // Comprobar si la letra seleccionada está en la palabra
        private void ComprobarLetra(string letra)
        {
            bool acierto = false;
            char[] palabraConGuiones = _palabraSecreta.ToCharArray();

            for (int i = 0; i < palabraConGuiones.Length; i++)
            {
                if (palabraConGuiones[i].ToString().ToLower() == letra.ToLower())
                {
                    acierto = true;
                    stackPalabra.Children[i] = new TextBlock { Text = letra }; // Reemplazar guion bajo por la letra
                }
            }

            if (!acierto)
            {
                _intentosRestantes--;
                ActualizarImagenAhorcado();
            }

            if (_intentosRestantes == 0)
            {
                MessageBox.Show("¡Has perdido! La palabra era: " + _palabraSecreta);
            }
        }

        // Actualizar la imagen del ahorcado según los intentos restantes
        private void ActualizarImagenAhorcado()
        {
            imgAhorcado.Source = new BitmapImage(new Uri($"ClienteAhorcado.Images/{_intentosRestantes}.png"));
        }

        // Cargar la palabra con guiones
        private void CargarPalabra()
        {
            _palabraSecreta = "Ejemplo"; // Puedes cargar esta palabra desde el servidor si es necesario
            foreach (var letra in _palabraSecreta)
            {
                stackPalabra.Children.Add(new TextBlock { Text = "_" });
            }
        }

        // Enviar mensaje al chat
        private void EnviarMensaje(object sender, RoutedEventArgs e)
        {
            var mensaje = txtMensaje.Text;
            if (!string.IsNullOrEmpty(mensaje))
            {
                _ahorcadoService.EnviarMensajeChat(_idPartida, "Jugador 1", mensaje); // Enviar el mensaje
                lstChat.Items.Add("Jugador 1: " + mensaje); // Mostrar en el chat
                txtMensaje.Clear();
            }
        }
    }
}

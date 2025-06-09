using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using ServidorAhorcadoService.DTO;
using ServidorAhorcadoService;
using PartidaEstadoDTO = ServidorAhorcadoService.DTO.PartidaEstadoDTO;

using ClienteAhorcado.Vistas;




namespace ClienteAhorcadoApp
{
    public partial class MainWindow : Window //, IAhorcadoCallback
    {
        
        public MainWindow()
        {
            InitializeComponent();
            MainContent.Content = new IniciarSesion(this); 
        }

        public void CambiarVista(UserControl nuevaVista)
        {
            MainContent.Content = nuevaVista;
        }

      
  


        /*IAhorcadoService proxy;
        JugadorDTO usuarioActual;
        int idPartidaActual;

        public MainWindow()
        {
            try
            {
                InitializeComponent();
                var contexto = new InstanceContext(this);
                var factory = new DuplexChannelFactory<IAhorcadoService>(contexto, "AhorcadoEndpoint");
                proxy = factory.CreateChannel();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al conectar con el servicio: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            var correo = txtEmail.Text;
            var pass = txtPassword.Password;

            usuarioActual = proxy.IniciarSesion(correo, pass);
            if (usuarioActual != null)
            {
                MessageBox.Show($"Bienvenido, {usuarioActual.Nombre}");
                MostrarMenuPrincipal();
            }
            else
            {
                MessageBox.Show("Usuario o contraseña incorrectos.");
            }
        }

        private void MostrarMenuPrincipal()
        {
            // Aquí puedes cambiar de ventana o habilitar controles
            btnViewGames.IsEnabled = true;
            btnCreateGame.IsEnabled = true;
        }

        private void btnViewGames_Click(object sender, RoutedEventArgs e)
        {
            var partidas = proxy.ObtenerPartidasDisponibles();
            lstPartidas.Items.Clear();
            foreach (var partida in partidas)
            {
                lstPartidas.Items.Add($"ID:{partida.IDPartida} | Creador: {partida.CreadorNombre} | Estado: {partida.Estado}");
            }
        }

        private void btnCreateGame_Click(object sender, RoutedEventArgs e)
        {
            int idPalabra = ObtenerPalabraSeleccionada();
            bool ok = proxy.CrearPartida(usuarioActual.IDJugador, idPalabra);
            if (ok)
                MessageBox.Show("Partida creada, esperando retador...");
            else
                MessageBox.Show("No se pudo crear la partida.");
        }

        private void btnJoinGame_Click(object sender, RoutedEventArgs e)
        {
            if (lstPartidas.SelectedIndex == -1)
            {
                MessageBox.Show("Seleccione una partida.");
                return;
            }
            var partidaSeleccionada = lstPartidas.SelectedItem.ToString();
            int idPartida = ExtraerIDPartida(partidaSeleccionada);

            bool ok = proxy.UnirseAPartida(idPartida, usuarioActual.IDJugador);
            if (ok)
            {
                idPartidaActual = idPartida;
                MessageBox.Show("Unido a la partida. Esperando turno...");
            }
            else
            {
                MessageBox.Show("No se pudo unir.");
            }
        }

        private int ExtraerIDPartida(string texto)
        {
            var partes = texto.Split('|');
            var idPart = partes[0].Replace("ID:", "").Trim();
            return int.Parse(idPart);
        }

        private int ObtenerPalabraSeleccionada()
        {
            // Aquí puedes mostrar un diálogo o selector de palabra
            return 1; // Por ahora regresamos una palabra fija
        }

        private void btnSendLetter_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtLetra.Text))
            {
                MessageBox.Show("Ingresa una letra.");
                return;
            }
            char letra = txtLetra.Text.ToLower()[0];
            bool ok = proxy.EnviarLetra(idPartidaActual, usuarioActual.IDJugador, letra);
            if (ok)
            {
                txtLetra.Text = "";
            }
            else
            {
                MessageBox.Show("Error al enviar letra.");
            }
        }

        // Callbacks del servidor

        public void RecibirMensajeChat(string nombreJugador, string mensaje)
        {
            Dispatcher.Invoke(() => {
                lstChat.Items.Add($"[{nombreJugador}] {mensaje}");
            });
        }

        public void ActualizarEstadoPartida(PartidaEstadoDTO estadoActual)
        {
            Dispatcher.Invoke(() =>
            {
                lblEstado.Content = estadoActual.PalabraConGuiones;
                lblIntentos.Content = $"Intentos restantes: {estadoActual.IntentosRestantes}";
                lstLetrasUsadas.ItemsSource = estadoActual.LetrasUsadas;
            });
        }

        public void NotificarFinPartida(string resultado, string palabra)
        {
            Dispatcher.Invoke(() => {
                MessageBox.Show($"{resultado}. La palabra era: {palabra}");
            });
        }

        /*public void JugadorSeUnio(string nombreJugador)
        {
            throw new NotImplementedException();
        }

        public void JugadorAbandono(string nombreJugador)
        {
            throw new NotImplementedException();
        }

        public void CambiarTurno(string nombreJugadorActual)
        {
            throw new NotImplementedException();
        }

        public void ActualizarCantidadJugadores(int cantidadConectados)
        {
            throw new NotImplementedException();
        }*/
    }
}

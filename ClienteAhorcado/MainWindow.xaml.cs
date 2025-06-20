using System;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using BibliotecaClasesNetFramework.Contratos;
using BibliotecaClasesNetFramework.DTO;
using ClienteAhorcado.Vistas;
using ClienteAhorcado.Utilidades;
using System.IO;

namespace ClienteAhorcado
{
    public partial class MainWindow : Window, IAhorcadoCallback
    {
        public IAhorcadoService proxy;
        public DuplexChannelFactory<IAhorcadoService> factory;
        public JugadorDTO jugadorActual;
        public PartidaDTO partidaActual;
        public PalabraDTO palabraSeleccionada;
        public int idPartida;
        public bool esCreador;
        public int codigoIdioma;
        private string rutaLog = "logCliente.txt";


        private void LogCliente(string mensaje)
        {
            try
            {
                File.AppendAllText("logServidor.txt", DateTime.Now + " | " + mensaje + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al escribir en logServidor.txt: " + ex.Message);
                throw; // Solo mientras depuras
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            InicializarProxy();
            MainContent.Content = new IniciarSesionUserControl(this, proxy);
            
        }
        

        private void InicializarProxy()
        {
            var callbackInstance = new InstanceContext(new AhorcadoCallbackCliente(this)); // Si usas tu propia clase de callback
            factory = new DuplexChannelFactory<IAhorcadoService>(callbackInstance, "AhorcadoServiceEndpoint");
            proxy = factory.CreateChannel();
        }

        public MainWindow(JugadorDTO jugador, PartidaDTO partida, bool creador)
        {
            InitializeComponent();
            jugadorActual = jugador;
            partidaActual = partida;
            esCreador = creador;
            InicializarProxy();
            CargarPantallaJuego(jugadorActual, palabraSeleccionada, idPartida, esCreador);
        }

        public void CambiarVista(UserControl nuevaVista)
        {
            MainContent.Content = nuevaVista;
        }

        public void CargarPantallaJuego(JugadorDTO jugador, PalabraDTO palabra, int idPartida, bool esCreador)
        {
            this.idPartida = idPartida; // Evita que los callbacks no se descarten por IDPartida
            var controlJuego = new JuegoAhorcadoUserControl1(jugador, palabra, idPartida, esCreador, proxy);
            MainContent.Content = controlJuego;
        }
        

        // --- Métodos de Callback ---
        public void ActualizarEstadoPartida(PartidaEstadoDTO estadoActual)
        {
            LogCliente($"MainWindow.ActualizarEstadoPartida | Jugador {jugadorActual?.IDJugador} | idPartida local {idPartida} | callback con IDPartida {estadoActual?.IDPartida}");

            if (estadoActual.IDPartida != this.idPartida)
            {
                LogCliente($"MainWindow.ActualizarEstadoPartida | Jugador {jugadorActual?.IDJugador} | IDPartida distinta, se ignora callback.");
                return;
            }

            Dispatcher.Invoke(() =>
            {
                if (MainContent.Content is JuegoAhorcadoUserControl1 juegoControl)
                {
                    LogCliente($"MainWindow.ActualizarEstadoPartida | Se llama a ActualizarDesdeCallback en UserControl");
                    juegoControl.ActualizarDesdeCallback(estadoActual);
                }
                else
                {
                    LogCliente($"MainWindow.ActualizarEstadoPartida | MainContent no es JuegoAhorcadoUserControl1");
                }
            });
        }

        public void RecibirMensajeChat(string nombreJugador, string mensaje)
        {
            Dispatcher.Invoke(() =>
            {
                if (MainContent.Content is JuegoAhorcadoUserControl1 juegoControl)
                {
                    // juegoControl.MostrarMensajeChat(mensaje);
                }
            });
        }

        public void NotificarFinPartida(string resultado, string palabra, int IDPartida, int idJugadorDestino)
        {
            if (jugadorActual?.IDJugador != idJugadorDestino)
                return; // Solo muestro mensaje si es para este jugador

            LogCliente($"MainWindow.NotificarFinPartida | Jugador {jugadorActual?.IDJugador} | idPartida local {idPartida} | callback con IDPartida {IDPartida} | resultado: {resultado}");

            if (IDPartida != this.idPartida)
            {
                LogCliente($"MainWindow.NotificarFinPartida | Jugador {jugadorActual?.IDJugador} | IDPartida distinta, se ignora callback.");
                return;
            }

            Dispatcher.Invoke(() =>
            {
                string mensaje = "";
                if (resultado == "¡Ganaste!")
                    mensaje = "¡Felicidades, ganaste la partida! La palabra era: " + palabra;
                else if (resultado == "¡Perdiste!")
                    mensaje = "¡Perdiste!... Pero te apoyamos c: La palabra era: " + palabra;
                else if (resultado == "La palabra ha sido adivinada")
                    mensaje = "¡La palabra ha sido adivinada! Tu labor como creador ha terminado. La palabra era: " + palabra;
                else // "¡Juego terminado!" (abandono/cancelación)
                    mensaje = "La partida ha sido cancelada. La palabra era: " + palabra;


                MessageBox.Show(mensaje, "Fin de la partida", MessageBoxButton.OK, MessageBoxImage.Information);

                // Si la vista actual es el juego, habilita el botón "Volver menú principal"
                if (MainContent.Content is JuegoAhorcadoUserControl1 juegoControl)
                {
                    juegoControl.btnVolverMenu.Visibility = Visibility.Visible;
                }
            });
        }



    }
}

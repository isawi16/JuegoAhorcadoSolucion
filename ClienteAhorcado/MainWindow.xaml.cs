using System;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using BibliotecaClasesNetFramework.Contratos;
using BibliotecaClasesNetFramework.DTO;
using ClienteAhorcado.Vistas;
using ClienteAhorcado.Utilidades;

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

        public MainWindow()
        {
            InitializeComponent();
            InicializarProxy();
            MainContent.Content = new IniciarSesionUserControl(this, proxy);
            try
            {
                var respuesta = proxy.Ping();
                MessageBox.Show(respuesta);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al llamar Ping: " + ex.Message);
            }
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
            var controlJuego = new JuegoAhorcadoUserControl1(jugador, palabra, idPartida, esCreador, proxy);
            MainContent.Content = controlJuego;
        }

        // --- Métodos de Callback ---
        public void ActualizarEstadoPartida(PartidaEstadoDTO estadoActual)
        {
            Dispatcher.Invoke(() =>
            {
                if (MainContent.Content is JuegoAhorcadoUserControl1 juegoControl)
                {
                    juegoControl.ActualizarDesdeCallback(estadoActual);
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

        public void NotificarFinPartida(string resultado, string palabra)
        {
            Dispatcher.Invoke(() =>
            {
                MessageBox.Show($"{resultado}. La palabra era: {palabra}", "Fin de la partida");

                // Si la vista actual es el juego, habilita el botón "Volver menú principal"
                if (MainContent.Content is JuegoAhorcadoUserControl1 juegoControl)
                {
                    juegoControl.btnVolverMenu.Visibility = Visibility.Visible;
                }
            });
        }
    }
}

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
                MessageBox.Show(respuesta); // Debería mostrar "pong"
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al llamar Ping: " + ex.Message);
            }
        }
        

        private void InicializarProxy()
        {
            // Puedes usar MainWindow como callback, o tu propia clase, como prefieras:
            // var callbackInstance = new InstanceContext(this);
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
            });
        }

        // Si tienes métodos opcionales del callback, puedes implementarlos aquí...
        // public void JugadorSeUnio(string nombreJugador) { }
        // public void JugadorAbandono(string nombreJugador) { }
        // public void CambiarTurno(string nombreJugadorActual) { }
        // public void ActualizarCantidadJugadores(int cantidadConectados) { }

        // --- Puedes agregar métodos extra y lógica aquí según lo que necesites ---
    }
}

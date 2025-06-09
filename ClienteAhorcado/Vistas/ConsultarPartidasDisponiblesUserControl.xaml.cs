using ClienteAhorcadoApp;
using ServidorAhorcadoService;
using ServidorAhorcadoService.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
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
    /// <summary>
    /// Interaction logic for ConsultarPartidasDisponibles.xaml
    /// </summary>
    public partial class ConsultarPartidasDisponiblesUserControl : UserControl
    {
        private MainWindow _mainWindow;
        IAhorcadoService proxy;
        JugadorDTO jugadorSesion = new JugadorDTO();

        List<PartidaDTO> partidasDisponibles = new List<PartidaDTO>();

        public ConsultarPartidasDisponiblesUserControl(MainWindow mainWindow, JugadorDTO jugador)
        {
            try
            {
                InitializeComponent();
                _mainWindow = mainWindow;

                jugadorSesion = jugador;

                var contexto = new InstanceContext(new DummyCallback());
                var factory = new DuplexChannelFactory<IAhorcadoService>(contexto, "AhorcadoEndpoint");
                proxy = factory.CreateChannel();

                llenarTablaPartidas();

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al conectar con el servicio: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        public void llenarTablaPartidas()
        {
            dgPartidas.ItemsSource = null; // Limpiar la fuente de datos antes de cargar nuevos datos
            partidasDisponibles = proxy.ObtenerPartidasDisponibles();

            if (partidasDisponibles != null && partidasDisponibles.Count > 0)
            {
                var lista = partidasDisponibles
                        .OrderBy(p => p.Fecha)
                        .ToList();

                dgPartidas.ItemsSource = lista;
            }
            else
            {
                MessageBox.Show("No se encontraron partidas disponible, intentelo de nuevo mas tarde.", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }


        public class DummyCallback : IAhorcadoCallback
        {
            public void ActualizarEstadoPartida(PartidaEstadoDTO estadoActual) { }
            public void NotificarFinPartida(string resultado, string palabra) { }
            public void RecibirMensajeChat(string nombreJugador, string mensaje) { }
        }

        private void UnirsePartida_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ActualizarLista_Click(object sender, RoutedEventArgs e)
        {
            llenarTablaPartidas();
        }

        private void CerrarButton_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.CambiarVista(new MenuPrincipalUserControl(_mainWindow, jugadorSesion));
        }
    }
}

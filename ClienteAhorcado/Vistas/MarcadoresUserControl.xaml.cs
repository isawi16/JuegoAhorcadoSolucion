using ClienteAhorcadoApp;
using ServidorAhorcadoService;
using ServidorAhorcadoService.DTO;
using ServidorAhorcadoService.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.ComponentModel;

namespace ClienteAhorcado.Vistas
{
    public class JugadorMarcador : INotifyPropertyChanged
    {
        public int Puesto { get; set; }
        public string Nombre { get; set; }
        public int PuntajeGlobal { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
    }

    public partial class MarcadoresUserControl : UserControl
    {
        private MainWindow _mainWindow;
        IAhorcadoService proxy;

        JugadorDTO jugadorSesion = new JugadorDTO();
        List<JugadorDTO> jugadoresMarcadores = new List<JugadorDTO>();

        public MarcadoresUserControl(MainWindow mainWindow, JugadorDTO jugador)
        {
            try
            {
                InitializeComponent();

                _mainWindow = mainWindow;
                jugadorSesion = jugador;

                var contexto = new InstanceContext(new DummyCallback());
                var factory = new DuplexChannelFactory<IAhorcadoService>(contexto, "AhorcadoEndpoint");
                proxy = factory.CreateChannel();

                llenarTablaMarcadores();

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al conectar con el servicio: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void llenarTablaMarcadores()
        {
            dgMarcadores.ItemsSource = null; // Limpiar la fuente de datos antes de cargar nuevos datos
            jugadoresMarcadores = proxy.ObtenerJugadoresMarcadores();

            if (jugadoresMarcadores != null && jugadoresMarcadores.Count > 0)
            {
                var lista = jugadoresMarcadores
                        .OrderByDescending(j => j.PuntajeGlobal)
                        .Select((j, lugar) => new JugadorMarcador
                        {
                            Puesto = lugar + 1,
                            Nombre = j.Nombre,
                            PuntajeGlobal = j.PuntajeGlobal
                        })
                        .ToList();

                dgMarcadores.ItemsSource = lista;
            }
            else
            {
                MessageBox.Show("No se encontraron jugadores en los marcadores.", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        public class DummyCallback : IAhorcadoCallback
        {
            public void ActualizarEstadoPartida(PartidaEstadoDTO estadoActual) { }
            public void NotificarFinPartida(string resultado, string palabra) { }
            public void RecibirMensajeChat(string nombreJugador, string mensaje) { }
        }

        private void btnRegresar_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.CambiarVista(new MenuPrincipalUserControl(_mainWindow, jugadorSesion));
        }
    }
}

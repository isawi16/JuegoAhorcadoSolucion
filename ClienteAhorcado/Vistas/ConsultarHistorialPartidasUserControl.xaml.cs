using BibliotecaClasesNetFramework.Contratos;
using BibliotecaClasesNetFramework.DTO;
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
    /// Interaction logic for ConsultarHistorialPartidasUserControl.xaml
    /// </summary>
    public partial class ConsultarHistorialPartidasUserControl : UserControl
    {

        private MainWindow _mainWindow;
        IAhorcadoService proxy;
        private JugadorDTO jugadorSesion;

        private List<PartidaDTO> historialPartidas = new List<PartidaDTO>();
        public ConsultarHistorialPartidasUserControl(MainWindow mainWindow, JugadorDTO jugador, IAhorcadoService proxy)
        {
            try
            {
                InitializeComponent();
                _mainWindow = mainWindow;
                jugadorSesion = jugador;     
                this.proxy = proxy;

                LlenarTablaHistorialPartidas();

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al conectar con el servicio: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LlenarTablaHistorialPartidas()
        {
            dgHistorial.ItemsSource = null; // Limpiar la fuente de datos antes de cargar nuevos datos
            historialPartidas = proxy.ConsultarPartidasJugadas (jugadorSesion.IDJugador);
            if (historialPartidas != null && historialPartidas.Count > 0)
            {
                dgHistorial.ItemsSource = historialPartidas;
            }
            else
            {
                MessageBox.Show("No se encontraron partidas en el historial.", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }


        private void BtnVolver_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.CambiarVista(new MenuPrincipalUserControl(_mainWindow, jugadorSesion, proxy));
        }

        
    }
}

using ClienteAhorcado;
using ServidorAhorcadoService;
using ServidorAhorcadoService.DTO;
using ServidorAhorcadoService.Model;
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
using static ClienteAhorcado.Vistas.RegistrarJugadorUserControl;

namespace ClienteAhorcado.Vistas
{
  
    public partial class MenuPrincipalUserControl : UserControl
    {
        private MainWindow _mainWindow;
        IAhorcadoService proxy;
        JugadorDTO jugadorSesion = new JugadorDTO();

        public MenuPrincipalUserControl(MainWindow mainWindow, JugadorDTO jugador)
        {
            try
            {
                InitializeComponent();
                _mainWindow = mainWindow;
                jugadorSesion = jugador;
                var contexto = new InstanceContext(new DummyCallback());
                var factory = new DuplexChannelFactory<IAhorcadoService>(contexto, "AhorcadoEndpoint");
                proxy = factory.CreateChannel();

                tblNombre.Text = $"Nombre: {jugador.Nombre}";
                tblCorreo.Text = $"Correo: {jugador.Correo}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al conectar con el servicio: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnPerfil_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.CambiarVista(new PerfilJugadorUserControl(_mainWindow, jugadorSesion));
        }

        public class DummyCallback : IAhorcadoCallback
        {
            public void ActualizarEstadoPartida(PartidaEstadoDTO estadoActual) { }
            public void NotificarFinPartida(string resultado, string palabra) { }
            public void RecibirMensajeChat(string nombreJugador, string mensaje) { }
        }

        private void BtnIniciarPartida_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnUnirsePartida_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.CambiarVista(new ConsultarPartidasDisponiblesUserControl(_mainWindow, jugadorSesion));
        }

        private void BtnHistorialPartidas_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.CambiarVista(new ConsultarHistorialPartidasUserControl(_mainWindow, jugadorSesion));
        }

        private void BtnMarcadores_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.CambiarVista(new MarcadoresUserControl(_mainWindow, jugadorSesion));
        }
    }
}

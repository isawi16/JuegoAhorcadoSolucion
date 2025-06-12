using ClienteAhorcado.Utilidades;
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

namespace ClienteAhorcado.Vistas
{
    
    public partial class PerfilJugadorUserControl : UserControl
    {
        private MainWindow _mainWindow;
        IAhorcadoService proxy;

        JugadorDTO jugadorPerfil = new JugadorDTO();
        public PerfilJugadorUserControl(MainWindow mainWindow, JugadorDTO jugador)
        {
            try
            {
                InitializeComponent();
                _mainWindow = mainWindow;

                jugadorPerfil = jugador;

                var contexto = new InstanceContext(new DummyCallback());
                var factory = new DuplexChannelFactory<IAhorcadoService>(contexto, "AhorcadoEndpoint");
                proxy = factory.CreateChannel();

                tblockNombre.Text = $"Nombre: {jugador.Nombre}";
                tblockCorreo.Text = $"Correo: {jugador.Correo}";
                tblockTelefono.Text = $"Teléfono: {jugador.Telefono}";
                tblockFechaNacimiento.Text = $"Fecha de Nacimiento: {jugador.FechaNacimiento.ToShortDateString()}";
                

                tbNombre.Visibility = Visibility.Collapsed;
                tbTelefono.Visibility = Visibility.Collapsed;
                
                dpFechaNacimiento.Visibility = Visibility.Collapsed;
                btnGuardar.Visibility = Visibility.Collapsed;

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al conectar con el servicio: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public class DummyCallback : IAhorcadoCallback
        {
            public void ActualizarEstadoPartida(PartidaEstadoDTO estadoActual) { }
            public void NotificarFinPartida(string resultado, string palabra) { }
            public void RecibirMensajeChat(string nombreJugador, string mensaje) { }
        }

        private void BtnRegresar_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.CambiarVista(new MenuPrincipalUserControl(_mainWindow, jugadorPerfil));
        }

        private void BtnModificarPerfil_Click(object sender, RoutedEventArgs e)
        {
            tbNombre.Visibility = Visibility.Visible;
            tbTelefono.Visibility = Visibility.Visible;
            
            dpFechaNacimiento.Visibility = Visibility.Visible;
            btnGuardar.Visibility = Visibility.Visible;

            tbNombre.Text = jugadorPerfil.Nombre;
            tbTelefono.Text = jugadorPerfil.Telefono;
            
            dpFechaNacimiento.SelectedDate = jugadorPerfil.FechaNacimiento;
           

        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            JugadorDTO jugadorModificado = new JugadorDTO();
            bool modificadoExitoso = false;

            if (EntradasValidas())
            {
                if (proxy == null)
                {
                    MessageBox.Show("El servicio no está disponible. Inténtelo más tarde.");
                    return;
                }

                jugadorModificado.IDJugador = jugadorPerfil.IDJugador;
                jugadorModificado.Correo = jugadorPerfil.Correo;
                jugadorModificado.PuntajeGlobal = jugadorPerfil.PuntajeGlobal;

                jugadorModificado.Nombre = tbNombre.Text.Trim();
                jugadorModificado.FechaNacimiento = dpFechaNacimiento.SelectedDate.Value;
                
                jugadorModificado.Telefono = tbTelefono.Text.Trim();

                modificadoExitoso = proxy.ModificarPerfil(jugadorModificado);

                if (modificadoExitoso)
                {
                    MessageBox.Show("Se modifico la informacion exitosamente");
                    _mainWindow.CambiarVista(new MenuPrincipalUserControl(_mainWindow, jugadorModificado));
                }
                else
                {
                    MessageBox.Show("No se pudo modificar la informacion, intentelo mas tarde");
                }
            }
        }

        private bool EntradasValidas()
        {
            bool valido = true;

            
            string errorNombre = ValidacionesEntrada.ValidarNombre(tbNombre);
            string errorTelefono = ValidacionesEntrada.ValidarTelefono(tbTelefono);
            string errorFechaNacimiento = ValidacionesEntrada.ValidarFechaNacimiento(dpFechaNacimiento);

            
            tblockErrorNombre.Text = errorNombre ?? "";
            tblockErrorTelefono.Text = errorTelefono ?? "";
            tblockErrorFecha.Text = errorFechaNacimiento ?? "";

            if ( errorNombre != null || errorTelefono != null || errorFechaNacimiento != null)
                valido = false;

            return valido;
        }

    }
}

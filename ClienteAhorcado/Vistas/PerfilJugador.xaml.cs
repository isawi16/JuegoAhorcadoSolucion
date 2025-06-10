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
    
    public partial class PerfilJugador : UserControl
    {
        private MainWindow _mainWindow;
        IAhorcadoService proxy;

        JugadorDTO jugadorPerfil = new JugadorDTO();
        public PerfilJugador(MainWindow mainWindow, JugadorDTO jugador)
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
                tblockPassword.Text = $"Contraseña: {jugador.Contraseña}";

                tbNombre.Visibility = Visibility.Collapsed;
                tbTelefono.Visibility = Visibility.Collapsed;
                tbPassword.Visibility = Visibility.Collapsed;
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

        private void btnRegresar_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.CambiarVista(new MenuPrincipal(_mainWindow, jugadorPerfil));
        }

        private void btnModificarPerfil_Click(object sender, RoutedEventArgs e)
        {
            tbNombre.Visibility = Visibility.Visible;
            tbTelefono.Visibility = Visibility.Visible;
            tbPassword.Visibility = Visibility.Visible;
            dpFechaNacimiento.Visibility = Visibility.Visible;
            btnGuardar.Visibility = Visibility.Visible;

            tbNombre.Text = jugadorPerfil.Nombre;
            tbTelefono.Text = jugadorPerfil.Telefono;
            tbPassword.Text = jugadorPerfil.Contraseña;
            dpFechaNacimiento.SelectedDate = jugadorPerfil.FechaNacimiento;
            tbPassword.Text = jugadorPerfil.Contraseña;

        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
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
                jugadorModificado.Contraseña = tbPassword.Text.Trim();
                jugadorModificado.Telefono = tbTelefono.Text.Trim();

                modificadoExitoso = proxy.ModificarPerfil(jugadorModificado);

                if (modificadoExitoso)
                {
                    MessageBox.Show("Se modifico la informacion exitosamente");
                    _mainWindow.CambiarVista(new MenuPrincipal(_mainWindow, jugadorModificado));
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

            string errorPass = ValidacionesEntrada.ValidarPasswordTextBox(tbPassword);
            string errorNombre = ValidacionesEntrada.ValidarNombre(tbNombre);
            string errorTelefono = ValidacionesEntrada.ValidarTelefono(tbTelefono);
            string errorFechaNacimiento = ValidacionesEntrada.ValidarFechaNacimiento(dpFechaNacimiento);

            tblockErrorPassword.Text = errorPass ?? "";
            tblockErrorNombre.Text = errorNombre ?? "";
            tblockErrorTelefono.Text = errorTelefono ?? "";
            tblockErrorFecha.Text = errorFechaNacimiento ?? "";

            if (errorPass != null || errorNombre != null || errorTelefono != null || errorFechaNacimiento != null)
                valido = false;

            return valido;
        }

    }
}

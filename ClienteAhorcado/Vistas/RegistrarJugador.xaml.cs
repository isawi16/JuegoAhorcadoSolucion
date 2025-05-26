using ClienteAhorcado.Utilidades;
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
using System.Windows.Shapes;

namespace ClienteAhorcado.Vistas
{
    public partial class RegistrarJugador : UserControl
    {
        private MainWindow _mainWindow;
        IAhorcadoService proxy;
        JugadorDTO jugadorRegistro = new JugadorDTO();

        public RegistrarJugador(MainWindow mainWindow)
        {
            try
            {
                InitializeComponent();
                _mainWindow = mainWindow;
                var contexto = new InstanceContext(new DummyCallback());
                var factory = new DuplexChannelFactory<IAhorcadoService>(contexto, "AhorcadoEndpoint");
                proxy = factory.CreateChannel();
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

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.CambiarVista(new IniciarSesion(_mainWindow));
        }

        private void btnRegistrarme_Click(object sender, RoutedEventArgs e)
        {
            bool registroExitoso = false;

            if (EntradasValidas()) { 
                if (proxy == null)
                {
                    MessageBox.Show("El servicio no está disponible. Inténtelo más tarde.");
                    return;
                }

                jugadorRegistro.Nombre = tbNombre.Text.Trim();
                jugadorRegistro.FechaNacimiento = dpFechaNacimiento.SelectedDate.Value;
                jugadorRegistro.Correo = tbCorreo.Text.Trim();
                jugadorRegistro.Contraseña = tbPassword.Text.Trim();
                jugadorRegistro.Telefono = tbTelefono.Text.Trim();
                jugadorRegistro.PuntajeGlobal = 0;

                registroExitoso = proxy.RegistrarJugador(jugadorRegistro);

                if (registroExitoso)
                {
                    MessageBox.Show("El registro fue exitoso");
                }
                else
                {
                    MessageBox.Show("No se pudo registrar, intentelo mas tarde");
                }
            }
        }

        private bool EntradasValidas()
        {
            bool valido = true;

            string errorCorreo = ValidacionesEntrada.ValidarCorreo(tbCorreo);
            string errorPass = ValidacionesEntrada.ValidarPasswordTextBox(tbPassword);
            string errorNombre = ValidacionesEntrada.ValidarNombre(tbNombre);
            string errorTelefono = ValidacionesEntrada.ValidarTelefono(tbTelefono);
            string errorFechaNacimiento = ValidacionesEntrada.ValidarFechaNacimiento(dpFechaNacimiento);


            tblockErrorCorreo.Text = errorCorreo ?? "";
            tblockErrorPassword.Text = errorPass ?? "";
            tblockErrorNombre.Text = errorNombre ?? "";
            tblockErrorTelefono.Text = errorTelefono ?? "";
            tblockErrorFecha.Text = errorFechaNacimiento ?? "";

            if (errorCorreo != null || errorPass != null || errorNombre != null || errorTelefono != null || errorFechaNacimiento != null)
                valido = false;

            return valido;
        }
    }
}

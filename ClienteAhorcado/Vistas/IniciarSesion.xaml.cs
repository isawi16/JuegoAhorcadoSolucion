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
    public partial class IniciarSesion : UserControl, IAhorcadoCallback
    {
        private MainWindow _mainWindow;
        private bool mostrandoPassword = false;

        IAhorcadoService proxy;
        JugadorDTO usuarioActual;
        
        public IniciarSesion(MainWindow mainWindow)
        {
            try
            {
                InitializeComponent();
                _mainWindow = mainWindow;
                var contexto = new InstanceContext(this);
                var factory = new DuplexChannelFactory<IAhorcadoService>(contexto, "AhorcadoEndpoint");
                proxy = factory.CreateChannel();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al conectar con el servicio: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnIniciarSesion_Click(object sender, RoutedEventArgs e)
        {
            if (EntradasValidas())
            {
                var correo = tbCorreo.Text;
                string pass;

                if (mostrandoPassword)
                    pass = tbPasswordVisible.Text;
                else
                    pass = pbPassword.Password;

                usuarioActual = proxy.IniciarSesion(correo, pass);

                if (usuarioActual != null)
                {
                    MessageBox.Show($"Bienvenido, {usuarioActual.Nombre}");
                    MostrarMenuPrincipal(usuarioActual);
                }
                else
                {
                    MessageBox.Show("Usuario o contraseña incorrectos.");
                }
            }
        }

        private bool EntradasValidas()
        {
            bool valido = true;
            string correo = ValidacionesEntrada.ValidarCorreo(tbCorreo);
            string pass = ValidacionesEntrada.ValidarPassword(pbPassword);

            tblockErrorCorreo.Text = correo ?? "";
            tblockErrorPassword.Text = pass ?? "";

            if (correo != null || pass != null)
                valido = false;

            return valido;
        }

        private void MostrarMenuPrincipal(JugadorDTO jugador)
        {
            _mainWindow.CambiarVista(new MenuPrincipal(_mainWindow, jugador));
        }

        private void btnRegistrarse_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.CambiarVista(new RegistrarJugador(_mainWindow)); 
        }

        private void btnVerPassword_Click(object sender, RoutedEventArgs e)
        {
            mostrandoPassword = !mostrandoPassword;
            if (mostrandoPassword)
            {
                tbPasswordVisible.Text = pbPassword.Password;
                tbPasswordVisible.Visibility = Visibility.Visible;
                pbPassword.Visibility = Visibility.Collapsed;
                btnVerPassword.Content = "🙈"; 
            }
            else
            {
                pbPassword.Password = tbPasswordVisible.Text;
                pbPassword.Visibility = Visibility.Visible;
                tbPasswordVisible.Visibility = Visibility.Collapsed;
                btnVerPassword.Content = "👁";
            }
        }

        private void pbPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!mostrandoPassword)
                tbPasswordVisible.Text = pbPassword.Password;
        }

        private void tbPasswordVisible_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (mostrandoPassword)
                pbPassword.Password = tbPasswordVisible.Text;
        }


        public void ActualizarEstadoPartida(PartidaEstadoDTO estadoActual)
        {
            throw new NotImplementedException();
        }
        public void NotificarFinPartida(string resultado, string palabra)
        {
            throw new NotImplementedException();
        }
        public void RecibirMensajeChat(string nombreJugador, string mensaje)
        {
            throw new NotImplementedException();
        }
    }
}

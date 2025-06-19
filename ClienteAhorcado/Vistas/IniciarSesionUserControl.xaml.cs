using ClienteAhorcado.Utilidades;
using BibliotecaClasesNetFramework.DTO;
using System;
using System.Linq;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using BibliotecaClasesNetFramework.Contratos;

namespace ClienteAhorcado.Vistas
{
    public partial class IniciarSesionUserControl : UserControl
    {
        private MainWindow _mainWindow;
        private bool mostrandoPassword = false;
        IAhorcadoService proxy;
        JugadorDTO usuarioActual;

        String idiomaSesion;

        public IniciarSesionUserControl(MainWindow mainWindow, IAhorcadoService proxy)
        {
            try
            {
                InitializeComponent();
                _mainWindow = mainWindow;
                this.proxy = proxy;

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al conectar con el servicio: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnIniciarSesion_Click(object sender, RoutedEventArgs e)
        {
            if (EntradasValidas())
            {
                var correo = tbCorreo.Text;
                string pass;

                if (mostrandoPassword)
                    pass = tbPasswordVisible.Text;
                else
                    pass = pbPassword.Password;

                pass = RegistrarJugadorUserControl.EncriptarContraseña(pass.Trim());

                try
                {
                    usuarioActual = proxy.IniciarSesion(correo, pass);

                    if (usuarioActual != null)
                    {
                        string mensajeBienvenida = Application.Current.TryFindResource("Msg_Titulo_Bienvenida") as string ?? "¡Bienvenido!";

                        MessageBox.Show(mensajeBienvenida);
                        MostrarMenuPrincipal(usuarioActual);
                    }
                    else
                    {
                        string mensajeErrorSesion = Application.Current.TryFindResource("Msg_Error_InicioSesion") as string;

                        MessageBox.Show(mensajeErrorSesion);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al iniciar sesión: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool EntradasValidas()
        {
            bool valido = true;
            string correo = ValidacionesEntrada.ValidarCorreo(tbCorreo);
            string pass = ValidacionesEntrada.ValidarPassword(pbPassword);

            string ErrorCorreo = !string.IsNullOrEmpty(correo)
                ? Application.Current.TryFindResource(correo) as string : null;

            string ErrorPassword = !string.IsNullOrEmpty(pass)
                ? Application.Current.TryFindResource(pass) as string : null;

            tblockErrorCorreo.Text = ErrorCorreo ?? "";
            tblockErrorPassword.Text = ErrorPassword ?? "";

            if (correo != null || pass != null)
                valido = false;

            return valido;
        }

        // ¡IMPORTANTE! Aquí pasas el proxy a la siguiente ventana
        private void MostrarMenuPrincipal(JugadorDTO jugador)
        {
            _mainWindow.CambiarVista(new MenuPrincipalUserControl(_mainWindow, jugador, proxy));
        }

        private void BtnRegistrarse_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.CambiarVista(new RegistrarJugadorUserControl(_mainWindow, proxy));
        }

        private void BtnVerPassword_Click(object sender, RoutedEventArgs e)
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

        private void PbPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!mostrandoPassword)
                tbPasswordVisible.Text = pbPassword.Password;
        }

        private void TbPasswordVisible_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (mostrandoPassword)
                pbPassword.Password = tbPasswordVisible.Text;
        }

        private void cmbIdioma_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            if (cmbIdioma.SelectedItem is ComboBoxItem itemSeleccionado)
            {
                string codigoCultura = itemSeleccionado.Tag.ToString();

                var app = (App)Application.Current;
                app.CambiarIdioma(codigoCultura);

                string idiomaBase = codigoCultura.Split('-')[0];
                idiomaSesion = idiomaBase;
            }
        }
    }
}

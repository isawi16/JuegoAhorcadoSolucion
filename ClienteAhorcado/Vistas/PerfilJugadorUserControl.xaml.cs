using ClienteAhorcado;
using ClienteAhorcado.Utilidades;
using Microsoft.Win32;
using BibliotecaClasesNetFramework.Contratos;
using BibliotecaClasesNetFramework.DTO;
using System;
using System.Collections.Generic;
using System.IO;
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
        private IAhorcadoService proxy;
        private JugadorDTO jugadorPerfil;
        private bool mostrandoPassword = false;


        public PerfilJugadorUserControl(MainWindow mainWindow, JugadorDTO jugador, IAhorcadoService proxy)
        {
            try
            {
                InitializeComponent();
                _mainWindow = mainWindow;
                jugadorPerfil = jugador;   
                this.proxy = proxy;

                tblockNombreContenido.Text = jugador.Nombre;
                tblockCorreoContenido.Text = jugador.Correo;
                tblockTelefonoContenido.Text = jugador.Telefono;
                tblockFechaContenido.Text = jugador.FechaNacimiento.ToShortDateString();
                tblockPassword.Visibility = Visibility.Collapsed;

                tbNombre.Visibility = Visibility.Collapsed;
                tbTelefono.Visibility = Visibility.Collapsed;                
                dpFechaNacimiento.Visibility = Visibility.Collapsed;
                btnGuardar.Visibility = Visibility.Collapsed;
                btnSeleccionarFoto.Visibility = Visibility.Collapsed;
                btnVerPassword.Visibility = Visibility.Collapsed;
                tbPassword.Visibility = Visibility.Collapsed;
                pbPassword.Visibility = Visibility.Collapsed;

                if (jugador.FotoPerfil != null && jugador.FotoPerfil.Length > 0)
                {
                    var bitmap = new BitmapImage();
                    using (var ms = new System.IO.MemoryStream(jugador.FotoPerfil))
                    {
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.StreamSource = ms;
                        bitmap.EndInit();
                    }
                    imgPerfil.Source = bitmap;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al conectar con el servicio: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

       
        private void BtnRegresar_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.CambiarVista(new MenuPrincipalUserControl(_mainWindow, jugadorPerfil, proxy));
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
           
            btnSeleccionarFoto.Visibility = Visibility.Visible;

            tblockPassword.Visibility = Visibility.Visible;
            btnVerPassword.Visibility = Visibility.Visible;
            tbPassword.Visibility = Visibility.Collapsed;
            pbPassword.Visibility = Visibility.Visible;
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

                var bitmapSource = imgPerfil.Source as BitmapSource;
                if (bitmapSource != null)
                {
                    JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                    using (MemoryStream ms = new MemoryStream())
                    {
                        encoder.Save(ms);
                        jugadorModificado.FotoPerfil = ms.ToArray();
                    }
                }

                string nuevaContrasena = mostrandoPassword ? tbPassword.Text : pbPassword.Password;

                if (!string.IsNullOrWhiteSpace(nuevaContrasena))
                {
                    jugadorModificado.Contraseña = RegistrarJugadorUserControl.EncriptarContraseña(nuevaContrasena.Trim());
                }
                else
                {
                    jugadorModificado.Contraseña = jugadorPerfil.Contraseña;
                }

                modificadoExitoso = proxy.ModificarPerfil(jugadorModificado);

                if (modificadoExitoso)
                {
                    string mensajeModificacionExitosa = Application.Current.TryFindResource("Msg_ModificacionPerfilExitosa") as string;
                    MessageBox.Show(mensajeModificacionExitosa);
                    _mainWindow.CambiarVista(new MenuPrincipalUserControl(_mainWindow, jugadorModificado, proxy));
                }
                else
                {
                    string mensajeErrorModificar = Application.Current.TryFindResource("Msg_ErrorModificacionPerfil") as string;
                    MessageBox.Show(mensajeErrorModificar);
                }
            }
        }

        private bool EntradasValidas()
        {
            bool valido = true;

            
            string errorNombre = ValidacionesEntrada.ValidarNombre(tbNombre);
            string errorTelefono = ValidacionesEntrada.ValidarTelefono(tbTelefono);
            string errorFechaNacimiento = ValidacionesEntrada.ValidarFechaNacimiento(dpFechaNacimiento);
            string errorPass = ValidacionesEntrada.ValidarPassword(pbPassword);


            string mensajePass = !string.IsNullOrEmpty(errorPass)
                ? Application.Current.TryFindResource(errorPass) as string : null;

            string mensajeNombre = !string.IsNullOrEmpty(errorNombre)
                ? Application.Current.TryFindResource(errorNombre) as string : null;

            string mensajeTelefono = !string.IsNullOrEmpty(errorTelefono)
                ? Application.Current.TryFindResource(errorTelefono) as string : null;

            string mensajeFecha = !string.IsNullOrEmpty(errorFechaNacimiento)
                ? Application.Current.TryFindResource(errorFechaNacimiento) as string : null;

            tblockErrorPassword.Text = mensajePass ?? "";
            tblockErrorNombre.Text = mensajeNombre ?? "";
            tblockErrorTelefono.Text = mensajeTelefono ?? "";
            tblockErrorFecha.Text = mensajeFecha ?? "";

            if ( errorNombre != null || errorTelefono != null || errorFechaNacimiento != null || errorPass != null)
                valido = false;

            return valido;
        }

        private void BtnSeleccionarFoto_Click(object sender, RoutedEventArgs e)
        {
            string tituloEligeFoto = Application.Current.TryFindResource("ElegirFoto_Titulo") as string;
            var openFileDialog = new OpenFileDialog
            {
                Title = tituloEligeFoto,
                Filter = "Archivos de imagen|*.jpg;*.jpeg;*.png;*.bmp"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(openFileDialog.FileName);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                imgPerfil.Source = bitmap;


                byte[] imagenBytes = File.ReadAllBytes(openFileDialog.FileName);
            }
        }

        private void BtnVerPassword_Click(object sender, RoutedEventArgs e)
        {
            mostrandoPassword = !mostrandoPassword;
            if (mostrandoPassword)
            {
                tbPassword.Text = pbPassword.Password;
                tbPassword.Visibility = Visibility.Visible;
                pbPassword.Visibility = Visibility.Collapsed;
                btnVerPassword.Content = "🙈";
            }
            else
            {
                pbPassword.Password = tbPassword.Text;
                pbPassword.Visibility = Visibility.Visible;
                tbPassword.Visibility = Visibility.Collapsed;
                btnVerPassword.Content = "👁";
            }
        }

        private void PbPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!mostrandoPassword)
                tbPassword.Text = pbPassword.Password;
        }

        private void TbPasswordVisible_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (mostrandoPassword)
                pbPassword.Password = tbPassword.Text;
        }
    }
}

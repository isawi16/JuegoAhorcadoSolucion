using ClienteAhorcado.Utilidades;
using ClienteAhorcado;
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
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using Microsoft.Win32;
using System.IO;

namespace ClienteAhorcado.Vistas
{
    public partial class RegistrarJugadorUserControl : UserControl
    {
        private MainWindow _mainWindow;
        IAhorcadoService proxy;
        JugadorDTO jugadorRegistro = new JugadorDTO();

        public RegistrarJugadorUserControl(MainWindow mainWindow, IAhorcadoService proxy)
        {
            try
            {
                InitializeComponent();
                _mainWindow = mainWindow;
                this.proxy = proxy;


                imgPerfil.Source = new BitmapImage(new Uri("pack://application:,,,/Images/iconoDefault.png"));

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al conectar con el servicio: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static string EncriptarContraseña(string contraseña)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(contraseña));
                StringBuilder builder = new StringBuilder();
                foreach (var byteValue in bytes)
                {
                    builder.Append(byteValue.ToString("x2"));
                }
                return builder.ToString();
            }
        }


        

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.CambiarVista(new IniciarSesionUserControl(_mainWindow, proxy));
        }

        private void BtnRegistrarme_Click(object sender, RoutedEventArgs e)
        {
            bool registroExitoso = false;

            if (EntradasValidas())
            {
                if (proxy == null)
                {
                    MessageBox.Show("El servicio no está disponible. Inténtelo más tarde.");
                    return;
                }

                jugadorRegistro.Nombre = tbNombre.Text.Trim();
                jugadorRegistro.FechaNacimiento = dpFechaNacimiento.SelectedDate.Value;
                jugadorRegistro.Correo = tbCorreo.Text.Trim();
                jugadorRegistro.Contraseña = EncriptarContraseña(tbPassword.Text.Trim());
                jugadorRegistro.Telefono = tbTelefono.Text.Trim();
                jugadorRegistro.PuntajeGlobal = 0;

                if (imgPerfil.Source != null)
                {
                    var bitmapSource = imgPerfil.Source as BitmapSource;
                    if (bitmapSource != null)
                    {
                        JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                        using (MemoryStream ms = new MemoryStream())
                        {
                            encoder.Save(ms);
                            jugadorRegistro.FotoPerfil = ms.ToArray();
                        }
                    }
                }
                else
                {
                    string rutaIcono = "Images\\iconoDefault.png";
                    if (File.Exists(rutaIcono))
                    {
                        jugadorRegistro.FotoPerfil = File.ReadAllBytes(rutaIcono);
                    }
                    else
                    {
                        jugadorRegistro.FotoPerfil = null;
                    }
                }

                registroExitoso = proxy.RegistrarJugador(jugadorRegistro);

                if (registroExitoso)
                {
                    string mensajeRegistroExito = Application.Current.TryFindResource("Msg_registroExitoso") as string;
                    MessageBox.Show(mensajeRegistroExito);
                }
                else
                {
                    string mensajeErrorRegistro = Application.Current.TryFindResource("Msg_ErrorRegistro") as string;
                    MessageBox.Show(mensajeErrorRegistro);
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


            string mensajeCorreo = !string.IsNullOrEmpty(errorCorreo)
                ? Application.Current.TryFindResource(errorCorreo) as string : null;

            string mensajePass = !string.IsNullOrEmpty(errorPass)
                ? Application.Current.TryFindResource(errorPass) as string : null;

            string mensajeNombre = !string.IsNullOrEmpty(errorNombre)
                ? Application.Current.TryFindResource(errorNombre) as string : null;

            string mensajeTelefono = !string.IsNullOrEmpty(errorTelefono)
                ? Application.Current.TryFindResource(errorTelefono) as string : null;

            string mensajeFecha = !string.IsNullOrEmpty(errorFechaNacimiento)
                ? Application.Current.TryFindResource(errorFechaNacimiento) as string : null;


            tblockErrorCorreo.Text = mensajeCorreo ?? "";
            tblockErrorPassword.Text = mensajePass ?? "";
            tblockErrorNombre.Text = mensajeNombre ?? "";
            tblockErrorTelefono.Text = mensajeTelefono ?? "";
            tblockErrorFecha.Text = mensajeFecha ?? "";

            if (errorCorreo != null || errorPass != null || errorNombre != null || errorTelefono != null || errorFechaNacimiento != null)
                valido = false;

            return valido;
        }

        public static string ValidarPasswordTextBox(TextBox passwordBox)
        {
            var password = passwordBox.Text.Trim();
            if (password.Length < 8)
            {
                return "La contraseña debe tener al menos 8 caracteres.";
            }
            if (!password.Any(char.IsLetter) || !password.Any(char.IsDigit))
            {
                return "La contraseña debe contener al menos una letra y un número.";
            }
            return null; 
        }

        public static string ValidarTelefono(TextBox telefonoBox)
        {
            var telefono = telefonoBox.Text.Trim();
            string pattern = @"^(?:\+52)?(?:\(\d{3}\)\s?|\d{3}-)\d{3}-\d{4}$";
            if (!Regex.IsMatch(telefono, pattern))
            {
                return "Número de teléfono no válido.";
            }
            return null;
        }

        public static string ValidarFechaNacimiento(DatePicker datePicker)
        {
            var fechaNacimiento = datePicker.SelectedDate;
            if (fechaNacimiento == null)
            {
                return "La fecha de nacimiento es obligatoria.";
            }
            if (fechaNacimiento.Value > DateTime.Now)
            {
                return "La fecha de nacimiento no puede ser en el futuro.";
            }
            return null;
        }

        private void BtnElegirFoto_Click(object sender, RoutedEventArgs e)
        {
            string tituloEligeFoto = Application.Current.TryFindResource("ElegirFoto_Titulo") as string;
            var openFileDialog = new OpenFileDialog
            {
                Title = tituloEligeFoto,
                Filter = "Archivos de imagen|*.jpg;*.jpeg;*.png;*.bmp"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                // Mostrar la imagen en el control Image
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(openFileDialog.FileName);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                imgPerfil.Source = bitmap;

                
                byte[] imagenBytes = File.ReadAllBytes(openFileDialog.FileName);
            }
        }
    }
}

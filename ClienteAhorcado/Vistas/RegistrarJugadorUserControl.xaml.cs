using ClienteAhorcado.Utilidades;
using ClienteAhorcado;
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

        public RegistrarJugadorUserControl(MainWindow mainWindow)
        {
            try
            {
                InitializeComponent();
                _mainWindow = mainWindow;
                var contexto = new InstanceContext(new DummyCallback());
                var factory = new DuplexChannelFactory<IAhorcadoService>(contexto, "AhorcadoEndpoint");
                proxy = factory.CreateChannel();

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


        public class DummyCallback : IAhorcadoCallback
        {
            public void ActualizarEstadoPartida(PartidaEstadoDTO estadoActual) { }
            public void NotificarFinPartida(string resultado, string palabra) { }
            public void RecibirMensajeChat(string nombreJugador, string mensaje) { }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.CambiarVista(new IniciarSesionUserControl(_mainWindow));
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
            return null; // No error
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
            var openFileDialog = new OpenFileDialog
            {
                Title = "Selecciona tu foto de perfil",
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
                // Guarda imagenBytes en tu DTO o donde lo necesites
            }
        }
    }
}

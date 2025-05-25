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
    /// <summary>
    /// Lógica de interacción para IniciarSesion.xaml
    /// </summary>
    public partial class IniciarSesion : UserControl, IAhorcadoCallback
    {
        private MainWindow _mainWindow;

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
            var correo = tbCorreo.Text;
            var pass = pbPassword.Password;

            usuarioActual = proxy.IniciarSesion(correo, pass);
            if (usuarioActual != null)
            {
                MessageBox.Show($"Bienvenido, {usuarioActual.Nombre}");
                MostrarMenuPrincipal();
            }
            else
            {
                MessageBox.Show("Usuario o contraseña incorrectos.");
            }
        }

        private void MostrarMenuPrincipal()
        {
            // Aquí puedes cambiar de ventana o habilitar controles
        }

        private void btnRegistrarse_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.CambiarVista(new RegistrarJugador(_mainWindow)); 
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

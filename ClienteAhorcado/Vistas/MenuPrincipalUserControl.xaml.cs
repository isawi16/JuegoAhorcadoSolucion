using BibliotecaClasesNetFramework.DTO;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using ClienteAhorcado.Utilidades;
using BibliotecaClasesNetFramework.Contratos;
using WPFLocalizeExtension.Engine;

namespace ClienteAhorcado.Vistas
{
    public partial class MenuPrincipalUserControl : UserControl
    {
        private MainWindow _mainWindow;
        private IAhorcadoService proxy;
        private JugadorDTO jugadorSesion;

        public MenuPrincipalUserControl(MainWindow mainWindow, JugadorDTO jugador, IAhorcadoService proxy)
        {
            try
            {
                InitializeComponent();
                _mainWindow = mainWindow;
                jugadorSesion = jugador;
                this.proxy = proxy;

                tblNombre.Text = $"Nombre: {jugador.Nombre}";
                tblCorreo.Text = $"Correo: {jugador.Correo}";

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
                    imagenPerfil.Source = bitmap;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al conectar con el servicio: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnPerfil_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.CambiarVista(new PerfilJugadorUserControl(_mainWindow, jugadorSesion, proxy));
        }

        private void BtnCrearPartida_Click(object sender, RoutedEventArgs e)
        {
            int? idiomaDefault = IdiomaHelper.ObtenerIDIdiomaDesdeSistema(proxy.ObtenerIdiomas());
            _mainWindow.CambiarVista(
                new SeleccionCategoriaIdiomaUserControl(
                    _mainWindow,
                    jugadorSesion,
                    proxy,
                    idiomaDefault
                )
            );
        }

        private void BtnUnirsePartida_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.CambiarVista(new ConsultarPartidasDisponiblesUserControl(_mainWindow, jugadorSesion, proxy));
        }

        private void BtnHistorialPartidas_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.CambiarVista(new ConsultarHistorialPartidasUserControl(_mainWindow, jugadorSesion, proxy));
        }

        private void BtnMarcadores_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.CambiarVista(new MarcadoresUserControl(_mainWindow, jugadorSesion, proxy));
        }

        private void BtnCerrarSesion_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.CambiarVista(new IniciarSesionUserControl(_mainWindow, proxy));
        }
    }
}

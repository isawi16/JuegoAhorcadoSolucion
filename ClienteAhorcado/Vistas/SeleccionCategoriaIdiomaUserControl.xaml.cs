using ServidorAhorcadoService;
using ServidorAhorcadoService.DTO;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClienteAhorcado.Vistas
{
    public partial class SeleccionCategoriaIdiomaUserControl : UserControl
    {
        private readonly IAhorcadoService proxy;
        private readonly JugadorDTO jugadorCreador;
        private readonly MainWindow _mainwindow;

        private int idiomaSeleccionadoId;
        private CategoriaDTO categoriaSeleccionada;
        private PalabraDTO palabraSeleccionada;
        private string idiomaSeleccionadoCodigo = "";
        private int? _indexPorDefecto = null;

        public SeleccionCategoriaIdiomaUserControl(MainWindow mainWindow,
                                           JugadorDTO jugadorCreador,
                                           int? idiomaPorDefecto = null)
        {
            InitializeComponent();

            _mainwindow = mainWindow;
            this.jugadorCreador = jugadorCreador;

            var contexto = new InstanceContext(new DummyCallback());
            var factory = new DuplexChannelFactory<IAhorcadoService>(contexto, "AhorcadoEndpoint");
            proxy = factory.CreateChannel();

            Loaded += (s, e) => CargarIdiomas(idiomaPorDefecto);
        }

        #region Carga inicial

        private void ReiniciarInterfaz()
        {
            //cbIdioma.SelectedIndex = -1;
            lstCategorias.ItemsSource = null;
            lstCategorias.IsEnabled = false;
            categoriaSeleccionada = null;

            lstPalabras.ItemsSource = null;
            lstPalabras.Visibility = Visibility.Collapsed;
            palabraSeleccionada = null;

            btnCrearPartida.IsEnabled = false;
        }

        #endregion

        #region Eventos de selección

        private void cbIdioma_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbIdioma.SelectedItem is IdiomaDTO idioma)
            {
                idiomaSeleccionadoId = idioma.CodigoIdioma; 
                idiomaSeleccionadoCodigo = idioma.CodigoIdioma == 1 ? "es" : "en";
                ReiniciarInterfaz();
                CargarCategoriasPorIdioma(idioma.CodigoIdioma);
                lstCategorias.IsEnabled = true;
            }
            else
            {
                idiomaSeleccionadoCodigo = "";
                idiomaSeleccionadoId = 0;
                ReiniciarInterfaz();
            }
        }

        public class DummyCallback : IAhorcadoCallback
        {
            public void ActualizarEstadoPartida(PartidaEstadoDTO estadoActual) { }
            public void NotificarFinPartida(string resultado, string palabra) { }
            public void RecibirMensajeChat(string nombreJugador, string mensaje) { }
        }

        private void CargarIdiomas(int? idiomaPorDefecto)
        {
            var idiomas = proxy.ObtenerIdiomas();
            cbIdioma.ItemsSource = idiomas;
            cbIdioma.DisplayMemberPath = "Nombre";

            if (idiomaPorDefecto.HasValue)
            {
                var idioma = idiomas.FirstOrDefault(i => i.CodigoIdioma == idiomaPorDefecto.Value);
                if (idioma != null)
                    cbIdioma.SelectedItem = idioma;
            }

        }

        private void CargarCategoriasPorIdioma(int codigoIdioma)
        {
            var categorias = proxy.ObtenerCategoriasPorIdioma(codigoIdioma);
            lstCategorias.ItemsSource = categorias;
        }

        private void lstCategorias_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstCategorias.SelectedItem is CategoriaDTO categoria)
            {
                categoriaSeleccionada = categoria;
                palabraSeleccionada = null;
                btnCrearPartida.IsEnabled = false;
                CargarPalabras(categoriaSeleccionada.IDCategoria, idiomaSeleccionadoId);
            }
        }

        private void CargarPalabras(int idCategoria, int codigoIdioma)
        {
            System.Diagnostics.Debug.WriteLine($"Buscando palabras para categoria={idCategoria}, idioma={codigoIdioma}");

            var palabras = proxy.ObtenerPalabrasPorIdiomaYCategoria(codigoIdioma, idCategoria);

            System.Diagnostics.Debug.WriteLine($"Palabras recibidas: {palabras.Count}");

            lstPalabras.ItemsSource = palabras;
            lstPalabras.Visibility = palabras.Any() ? Visibility.Visible : Visibility.Collapsed;
            palabraSeleccionada = null;
            btnCrearPartida.IsEnabled = false;
        }

        private void lstPalabras_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstPalabras.SelectedItem is PalabraDTO palabra)
            {
                palabraSeleccionada = proxy.ObtenerPalabraConDescripcion(palabra.IDPalabra);
                btnCrearPartida.IsEnabled = palabraSeleccionada != null;
            }
            else
            {
                palabraSeleccionada = null;
                btnCrearPartida.IsEnabled = false;
            }
        }

        #endregion

        #region Crear partida

        private void btnCrearPartida_Click(object sender, RoutedEventArgs e)
        {
            if (palabraSeleccionada == null) return;

            int idPartida = proxy.CrearPartida(jugadorCreador.IDJugador, palabraSeleccionada.IDPalabra);

            if (idPartida > 0)
            {
                _mainwindow.CambiarVista(
                    new SeleccionCategoriaIdiomaUserControl(
                        _mainwindow,
                        jugadorCreador,
                        idiomaSeleccionadoId)
                );
            }
            else
            {
                MessageBox.Show("No se pudo crear la partida. Intenta de nuevo.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion
    }
}

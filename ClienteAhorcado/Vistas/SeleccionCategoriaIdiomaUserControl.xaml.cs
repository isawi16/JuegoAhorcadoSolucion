using ServidorAhorcadoService;
using ServidorAhorcadoService.DTO;
using System;
using System.Linq;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;

namespace ClienteAhorcado.Vistas
{
    public partial class SeleccionCategoriaIdiomaUserControl : UserControl
    {
        private readonly IAhorcadoService proxy;
        private readonly JugadorDTO jugadorCreador;
        private readonly MainWindow _mainwindow;

        private int idiomaSeleccionadoId;        // ahora es int (1 = Español, 2 = English)
        private CategoriaDTO categoriaSeleccionada;
        private string dificultadSeleccionada = "";
        private PalabraDTO palabraSeleccionada;
        private string idiomaSeleccionadoCodigo = "";


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

            CargarIdiomas(idiomaPorDefecto);
        }

        #region Carga inicial

        private void CargarIdiomas(int? idiomaPorDefecto)
        {
            cbIdioma.Items.Clear();
            int indexPorDefecto = -1;
            int i = 0;

            foreach (var idioma in proxy.ObtenerIdiomas()) // List<IdiomaDTO>
            {
                cbIdioma.Items.Add(new ComboBoxItem
                {
                    Content = idioma.Nombre,       // "Español"
                    Tag = idioma.CodigoIdioma  // 1, 2 …
                });

                if (idiomaPorDefecto != null && idioma.CodigoIdioma == idiomaPorDefecto)
                    indexPorDefecto = i;

                i++;
            }

            ConfigurarEstadoInicial();

            if (indexPorDefecto >= 0)
                cbIdioma.SelectedIndex = indexPorDefecto; // dispara SelectionChanged
        }

        private void ConfigurarEstadoInicial()
        {
            cbIdioma.SelectedIndex = -1;
            lstCategorias.Items.Clear();
            lstCategorias.IsEnabled = false;

            spDificultad.Visibility = Visibility.Collapsed;
            rbFacil.IsChecked = rbMedia.IsChecked = rbDificil.IsChecked = false;

            lstPalabras.Items.Clear();
            lstPalabras.Visibility = Visibility.Collapsed;

            btnCrearPartida.IsEnabled = false;
        }

        #endregion

        #region Eventos de selección

        private void cbIdioma_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbIdioma.SelectedItem is ComboBoxItem item && item.Tag is int codigo)
            {
                idiomaSeleccionadoId = codigo; // Asigna el ID del idioma seleccionado
                idiomaSeleccionadoCodigo = codigo == 1 ? "es" : "en"; // Opcional: si necesitas el código string
                ReiniciarCategorias();
                CargarCategoriasPorIdioma(codigo);
                lstCategorias.IsEnabled = true;
            }
            else
            {
                idiomaSeleccionadoCodigo = "";
                idiomaSeleccionadoId = 0;
                ReiniciarCategorias();
            }
        }
        public class DummyCallback : IAhorcadoCallback
        {
            public void ActualizarEstadoPartida(PartidaEstadoDTO estadoActual) { }
            public void NotificarFinPartida(string resultado, string palabra) { }
            public void RecibirMensajeChat(string nombreJugador, string mensaje) { }
        }

        private void ReiniciarCategorias()
        {
            if (lstCategorias == null) throw new NullReferenceException("lstCategorias es null");
            if (spDificultad == null) throw new NullReferenceException("spDificultad es null");
            if (rbFacil == null) throw new NullReferenceException("rbFacil es null");
            if (rbMedia == null) throw new NullReferenceException("rbMedia es null");
            if (rbDificil == null) throw new NullReferenceException("rbDificil es null");
            if (lstPalabras == null) throw new NullReferenceException("lstPalabras es null");
            if (btnCrearPartida == null) throw new NullReferenceException("btnCrearPartida es null");

            lstCategorias.Items.Clear();
            lstCategorias.IsEnabled = false;
            categoriaSeleccionada = null;

            spDificultad.Visibility = Visibility.Collapsed;
            rbFacil.IsChecked = rbMedia.IsChecked = rbDificil.IsChecked = false;
            dificultadSeleccionada = "";

            lstPalabras.Items.Clear();
            lstPalabras.Visibility = Visibility.Collapsed;
            palabraSeleccionada = null;

            btnCrearPartida.IsEnabled = false;
        }

        private void CargarCategoriasPorIdioma(int codigoIdioma)
        {
            foreach (var categoria in proxy.ObtenerCategoriasPorIdioma(codigoIdioma))
            {
                lstCategorias.Items.Add(new ListBoxItem
                {
                    Content = categoria.Nombre,
                    Tag = categoria.IDCategoria
                });
            }
        }

        private void lstCategorias_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstCategorias.SelectedItem is ListBoxItem item)
            {
                categoriaSeleccionada = new CategoriaDTO
                {
                    IDCategoria = (int)item.Tag,
                    Nombre = item.Content.ToString()
                };

                spDificultad.Visibility = Visibility.Visible;

                rbFacil.IsChecked = rbMedia.IsChecked = rbDificil.IsChecked = false;
                lstPalabras.Items.Clear();
                lstPalabras.Visibility = Visibility.Collapsed;
                palabraSeleccionada = null;
                btnCrearPartida.IsEnabled = false;
            }
        }

        private void rbDificultad_Checked(object sender, RoutedEventArgs e)
        {
            if (categoriaSeleccionada == null) return;

            if (rbFacil.IsChecked == true) dificultadSeleccionada = "Facil";
            else if (rbMedia.IsChecked == true) dificultadSeleccionada = "Media";
            else if (rbDificil.IsChecked == true) dificultadSeleccionada = "Dificil";
            else dificultadSeleccionada = "";

            if (dificultadSeleccionada != "")
            {
                CargarPalabras(categoriaSeleccionada.IDCategoria,
                               idiomaSeleccionadoId,
                               dificultadSeleccionada);
            }
        }

        private void CargarPalabras(int idCategoria, int codigoIdioma, string dificultad)
        {
            var palabras = proxy.ObtenerPalabrasPorIdiomaYCategoria(codigoIdioma, idCategoria)
                                .Where(p => p.Dificultad.Equals(dificultad, StringComparison.OrdinalIgnoreCase))
                                .ToList();

            lstPalabras.Items.Clear();
            foreach (var palabra in palabras)
            {
                lstPalabras.Items.Add(new ListBoxItem
                {
                    Content = palabra.Texto,
                    Tag = palabra.IDPalabra
                });
            }

            lstPalabras.Visibility = palabras.Any() ? Visibility.Visible : Visibility.Collapsed;
            palabraSeleccionada = null;
            btnCrearPartida.IsEnabled = false;
        }

        private void lstPalabras_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstPalabras.SelectedItem is ListBoxItem item)
            {
                int idPalabra = (int)item.Tag;
                palabraSeleccionada = proxy.ObtenerPalabraConDescripcion(idPalabra, idiomaSeleccionadoId);
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

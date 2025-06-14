using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ServidorAhorcadoService;
using ServidorAhorcadoService.DTO;

namespace ClienteAhorcado.Vistas
{
    public partial class SeleccionCategoriaIdiomaUserControl : UserControl
    {
        private readonly IAhorcadoService proxy;
        private readonly JugadorDTO jugadorCreador;
        private readonly MainWindow _mainwindow;

        private string idiomaSeleccionado = "";
        private CategoriaDTO categoriaSeleccionada;
        private string dificultadSeleccionada = "";
        private PalabraDTO palabraSeleccionada;

        public SeleccionCategoriaIdiomaUserControl(MainWindow mainWindow,
                                                   IAhorcadoService proxy,
                                                   JugadorDTO jugadorCreador)
        {
            InitializeComponent();
            this._mainwindow = mainWindow;
            this.proxy = proxy;
            this.jugadorCreador = jugadorCreador;

            ConfigurarEstadoInicial();
            CargarIdiomas();
        }

        #region Inicialización y helpers

        private void ConfigurarEstadoInicial()
        {
            // Nada seleccionado al inicio
            cbIdioma.SelectedIndex = -1;
            lstCategorias.IsEnabled = false;
            spDificultad.Visibility = Visibility.Collapsed;
            lstPalabras.Visibility = Visibility.Collapsed;
            btnCrearPartida.IsEnabled = false;
        }

        private void CargarIdiomas()
        {
            cbIdioma.Items.Clear();
            foreach (var idioma in proxy.ObtenerIdiomas())
            {
                cbIdioma.Items.Add(new ComboBoxItem
                {
                    Content = idioma.Nombre,
                    Tag = idioma.CodigoIdioma
                });
            }
        }

        #endregion

        #region Eventos de selección

        private void cbIdioma_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbIdioma.SelectedItem is ComboBoxItem item)
            {
                idiomaSeleccionado = item.Tag.ToString();   // Usa el código, no el nombre
                ReiniciarCategorias();
                CargarCategoriasPorIdioma(idiomaSeleccionado);
            }
            else
            {
                idiomaSeleccionado = "";
                ReiniciarCategorias();
            }
        }

        private void ReiniciarCategorias()
        {
            lstCategorias.Items.Clear();
            lstCategorias.IsEnabled = true;
            categoriaSeleccionada = null;

            spDificultad.Visibility = Visibility.Collapsed;
            rbFacil.IsChecked = false;
            rbMedia.IsChecked = false;
            rbDificil.IsChecked = false;
            dificultadSeleccionada = "";

            lstPalabras.Items.Clear();
            lstPalabras.Visibility = Visibility.Collapsed;
            palabraSeleccionada = null;

            btnCrearPartida.IsEnabled = false;
        }

        private void CargarCategoriasPorIdioma(string codigoIdioma)
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

                // Muestra panel de dificultad
                spDificultad.Visibility = Visibility.Visible;

                // Reinicia selección de dificultad/palabra
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
                               idiomaSeleccionado,
                               dificultadSeleccionada);
            }
        }

        private void CargarPalabras(int idCategoria, string idioma, string dificultad)
        {
            var palabras = proxy.ObtenerPalabrasPorIdiomaYCategoria(idioma, idCategoria)
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
                palabraSeleccionada = proxy.ObtenerPalabraConDescripcion(idPalabra, idiomaSeleccionado);
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

            int idPartida = proxy.CrearPartida(jugadorCreador.IDJugador,
                                               palabraSeleccionada.IDPalabra);

            if (idPartida > 0)
            {
                _mainwindow.CambiarVista(
                    new JuegoAhorcadoUserControl1(jugadorCreador,
                                                  palabraSeleccionada,
                                                  idPartida,
                                                  true /* esCreador */));
            }
            else
            {
                MessageBox.Show("No se pudo crear la partida. Intenta de nuevo.");
            }
        }

        #endregion
    }
}

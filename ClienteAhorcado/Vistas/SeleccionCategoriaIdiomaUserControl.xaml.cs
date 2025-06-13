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
        // Debes recibir estos del contexto principal o pasarlos al crear el control
        private IAhorcadoService proxy;
        private JugadorDTO jugadorCreador;

        // Para llevar el seguimiento de selecciones
        private string idiomaSeleccionado = "";
        private CategoriaDTO categoriaSeleccionada;
        private string dificultadSeleccionada = "";
        private PalabraDTO palabraSeleccionada;

        public SeleccionCategoriaIdiomaUserControl(IAhorcadoService proxy, JugadorDTO jugadorCreador)
        {
            InitializeComponent();
            this.proxy = proxy;
            this.jugadorCreador = jugadorCreador;
            CargarIdiomas();
        }

        private void CargarIdiomas()
        {
            var idiomas = proxy.ObtenerIdiomas(); // Devuelve List<IdiomaDTO>
            cbIdioma.Items.Clear();
            foreach (var idioma in idiomas)
            {
                var item = new ComboBoxItem { Content = idioma.Nombre, Tag = idioma.CodigoIdioma };
                cbIdioma.Items.Add(item);
            }
            cbIdioma.SelectedIndex = 0; // Selecciona el primero por default
        }

        private void cbIdioma_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbIdioma.SelectedItem is ComboBoxItem item)
            {
                idiomaSeleccionado = item.Content.ToString();
                categoriaSeleccionada = null;
                dificultadSeleccionada = "";
                palabraSeleccionada = null;
                btnCrearPartida.IsEnabled = false;
                spDificultad.Visibility = Visibility.Collapsed;
                lstPalabras.Visibility = Visibility.Collapsed;
                lstCategorias.Items.Clear();
                CargarCategoriasPorIdioma(idiomaSeleccionado);
            }
        }

        private void CargarCategoriasPorIdioma(string idioma)
        {
            var categorias = proxy.ObtenerCategoriasPorIdioma(idioma);
            lstCategorias.Items.Clear();
            foreach (var categoria in categorias)
            {
                var item = new ListBoxItem { Content = categoria.Nombre, Tag = categoria.IDCategoria };
                lstCategorias.Items.Add(item);
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
                // Mostrar opciones de dificultad
                spDificultad.Visibility = Visibility.Visible;
                lstPalabras.Items.Clear();
                lstPalabras.Visibility = Visibility.Collapsed;
                palabraSeleccionada = null;
                btnCrearPartida.IsEnabled = false;
                // Desmarcar radios
                rbFacil.IsChecked = false;
                rbMedia.IsChecked = false;
                rbDificil.IsChecked = false;
            }
        }

        private void rbDificultad_Checked(object sender, RoutedEventArgs e)
        {
            if (categoriaSeleccionada == null) return;

            if (rbFacil.IsChecked == true) dificultadSeleccionada = "Facil";
            else if (rbMedia.IsChecked == true) dificultadSeleccionada = "Media";
            else if (rbDificil.IsChecked == true) dificultadSeleccionada = "Dificil";
            else dificultadSeleccionada = "";

            if (!string.IsNullOrEmpty(dificultadSeleccionada))
            {
                CargarPalabras(categoriaSeleccionada.IDCategoria, idiomaSeleccionado, dificultadSeleccionada);
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
                var item = new ListBoxItem { Content = palabra.Texto, Tag = palabra.IDPalabra };
                lstPalabras.Items.Add(item);
            }

            lstPalabras.Visibility = palabras.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            palabraSeleccionada = null;
            btnCrearPartida.IsEnabled = false;
        }

        private void lstPalabras_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstPalabras.SelectedItem is ListBoxItem item)
            {
                int idPalabra = (int)item.Tag;
                // Puedes obtener más info si quieres
                palabraSeleccionada = proxy.ObtenerPalabraConDescripcion(idPalabra, idiomaSeleccionado);
                btnCrearPartida.IsEnabled = palabraSeleccionada != null;
            }
            else
            {
                palabraSeleccionada = null;
                btnCrearPartida.IsEnabled = false;
            }
        }

        // Fix for CS1061: Replace 'Content.Content' with 'this.Content'
        private void btnCrearPartida_Click(object sender, RoutedEventArgs e)
        {
            if (palabraSeleccionada == null || jugadorCreador == null) return;

            // Ahora recibes el ID de la partida creada
            int idPartida = proxy.CrearPartida(jugadorCreador.IDJugador, palabraSeleccionada.IDPalabra);

            if (idPartida > 0)
            {
                var parentWindow = Window.GetWindow(this) as MainWindow;
                if (parentWindow != null)
                {
                    parentWindow.MainContent.Content = new JuegoAhorcadoUserControl1(jugadorCreador, palabraSeleccionada, idPartida, true /* esCreador */);
                }
            }
            else
            {
                MessageBox.Show("No se pudo crear la partida. Intenta de nuevo.");
            }
        }



    }
}

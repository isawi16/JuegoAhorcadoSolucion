using ClienteAhorcado;
using BibliotecaClasesNetFramework.Contratos;
using BibliotecaClasesNetFramework.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;

namespace ClienteAhorcado.Vistas
{
    public partial class ConsultarPartidasDisponiblesUserControl : UserControl
    {
        private MainWindow _mainWindow;
        IAhorcadoService proxy;
        JugadorDTO jugadorSesion = new JugadorDTO();
       

        List<PartidaDTO> partidasDisponibles = new List<PartidaDTO>();

        public ConsultarPartidasDisponiblesUserControl(MainWindow mainWindow, JugadorDTO jugador, IAhorcadoService proxy)
        {
            try
            {
                InitializeComponent();
                _mainWindow = mainWindow;
                jugadorSesion = jugador;
                this.proxy = proxy;


                LlenarTablaPartidas();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al conectar con el servicio: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            
        }

        public void LlenarTablaPartidas()
        {
            dgPartidas.ItemsSource = null;

            Console.WriteLine("Antes de llamar a ObtenerPartidasDisponibles");
            var partidasDisponibles = proxy.ObtenerPartidasDisponibles();
            Console.WriteLine("Después de llamar a ObtenerPartidasDisponibles");

            if (partidasDisponibles != null && partidasDisponibles.Count > 0)
            {
                // Puedes ordenar si lo necesitas, por ejemplo por IDPartida
                var lista = partidasDisponibles.OrderBy(p => p.IDPartida).ToList();

                dgPartidas.ItemsSource = lista;

                Console.WriteLine("Partidas cargadas en la tabla:");
                foreach (var partida in lista)
                {
                    Console.WriteLine($"IDPartida: {partida.IDPartida}, Categoría: {partida.CategoriaNombre}");
                }
            }
            else
            {
                string mensajeSinPartidas = Application.Current.TryFindResource("Msg_NoPartidasDisponibles") as string;
                MessageBox.Show(

                    mensajeSinPartidas,
                    "Info",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
        }


       
        private void UnirsePartida_Click(object sender, RoutedEventArgs e)
        {
            if (dgPartidas.SelectedItem is PartidaCategoriaDTO partidaSeleccionada)
            {
                // 1. Intentar unirse a la partida
                bool unido = proxy.UnirseAPartida(partidaSeleccionada.IDPartida, jugadorSesion.IDJugador);
                if (!unido)
                {
                    string mensajeNoUnirse = Application.Current.TryFindResource("Msg_NoPudoUnirsePartida") as string
                        ?? "Oops! Ya no está disponible, por favor selecciona otra";
                    MessageBox.Show(mensajeNoUnirse, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    LlenarTablaPartidas();
                    return;
                }

                // 2. Obtener la palabra secreta para la partida
                var palabra = proxy.ObtenerPalabraConDescripcion(partidaSeleccionada.IDPalabra);
                if (palabra == null)
                {
                    string mensajeNoObtuvoPalabra = Application.Current.TryFindResource("Msg_NoObtenerPalabra") as string
                        ?? "No se pudo obtener la palabra de la partida.";
                    MessageBox.Show(mensajeNoObtuvoPalabra, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // 3. Abrir la pantalla de juego como retador (esCreador = false)
                _mainWindow.CargarPantallaJuego(jugadorSesion, palabra, partidaSeleccionada.IDPartida, false);
            }
            else
            {
                string mensajeSeleccionaPartida = Application.Current.TryFindResource("Msg_SeleccionaPartida") as string
                    ?? "Selecciona una partida de la lista.";
                MessageBox.Show(mensajeSeleccionaPartida, "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private string ObtenerNombreIdioma(int codigoIdioma)
        {
            return codigoIdioma == 1 ? "Español" : "English";
        }

        private void ActualizarLista_Click(object sender, RoutedEventArgs e)
        {
            LlenarTablaPartidas();
        }

        private void CerrarButton_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.CambiarVista(new MenuPrincipalUserControl(_mainWindow, jugadorSesion, proxy));
        }
    }
}

using ClienteAhorcado;
using ClienteAhorcado.Vistas;
using ServidorAhorcadoService;
using ServidorAhorcadoService.DTO;
using ServidorAhorcadoService.Model;
using System;
using System.ServiceModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using ClienteAhorcado.Utilidades;

namespace ClienteAhorcado
{
    public partial class MainWindow : Window, IAhorcadoCallback
    {
        public IAhorcadoService proxy;
        private JugadorDTO jugadorActual;
        private PartidaDTO partidaActual;
        private PalabraDTO palabraSeleccionada;
        private int idPartida;
        private bool esCreador;
        private int codigoIdioma;

        public MainWindow()
        {
            InitializeComponent();

            MainContent.Content = new IniciarSesionUserControl(this); 


            // Cargar la vista de login como primera pantalla
            MainContent.Content = new IniciarSesionUserControl(this);
        }




        /*public MainWindow()
        {
            InitializeComponent();
            var callbackInstance = new InstanceContext(new AhorcadoCallbackCliente(this));
            var factory = new DuplexChannelFactory<IAhorcadoService>(callbackInstance, "AhorcadoEndpoint");
            proxy = factory.CreateChannel();


        }*/

        public MainWindow(JugadorDTO jugador, PartidaDTO partida, bool creador)
        {
            InitializeComponent();
            jugadorActual = jugador;
            partidaActual = partida;
            esCreador = creador;

            CargarPantallaJuego(jugadorActual, palabraSeleccionada, idPartida, esCreador);        }

        public void CambiarVista(UserControl nuevaVista)
        {
            MainContent.Content = nuevaVista;
        }

       

        public void CargarPantallaJuego(JugadorDTO jugador, PalabraDTO palabra, int idPartida, bool esCreador)
        {
            var controlJuego = new JuegoAhorcadoUserControl1(jugador, palabra, idPartida, esCreador);
            MainContent.Content = controlJuego;
        }


        public void ActualizarEstadoPartida(PartidaEstadoDTO estadoActual)
        {
            Dispatcher.Invoke(() =>
            {
                if (MainContent.Content is JuegoAhorcadoUserControl1 juegoControl)
                {
                    juegoControl.ActualizarEstadoDesdeCallback(estadoActual);
                }
            });
        }
    


        public void RecibirMensajeChat(string nombreJugador, string mensaje)
        {
            Dispatcher.Invoke(() =>
            {
                if (MainContent.Content is JuegoAhorcadoUserControl1 juegoControl)
                {
                    juegoControl.MostrarMensajeChat(mensaje);
                }
            });
        }

        public void NotificarFinPartida(string resultado, string palabra)
        {
            Dispatcher.Invoke(() =>
            {
                MessageBox.Show($"{resultado}. La palabra era: {palabra}", "Fin de la partida");
            });
        }

        public void MostrarMensajeChat(string nombreJugador, string mensaje)
        {
            Dispatcher.Invoke(() =>
            {
                if (MainContent.Content is JuegoAhorcadoUserControl1 juegoControl)
                {
                    juegoControl.MostrarMensajeChat(mensaje);
                }
            });
        }

    }
}


/*IAhorcadoService proxy;
JugadorDTO usuarioActual;
int idPartidaActual;

public MainWindow()
{
    try
    {
        InitializeComponent();
        var contexto = new InstanceContext(this);
        var factory = new DuplexChannelFactory<IAhorcadoService>(contexto, "AhorcadoEndpoint");
        proxy = factory.CreateChannel();
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Error al conectar con el servicio: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }

}

private void btnLogin_Click(object sender, RoutedEventArgs e)
{
    var correo = txtEmail.Text;
    var pass = txtPassword.Password;

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
    btnViewGames.IsEnabled = true;
    btnCreateGame.IsEnabled = true;
}

private void btnViewGames_Click(object sender, RoutedEventArgs e)
{
    var partidas = proxy.ObtenerPartidasDisponibles();
    lstPartidas.Items.Clear();
    foreach (var partida in partidas)
    {
        lstPartidas.Items.Add($"ID:{partida.IDPartida} | Creador: {partida.CreadorNombre} | Estado: {partida.Estado}");
    }
}

private void btnCreateGame_Click(object sender, RoutedEventArgs e)
{
    int idPalabra = ObtenerPalabraSeleccionada();
    bool ok = proxy.CrearPartida(usuarioActual.IDJugador, idPalabra);
    if (ok)
        MessageBox.Show("Partida creada, esperando retador...");
    else
        MessageBox.Show("No se pudo crear la partida.");
}

private void btnJoinGame_Click(object sender, RoutedEventArgs e)
{
    if (lstPartidas.SelectedIndex == -1)
    {
        MessageBox.Show("Seleccione una partida.");
        return;
    }
    var partidaSeleccionada = lstPartidas.SelectedItem.ToString();
    int idPartida = ExtraerIDPartida(partidaSeleccionada);

    bool ok = proxy.UnirseAPartida(idPartida, usuarioActual.IDJugador);
    if (ok)
    {
        idPartidaActual = idPartida;
        MessageBox.Show("Unido a la partida. Esperando turno...");
    }
    else
    {
        MessageBox.Show("No se pudo unir.");
    }
}

private int ExtraerIDPartida(string texto)
{
    var partes = texto.Split('|');
    var idPart = partes[0].Replace("ID:", "").Trim();
    return int.Parse(idPart);
}

private int ObtenerPalabraSeleccionada()
{
    // Aquí puedes mostrar un diálogo o selector de palabra
    return 1; // Por ahora regresamos una palabra fija
}

private void btnSendLetter_Click(object sender, RoutedEventArgs e)
{
    if (string.IsNullOrWhiteSpace(txtLetra.Text))
    {
        MessageBox.Show("Ingresa una letra.");
        return;
    }
    char letra = txtLetra.Text.ToLower()[0];
    bool ok = proxy.EnviarLetra(idPartidaActual, usuarioActual.IDJugador, letra);
    if (ok)
    {
        txtLetra.Text = "";
    }
    else
    {
        MessageBox.Show("Error al enviar letra.");
    }
}

// Callbacks del servidor

public void RecibirMensajeChat(string nombreJugador, string mensaje)
{
    Dispatcher.Invoke(() => {
        lstChat.Items.Add($"[{nombreJugador}] {mensaje}");
    });
}

public void ActualizarEstadoPartida(PartidaEstadoDTO estadoActual)
{
    Dispatcher.Invoke(() =>
    {
        lblEstado.Content = estadoActual.PalabraConGuiones;
        lblIntentos.Content = $"Intentos restantes: {estadoActual.IntentosRestantes}";
        lstLetrasUsadas.ItemsSource = estadoActual.LetrasUsadas;
    });
}

public void NotificarFinPartida(string resultado, string palabra)
{
    Dispatcher.Invoke(() => {
        MessageBox.Show($"{resultado}. La palabra era: {palabra}");
    });
}

/*public void JugadorSeUnio(string nombreJugador)
{
    throw new NotImplementedException();
}

public void JugadorAbandono(string nombreJugador)
{
    throw new NotImplementedException();
}

public void CambiarTurno(string nombreJugadorActual)
{
    throw new NotImplementedException();
}

public void ActualizarCantidadJugadores(int cantidadConectados)
{
    throw new NotImplementedException();
}*/


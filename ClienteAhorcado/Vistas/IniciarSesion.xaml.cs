using System;
using System.Collections.Generic;
using System.Linq;
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
    public partial class IniciarSesion : Window
    {
        public IniciarSesion()
        {
            InitializeComponent();
        }

        private void Click_IniciarSesion(object sender, RoutedEventArgs e)
        {
            // Lógica para manejar el evento de clic en el botón "IniciarSesion"
            MessageBox.Show("Botón Iniciar Sesión presionado.");
        }
    }
}

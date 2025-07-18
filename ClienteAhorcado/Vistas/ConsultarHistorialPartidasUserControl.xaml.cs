﻿using BibliotecaClasesNetFramework.Contratos;
using BibliotecaClasesNetFramework.DTO;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace ClienteAhorcado.Vistas
{
       public partial class ConsultarHistorialPartidasUserControl : UserControl
    {

        private MainWindow _mainWindow;
        IAhorcadoService proxy;
        private JugadorDTO jugadorSesion;

        private List<PartidaDTO> historialPartidas = new List<PartidaDTO>();
        public ConsultarHistorialPartidasUserControl(MainWindow mainWindow, JugadorDTO jugador, IAhorcadoService proxy)
        {
            try
            {
                InitializeComponent();
                _mainWindow = mainWindow;
                jugadorSesion = jugador;     
                this.proxy = proxy;

                LlenarTablaHistorialPartidas();

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al conectar con el servicio: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LlenarTablaHistorialPartidas()
        {
            dgHistorial.ItemsSource = null; 
            historialPartidas = proxy.ConsultarPartidasJugadas (jugadorSesion.IDJugador);
            if (historialPartidas != null && historialPartidas.Count > 0)
            {
                dgHistorial.ItemsSource = historialPartidas;
            }
            else
            {
                string mensajeHistorialVacio = Application.Current.TryFindResource("Msg_HistorialVacio") as string;
                MessageBox.Show(mensajeHistorialVacio, "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }


        private void BtnVolver_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.CambiarVista(new MenuPrincipalUserControl(_mainWindow, jugadorSesion, proxy));
        }

        
    }
}

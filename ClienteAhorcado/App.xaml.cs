using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using WPFLocalizeExtension.Engine;

namespace ClienteAhorcado
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static event Action IdiomaCambiado;
        public App()
        {
            this.Startup += OnStartup;
        }

        private void OnStartup(object sender, StartupEventArgs e)
        {
            CambiarIdioma("es-MX");
        }

        public void CambiarIdioma(string codigoCultura)
        {
            string nombreArchivo;

            if (codigoCultura == "en")
                nombreArchivo = "en-US";
            else if (codigoCultura == "es")
                nombreArchivo = "es-MX";
            else
                nombreArchivo = codigoCultura;

            var cultura = new CultureInfo(nombreArchivo);
            Thread.CurrentThread.CurrentCulture = cultura;
            Thread.CurrentThread.CurrentUICulture = cultura;

            var diccionario = new ResourceDictionary
            {
                Source = new Uri($"Properties/Strings.{nombreArchivo}.xaml", UriKind.Relative)
            };

            var diccionarioExistente = Resources.MergedDictionaries
                .FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains("Strings."));
            if (diccionarioExistente != null)
            {
                Resources.MergedDictionaries.Remove(diccionarioExistente);
            }

            Resources.MergedDictionaries.Add(diccionario);
            IdiomaCambiado?.Invoke();
        }

    }
}



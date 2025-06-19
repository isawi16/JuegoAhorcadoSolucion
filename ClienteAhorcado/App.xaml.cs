using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using WPFLocalizeExtension.Engine;

namespace ClienteAhorcado
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var culture = new System.Globalization.CultureInfo("es");
            WPFLocalizeExtension.Engine.LocalizeDictionary.Instance.Culture = culture;
            System.Threading.Thread.CurrentThread.CurrentCulture = culture;
            System.Threading.Thread.CurrentThread.CurrentUICulture = culture;
        }
    }
}

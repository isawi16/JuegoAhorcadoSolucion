using BibliotecaClasesNetFramework.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClienteAhorcado.Utilidades
{
    public static class IdiomaHelper
    {
        public static int? ObtenerIDIdiomaDesdeSistema(List<IdiomaDTO> idiomas)
        {
            string iso = Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName;

            foreach (var idioma in idiomas)
            {
                if ((iso == "es" && idioma.Nombre.Equals("Español", StringComparison.OrdinalIgnoreCase)) ||
                    (iso == "en" && idioma.Nombre.Equals("English", StringComparison.OrdinalIgnoreCase)))
                {
                    return idioma.CodigoIdioma;
                }
            }

            return null;
        }
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServidorAhorcadoService.DTO
{
    public class HistorialPuntajeDTO
    {
        public string Tipo { get; set; }
        public int Puntaje { get; set; }
        public DateTime Fecha { get; set; }
        public string Palabra { get; set; }
        public string Rival { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServidorAhorcadoService.DTO
{
    public class PalabraDTO
    {
        public int IDPalabra { get; set; }
        public string Texto { get; set; }
        public string Definicion { get; set; }
        public string Dificultad { get; set; }
        public string IDCategoria { get; set; }
    }
}

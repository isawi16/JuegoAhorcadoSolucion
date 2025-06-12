using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServidorAhorcadoService.DTO
{
    public class PartidaDTO
    {
        public int IDPartida { get; set; }
        public int IDJugadorCreador { get; set; }
        public int IDJugadorRetador { get; set; }
        public int IDEstado { get; set; }
        public string Estado { get; set; }
        public string Fecha { get; set; }
        public int? Ganador { get; set; }
        public int IDPalabra { get; set; }
        public string PalabraTexto { get; set; }
        public int Puntaje { get; set; }
        public int? IDCancelador { get; set; }
        public List<char> LetrasUsadas { get; set; }

        public int IntentosRestantes { get; set; }

        public string PalabraConGuiones { get; set; }



        public string CreadorNombre { get; set; }
        public string RetadorNombre { get; set; }
        public string GanadorNombre { get; set; }
        public string RivalNombre { get; set; }
    }

}


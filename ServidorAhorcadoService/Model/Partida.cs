using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace ServidorAhorcadoService.Model
{
    public class Partida
    {
        [Key]
        public int IDPartida { get; set; }

        [ForeignKey("Creador")]
        public int IDJugadorCreador { get; set; }

        [ForeignKey("Retador")]
        public int? IDJugadorRetador { get; set; }

        [ForeignKey("Estado")]
        public int IDEstado { get; set; }

        public DateTime Fecha { get; set; }

        [ForeignKey("Palabra")]
        public int IDPalabra { get; set; }

        public int Puntaje { get; set; }

        [ForeignKey("GanadorJugador")]
        public int? Ganador { get; set; }

        [ForeignKey("Cancelador")]
        public int? IDCancelador { get; set; }

        [StringLength(200)]
        public string LetrasUsadas { get; set; }

        public int IntentosRestantes { get; set; }

        public virtual Jugador Creador { get; set; }
        public virtual Jugador Retador { get; set; }
        public virtual Jugador GanadorJugador { get; set; }
        public virtual Jugador Cancelador { get; set; }
        public virtual Palabra Palabra { get; set; }
        public virtual Estado Estado { get; set; }

    }
}

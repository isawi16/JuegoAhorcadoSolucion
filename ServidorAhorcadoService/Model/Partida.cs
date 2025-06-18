using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BibliotecaClasesNetFramework.Contratos;
using BibliotecaClasesNetFramework.DTO;

namespace ServidorAhorcadoService.Model
{
    public class Partida
    {
        [Key]
        public int IDPartida { get; set; }

        // Foreign keys
        public int IDJugadorCreador { get; set; }
        public int? IDJugadorRetador { get; set; }
        public int IDEstado { get; set; }
        public int IDPalabra { get; set; }
        public int? Ganador { get; set; }
        public int? IDCancelador { get; set; }

        // Other columns
        public DateTime Fecha { get; set; }
        public int? Puntaje { get; set; }
        [StringLength(200)]
        public string LetrasUsadas { get; set; }

        public int IntentosRestantes { get; set; }

        // Navigation properties
        public virtual Jugador Creador { get; set; }
        public virtual Jugador Retador { get; set; }
        public virtual Jugador GanadorJugador { get; set; }
        public virtual Jugador Cancelador { get; set; }
        public virtual Palabra Palabra { get; set; }
        public virtual Estado Estado { get; set; }
    }
}

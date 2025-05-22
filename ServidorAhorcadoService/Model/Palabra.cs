using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServidorAhorcadoService.Model
{
    public class Palabra
    {
        [Key]
        public int IDPalabra { get; set; }

        [Required]
        public int IDCategoria { get; set; }

        public string LetrasUsadas { get; set; }

        [Required]
        public string PalabraTexto { get; set; }

        [Required]
        [MaxLength(200)]
        public string Definicion { get; set; }

        [Required]
        public string Dificultad { get; set; }

        [ForeignKey("IDCategoria")]
        public virtual Categoria Categoria { get; set; }

        public virtual ICollection<Partida> Partidas { get; set; }


    }
}

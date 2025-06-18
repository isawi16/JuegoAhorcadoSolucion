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
    public class Categoria
    {

        [Key]
        public int IDCategoria { get; set; }

        [Required]
        public int CodigoIdioma { get; set; }

        [Required]
        public string Nombre { get; set; }

        [ForeignKey("CodigoIdioma")]
        public virtual Idioma Idioma { get; set; }

        public virtual ICollection<Palabra> Palabras { get; set; }
    }
}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ServidorAhorcadoService.Model
{
    public class Idioma
    {
        [Key]
        public int CodigoIdioma { get; set; }

        [Required]
        public string Nombre { get; set; }

        public virtual ICollection<Categoria> Categorias { get; set; }
    }
}

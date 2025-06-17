using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BibliotecaClasesNetFramework.Contratos;
using BibliotecaClasesNetFramework.DTO;

namespace ServidorAhorcadoService.Model
{
    public class Estado
    {
        [Key]
        public int IDEstado { get; set; }

        public string Nombre { get; set; }

        public virtual ICollection<Partida> Partidas { get; set; }
    }
}

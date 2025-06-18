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
    public class Jugador
    {
        [Key]
        public int IDJugador { get; set; }

        public string Nombre { get; set; }
        public string Correo { get; set; }
        public string Contraseña { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public string Telefono { get; set; }
        public int PuntajeGlobal { get; set; }
        public byte[] FotoPerfil { get; set; } 

        public virtual ICollection<Partida> PartidasCreadas { get; set; }
        public virtual ICollection<Partida> PartidasRetadas { get; set; }
    }
}


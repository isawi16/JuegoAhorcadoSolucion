using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServidorAhorcadoService.DTO
{
    public class JugadorDTO
    {
        public int IDJugador { get; set; }
        public string Nombre { get; set; }
        public string Correo { get; set; }
        public string Contraseña { get; set; }
        public System.DateTime FechaNacimiento { get; set; }
        public string Telefono { get; set; }
        public int PuntajeGlobal { get; set; }

        public byte[] FotoPerfil { get; set; }
    }
}


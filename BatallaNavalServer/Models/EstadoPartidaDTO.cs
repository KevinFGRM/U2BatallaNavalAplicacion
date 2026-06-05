using System;
using System.Collections.Generic;
using System.Text;

namespace BatallaNavalServer.Models
{
    public class EstadoPartidaDTO
    {
        public string[][] TableroPropio { get; set; } = new string[10][];
        public string[][] TableroEnemigo { get; set; } = new string[10][];
        public string MensajeSuperior { get; set; } = "";
        public string MensajeInferior { get; set; } = "";
        public string IdTurno { get; set; } = "";
        public string? IdGanador { get; set; }
        public int DisparosAcertados { get; set; }
        public int DisparosFallados { get; set; }
        public bool Abandono { get; set; } = false;

    }
}
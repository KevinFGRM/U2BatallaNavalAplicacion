namespace BatallaNavalServer.Models
{
    public class Partida
    {
        public string Id { get; set; } = null!;
        public Jugador? Jugador1 { get; set; }
        public Jugador? Jugador2 { get; set; }
        public string IdTurno { get; set; } = "";
        public string? IdGanador { get; set; }
        public int ContadorDisparos { get; set; } // para que se actualize el tablero en long polling cada vez que se realize un disparo.
        public int DisparosAcertados { get; set; }
        public int DisparosFallados { get; set; }
        public bool Abandono { get; set; } = false;
        public bool Borrar {  get; set; } = false;
    }
}

namespace BatallaNavalServer.Models
{
    public class Jugador
    {
        public string Id { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public bool Listo { get; set; } = false;
        public Tablero Tablero { get; set; } = new Tablero();
    }
}

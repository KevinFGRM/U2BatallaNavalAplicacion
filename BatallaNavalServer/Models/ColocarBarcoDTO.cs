namespace BatallaNavalServer.Models
{
    public class ColocarBarcoDTO
    {
        public string JugadorId { get; set; } = "";
        public string NombreBarco { get; set; } = "";
        public int X { get; set; }
        public int Y { get; set; }
        public bool Horizontal { get; set; }
    }
}

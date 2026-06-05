namespace BatallaNavalServer.Models
{
    public class Barco
    {
        public int Id { get; set; }
        public int Tamaño { get; set; }
        public string Nombre { get; set; } = null!;
        public bool Hundido { get; set; } = false;
        public bool Colocado { get; set; } = false;
    }
}

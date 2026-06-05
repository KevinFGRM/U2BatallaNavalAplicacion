namespace BatallaNavalServer.Models
{
    public class Tablero
    {
        public string?[][] EstadoTablero { get; set; } = new string[10][];
        public List<Barco> Barcos { get; set; } = new List<Barco>()
        {
            new() { Id = 1, Nombre = "Portaaviones", Tamaño = 5 },
            new() { Id = 2, Nombre = "Acorazado", Tamaño = 4 },
            new() { Id = 3, Nombre = "Crucero", Tamaño = 3 },
            new() { Id = 4, Nombre = "Submarino", Tamaño = 3 },
            new() { Id = 5, Nombre = "Destructor", Tamaño = 2 }
        };
        public Tablero()
        {
            EstadoTablero = new string?[10][];

            for (int i = 0; i < 10; i++)
            {
                EstadoTablero[i] = new string?[10];
            }
        }
    }
}

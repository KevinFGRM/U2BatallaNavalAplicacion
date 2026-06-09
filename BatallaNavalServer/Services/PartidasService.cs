using BatallaNavalServer.Models;
using System.Drawing;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BatallaNavalServer.Services
{
    public class PartidasService
    {
        public List<Partida> Partidas { get; set; } = new List<Partida>();

        public Partida? BuscarPartida(string id, string? nombreJugador)
        {
            var existe = Partidas.FirstOrDefault(x => x.Jugador1?.Id == id || x.Jugador2?.Id == id);
            if (existe != null)
                return existe;

            if (string.IsNullOrEmpty(nombreJugador))
                return null;
            var partidaSinOcupar = Partidas.Find(s => s.Jugador2 == null);
            if (partidaSinOcupar == null)
            {
                Partida p = new Partida
                {
                    Jugador1 = new()
                    {
                        Nombre = nombreJugador ?? "",
                        Id = id,
                        Tablero = new()
                    }
                };

                Partidas.Add(p);
                return p;
            }
            else
            {
                partidaSinOcupar.Jugador2 = new()
                {
                    Id = id,
                    Nombre = nombreJugador,
                    Tablero = new()
                };
                return partidaSinOcupar;
            }
        }

        public bool RealizarDisparo(Partida partida, DisparoDTO disparo)
        {
            if (partida == null)
                return false;

            var jugadorActual = partida.Jugador1?.Id == disparo.JugadorId ? partida.Jugador1 : partida.Jugador2;
            var jugadorEnemigo = partida.Jugador1?.Id == disparo.JugadorId ? partida.Jugador2 : partida.Jugador1;

            if (jugadorActual == null || jugadorEnemigo == null)
                return false;

            var validarEstado = jugadorEnemigo.Tablero.EstadoTablero[disparo.Y][disparo.X];

            if (validarEstado == null)
            {
                partida.DisparosFallados++;
                partida.ContadorDisparos++;
                jugadorEnemigo.Tablero.EstadoTablero[disparo.Y][disparo.X] = "O";
                partida.IdTurno = jugadorEnemigo.Id;
                return true;
            }

            if (validarEstado != "O" && validarEstado != "X")
            {
                partida.DisparosAcertados++;
                partida.ContadorDisparos++;
                int idBarco = int.Parse(validarEstado[0].ToString());
                jugadorEnemigo.Tablero.EstadoTablero[disparo.Y][disparo.X] = "X";

                bool barcoFlotando = false;
                foreach (var fila in jugadorEnemigo.Tablero.EstadoTablero)
                {
                    foreach (var posicion in fila)
                    {
                        if (posicion != null && posicion != "O" && posicion != "X")
                        {
                            if (int.Parse(posicion[0].ToString()) == idBarco)
                            {
                                barcoFlotando = true;
                                break;
                            }
                        }
                    }
                    if (barcoFlotando)
                        break;
                }

                if (!barcoFlotando)
                {
                    var barcoHundido = jugadorEnemigo.Tablero.Barcos.FirstOrDefault(b => b.Id == idBarco);
                    if (barcoHundido != null)
                    {
                        barcoHundido.Hundido = true;
                    }
                }

            }
            else if (validarEstado == "O" || validarEstado == "X")
            {
                return false;
            }

            if (jugadorEnemigo.Tablero.Barcos.All(b => b.Hundido))
            {
                partida.IdGanador = jugadorActual.Id;
                partida.Borrar = true;
                return true;
            }

            return true;
        }

        public bool PosicionarBarco(Partida partida, ColocarBarcoDTO posicionamiento)
        {
            var jugadorActual = partida.Jugador1?.Id == posicionamiento.JugadorId ? partida.Jugador1 : partida.Jugador2;
            var jugadorEnemigo = partida.Jugador1?.Id == posicionamiento.JugadorId ? partida.Jugador2 : partida.Jugador1;

            if (jugadorActual == null || jugadorEnemigo == null)
                return false;

            int tamaño = 0;
            switch (posicionamiento.NombreBarco.ToLower())
            {
                case "portaaviones": tamaño = 5; break;
                case "acorazado": tamaño = 4; break;
                case "crucero": tamaño = 3; break;
                case "submarino": tamaño = 3; break;
                case "destructor": tamaño = 2; break;
            }

            if (tamaño == 0)
                return false;

            int inicioX = posicionamiento.X;
            int inicioY = posicionamiento.Y;

            if (posicionamiento.Horizontal)
            {
                if (inicioX < 0 || inicioY < 0 || inicioX + tamaño > 10 || inicioY >= 10)
                    return false;
            }
            else
            {
                if (inicioX < 0 || inicioY < 0 || inicioX >= 10 || inicioY + tamaño > 10)
                    return false;
            }

            int idBarco = jugadorActual.Tablero.Barcos.FirstOrDefault(c => c.Nombre.ToLower() == posicionamiento.NombreBarco.ToLower())?.Id ?? 1;

            if (idBarco == 0)
                return false;

            string orientacion = posicionamiento.Horizontal ? "h" : "v";

            List<Point> posicionesAnteriores = new();

            for (int y = 0; y < 10; y++)
            {
                for (int x = 0; x < 10; x++)
                {
                    string? celda = jugadorActual.Tablero.EstadoTablero[y][x];
                    if (!string.IsNullOrEmpty(celda) && celda.Length > 0 && int.TryParse(celda[0].ToString(), out int id))
                    {
                        if (id == idBarco)
                        {
                            posicionesAnteriores.Add(new Point(x, y));
                        }
                    }
                }
            }

            foreach (var point in posicionesAnteriores)
            {
                jugadorActual.Tablero.EstadoTablero[point.Y][point.X] = null;
            }

            // validar que las nuevas posiciones no colisionen con otros barcos
            for (int i = 0; i < tamaño; i++)
            {
                int actualX = posicionamiento.Horizontal ? inicioX + i : inicioX;
                int actualY = posicionamiento.Horizontal ? inicioY : inicioY + i;

                string? celda = jugadorActual.Tablero.EstadoTablero[actualY][actualX];

                if (!string.IsNullOrEmpty(celda))
                {
                    // restaurar las posiciones anteriores
                    foreach (var point in posicionesAnteriores)
                    {
                        int segmentoOriginal = point.X - inicioX;
                        if (!posicionamiento.Horizontal)
                            segmentoOriginal = point.Y - inicioY;
                        jugadorActual.Tablero.EstadoTablero[point.Y][point.X] = $"{idBarco}{orientacion}{segmentoOriginal}";
                    }
                    return false;
                }
            }

            for (int i = 0; i < tamaño; i++)
            {
                int actualX = posicionamiento.Horizontal ? inicioX + i : inicioX;
                int actualY = posicionamiento.Horizontal ? inicioY : inicioY + i;

                string valorBarco = $"{idBarco}{orientacion}{i}";
                jugadorActual.Tablero.EstadoTablero[actualY][actualX] = valorBarco;
            }

            var barcoColocado = jugadorActual.Tablero.Barcos.FirstOrDefault(b => b.Id == idBarco);
            if (barcoColocado != null)
            {
                barcoColocado.Colocado = true;
            }
            return true;
        }

        public bool MarcarListo(Partida partida, string jugadorId)
        {
            var jugador = partida.Jugador1?.Id == jugadorId ? partida.Jugador1 : partida.Jugador2;

            if (jugador == null)
                return false;

            jugador.Listo = true;
            partida.IdTurno = jugador.Id;
            return true;
        }

        public bool ValidarBarcos(Partida partida, string jugadorId)
        {
            var jugador = partida.Jugador1?.Id == jugadorId ? partida.Jugador1 : partida.Jugador2;

            if (jugador == null)
                return false;

            if(!jugador.Tablero.Barcos.All(b => b.Colocado)) // validar que el jugador haya colocado todos los barcos
                return false;

            return true;
        }
        public bool MarcarVictoriaPorAbandono(Partida partida, string jugadorId)
        {
            if (partida == null)
                return false;

            partida.IdGanador = jugadorId;
            partida.Abandono = true;
            partida.Borrar = true;
            return true;
        }


        //public void LimpiarPartidas()
        //{
        //    Partidas.RemoveAll(x => x.Borrar);
        //}
    }
}

using BatallaNavalServer.Models;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;

namespace BatallaNavalServer.Services
{
    public class JuegoService
    {
        private HttpListener server;
        private bool activo;
        private PartidasService partidasService;
        public List<string> Logs { get; private set; } = new List<string>();

        public JuegoService(PartidasService partidasService)
        {
            this.partidasService = partidasService;
            server = new HttpListener();
            string url = "http://+:8080/batallanaval/";
            server.Prefixes.Add(url);
        }

        public void Iniciar()
        {
            server.Start();
            activo = true;

            Thread hiloPrincipal = new Thread(EscucharPeticiones)
            {
                IsBackground = true
            };
            hiloPrincipal.Start();

        }

        private void EscucharPeticiones(object? obj)
        {
            while (activo)
            {
                try
                {
                    var context = server.GetContext();

                    Thread hiloPeticion = new Thread(() => ProcesarPeticion(context))
                    {
                        IsBackground = true,
                    };
                    hiloPeticion.Start();
                }
                catch (HttpListenerException ex)
                {
                    Logs.Add($"Error: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Logs.Add($"Error inesperado: {ex.Message}");
                }
            }
        }

        private void ProcesarPeticion(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;

            try
            {
                if (request.HttpMethod == "GET" && request.RawUrl == "/batallanaval/")
                {
                    ServirArchivo(response, "index.html", "text/html");
                }
                else if (request.HttpMethod == "GET" && request.Url?.AbsolutePath.StartsWith("/batallanaval/img/") == true)
                {
                    string archivo = request.Url.AbsolutePath.Replace("/batallanaval/", "");

                    ServirArchivo(response, archivo, "image/png");
                }
                else if (request.HttpMethod == "GET" && request.Url?.AbsolutePath.StartsWith("/batallanaval/sounds/") == true)
                {
                    string archivo = request.Url.AbsolutePath.Replace("/batallanaval/", "");

                    ServirArchivo(response, archivo, "audio/mpeg");
                }
                else if (request.HttpMethod == "GET" && request.Url?.AbsolutePath == "/batallanaval/style.css")
                {
                    ServirArchivo(response, "style.css", "text/css");
                }


                else if (request.HttpMethod == "POST" && request.RawUrl == "/batallanaval/disparar")
                {
                    string json = GetJson(request);
                    var disparo = JsonSerializer.Deserialize<DisparoDTO>(json);
                    if (disparo != null)
                    {
                        Partida? partida = partidasService.BuscarPartida(disparo.JugadorId, null);
                        if (partida == null)
                        {
                            response.StatusCode = 404;
                        }
                        else
                        {
                            if (partida.IdTurno == disparo.JugadorId)
                            {
                                var res = partidasService.RealizarDisparo(partida, disparo);
                                if (res)
                                {
                                    RegresarTablero(response, partida, disparo.JugadorId);
                                }
                                else
                                {
                                    response.StatusCode = 400;
                                }
                            }
                        }

                    }
                    else
                    {
                        response.StatusCode = 400;
                    }
                }
                else if (request.HttpMethod == "POST" && request.RawUrl == "/batallanaval/posicionarbarco")
                {
                    var json = GetJson(request);
                    var posicionamiento = JsonSerializer.Deserialize<ColocarBarcoDTO>(json);
                    if (posicionamiento != null)
                    {
                        Partida? partida = partidasService.BuscarPartida(posicionamiento.JugadorId, null);
                        if (partida == null)
                        {
                            response.StatusCode = 404;
                        }
                        else
                        {
                            var res = partidasService.PosicionarBarco(partida, posicionamiento);
                            if (res)
                            {
                                RegresarTablero(response, partida, posicionamiento.JugadorId);
                            }
                            else
                            {
                                response.StatusCode = 400;
                            }
                        }
                    }
                    else
                    {
                        response.StatusCode = 400;
                    }
                }
                else if (request.HttpMethod == "POST" && request.RawUrl == "/batallanaval/listo")
                {
                    string json = GetJson(request);

                    var dto = JsonSerializer.Deserialize<ListoDTO>(json);

                    if (dto == null)
                    {
                        response.StatusCode = 400;
                    }
                    else
                    {
                        var partida = partidasService.BuscarPartida(dto.JugadorId, null);

                        if (partida == null)
                        {
                            response.StatusCode = 404;
                        }
                        else
                        {
                            var resultado = partidasService.MarcarListo(partida, dto.JugadorId);

                            if (!resultado)
                                response.StatusCode = 400;
                            else
                            {
                                while (partida.Jugador1 != null && partida.Jugador2 != null &&
                                    (!partida.Jugador1.Listo || !partida.Jugador2.Listo)
                                )
                                {
                                    Thread.Sleep(500);
                                }

                                RegresarTablero(response, partida, dto.JugadorId);
                            }
                        }
                    }
                }
                else if (request.HttpMethod == "GET" && request.Url?.AbsolutePath == "/batallanaval/barcosColocados")
                {
                    var id = request.QueryString["JugadorId"];

                    if (id == null)
                    {
                        response.StatusCode = 400;
                    }
                    else
                    {
                        var partida = partidasService.BuscarPartida(id, null);

                        if (partida == null)
                        {
                            response.StatusCode = 404;
                        }
                        else
                        {
                            var resultado = partidasService.ValidarBarcos(partida, id);

                            if (!resultado)
                                response.StatusCode = 400;
                            else
                            {
                                RegresarTablero(response, partida, id);
                            }
                        }
                    }
                }
                else if (request.HttpMethod == "GET" && request.Url?.AbsolutePath == "/batallanaval/esperarTurno")
                {
                    var id = request.QueryString["JugadorId"];

                    if (string.IsNullOrEmpty(id))
                    {
                        response.StatusCode = 400;
                    }
                    else
                    {
                        var partida = partidasService.BuscarPartida(id, null);
                        if (partida == null)
                        {
                            response.StatusCode = 404;
                        }
                        else
                        {
                            var actualizar = partida.ContadorDisparos;
                            while (partida.IdTurno != id)
                            {
                                var partida2 = partidasService.BuscarPartida(id, null);
                                if (actualizar != partida2.ContadorDisparos)
                                {
                                    actualizar = partida2.ContadorDisparos;
                                    RegresarTablero(response, partida, id);
                                }

                                Thread.Sleep(500);
                            }
                            RegresarTablero(response, partida, id);
                        }
                    }
                }
                else if (request.HttpMethod == "POST" && request.RawUrl == "/batallanaval/registrar")
                {
                    string json = GetJson(request);

                    var usuario = JsonSerializer.Deserialize<RegistrarDTO>(json);

                    if (usuario == null)
                    {
                        response.StatusCode = 400;
                    }
                    else
                    {
                        var partida = partidasService.BuscarPartida(usuario.Id, usuario.Nombre);

                        if (partida.Jugador2 != null)
                        {
                            RegresarTablero(response, partida, usuario.Id);
                        }
                        else //long polling
                        {
                            while (partida.Jugador2 == null)
                            {
                                Thread.Sleep(500);
                            }
                            RegresarTablero(response, partida, usuario.Id);
                        }
                    }


                }
                else if (request.HttpMethod == "POST" && request.RawUrl == "/batallanaval/victoriaPorAbandono")
                {
                    string json = GetJson(request);
                    var abandono = JsonSerializer.Deserialize<AbandonoDTO>(json);

                    if (abandono == null)
                    {
                        response.StatusCode = 400;
                    }
                    else
                    {
                        var partida = partidasService.BuscarPartida(abandono.JugadorId, null);
                        if (partida == null)
                        {
                            response.StatusCode = 404;
                        }
                        else
                        {
                            var resultado = partidasService.MarcarVictoriaPorAbandono(partida, abandono.JugadorId);
                            if (resultado)
                            {
                                RegresarTablero(response, partida, abandono.JugadorId);
                            }
                            else
                            {
                                response.StatusCode = 400;
                            }
                        }
                    }
                }
                else
                {
                    response.StatusCode = 404;
                }
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
            }
            finally
            {
                response.Close();
            }
        }

        private static string GetJson(HttpListenerRequest request)
        {
            var buffer = new byte[request.ContentLength64];
            request.InputStream.ReadExactly(buffer, 0, buffer.Length);
            var json = Encoding.UTF8.GetString(buffer);
            return json;
        }

        private void RegresarTablero(HttpListenerResponse response, Partida partida, string idJugador)
        {
            EstadoPartidaDTO tablero = new EstadoPartidaDTO
            {
                TableroPropio = partida.Jugador1.Id == idJugador ? partida.Jugador1.Tablero.EstadoTablero : partida.Jugador2.Tablero.EstadoTablero,
                TableroEnemigo = partida.Jugador1.Id == idJugador ? partida.Jugador2.Tablero.EstadoTablero : partida.Jugador1.Tablero.EstadoTablero,

                IdTurno = partida.IdTurno,
                IdGanador = partida.IdGanador,
                DisparosAcertados = partida.DisparosAcertados,
                DisparosFallados = partida.DisparosFallados,
                Abandono = partida.Abandono
            };

            //if  si termino juego.

            //
            var json = JsonSerializer.Serialize(tablero);
            byte[] buffer = Encoding.UTF8.GetBytes(json);
            response.ContentLength64 = buffer.Length;
            response.ContentType = "application/json";
            response.OutputStream.Write(buffer, 0, buffer.Length);
        }

        private void ServirArchivo(HttpListenerResponse response, string nombreArchivo, string contentType)
        {
            string ruta = Path.Combine("Assets", nombreArchivo);

            if (File.Exists(ruta))
            {
                byte[] buffer = File.ReadAllBytes(ruta);

                response.ContentLength64 = buffer.Length;
                response.ContentType = contentType;
                response.OutputStream.Write(buffer, 0, buffer.Length);
                response.StatusCode = 200;
            }
            else
            {
                response.StatusCode = 404;
            }
        }
        public void Detener()
        {
            activo = false;
            server.Stop();
        }
    }
}

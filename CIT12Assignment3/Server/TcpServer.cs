using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace CIT12Assignment3
{
    public class TcpServer
    {
        private readonly int _port;
        private TcpListener? _listener;
        private bool _running;

        public TcpServer(int port)
        {
            _port = port;
        }
        public void Start()
        {
            _listener = new TcpListener(IPAddress.Any, _port);
            _listener.Start();
            _running = true;
            Console.WriteLine($"Server started on port {_port}...");

            while (_running)
            {
                TcpClient client = _listener.AcceptTcpClient();
                Console.WriteLine("Client connected.");
                Thread t = new Thread(() => HandleClient(client));
                t.Start();
            }

        }

        private void HandleClient(TcpClient client)
        {
            using NetworkStream stream = client.GetStream();

            var crud = new CrudService();
            var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

            try
            {
                // Read request bytes until no more data is available
                stream.ReadTimeout = 750;
                byte[] buffer = new byte[2048];
                using var mem = new MemoryStream();
                int read;
                do
                {
                    read = stream.Read(buffer, 0, buffer.Length);
                    if (read > 0) mem.Write(buffer, 0, read);
                } while (read == buffer.Length);

                var requestJson = Encoding.UTF8.GetString(mem.ToArray()).Trim();
                var request = JsonSerializer.Deserialize<Request>(requestJson, jsonOptions);
                if (request == null)
                {
                    var errorBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new Response { Status = "4 Bad Request" }, jsonOptions));
                    stream.Write(errorBytes, 0, errorBytes.Length);
                    return;
                }

                // Ensure echo body is captured as plain string exactly as sent
                if (request.Method?.ToLower() == "echo")
                {
                    try
                    {
                        using var doc = JsonDocument.Parse(requestJson);
                        if (doc.RootElement.TryGetProperty("body", out var be) && be.ValueKind == JsonValueKind.String)
                        {
                            request.Body = be.GetString();
                        }
                    }
                    catch { /* ignore and let validator handle */ }
                }

                // Convert UrlParser.Id now string -> int for CategoryService API
                if (int.TryParse(_urlParserIdSafe(crud), out _)) { }
                var response = crud.HandleRequest(request);
                var responseJson = JsonSerializer.Serialize(response, jsonOptions);
                var responseBytes = Encoding.UTF8.GetBytes(responseJson);
                stream.Write(responseBytes, 0, responseBytes.Length);
                stream.Flush();
            }
            catch (IOException)
            {
                // Client closed connection (e.g., test finishes quickly)
                Console.WriteLine("Client disconnected unexpectedly.");
            }
            catch (SocketException)
            {
                // Broken pipe or reset by peer — safe to ignore
                Console.WriteLine("Client socket error (likely closed by peer).");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
                try
                {
                    var errorBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new Response { Status = "4 Bad Request" }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
                    stream.Write(errorBytes, 0, errorBytes.Length);
                }
                catch
                {
                    // If the stream is already closed, just swallow
                }
            }
        }

        // Helper to avoid warnings when converting Id types inside CrudService
        private string _urlParserIdSafe(CrudService _) => "0";

    }
}

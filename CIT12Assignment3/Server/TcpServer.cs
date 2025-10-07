using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;

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
                // are new threads necessary to handle multiple connections?
                // it might overcomplicate the print statements (some are invisible)
                TcpClient client = _listener.AcceptTcpClient();
                Console.WriteLine("Client connected.");
                Thread t = new Thread(() => HandleClient(client));
                t.Start();
            }

        }

        private void HandleClient(TcpClient client)
        {
            // using = dispose of it at end of code block
            // get client stream 
            using NetworkStream stream = client.GetStream();
            // bytes -> strings
            using var reader = new StreamReader(stream, Encoding.UTF8);
            // strings -> bytes | AutoFlush sends responses immediately without buffering
            using var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

            var categoryService = new CategoryService();
            var validator = new RequestValidator();
            var parser = new UrlParser();

            while (client.Connected)
            {
                string? json = reader.ReadLine();
                if (json == null) break;

                try
                {
                    // deserialize JSON string
                    var request = JsonSerializer.Deserialize<Request>(json);
                    if (request == null)
                        throw new Exception("Bad JSON.");

                    // parse JSON into Request obj
                    var validationResult = validator.ValidateRequest(request);

                    
                    // if (!validationResult.IsValid) // should be if errors.Any [...]
                    // SendResponse(writer, 4, "Bad Request", null);
                    // continue;

                    // HandleRequest(request, writer, categoryService, parser);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error: " + e.Message);
                    // SendResponse(writer, response);
                }
            }
        }

        private void HandleRequest()
        {
            // Chris =>
        }

        private void SendResponse(StreamWriter writer, Response response)
        {
            // Chris =>
            // var res = new Response
            // {
            //     Status = status, // 1 or 4
            //     Message = message // Ok or Bad Request
            //                       // TODO: add body with errors
            // };

            // string json = JsonSerializer.Serialize(res);
            // writer.WriteLine(json);
        }
    }


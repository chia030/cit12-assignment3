using System;

namespace CIT12Assignment3
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== CIT Assignment 3 ===");
            Console.WriteLine("Network Service using CJTP Protocol");
            Console.WriteLine("=====================================");
            Console.WriteLine("Starting TCP server on port 5000 ...");

            var server = new TcpServer(5000);
            server.Start();
        }
    }
}

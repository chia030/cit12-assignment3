using System;

namespace CIT12Assignment3
{
    class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("=== CIT Assignment 3 ===");
            Console.WriteLine("Network Service using CJTP Protocol");
            Console.WriteLine("=====================================");
            var server = new TcpServer(5000);
            server.Start();
        }
    }
}

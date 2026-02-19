using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EasySave.LogServer
{
    class Program
    {
        private const int Port = 9000;
        private static readonly string LogDir = "/app/logs";

        static async Task Main(string[] args)
        {
            if (!Directory.Exists(LogDir)) Directory.CreateDirectory(LogDir);

            TcpListener listener = new TcpListener(IPAddress.Any, Port);
            listener.Start();
            Console.WriteLine($"[Serveur] En écoute sur le port {Port}...");

            while (true)
            {
                var client = await listener.AcceptTcpClientAsync();
                _ = HandleClientAsync(client);
            }
        }

        private static async Task HandleClientAsync(TcpClient client)
        {
            try
            {
                using (client)
                using (NetworkStream stream = client.GetStream())
                {
                    byte[] buffer = new byte[8192];
                    int bytesRead;

                    while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                    {
                        string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                        string extension = ".log";
                        if (message.StartsWith("XML"))
                        {
                            extension = ".xml";
                            message = message.Substring(3);
                        }
                        else if (message.StartsWith("JSON"))
                        {
                            extension = ".json";
                            message = message.Substring(4);
                        }

                        string fileName = Path.Combine(LogDir, $"{DateTime.Now:yyyy-MM-dd}{extension}");

                        Console.WriteLine($"[Reçu -> {fileName}]");

                        await File.AppendAllTextAsync(fileName, message + Environment.NewLine);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Erreur] {ex.Message}");
            }
        }
    }
}
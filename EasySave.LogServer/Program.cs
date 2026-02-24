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

        /// <summary>
        /// Entry point for the centralized log server.
        /// Starts an asynchronous TCP listener to handle multiple incoming log streams.
        /// </summary>
        static async Task Main(string[] args)
        {
            if (!Directory.Exists(LogDir)) Directory.CreateDirectory(LogDir);

            TcpListener listener = new TcpListener(IPAddress.Any, Port);
            listener.Start();
            Console.WriteLine($"[Server] Listening on port {Port}...");

            while (true)
            {
                var client = await listener.AcceptTcpClientAsync();
                // Handle each connection in a non-blocking background task
                _ = HandleClientAsync(client);
            }
        }

        /// <summary>
        /// Processes the data stream from a specific client connection.
        /// Protocol details:
        /// - Expects a prefix ("XML" or "JSON") to determine the file extension.
        /// - Remaining payload is the serialized log entry.
        /// - Logs are appended to a daily file on the server.
        /// </summary>
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

                        // Basic protocol parsing based on string prefixes
                        string extension = ".log";
                        if (message.StartsWith("XML"))
                        {
                            extension = ".xml";
                            message = message.Substring(3); // Remove the 'XML' prefix
                        }
                        else if (message.StartsWith("JSON"))
                        {
                            extension = ".json";
                            message = message.Substring(4); // Remove the 'JSON' prefix
                        }

                        string fileName = Path.Combine(LogDir, $"{DateTime.Now:yyyy-MM-dd}{extension}");
                        Console.WriteLine($"[Received -> {fileName}]");

                        // Append the log payload to the daily file
                        await File.AppendAllTextAsync(fileName, message + Environment.NewLine);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] {ex.Message}");
            }
        }
    }
}
using EasySave.Log.Interfaces;
using EasySave.Log.Models;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Xml;
using System.Xml.Serialization;

namespace EasySave.Log.Loggers
{
    public class NetworkLogger : ILogger
    {
        private readonly string _serverIp;
        private readonly int _port = 9000;
        private readonly string _format;

        public NetworkLogger(string format, string serverIp)
        {
            _format = format.ToLower();
            _serverIp = string.IsNullOrWhiteSpace(serverIp) ? "127.0.0.1" : serverIp;
        }

        public void WriteLog(LogEntry logEntry)
        {
            try
            {
                string payload = "";

                if (_format.Contains("xml"))
                {
                    var emptyNs = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });
                    var serializer = new XmlSerializer(typeof(LogEntry));
                    var settings = new XmlWriterSettings { OmitXmlDeclaration = true, Indent = true };

                    using (var stringWriter = new StringWriter())
                    using (var xmlWriter = XmlWriter.Create(stringWriter, settings))
                    {
                        serializer.Serialize(xmlWriter, logEntry, emptyNs);
                        payload = "XML" + stringWriter.ToString();
                    }
                }
                else
                {
                    var options = new JsonSerializerOptions { WriteIndented = true };
                    payload = "JSON" + JsonSerializer.Serialize(logEntry, options);
                }

                using (TcpClient client = new TcpClient())
                {
                    client.Connect(_serverIp, _port);
                    using (NetworkStream stream = client.GetStream())
                    {
                        byte[] data = Encoding.UTF8.GetBytes(payload);
                        stream.Write(data, 0, data.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[NetworkLogger Error] Impossible d'envoyer le log : {ex.Message}");
            }
        }
    }
}
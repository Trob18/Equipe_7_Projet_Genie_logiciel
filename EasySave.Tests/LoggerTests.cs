using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using EasySave.Log;
using EasySave.Log.Models;

namespace EasySave.Tests
{
    [TestClass]
    public class LoggerTests
    {
        private string _tempLogDirectory;

        [TestInitialize]
        public void Setup()
        {
            string uniqueId = System.Guid.NewGuid().ToString();
            _tempLogDirectory = Path.Combine(Path.GetTempPath(), $"EasySaveTests_{uniqueId}");

            Directory.CreateDirectory(_tempLogDirectory);
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(_tempLogDirectory))
            {
                Directory.Delete(_tempLogDirectory, true);
            }
        }

        [TestMethod]
        public void WriteLog_JsonFormat_ShouldCreateValidJsonFile()
        {
            var logger = LoggerCrea.CreateLogger("json", _tempLogDirectory, "");
            var fakeLog = new LogEntry
            {
                Name = "JobJsonTest",
                SourceFile = @"C:\source.txt",
                TargetFile = @"D:\cible.txt",
                FileSize = 1024,
                TransferTime = 15,
                EncryptionTime = 0
            };

            logger.WriteLog(fakeLog);

            var files = Directory.GetFiles(_tempLogDirectory, "*.json");
            Assert.IsTrue(files.Length > 0, "Aucun fichier JSON n'a été créé !");

            string fileContent = File.ReadAllText(files[0]);
            Assert.IsTrue(fileContent.Contains("JobJsonTest"), "Le fichier ne contient pas les bonnes données.");
            Assert.IsTrue(fileContent.Contains("{") && fileContent.Contains("}"), "Le format n'est pas du JSON valide.");
        }

        [TestMethod]
        public void WriteLog_XmlFormat_ShouldCreateValidXmlFile()
        {
            var logger = LoggerCrea.CreateLogger("xml", _tempLogDirectory, "");
            var fakeLog = new LogEntry
            {
                Name = "JobXmlTest",
                SourceFile = @"C:\source.txt",
                TargetFile = @"D:\cible.txt",
                FileSize = 2048,
                TransferTime = 30,
                EncryptionTime = 5
            };

            logger.WriteLog(fakeLog);

            var files = Directory.GetFiles(_tempLogDirectory, "*.xml");
            Assert.IsTrue(files.Length > 0, "Aucun fichier XML n'a été créé !");

            string fileContent = File.ReadAllText(files[0]);
            Assert.IsTrue(fileContent.Contains("JobXmlTest"), "Le fichier ne contient pas les bonnes données.");
            Assert.IsTrue(fileContent.Contains("<") && fileContent.Contains(">"), "Le format n'est pas du XML valide.");
        }
    }
}
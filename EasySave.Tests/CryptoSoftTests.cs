using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Diagnostics;

namespace EasySave.Tests
{
    [TestClass]
    [DoNotParallelize]
    public class CryptoSoftTests
    {
        private string _tempDirectory;
        private string _fileToEncrypt;

        [TestInitialize]
        public void Setup()
        {
            _tempDirectory = Path.Combine(Path.GetTempPath(), $"CryptoTest_{Guid.NewGuid()}");
            Directory.CreateDirectory(_tempDirectory);

            _fileToEncrypt = Path.Combine(_tempDirectory, "secret.txt");

            File.WriteAllText(_fileToEncrypt, "Ceci est un message ultra secret pour le jury !");
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(_tempDirectory))
            {
                Directory.Delete(_tempDirectory, true);
            }
        }

        [TestMethod]
        public void EncryptFile_ShouldModifyFileContent()
        {
            string originalContent = File.ReadAllText(_fileToEncrypt);

            string cryptoSoftPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "..", "..", "..", "..",
                "CryptoSoft", "bin", "Debug", "net8.0", "win-x64", "CryptoSoft.exe"
            );

            if (!File.Exists(cryptoSoftPath))
            {
                cryptoSoftPath = Path.GetFullPath(cryptoSoftPath);
            }

            Assert.IsTrue(File.Exists(cryptoSoftPath), $"Impossible de trouver CryptoSoft.exe au chemin : {cryptoSoftPath}");

            string encryptionKey = "EasySaveEncryptionKey";

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = cryptoSoftPath,
                Arguments = $"\"{_fileToEncrypt}\" \"{encryptionKey}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(startInfo))
            {
                process.WaitForExit();
                Assert.IsTrue(process.ExitCode >= 0, "CryptoSoft a retourné un code d'erreur !");
            }
            string encryptedContent = File.ReadAllText(_fileToEncrypt);

            Assert.AreNotEqual(originalContent, encryptedContent, "Le fichier a le même contenu, le chiffrement a échoué !");

            Assert.IsTrue(encryptedContent.Length > 0, "Le fichier chiffré est vide !");
        }



        [TestMethod]
        public void CryptoSoft_MonoInstance_ShouldReturnBusyCodeIfAlreadyRunning()
        {
            string cryptoSoftPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "..", "..", "..", "..",
                "CryptoSoft", "bin", "Debug", "net8.0", "win-x64", "CryptoSoft.exe"
            );
            if (!File.Exists(cryptoSoftPath)) cryptoSoftPath = Path.GetFullPath(cryptoSoftPath);

            Assert.IsTrue(File.Exists(cryptoSoftPath), $"Impossible de trouver CryptoSoft.exe au chemin : {cryptoSoftPath}");

            const string mutexName = @"Global\CryptoSoft_MonoInstance";
            bool createdNew;

            using (var simulateRunningCrypto = new System.Threading.Mutex(true, mutexName, out createdNew))
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = cryptoSoftPath,
                    Arguments = $"\"fichier_fantome.txt\" \"CleTest\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = Process.Start(startInfo))
                {
                    process.WaitForExit();
                    Assert.AreEqual(-20, process.ExitCode, "Le second processus aurait dû se fermer avec le code d'erreur ERR_BUSY (-20) !");
                }
            }
        }
    }
}
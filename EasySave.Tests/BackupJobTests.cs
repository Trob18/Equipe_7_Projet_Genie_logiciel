using Microsoft.VisualStudio.TestTools.UnitTesting;
using EasySave.WPF.Models;
using EasySave.WPF.Enumerations;

namespace EasySave.Tests
{
    [TestClass]
    public class BackupJobTests
    {
        [TestMethod]
        public void BackupJob_Constructor_ShouldInitializePropertiesCorrectly()
        {
            string expectedName = "Sauvegarde Annuelle";
            string expectedSource = @"C:\DossierSource";
            string expectedTarget = @"D:\DossierCible";
            BackupType expectedType = BackupType.Differential;

            var job = new BackupJob(expectedName, expectedSource, expectedTarget, expectedType);

            Assert.AreEqual(expectedName, job.Name);
            Assert.AreEqual(expectedSource, job.SourceDirectory);
            Assert.AreEqual(expectedTarget, job.TargetDirectory);
            Assert.AreEqual(expectedType, job.Type);

            Assert.AreEqual(BackupState.Inactive, job.State, "Le statut par défaut doit être Inactive");
            Assert.AreEqual(0, job.Progress, "La progression par défaut doit être 0");
            Assert.AreEqual("", job.RemainingTimeText, "Le temps restant doit être vide à la création");
        }

        [TestMethod]
        public void ShortSourceDirectory_WithLongPath_ShouldReturnTruncatedPath()
        {
            var job = new BackupJob();
            string longPath = @"C:\Users\Admin\Documents\Projets\EasySave\TestFolder";
            string expectedShortPath = @"...\Projets\EasySave\TestFolder";

            job.SourceDirectory = longPath;
            string actualShortPath = job.ShortSourceDirectory;

            Assert.AreEqual(expectedShortPath, actualShortPath);
        }

        [TestMethod]
        public void ShortSourceDirectory_WithShortPath_ShouldReturnSamePath()
        {
            var job = new BackupJob();
            string shortPath = @"C:\TestFolder";
            string expectedShortPath = @"C:\TestFolder";

            job.SourceDirectory = shortPath;
            string actualShortPath = job.ShortSourceDirectory;

            Assert.AreEqual(expectedShortPath, actualShortPath, "Un chemin court ne doit pas être tronqué");
        }
    }
}
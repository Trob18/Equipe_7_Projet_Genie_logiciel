using System.Collections.Generic;
using EasySave.WPF.Enumerations;

namespace EasySave.WPF.Config
{
    public static class ResourceSettings
    {
        private static readonly Dictionary<string, string> _fr = new Dictionary<string, string>
        {
            { "JobName", "Nom du travail" },
            { "TabHome", "Accueil" },
            { "TabSettings", "Configuration" },
            { "TitleSettings", "Paramètres Généraux" },
            { "State", "État" },
            { "SourcePath", "Dossier Source" },
            { "TargetPath", "Dossier Cible" },
            { "Type", "Type" },
            { "Encrypt", "Encrypt" },
            { "Create", "Créer" },
            { "Delete", "Supprimer" },
            { "Execute", "Exécuter" },
            { "StatusReady", "Prêt." },
            { "LogFormat", "Format des Logs :" },
            { "LangInterface", "Langue de l'interface :" },
            { "EncryptExtensions", "Extensions à encrypter :" },
            { "EncryptAll", "Encrypter tous les fichiers" },
            { "AddExtension", "Ajouter" },
            { "BlockedProcesses", "Processus Métiers :" },
            { "AddProcess", "Ajouter Processus" },
            { "General", "Général" },
            { "Encryption", "Chiffrement" },
            { "LogFormatDesc", "Format des fichiers de log" },
            { "LangInterfaceDesc", "Langue de l'application" },
            { "EncryptAllDesc", "Chiffrer tous les fichiers" },
            { "BlockedProcessesDesc", "La sauvegarde se mettra en pause si ces processus sont détectés" },
            { "Success", "Succès" },
            { "Error", "Erreur" },
            { "EmptyFields", "Champs vides !" },
            { "JobCreated", "créé." },
            { "JobDeleted", "Travail supprimé." },
            { "ExecutingJobs", "Exécution de {0} travaux..." },
            { "JobFinished", "terminé !" },
            { "Status", "Statut :" },
            { "ProcessBlockedMessage", "Le processus '{0}' est en cours d'exécution et doit être fermé pour continuer la sauvegarde." },
            { "RestartWarning", "Le changement de langue nécessitera un redémarrage pour être complet." }
        };

        private static readonly Dictionary<string, string> _en = new Dictionary<string, string>
        {
            { "JobName", "Job Name" },
            { "TabHome", "Home" },
            { "TabSettings", "Settings" },
            { "TitleSettings", "General Settings" },
            { "State", "State" },
            { "SourcePath", "Source Folder" },
            { "TargetPath", "Target Folder" },
            { "Type", "Type" },
            { "Encrypt", "Encrypt" },
            { "Create", "Create" },
            { "Delete", "Delete" },
            { "Execute", "Execute" },
            { "StatusReady", "Ready." },
            { "LogFormat", "Log Format :" },
            { "LangInterface", "Interface Language :" },
            { "EncryptExtensions", "Extensions to encrypt :" },
            { "EncryptAll", "Encrypt all files" },
            { "AddExtension", "Add" },
            { "BlockedProcesses", "Business Processes :" },
            { "AddProcess", "Add Process" },
            { "General", "General" },
            { "Encryption", "Encryption" },
            { "LogFormatDesc", "Format of log files" },
            { "LangInterfaceDesc", "Application language" },
            { "EncryptAllDesc", "Encrypt all files" },
            { "BlockedProcessesDesc", "The backup will pause if these processes are detected" },
            { "Success", "Success" },
            { "Error", "Error" },
            { "EmptyFields", "Empty fields !" },
            { "JobCreated", "created." },
            { "JobDeleted", "Job deleted." },
            { "ExecutingJobs", "Executing {0} jobs..." },
            { "JobFinished", "finished !" },
            { "Status", "Status :" },
            { "ProcessBlockedMessage", "The process '{0}' is running and must be closed to continue the backup." },
            { "RestartWarning", "Language change requires a restart to be fully applied." }
        };

        public static string GetString(string key)
        {
            var currentLang = AppSettings.Instance.Language;

            var dict = currentLang == Language.Francais ? _fr : _en;

            return dict.ContainsKey(key) ? dict[key] : $"[{key}]";
        }
    }
}
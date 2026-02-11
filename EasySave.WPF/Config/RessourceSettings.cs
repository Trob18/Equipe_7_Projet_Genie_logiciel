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
            { "Create", "Créer" },
            { "Delete", "Supprimer" },
            { "Execute", "Exécuter" },
            { "StatusReady", "Prêt." },
            { "LogFormat", "Format des Logs :" },
            { "LangInterface", "Langue de l'interface :" },
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
            { "Create", "Create" },
            { "Delete", "Delete" },
            { "Execute", "Execute" },
            { "StatusReady", "Ready." },
            { "LogFormat", "Log Format :" },
            { "LangInterface", "Interface Language :" },
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
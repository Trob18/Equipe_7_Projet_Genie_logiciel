using System.Collections.Generic;
using EasySave.App.Enumerations;

namespace EasySave.App.Config
{
    public static class ResourceSettings
    {
        private static readonly Dictionary<string, string> _fr = new Dictionary<string, string>
        {
            { "Welcome", "Bienvenue dans EasySave v1.0" },
            { "MenuTitle", "MENU PRINCIPAL" },
            { "Option1", "1. Lister les travaux de sauvegarde" },
            { "Option2", "2. Créer un travail de sauvegarde" },
            { "Option3", "3. Exécuter un travail" },
            { "Option4", "4. Quitter" },
            { "EnterName", "Entrez le nom du travail :" },
            { "EnterSource", "Entrez le dossier source :" },
            { "EnterTarget", "Entrez le dossier de destination :" },
            { "EnterType", "Type de sauvegarde (1: Complet, 2: Différentiel) :" },
            { "Success", "Opération réussie !" },
            { "Error", "Erreur : " }
        };

        private static readonly Dictionary<string, string> _en = new Dictionary<string, string>
        {
            { "Welcome", "Welcome to EasySave v1.0" },
            { "MenuTitle", "MAIN MENU" },
            { "Option1", "1. List backup jobs" },
            { "Option2", "2. Create backup job" },
            { "Option3", "3. Execute backup job" },
            { "Option4", "4. Exit" },
            { "EnterName", "Enter job name:" },
            { "EnterSource", "Enter source directory:" },
            { "EnterTarget", "Enter target directory:" },
            { "EnterType", "Backup Type (1: Full, 2: Differential):" },
            { "Success", "Operation successful!" },
            { "Error", "Error: " }
        };

        public static string GetString(string key)
        {
            var currentLang = AppSettings.Instance.Language;

            var dict = currentLang == Language.Francais ? _fr : _en;

            return dict.ContainsKey(key) ? dict[key] : $"[{key}]";
        }
    }
}
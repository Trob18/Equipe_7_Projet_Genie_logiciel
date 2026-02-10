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
            { "OptionDelete", "4. Supprimer un travail de sauvegarde" },
            { "OptionConfig", "5. Réglages" },
            { "Option4", "6. Quitter" },
            { "Choice", "Choix" },
            { "LIST" , "LISTE" },
            { "NoJobAvailed", "Aucun travail disponible." },
            { "ChangeLang",   "Changer la langue (fr/en) :" },
            { "LangChanged",  "Langue changée en Français !" },
            { "EnterJobID", "Entrez le numéro du travail à supprimer :" },
            { "JobDeleted",   "Travail supprimé avec succès !" },
            { "EnterName", "Entrez le nom du travail :" },
            { "EnterSource", "Entrez le dossier source :" },
            { "EnterTarget", "Entrez le dossier de destination :" },
            { "EnterType", "Type de sauvegarde (1: Complet, 2: Différentiel) :" },
            { "Success", "Opération réussie !" },
            { "ConfigMenu", "1. Changer la langue\n2. Format des logs (json/xml)" },
            { "MaxJobsReached", "Erreur : Vous ne pouvez pas créer plus de 5 travaux." },
            { "Error", "Erreur : " }
        };

        private static readonly Dictionary<string, string> _en = new Dictionary<string, string>
        {
            { "Welcome", "Welcome to EasySave v1.0" },
            { "MenuTitle", "MAIN MENU" },
            { "Option1", "1. List backup jobs" },
            { "Option2", "2. Create backup job" },
            { "Option3", "3. Execute backup job" },
            { "OptionDelete", "4. Delete backup job" },
            { "OptionConfig", "5. Settings" },
            { "Option4", "6. Exit" },
            { "Choice", "Choice" },
            { "LIST" , "LIST" },
            { "NoJobAvailed", "No jobs available." },
            { "ChangeLang",   "Change Language (fr/en):" },
            { "LangChanged",  "Language changed to English!" },
            { "EnterJobID", "Enter the number of the job to be deleted :" },
            { "JobDeleted",   "Job deleted successfully!" },
            { "EnterName", "Enter job name: " },
            { "EnterSource", "Enter source directory: " },
            { "EnterTarget", "Enter target directory: " },
            { "EnterType", "Backup Type (1: Full, 2: Differential): " },
            { "Success", "Operation successful!" },
            { "ConfigMenu", "1. Change Language\n2. Log Format (json/xml)" },
            { "MaxJobsReached", "Error: You cannot create more than 5 backup jobs." },
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
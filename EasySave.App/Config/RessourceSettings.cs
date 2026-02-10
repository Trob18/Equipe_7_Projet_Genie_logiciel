using System.Collections.Generic;
using EasySave.App.Enumerations;

namespace EasySave.App.Config
{
    public static class ResourceSettings
    {
        private static readonly Dictionary<string, string> _fr = new Dictionary<string, string>
        {
            { "Welcome", "Bienvenue dans EasySave v1.1" },
            { "MenuTitle", "MENU PRINCIPAL" },
            { "Option1", "1. Lister les travaux de sauvegarde\n2. Créer un travail de sauvegarde\n3. Exécuter un travail" },
            { "Option2", "4. Supprimer un travail de sauvegarde\n5. Réglages\n6. Quitter"},
            { "Choice", "Choix" },
            { "LIST" , "LISTE" },
            { "Settings", "RÉGLAGES" },
            { "NoJobAvailed", "Aucun travail disponible." },
            { "ChangeLang", "CHANGER LA LANGUE" },
            { "OptionLang", "1. Français\n2. English"},
            { "ChangeFormat", "CHANGER LE FORMAT" },
            { "OptionFormat", "1. JSON\n2. XML" },
            { "LangChanged",  "Langue changée en Français !" },
            { "FormatChanged", "Le format a été changé en " },
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
            { "Welcome", "Welcome to EasySave v1.1" },
            { "MenuTitle", "MAIN MENU" },
            { "Option1", "1. List backup jobs\n2. Create backup job\n3. Execute backup job" },
            { "Option2", "4. Delete backup job\n5. Settings\n6. Exit" },
            { "Choice", "Choice" },
            { "LIST" , "LIST" },
            { "Settings", "SETTINGS" },
            { "NoJobAvailed", "No jobs available." },
            { "ChangeLang", "CHANGE LANGUAGE" },
            { "OptionLang", "1. Français\n2. English"},
            { "ChangeFormat", "CHANGE FORMAT" },
            { "OptionFormat", "1. JSON\n2. XML" },
            { "FormatChanged", "The format has been changed to " },
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
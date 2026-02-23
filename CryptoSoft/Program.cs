namespace CryptoSoft;

public static class Program
{
    public static void Main(string[] args)
    {
        try
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: CryptoSoft.exe <file_or_directory_path> <key>");
                Environment.Exit(-1);
            }

            string path = args[0];
            string key = args[1];
            int totalTime = 0;

            if (Directory.Exists(path))
            {
                // Mode dossier : on traite tout récursivement
                string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    var fileManager = new FileManager(file, key);
                    int time = fileManager.TransformFile();
                    if (time > 0) totalTime += time;
                }
            }
            else if (File.Exists(path))
            {
                // Mode fichier unique
                var fileManager = new FileManager(path, key);
                totalTime = fileManager.TransformFile();
            }
            else
            {
                Console.WriteLine("Path not found.");
                Environment.Exit(-2);
            }

            Environment.Exit(totalTime);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            Environment.Exit(-99);
        }
    }
}
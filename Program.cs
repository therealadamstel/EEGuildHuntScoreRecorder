using EEGuildHuntTool;
using System.Text;

public static class Program
{
    public static void Main(string[] args)
    {
        if (args.Length != 2)
        {
            DisplayUsage();
        }
        else
        {
            if (args[0].Equals("setup", StringComparison.OrdinalIgnoreCase))
            {
                string path = args[1];
                if (!Directory.Exists(path))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Unable to find path {path}");
                    Console.WriteLine();
                    Console.ResetColor();
                    DisplayUsage();
                }
                else
                {
                    Setup(path);
                }
            }
            else if (args[0].Equals("run", StringComparison.OrdinalIgnoreCase))
            {
                string path = args[1];
                if (!Directory.Exists(path))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Unable to find path {path}");
                    Console.WriteLine();
                    Console.ResetColor();
                    DisplayUsage();
                }
                else
                {
                    Run(path);
                }
            }
            else
            {
                DisplayUsage();
            }
        }

        static int GetSortNumber(string challenge)
        {
            challenge = challenge.Replace("Whale ", "").Replace("Turbine ", "").Replace("Spider ", "");

            return int.Parse(challenge);
        }

        static void Setup(string path)
        {
            // Validate any of the folders exist
            if (Directory.GetDirectories(path).Any())
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Warning: one or more files or folders exist in {path}");
                Console.WriteLine("All files and folders will be deleted. Are you sure you want to continue? y/n (ENTER=y)");
                Console.ResetColor();

                string response = Console.ReadLine();
                while (response != null && response != "" && response != "n" && response != "N" && response != "y" && response != "Y")
                {
                    response = Console.ReadLine();
                }

                if (response == "N" || response == "n")
                    return;
            }

            // Delete all folders and files
            foreach (var dir in Directory.GetDirectories(path))
            {
                Directory.Delete(dir, true);
            }
            foreach (var file in Directory.GetFiles(path))
            {
                File.Delete(file);
            }

            // Create the new folders
            // Spider: 1,4,7,10,13
            for (var i = 1; i <= 15; i += 3)
            {
                Directory.CreateDirectory(Path.Combine(path, $"Spider {i}"));
            }
            // Whale: 2,5,8,11,14
            for (var i = 2; i <= 15; i += 3)
            {
                Directory.CreateDirectory(Path.Combine(path, $"Whale {i}"));
            }
            // Turbine: 3,6,9,12,15
            for (var i = 3; i <= 15; i += 3)
            {
                Directory.CreateDirectory(Path.Combine(path, $"Turbine {i}"));
            }
        }

        static void Run(string path)
        {
            List<ScoreLog> logs = new List<ScoreLog>();
            foreach (var dir in Directory.GetDirectories(path))
            {
                string dirName = new DirectoryInfo(dir).Name;
                foreach (var file in Directory.GetFiles(dir))
                {
                    var thisFileLogs = ScoreImageParser.Parse(file);
                    thisFileLogs.ForEach(sl => sl.Challenge = dirName);
                    logs.AddRange(thisFileLogs);
                }
            }

            logs = logs.Distinct(new ScoreLogComparer()).OrderBy(x => GetSortNumber(x.Challenge)).ThenByDescending(x => x.DamageNumber).ToList();

            Console.Clear();

            StringBuilder sb = new StringBuilder();
            foreach (var log in logs)
            {
                sb.AppendLine($"{log.Name},{log.Challenge},{log.DamageNumber},{log.Attempts}");
            }
            File.WriteAllText(Path.Combine(path, "scores.csv"), sb.ToString());

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"{logs.Count} scores recorded to {Path.Combine(path, "scores.csv")}");
            Console.ResetColor();
        }

        static void DisplayUsage()
        {
            Console.WriteLine("Usage for EE Guild Hunt Tool:");
            Console.WriteLine();
            Console.WriteLine("EEGuildHuntTool setup <folder path>");
            Console.WriteLine("\tInitializes the given folder with all the necessary folder structure to record a guild hunt. If there are files or folders, they will be deleted.");
            Console.WriteLine();
            Console.WriteLine("EEGuildHuntTool run <folder path>");
            Console.WriteLine("\tRuns the image processing for the given folder path that was set up by the setup command");
        }
    }
}
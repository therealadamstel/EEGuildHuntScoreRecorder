using EEGuildHuntTool;
using System.Text;

string path = @"C:\Users\adams\OneDrive\Desktop\EE\Scores\";
//string path = @"C:\Users\adams\OneDrive\Desktop\EE\PoppaCapImages\";

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
Console.WriteLine($" * ******* {logs.Count} Recorded Scores ********");

StringBuilder sb = new StringBuilder();
foreach (var log in logs)
{
    sb.AppendLine($"{log.Name},{log.Challenge},{log.DamageNumber},{log.Attempts}");
}
Console.WriteLine(sb.ToString());

File.WriteAllText(Path.Combine(path, "scores.txt"), sb.ToString());

/// ***********************************
int GetSortNumber(string challenge)
{
    challenge = challenge.Replace("Whale ", "").Replace("Turbine ", "").Replace("Spider ", "");

    return int.Parse(challenge);
}
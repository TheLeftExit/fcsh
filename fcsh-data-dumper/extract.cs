#!/usr/bin/dotnet run

var logPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
    ".factorio",
    "factorio-current.log"
);
if (!File.Exists(logPath))
{
    Console.WriteLine($"factorio-current.log does not exist.");
}
using(var logReader = File.OpenText(logPath))
{
    using (var jsonWriter = File.CreateText("../FactorioCalculator/data.json"))
    {
        var lineCount = 0;
        var inDataDumperBlock = false;
        while (!logReader.EndOfStream)
        {
            var line = logReader.ReadLine();
            if (!inDataDumperBlock)
            {
                inDataDumperBlock = line == "fcsh-data-dumper-start";
                continue;
            }
            if (inDataDumperBlock && line == "fcsh-data-dumper-end")
            {
                break;
            }
            jsonWriter.WriteLine(line);
            lineCount++;
        }
        Console.WriteLine($"Wrote {lineCount} lines to ../FactorioCalculator/data.json");
    }
}
#!/usr/bin/dotnet --

#:project ../FactorioCalculator.Prototypes

var logPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
    ".factorio",
    "factorio-current.log"
);

var outputPath = "../data.json";

if (!File.Exists(logPath))
{
    Console.WriteLine($"factorio-current.log does not exist.");
}
using(var logReader = File.OpenText(logPath))
{
    using (var jsonWriter = File.CreateText(outputPath))
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
        Console.WriteLine($"Wrote {lineCount} lines to {outputPath}");
    }
}

using(var jsonReader = File.OpenRead(outputPath))
{
    try
    {
        DataRoot.FromStream(jsonReader);
        Console.WriteLine("Successfully deserialized resulting JSON");
    } catch(Exception e)
    {
        Console.WriteLine("Warning: Failed to deserialize resulting JSON!");
        Console.WriteLine($"{e.GetType()}: {e.Message}");
    }
}
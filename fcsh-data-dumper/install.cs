#!/usr/bin/dotnet run

var modFolder = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
    ".factorio",
    "mods",
    "fcsh-data-dumper"
);
if (Directory.Exists(modFolder))
{
    Directory.Delete(modFolder, true);
    Console.WriteLine($"{modFolder} deleted.");
}

Directory.CreateDirectory(modFolder);
Console.WriteLine($"{modFolder} created.");
var modFiles = new string[] { "data-final-fixes.lua", "info.json" };
foreach (var modFile in modFiles)
{
    File.Copy(modFile, Path.Combine(modFolder, modFile));
    Console.WriteLine($"Copied {modFile}");
}
Console.WriteLine("Exiting.");
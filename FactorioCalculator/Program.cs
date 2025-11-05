// See https://aka.ms/new-console-template for more information
using System.Reflection;
using System.Text.Json;

using(var jsonStream = File.OpenRead("data.json"))
{
    var root = JsonSerializer.Deserialize<DataDumperRoot>(jsonStream, new JsonSerializerOptions()
    {
        AllowTrailingCommas = true
    });

    ;
}


public class DataDumperRoot
{
    public AnyPrototype[] Recipes { get; init; } = null!;
    public AnyPrototype[] CraftingMachines { get; init; } = null!;
}

public class AnyPrototype
{
    public string Name { get; init; } = null!;
}
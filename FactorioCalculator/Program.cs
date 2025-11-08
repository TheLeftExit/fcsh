using System.Text.Json;

using (var jsonStream = File.OpenRead("data.json"))
{
    var root = JsonSerializer.Deserialize<DataRoot>(jsonStream, new JsonSerializerOptions()
    {
        AllowTrailingCommas = true
    }) ?? throw new InvalidOperationException();
    Repository.Initialize(root);
}

var repository = Repository.Instance;
;
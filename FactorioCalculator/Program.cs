using System.Runtime.InteropServices;
using System.Text.Json;

using (var jsonStream = File.OpenRead("data.json"))
{
    var root = DataRoot.FromStream(jsonStream);
    Repository.Initialize(root);
}

var repository = Repository.Instance;

var tree = new RecipeTree(
    Technologies: [],
    Buildings: [
        "assembling-machine-1",
        "electric-mining-drill",
        "stone-furnace",
    ],
    StartingBalance: [
        new("automation-science-pack", -1m)
    ],
    Recipes: [
        new("automation-science-pack"),
        new("iron-gear-wheel"),
        new("iron-plate"),
        new("copper-plate"),
        new("iron-ore"),
        new("copper-ore")
    ]);

var technologyProductivityBonuses = new Dictionary<string, decimal>();
foreach (var technology in tree.Technologies ?? [])
{
    var technologyPrototype = repository.ProductivityTechnologies[technology.Name];
    foreach (var recipe in technologyPrototype.Recipes)
    {
        technologyProductivityBonuses.GetOrAdd(recipe.RecipeName) += technology.Level * recipe.BonusProductivity;
    }
}

var finalBuildings = new List<(Recipe Recipe, BuildingType Building, Rational Count)>();
var balance = tree.StartingBalance?.ToDictionary(x => x.Name, x => (Rational)x.Count) ?? [];
var nodeBuildingBalance = new Dictionary<string, Rational>();

foreach (var node in tree.Recipes)
{
    var recipe = repository.Recipes[node.Recipe];
    var buildingName = node.Building
        ?? tree.Buildings?.First(x => repository.BuildingTypes[x].Categories.Contains(recipe.Category))
        ?? throw new ArgumentException();
    var building = repository.BuildingTypes[buildingName];
    var controlItem = node.ControlItem ?? recipe.Products.Single().Name;
    var targetMode = node.TargetMode ?? ItemCountTarget.AfterZero;
    var productivity = (Rational)1 + building.BonusProductivity + technologyProductivityBonuses.GetValueOrDefault(recipe.Name);
    var epsilon = node.Epsilon ?? 1;

    nodeBuildingBalance.Clear();
    foreach (var ingredient in recipe.Ingredients)
    {
        nodeBuildingBalance.GetOrAdd(ingredient.Name) -= ingredient.Amount * building.Speed / recipe.Energy;
    }
    foreach (var product in recipe.Products)
    {
        nodeBuildingBalance.GetOrAdd(product.Name) += product.Amount * productivity * building.Speed / recipe.Energy;
    }

    var itemCurrentCount = balance.GetOrAdd(controlItem);
    var itemDeltaCount = nodeBuildingBalance.GetOrAdd(controlItem);

    var buildingCount = -itemCurrentCount / itemDeltaCount;

    if (epsilon != 0)
    {
        var itemDeltaEpsilonCount = itemDeltaCount * epsilon;
        if (itemCurrentCount.Sign == itemDeltaEpsilonCount.Sign) throw new ArgumentException();

        var (buildingCountEpsilon, buildingCountEpsilonRemainder) = Rational.DivRem(itemCurrentCount, -itemDeltaEpsilonCount);

        if (Rational.Mod(buildingCountEpsilonRemainder) > 0)
        {
            buildingCountEpsilon += targetMode switch
            {
                ItemCountTarget.BeforeZero => 0,
                ItemCountTarget.AfterZero => 1,
                ItemCountTarget.Positive => itemDeltaCount > 0 ? 1 : 0,
                ItemCountTarget.Negative => itemDeltaCount < 0 ? 1 : 0,
                _ => throw new ArgumentException()
            };
        }

        buildingCount = buildingCountEpsilon * epsilon;
    }

    foreach (var itemSlot in nodeBuildingBalance)
    {
        balance.GetOrAdd(itemSlot.Key) += itemSlot.Value * buildingCount;
    }
    finalBuildings.Add((recipe, building, buildingCount));
}

Console.WriteLine("Building tally:");
Console.WriteLine(new string('=', 75));
foreach (var building in finalBuildings)
{
    Console.WriteLine($"{building.Recipe.Name,-30}| {building.Building.Name,-30}| {building.Count}");
}
Console.WriteLine();
Console.WriteLine("Leftover items:");
Console.WriteLine(new string('=', 45));
foreach(var itemSlot in balance.Where(x => x.Value != 0))
{
    Console.WriteLine($"{itemSlot.Key,-30}| {itemSlot.Value}");
}
;

file static class DictionaryExtensions
{
    public static ref TValue? GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key) where TKey : notnull where TValue : notnull
    {
        return ref CollectionsMarshal.GetValueRefOrAddDefault(dictionary, key, out _);
    }
}
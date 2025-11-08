using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Numerics;

public class Repository
{
    public static Repository Instance { get => field ?? throw new InvalidOperationException(); private set; }
    public required FrozenDictionary<string, Recipe> Recipes { get; init; }
    public required FrozenDictionary<string, Building> Buildings { get; init; }

    public static void Initialize(DataRoot data)
    {
        Instance = new()
        {
            Recipes = data.Recipes.Select(DecodeRecipe).ToFrozenDictionary(x => x.Name),
            Buildings = data.CraftingMachines.Select(DecodeBuilding).ToFrozenDictionary(x => x.Name)
        };
    }
    
    private static Building DecodeBuilding(CraftingMachinePrototype prototype)
    {
        return new()
        {
            Name = prototype.Name,
            Speed = prototype.CraftingSpeed,
            BaseProductivity = prototype.EffectReceiver?.BaseEffect?.Productivity ?? 1,
            Categories = prototype.CraftingCategories
        };
    }

    private static Recipe DecodeRecipe(RecipePrototype prototype)
    {
        return new()
        {
            Name = prototype.Name,
            Energy = prototype.EnergyRequired,
            Category = prototype.Category,
            Ingredients = prototype.Ingredients?.Select(x => new ItemSlot()
            {
                Name = x.Name,
                Amount = x.Amount
            }).ToArray() ?? Array.Empty<ItemSlot>(),
            Products = prototype.Results?.Select(x => new ItemSlot()
            {
                Name = x.Name,
                Amount = (Rational)x.Amount * (x.Probability ?? 1) + (x.ExtraCountFraction ?? 0)
            }).ToArray() ?? Array.Empty<ItemSlot>()
        };
    }
}

public class Building
{
    public required string Name { get; init; }
    public required Rational Speed { get; init; }
    public required Rational BaseProductivity { get; init; }
    public required string[] Categories { get; init; }
}

public class Recipe
{
    public required string Name { get; init; }
    public required Rational Energy { get; init; }
    public required string Category { get; init; }
    public required ItemSlot[] Ingredients { get; init; }
    public required ItemSlot[] Products { get; init; }
}

public record struct ItemSlot(string Name, Rational Amount)
{
    public static ItemSlot operator +(ItemSlot source, Rational delta) => source with { Amount = source.Amount + delta };
    public static ItemSlot operator -(ItemSlot source, Rational delta) => source with { Amount = source.Amount - delta };
    public static ItemSlot operator *(ItemSlot source, Rational delta) => source with { Amount = source.Amount * delta };
}
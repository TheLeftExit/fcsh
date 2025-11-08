using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;

public class Repository
{
    public static Repository Instance { get => field ?? throw new InvalidOperationException(); private set; }
    public required FrozenDictionary<string, Recipe> Recipes { get; init; }
    public required FrozenDictionary<string, Building> Buildings { get; init; }

    public static void Initialize(DataRoot data)
    {
        Instance = new()
        {
            Recipes = data.Recipes.Select(DecodeRecipe)
                .Concat(data.ResourceEntities.Select(DecodeRecipe))
                .ToFrozenDictionary(x => x.Name),
            Buildings = data.CraftingMachines.Select(DecodeBuilding)
                .Concat(data.MiningDrills.Select(DecodeBuilding))
                .ToFrozenDictionary(x => x.Name)
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
    
    private static ItemSlot DecodeProduct(ProductPrototype prototype)
    {
        var baseAmount = (prototype.Amount, prototype.AmountMin, prototype.AmountMax) switch
        {
            (decimal amount, null, null) => (Rational)amount,
            (null, decimal amountMin, decimal amountMax) => amountMin == amountMax ? (Rational)amountMin : throw new NotSupportedException(),
            _ => throw new NotSupportedException()
        };
        return new()
        {
            Name = prototype.Name,
            Amount = baseAmount * (prototype.Probability ?? 1) + (prototype.ExtraCountFraction ?? 0)
        };
    }

    private static Recipe DecodeRecipe(RecipePrototype prototype)
    {
        return new()
        {
            Name = prototype.Name,
            Energy = prototype.EnergyRequired,
            Category = prototype.Category,
            Ingredients = prototype.Ingredients?.Select(x => new ItemSlot(x.Name, x.Amount)).ToArray() ?? [],
            Products = prototype.Results?.Select(DecodeProduct).ToArray() ?? []
        };
    }

    private static Building DecodeBuilding(MiningDrillPrototype prototype)
    {
        return new()
        {
            Name = prototype.Name,
            Speed = prototype.MiningSpeed,
            BaseProductivity = prototype.EffectReceiver?.BaseEffect?.Productivity ?? 1,
            Categories = prototype.ResourceCategories
        };
    }

    private static Recipe DecodeRecipe(ResourceEntityPrototype prototype)
    {
        return new()
        {
            Name = prototype.Name,
            Category = prototype.Category,
            Energy = prototype.Minable.MiningTime,
            Ingredients = prototype.Minable.RequiredFluid is not null ? [new ItemSlot(prototype.Minable.RequiredFluid, prototype.Minable.FluidAmount)] : [],
            Products = prototype.Minable.Results?.Select(DecodeProduct).ToArray() ?? [new ItemSlot(prototype.Minable.Result ?? throw new NotSupportedException(), prototype.Minable.Count)]
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
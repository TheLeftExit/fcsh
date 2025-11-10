using System.Collections.Frozen;

public class Repository
{
    public static Repository Instance { get => field ?? throw new InvalidOperationException(); private set; }
    public required FrozenDictionary<string, Recipe> Recipes { get; init; }
    public required FrozenDictionary<string, BuildingType> BuildingTypes { get; init; }
    public required FrozenDictionary<string, ResearchProductivityTechnology> ProductivityTechnologies { get; init; }

    public static void Initialize(DataRoot data)
    {
        if (data.Recipes.Select(x => x.Category).Intersect(data.ResourceEntities.Select(y => y.Category)).Any())
        {
            throw new NotSupportedException(); // we conflate resource categories with recipe categories, so things might break if they overlap
        }
        var recipes = data.Recipes.Select(DecodeRecipe)
            .Concat(data.ResourceEntities.Select(DecodeRecipe));

        var buildingTypes = data.CraftingMachines.Select(DecodeBuilding)
            .Concat(data.MiningDrills.Select(DecodeBuilding));

        var productivityTechnologies = data.ProductivityTechnologies.Select(DecodeTechnology)
            .Append(CreateMiningProductivityTechnology(recipes));

        Instance = new()
        {
            Recipes = recipes.ToFrozenDictionary(x => x.Name),
            BuildingTypes = buildingTypes.ToFrozenDictionary(x => x.Name),
            ProductivityTechnologies = productivityTechnologies.ToFrozenDictionary(x => x.Name)
        };
    }

    private static ResearchProductivityTechnology DecodeTechnology(TechnologyPrototype prototype)
    {
        return new()
        {
            Name = prototype.Name,
            Recipes = prototype.Effects.Select(x => (x.Recipe, x.Change)).ToArray()
        };
    }

    // this is precisely the kind of thing I wanted to avoid by scraping as much as possible from the game itself,
    // but technology parsing seems to be non-trivial and done in the closed source code, so replicating it wouldn't be much more future-proof
    private static ResearchProductivityTechnology CreateMiningProductivityTechnology(IEnumerable<Recipe> recipes)
    {
        return new()
        {
            Name = "mining-productivity",
            Recipes = recipes
                .Where(x => x.Category is "basic-solid" or "basic-fluid" or "hard-solid")
                .Select(x => (x.Name, 0.1m))
                .ToArray()
        };
    }

    private static BuildingType DecodeBuilding(CraftingMachinePrototype prototype)
    {
        return new()
        {
            Name = prototype.Name,
            Speed = prototype.CraftingSpeed,
            BonusProductivity = prototype.EffectReceiver?.BaseEffect?.Productivity ?? 0,
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

    private static BuildingType DecodeBuilding(MiningDrillPrototype prototype)
    {
        return new()
        {
            Name = prototype.Name,
            Speed = prototype.MiningSpeed,
            BonusProductivity = prototype.EffectReceiver?.BaseEffect?.Productivity ?? 0,
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

public class BuildingType
{
    public required string Name { get; init; }
    public required Rational Speed { get; init; }
    public required Rational BonusProductivity { get; init; }
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

public class ResearchProductivityTechnology
{
    public required string Name { get; init; }
    public required (string RecipeName, decimal BonusProductivity)[] Recipes { get; init; }
}

public record struct ItemSlot(string Name, Rational Amount)
{
    public static ItemSlot operator +(ItemSlot source, Rational delta) => source with { Amount = source.Amount + delta };
    public static ItemSlot operator -(ItemSlot source, Rational delta) => source with { Amount = source.Amount - delta };
    public static ItemSlot operator *(ItemSlot source, Rational delta) => source with { Amount = source.Amount * delta };
}
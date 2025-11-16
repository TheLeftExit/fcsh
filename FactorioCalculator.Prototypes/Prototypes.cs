using System.Text.Json;
using System.Text.Json.Serialization;

[JsonSerializable(typeof(DataRoot))]
[JsonSerializable(typeof(RecipePrototype))]
[JsonSerializable(typeof(IngredientPrototype))]
[JsonSerializable(typeof(ProductPrototype))]
[JsonSerializable(typeof(CraftingMachinePrototype))]
[JsonSerializable(typeof(EffectReceiver))]
[JsonSerializable(typeof(Effect))]
[JsonSerializable(typeof(MiningDrillPrototype))]
[JsonSerializable(typeof(ResourceEntityPrototype))]
[JsonSerializable(typeof(MiningProperties))]
[JsonSerializable(typeof(TechnologyPrototype))]
[JsonSerializable(typeof(Modifier))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(decimal))]
[JsonSerializable(typeof(decimal?))]
[JsonSourceGenerationOptions(AllowTrailingCommas = true)]
public partial class DataRootContext : JsonSerializerContext
{
    
}

public record DataRoot(
    RecipePrototype[] Recipes,
    CraftingMachinePrototype[] CraftingMachines,
    MiningDrillPrototype[] MiningDrills,
    ResourceEntityPrototype[] ResourceEntities,
    TechnologyPrototype[] ProductivityTechnologies
)
{
    public static DataRoot FromStream(Stream stream)
    {
        return JsonSerializer.Deserialize(stream, DataRootContext.Default.DataRoot) ?? throw new InvalidOperationException("JSON deserialized to null.");
    }
}

public record RecipePrototype(
    string Name,
    decimal EnergyRequired,
    string Category,
    IngredientPrototype[]? Ingredients,
    ProductPrototype[]? Results
);

public record IngredientPrototype(
    string Name,
    string Type,
    decimal Amount,
    decimal? Temperature,
    decimal? MinimumTemperature,
    decimal? MaximumTemperature
);

public record ProductPrototype(
    string Name,
    string Type,
    decimal? Amount,
    decimal? AmountMin,
    decimal? AmountMax,
    decimal? Probability,
    decimal? ExtraCountFraction,
    decimal? Temperature
);

public record CraftingMachinePrototype(
    string Name,
    decimal CraftingSpeed,
    string[] CraftingCategories,
    EffectReceiver? EffectReceiver
);

public record EffectReceiver(
    Effect? BaseEffect
);

public record Effect(
    decimal? Consumption,
    decimal? Speed,
    decimal? Productivity,
    decimal? Pollution,
    decimal? Quality
);

public record MiningDrillPrototype(
    string Name,
    decimal MiningSpeed,
    string[] ResourceCategories,
    EffectReceiver? EffectReceiver
);

public record ResourceEntityPrototype(
    string Name,
    string Category,
    MiningProperties Minable
);

public record MiningProperties(
    decimal MiningTime,
    ProductPrototype[]? Results,
    string? Result,
    decimal Count,
    string? RequiredFluid,
    decimal FluidAmount
);

public record TechnologyPrototype(
    string Name,
    Modifier[] Effects
);

public record Modifier(
    string Type,
    string Recipe,
    decimal Change
);
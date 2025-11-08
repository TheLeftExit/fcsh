public record DataRoot(
    RecipePrototype[] Recipes,
    CraftingMachinePrototype[] CraftingMachines,
    MiningDrillPrototype[] MiningDrills,
    ResourceEntityPrototype[] ResourceEntities
);

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
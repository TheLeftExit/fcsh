public record RecipeTree(
    RecipeTreeTechnologyHeader[]? Technologies,
    string[]? Buildings,
    RecipeTreeStartingBalanceHeader[]? StartingBalance,
    RecipeTreeNode[] Recipes
);

public record RecipeTreeNode(
    string Recipe,
    string? Building = null,
    decimal? Epsilon = null,
    string? ControlItem = null,
    ItemCountTarget? TargetMode = null
);

public record RecipeTreeTechnologyHeader(
    string Name,
    int Level
);

public record RecipeTreeStartingBalanceHeader(
    string Name,
    decimal Count
);

public enum ItemCountTarget
{
    Positive,
    Negative,
    BeforeZero,
    AfterZero
}
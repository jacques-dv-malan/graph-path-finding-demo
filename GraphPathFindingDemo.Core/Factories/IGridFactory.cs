using GraphPathFindingDemo.Core.Layout;

namespace GraphPathFindingDemo.Core.Factories;

/// <summary>
/// Creates the grid scenarios a search runs on.
/// </summary>
public interface IGridFactory
{
    /// <summary>
    /// Gets the layout styles the factory can build.
    /// </summary>
    IReadOnlyList<LayoutStyle> AvailableStyles { get; }

    /// <summary>
    /// Creates a scenario for the requested grid.
    /// </summary>
    /// <param name="width">The number of columns to generate.</param>
    /// <param name="height">The number of rows to generate.</param>
    /// <param name="seed">The seed controlling the generated layout.</param>
    /// <param name="style">The style of the layout to generate.</param>
    /// <returns>The created scenario.</returns>
    GridScenario Create(
        int width,
        int height,
        int seed,
        LayoutStyle style = LayoutStyle.RecursiveDivision);
}

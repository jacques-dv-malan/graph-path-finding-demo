using GraphPathFindingDemo.Core.Endpoints;

namespace GraphPathFindingDemo.Core.Factories;

/// <summary>
/// Creates the grid scenarios a search runs on.
/// </summary>
public interface IGridFactory
{
    /// <summary>
    /// Creates a scenario for the requested grid.
    /// </summary>
    /// <param name="width">The number of columns to generate.</param>
    /// <param name="height">The number of rows to generate.</param>
    /// <param name="seed">The seed controlling the generated layout.</param>
    /// <param name="endpointMode">The mode deciding how the endpoints are chosen.</param>
    /// <returns>The created scenario.</returns>
    GridScenario Create(int width, int height, int seed, EndpointMode endpointMode = EndpointMode.FixedFixed);
}

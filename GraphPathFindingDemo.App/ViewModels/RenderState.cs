using GraphPathFindingDemo.Core.Factories;
using GraphPathFindingDemo.Core.Graph;

namespace GraphPathFindingDemo.App.ViewModels;

/// <summary>
/// The snapshot of a running search that the canvas draws.
/// </summary>
/// <remarks>
/// A new instance is published on every update, so that the canvas sees a new value and
/// repaints. The node collections are the live collections of the view model: they are read
/// while the canvas draws, so they are never copied.
/// </remarks>
public sealed class RenderState
{
    #region Properties
    /// <summary>
    /// Gets the scenario being searched.
    /// </summary>
    public GridScenario Scenario { get; }

    /// <summary>
    /// Gets the nodes on the frontier, reached but not yet expanded.
    /// </summary>
    public IReadOnlyCollection<GridNode> Discovered { get; }

    /// <summary>
    /// Gets the nodes that have been expanded.
    /// </summary>
    public IReadOnlyList<GridNode> Visited { get; }

    /// <summary>
    /// Gets the path found so far. Empty until the search succeeds.
    /// </summary>
    public IReadOnlyList<GridNode> Path { get; }
    #endregion Properties

    #region Constructor
    /// <summary>
    /// Initializes a new instance of the <see cref="RenderState"/> class.
    /// </summary>
    /// <param name="scenario">The scenario being searched.</param>
    /// <param name="discovered">The nodes on the frontier, reached but not yet expanded.</param>
    /// <param name="visited">The nodes that have been expanded.</param>
    /// <param name="path">The path found so far. Empty until the search succeeds.</param>
    public RenderState(
        GridScenario scenario,
        IReadOnlyCollection<GridNode> discovered,
        IReadOnlyList<GridNode> visited,
        IReadOnlyList<GridNode> path)
    {
        Scenario = scenario;
        Discovered = discovered;
        Visited = visited;
        Path = path;
    }
    #endregion Constructor
}

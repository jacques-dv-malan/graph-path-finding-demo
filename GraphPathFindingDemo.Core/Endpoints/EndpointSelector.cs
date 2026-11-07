using GraphPathFindingDemo.Core.Graph;

namespace GraphPathFindingDemo.Core.Endpoints;

/// <summary>
/// Chooses the start and end nodes of a search.
/// </summary>
public static class EndpointSelector
{
    #region Public Methods
    /// <summary>
    /// Selects the top-left node as the start and the bottom-right node as the end.
    /// </summary>
    /// <param name="graph">The graph the endpoints are selected in.</param>
    /// <returns>The top-left and bottom-right nodes of the graph.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="graph"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The start and end node are the same node.</exception>
    public static (GridNode Start, GridNode End) Select(GridGraph graph)
    {
        ArgumentNullException.ThrowIfNull(graph);

        var start = graph.NodeAt(new GridPoint(0, 0));
        var end = graph.NodeAt(new GridPoint(graph.Height - 1, graph.Width - 1));

        if (ReferenceEquals(start, end))
        {
            throw new ArgumentException("The start and end nodes must be different nodes.");
        }

        return (start, end);
    }
    #endregion Public Methods
}

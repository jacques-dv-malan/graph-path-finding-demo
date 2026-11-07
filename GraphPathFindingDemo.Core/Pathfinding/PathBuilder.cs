using GraphPathFindingDemo.Core.Graph;

namespace GraphPathFindingDemo.Core.Pathfinding;

/// <summary>
/// Rebuilds the path a search found from the links it discovered.
/// </summary>
internal static class PathBuilder
{
    #region Public Methods
    /// <summary>
    /// Rebuilds the path from the start node to the end node by walking the discovered links
    /// backwards.
    /// </summary>
    /// <param name="end">The node the search finished at.</param>
    /// <param name="cameFrom">The discovered links, mapping each node to the one it came from.</param>
    /// <returns>The ordered path from the start node to <paramref name="end"/>.</returns>
    internal static IReadOnlyList<GridNode> Build(
        GridNode end,
        Dictionary<GridNode, GridNode?> cameFrom)
    {
        var reversed = new List<GridNode>();
        GridNode? current = end;

        while (current is not null)
        {
            reversed.Add(current);
            current = cameFrom[current];
        }

        reversed.Reverse();
        return reversed;
    }
    #endregion Public Methods
}

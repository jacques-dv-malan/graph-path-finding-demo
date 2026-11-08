using GraphPathFindingDemo.Core.Graph;

namespace GraphPathFindingDemo.Core.Pathfinding.Search;

/// <summary>
/// Searches a graph for a shortest path using breadth-first search.
/// </summary>
/// <remarks>
/// The search is unit weight, so every passage costs the same and the first path found is
/// optimal.
/// </remarks>
public sealed class BreadthFirstSearch : ISearchAlgorithm
{
    #region Properties
    /// <inheritdoc />
    public SearchAlgorithmStyle Style => SearchAlgorithmStyle.BreadthFirstSearch;
    #endregion Properties

    #region Public Methods
    /// <summary>
    /// Searches the graph breadth first, reporting one <see cref="SearchStep"/> per event.
    /// </summary>
    /// <param name="graph">The graph to search.</param>
    /// <param name="start">The node the search starts from.</param>
    /// <param name="end">The node the search looks for.</param>
    /// <returns>
    /// The sequence of search steps. The sequence ends as soon as the end node is reached, or
    /// when the frontier empties without reaching it.
    /// </returns>
    /// <exception cref="ArgumentNullException">Any of the arguments is <see langword="null"/>.</exception>
    public IEnumerable<SearchStep> Search(GridGraph graph, GridNode start, GridNode end)
    {
        ArgumentNullException.ThrowIfNull(graph);
        ArgumentNullException.ThrowIfNull(start);
        ArgumentNullException.ThrowIfNull(end);

        var cameFrom = new Dictionary<GridNode, GridNode?>();
        var distances = new Dictionary<GridNode, int>();
        var queue = new Queue<GridNode>();

        cameFrom[start] = null;
        distances[start] = 0;
        queue.Enqueue(start);
        yield return new SearchStep(start, SearchStepKind.Discovered, 0);

        while (queue.Count > 0)
        {
            var node = queue.Dequeue();
            yield return new SearchStep(node, SearchStepKind.Visited, distances[node]);

            if (ReferenceEquals(node, end))
            {
                yield return new SearchStep(node, SearchStepKind.PathFound, distances[node], PathBuilder.Build(end, cameFrom));
                yield break;
            }

            foreach (var neighbour in graph.Neighbors(node))
            {
                if (cameFrom.ContainsKey(neighbour))
                {
                    continue;
                }

                cameFrom[neighbour] = node;
                distances[neighbour] = distances[node] + 1;
                queue.Enqueue(neighbour);
                yield return new SearchStep(neighbour, SearchStepKind.Discovered, distances[neighbour]);
            }
        }
    }
    #endregion Public Methods
}

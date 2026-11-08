using GraphPathFindingDemo.Core.Graph;

namespace GraphPathFindingDemo.Core.Pathfinding.Search;

/// <summary>
/// Searches a graph for the cheapest path using DijkstraSearch's algorithm.
/// </summary>
/// <remarks>
/// <para>
/// The search always leaves the cheapest cell it knows, so the path it finds costs the least of
/// all the paths there. It does not count the cells on the path: a route that is longer in cells
/// can still be the cheaper one when the ground it crosses is steep.
/// </para>
/// <para>
/// A cell can be reached again for a lower price before it is settled, so the search keeps a
/// queue of the cells it knows and takes the cheapest one first.
/// </para>
/// </remarks>
public sealed class DijkstraSearch : ISearchAlgorithm
{
    #region Properties
    /// <inheritdoc />
    public SearchAlgorithmStyle Style => SearchAlgorithmStyle.DijkstraSearch;
    #endregion Properties

    #region Public Methods
    /// <summary>
    /// Searches the graph for the cheapest path, reporting one <see cref="SearchStep"/> per event.
    /// </summary>
    /// <param name="graph">The graph to search.</param>
    /// <param name="start">The node the search starts from.</param>
    /// <param name="end">The node the search looks for.</param>
    /// <returns>
    /// The sequence of search steps. The sequence ends as soon as the end node is settled, or
    /// when the queue empties without reaching it.
    /// </returns>
    /// <exception cref="ArgumentNullException">Any of the arguments is <see langword="null"/>.</exception>
    public IEnumerable<SearchStep> Search(GridGraph graph, GridNode start, GridNode end)
    {
        ArgumentNullException.ThrowIfNull(graph);
        ArgumentNullException.ThrowIfNull(start);
        ArgumentNullException.ThrowIfNull(end);

        var costs = new Dictionary<GridNode, int>();
        var cameFrom = new Dictionary<GridNode, GridNode?>();
        var queue = new PriorityQueue<GridNode, int>();

        costs[start] = 0;
        cameFrom[start] = null;
        queue.Enqueue(start, 0);

        yield return new SearchStep(start, SearchStepKind.Discovered, 0);

        while (queue.TryDequeue(out var node, out var cost))
        {
            // The cell may have been queued more than once; the dearer entry is of no use.
            if (cost != costs[node])
            {
                continue;
            }

            yield return new SearchStep(node, SearchStepKind.Visited, cost);

            if (ReferenceEquals(node, end))
            {
                yield return new SearchStep(node, SearchStepKind.PathFound, cost, PathBuilder.Build(node, cameFrom));
                yield break;
            }

            foreach (var neighbour in graph.Neighbors(node))
            {
                var next = cost + graph.WeightBetween(node, neighbour);

                if (costs.TryGetValue(neighbour, out var known) && next >= known)
                {
                    continue;
                }

                costs[neighbour] = next;
                cameFrom[neighbour] = node;
                queue.Enqueue(neighbour, next);

                yield return new SearchStep(neighbour, SearchStepKind.Discovered, next);
            }
        }
    }
    #endregion Public Methods
}

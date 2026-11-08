using GraphPathFindingDemo.Core.Graph;

namespace GraphPathFindingDemo.Core.Pathfinding.Search;

/// <summary>
/// Searches a graph for the cheapest path using the A* algorithm.
/// </summary>
/// <remarks>
/// <para>
/// The search leaves the cell that promises the cheapest way to the end, counting what it has
/// already spent and what the heuristic says is left. The path it finds is the cheapest one, as
/// long as the heuristic never promises more than the truth.
/// </para>
/// <para>
/// The queue holds the cells by that promise, and where two cells promise the same, the one
/// nearer the end is taken first. That keeps the search from spreading wide over cells that
/// cannot beat the end.
/// </para>
/// </remarks>
public sealed class AStarSearch : ISearchAlgorithm, IHeuristicSearch
{
    #region Instance Fields
    private readonly List<IHeuristic> _heuristics;
    private IHeuristic _heuristic;
    #endregion Instance Fields

    #region Properties
    /// <inheritdoc />
    public SearchAlgorithmStyle Style => SearchAlgorithmStyle.AStarSearch;

    /// <inheritdoc />
    public IReadOnlyList<IHeuristic> Heuristics => _heuristics;

    /// <inheritdoc />
    public IHeuristic Heuristic
    {
        get => _heuristic;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            _heuristic = value;
        }
    }
    #endregion Properties

    #region Constructor
    /// <summary>
    /// Initializes a new instance of the <see cref="AStarSearch"/> class.
    /// </summary>
    /// <param name="heuristics">The heuristics the search can run with. The first one is the one
    /// it starts with.</param>
    /// <exception cref="ArgumentNullException"><paramref name="heuristics"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">No heuristic was given.</exception>
    public AStarSearch(params IHeuristic[] heuristics)
    {
        ArgumentNullException.ThrowIfNull(heuristics);

        if (heuristics.Length == 0)
        {
            throw new ArgumentException("At least one heuristic is required.", nameof(heuristics));
        }

        _heuristics = heuristics.ToList();
        _heuristic = heuristics[0];
    }
    #endregion Constructor

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

        var costs = new Dictionary<GridNode, double>();
        var cameFrom = new Dictionary<GridNode, GridNode?>();
        var queue = new PriorityQueue<(GridNode Node, double Cost), (double Promise, double Estimate)>();

        costs[start] = 0;
        cameFrom[start] = null;

        var startEstimate = _heuristic.Estimate(graph, start, end);

        queue.Enqueue((start, 0), (startEstimate, startEstimate));

        yield return new SearchStep(start, SearchStepKind.Discovered, 0);

        while (queue.TryDequeue(out var entry, out var key))
        {
            var node = entry.Node;
            var cost = entry.Cost;

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

                var estimate = _heuristic.Estimate(graph, neighbour, end);

                queue.Enqueue((neighbour, next), (next + estimate, estimate));

                yield return new SearchStep(neighbour, SearchStepKind.Discovered, next);
            }
        }
    }
    #endregion Public Methods
}

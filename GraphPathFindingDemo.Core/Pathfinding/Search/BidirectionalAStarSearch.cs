using GraphPathFindingDemo.Core.Graph;

namespace GraphPathFindingDemo.Core.Pathfinding.Search;

/// <summary>
/// Searches a graph for the cheapest path by growing A* searches from both ends.
/// </summary>
/// <remarks>
/// <para>
/// Both halves leave the cell that promises the cheapest way across, counting what they have
/// already spent and what the heuristic says is left. They grow towards each other and meet in the
/// middle, which usually expands fewer cells than a search from one end.
/// </para>
/// <para>
/// The search stops once the most promising unsettled cell of one half, or the two cheapest
/// unsettled cells added together, can no longer beat the best meeting found. The path found is
/// therefore the cheapest one.
/// </para>
/// </remarks>
public sealed class BidirectionalAStarSearch : ISearchAlgorithm, IHeuristicSearch
{
    #region Instance Fields
    private readonly List<IHeuristic> _heuristics;
    private IHeuristic _heuristic;
    #endregion Instance Fields

    #region Properties
    /// <inheritdoc />
    public SearchAlgorithmStyle Style => SearchAlgorithmStyle.BidirectionalAStarSearch;

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
    /// Initializes a new instance of the <see cref="BidirectionalAStarSearch"/> class.
    /// </summary>
    /// <param name="heuristics">The heuristics the search can run with. The first one is the one
    /// it starts with.</param>
    /// <exception cref="ArgumentNullException"><paramref name="heuristics"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">No heuristic was given.</exception>
    public BidirectionalAStarSearch(params IHeuristic[] heuristics)
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
    /// Searches the graph from both ends for the cheapest path, reporting one
    /// <see cref="SearchStep"/> per event.
    /// </summary>
    /// <param name="graph">The graph to search.</param>
    /// <param name="start">The node one search starts from.</param>
    /// <param name="end">The node the other search starts from.</param>
    /// <returns>
    /// The sequence of search steps. The sequence ends as soon as the halves meet on the cheapest
    /// path, or when both frontiers empty without meeting.
    /// </returns>
    /// <exception cref="ArgumentNullException">Any of the arguments is <see langword="null"/>.</exception>
    public IEnumerable<SearchStep> Search(GridGraph graph, GridNode start, GridNode end)
    {
        ArgumentNullException.ThrowIfNull(graph);
        ArgumentNullException.ThrowIfNull(start);
        ArgumentNullException.ThrowIfNull(end);

        return BidirectionalSearchDriver.Search(graph, start, end, useWeights: true, _heuristic);
    }
    #endregion Public Methods
}

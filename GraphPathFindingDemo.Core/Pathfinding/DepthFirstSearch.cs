using GraphPathFindingDemo.Core.Graph;

namespace GraphPathFindingDemo.Core.Pathfinding;

/// <summary>
/// Searches a graph for a path using depth-first search.
/// </summary>
/// <remarks>
/// <para>
/// The search follows one branch as deep as it goes before it backtracks, so it finds a path
/// but not necessarily a shortest one. In a perfect maze there is only one path between the two
/// ends, so it finds the same path a breadth-first search finds. In a maze with loops the two
/// can differ, and the depth-first search often finds a longer one.
/// </para>
/// <para>
/// The search keeps its own stack of the nodes on the current branch instead of calling itself,
/// recursively, so that a large grid cannot overflow the call stack.
/// </para>
/// </remarks>
public sealed class DepthFirstSearch : ISearchAlgorithm
{
    #region Properties
    /// <inheritdoc />
    public SearchAlgorithmStyle Style => SearchAlgorithmStyle.DepthFirstSearch;
    #endregion Properties

    #region Public Methods
    /// <summary>
    /// Searches the graph depth first, reporting one <see cref="SearchStep"/> per event.
    /// </summary>
    /// <param name="graph">The graph to search.</param>
    /// <param name="start">The node the search starts from.</param>
    /// <param name="end">The node the search looks for.</param>
    /// <returns>
    /// The sequence of search steps. The sequence ends as soon as the end node is reached, or
    /// when the branch is exhausted without reaching it.
    /// </returns>
    /// <exception cref="ArgumentNullException">Any of the arguments is <see langword="null"/>.</exception>
    public IEnumerable<SearchStep> Search(GridGraph graph, GridNode start, GridNode end)
    {
        ArgumentNullException.ThrowIfNull(graph);
        ArgumentNullException.ThrowIfNull(start);
        ArgumentNullException.ThrowIfNull(end);

        var cameFrom = new Dictionary<GridNode, GridNode?>();
        var depths = new Dictionary<GridNode, int>();
        var branch = new Stack<Branch>();

        cameFrom[start] = null;
        depths[start] = 0;
        branch.Push(new Branch(start, graph.Neighbors(start)));

        yield return new SearchStep(start, SearchStepKind.Discovered, 0);
        yield return new SearchStep(start, SearchStepKind.Visited, 0);

        if (ReferenceEquals(start, end))
        {
            yield return new SearchStep(start, SearchStepKind.PathFound, 0, PathBuilder.Build(start, cameFrom));
            yield break;
        }

        while (branch.Count > 0)
        {
            var current = branch.Peek();

            if (!current.TryTakeNeighbour(out var neighbour))
            {
                // The branch is exhausted, so the search backtracks to the node before it.
                branch.Pop();
                continue;
            }

            if (cameFrom.ContainsKey(neighbour))
            {
                continue;
            }

            cameFrom[neighbour] = current.Node;
            var depth = depths[current.Node] + 1;
            depths[neighbour] = depth;

            yield return new SearchStep(neighbour, SearchStepKind.Discovered, depth);
            yield return new SearchStep(neighbour, SearchStepKind.Visited, depth);

            if (ReferenceEquals(neighbour, end))
            {
                yield return new SearchStep(
                    neighbour,
                    SearchStepKind.PathFound,
                    depth,
                    PathBuilder.Build(neighbour, cameFrom));

                yield break;
            }

            branch.Push(new Branch(neighbour, graph.Neighbors(neighbour)));
        }
    }
    #endregion Public Methods

    #region Internal Classes
    /// <summary>
    /// One node on the branch the search is following, together with the neighbours of that node
    /// that have not been looked at yet.
    /// </summary>
    private sealed class Branch
    {
        #region Instance Fields
        private readonly IReadOnlyList<GridNode> _neighbours;
        private int _next;
        #endregion Instance Fields

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="Branch"/> class.
        /// </summary>
        /// <param name="node">The node at this point of the branch.</param>
        /// <param name="neighbours">The neighbours of the node.</param>
        public Branch(GridNode node, IReadOnlyList<GridNode> neighbours)
        {
            Node = node;
            _neighbours = neighbours;
        }
        #endregion Constructor

        #region Properties
        /// <summary>
        /// Gets the node at this point of the branch.
        /// </summary>
        public GridNode Node { get; }
        #endregion Properties

        #region Public Methods
        /// <summary>
        /// Takes the next neighbour of the node that has not been looked at yet.
        /// </summary>
        /// <param name="neighbour">The neighbour that was taken.</param>
        /// <returns><see langword="true"/> when a neighbour was left to take.</returns>
        public bool TryTakeNeighbour(out GridNode neighbour)
        {
            if (_next >= _neighbours.Count)
            {
                neighbour = null!;
                return false;
            }

            neighbour = _neighbours[_next];
            _next++;
            return true;
        }
        #endregion Public Methods
    }
    #endregion Internal Classes
}

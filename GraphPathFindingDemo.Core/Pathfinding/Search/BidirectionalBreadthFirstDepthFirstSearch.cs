using GraphPathFindingDemo.Core.Graph;

namespace GraphPathFindingDemo.Core.Pathfinding.Search;

/// <summary>
/// Searches a graph for a path by growing a breadth-first search from one end and a depth-first
/// search from the other.
/// </summary>
/// <remarks>
/// <para>
/// The two halves are unweighted: every passage is a plain step, whatever the terrain says a walk
/// over it costs.
/// </para>
/// <para>
/// The search joins the halves as soon as one of them reaches a cell the other has already
/// reached, and returns the route through that cell. The breadth-first half supplies a shortest
/// prefix, but the depth-first half supplies whichever suffix it reached first, so in a layout
/// with loops the joined route need not be the shortest one. In a perfect maze there is only one
/// route, so the halves meet on it.
/// </para>
/// </remarks>
public sealed class BidirectionalBreadthFirstDepthFirstSearch : ISearchAlgorithm
{
    #region Constants
    /// <summary>
    /// The length that stands for the halves not having met yet.
    /// </summary>
    private const int NoMeeting = int.MaxValue;
    #endregion Constants

    #region Properties
    /// <inheritdoc />
    public SearchAlgorithmStyle Style => SearchAlgorithmStyle.BidirectionalBreadthFirstDepthFirstSearch;
    #endregion Properties

    #region Internal Classes
    /// <summary>
    /// One cell of the branch the depth-first half is following, together with the neighbours of
    /// that cell that have not been looked at yet.
    /// </summary>
    private sealed class Branch
    {
        private readonly IReadOnlyList<GridNode> _neighbours;
        private int _next;

        /// <summary>
        /// Gets the cell at this point of the branch.
        /// </summary>
        public GridNode Node { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Branch"/> class.
        /// </summary>
        /// <param name="node">The cell at this point of the branch.</param>
        /// <param name="neighbours">The neighbours of the cell.</param>
        public Branch(GridNode node, IReadOnlyList<GridNode> neighbours)
        {
            Node = node;
            _neighbours = neighbours;
        }

        /// <summary>
        /// Takes the next neighbour of the cell that has not been looked at yet.
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
    }
    #endregion Internal Classes

    #region Public Methods
    /// <summary>
    /// Searches the graph from both ends, reporting one <see cref="SearchStep"/> per event.
    /// </summary>
    /// <param name="graph">The graph to search.</param>
    /// <param name="start">The node the breadth-first half starts from.</param>
    /// <param name="end">The node the depth-first half starts from.</param>
    /// <returns>
    /// The sequence of search steps. The sequence ends as soon as the halves meet, or when both
    /// halves run out without meeting.
    /// </returns>
    /// <exception cref="ArgumentNullException">Any of the arguments is <see langword="null"/>.</exception>
    public IEnumerable<SearchStep> Search(GridGraph graph, GridNode start, GridNode end)
    {
        ArgumentNullException.ThrowIfNull(graph);
        ArgumentNullException.ThrowIfNull(start);
        ArgumentNullException.ThrowIfNull(end);

        if (ReferenceEquals(start, end))
        {
            yield return new SearchStep(start, SearchStepKind.Discovered, 0);
            yield return new SearchStep(start, SearchStepKind.Visited, 0);
            yield return new SearchStep(start, SearchStepKind.PathFound, 0, [start]);
            yield break;
        }

        var frontier = new Queue<GridNode>();
        var distancesFromStart = new Dictionary<GridNode, int>();
        var cameFromStart = new Dictionary<GridNode, GridNode?>();

        distancesFromStart[start] = 0;
        cameFromStart[start] = null;
        frontier.Enqueue(start);

        var branch = new Stack<Branch>();
        var depthsFromEnd = new Dictionary<GridNode, int>();
        var cameFromEnd = new Dictionary<GridNode, GridNode?>();

        depthsFromEnd[end] = 0;
        cameFromEnd[end] = null;
        branch.Push(new Branch(end, graph.Neighbors(end)));

        yield return new SearchStep(start, SearchStepKind.Discovered, 0);
        yield return new SearchStep(end, SearchStepKind.Discovered, 0);
        yield return new SearchStep(end, SearchStepKind.Visited, 0);

        var bestLength = NoMeeting;
        GridNode? meeting = null;
        var breadthFirstTurn = true;

        while (true)
        {
            // The halves have met, so the route through the shared cell is the answer.
            if (meeting is not null)
            {
                break;
            }

            var breadthFirstAvailable = frontier.Count > 0;
            var depthFirstAvailable = branch.Count > 0;

            if (!breadthFirstAvailable && !depthFirstAvailable)
            {
                yield break;
            }

            var runBreadthFirst = breadthFirstAvailable && (breadthFirstTurn || !depthFirstAvailable);

            if (runBreadthFirst)
            {
                var node = frontier.Dequeue();
                var distance = distancesFromStart[node];

                yield return new SearchStep(node, SearchStepKind.Visited, distance);

                foreach (var neighbour in graph.Neighbors(node))
                {
                    if (distancesFromStart.ContainsKey(neighbour))
                    {
                        continue;
                    }

                    var next = distance + 1;

                    distancesFromStart[neighbour] = next;
                    cameFromStart[neighbour] = node;
                    frontier.Enqueue(neighbour);

                    if (depthsFromEnd.TryGetValue(neighbour, out var endDepth) && next + endDepth < bestLength)
                    {
                        bestLength = next + endDepth;
                        meeting = neighbour;
                    }

                    yield return new SearchStep(neighbour, SearchStepKind.Discovered, next);
                }
            }
            else if (depthFirstAvailable
                && TryExpandDepthFirst(graph, branch, depthsFromEnd, cameFromEnd, out var node, out var depth))
            {
                if (distancesFromStart.TryGetValue(node, out var startDistance)
                    && startDistance + depth < bestLength)
                {
                    bestLength = startDistance + depth;
                    meeting = node;
                }

                yield return new SearchStep(node, SearchStepKind.Discovered, depth);
                yield return new SearchStep(node, SearchStepKind.Visited, depth);
            }

            breadthFirstTurn = !breadthFirstTurn;
        }

        yield return new SearchStep(
            meeting!,
            SearchStepKind.PathFound,
            bestLength,
            BuildPath(meeting!, cameFromStart, cameFromEnd));
    }
    #endregion Public Methods

    #region Private Methods
    /// <summary>
    /// Advances the depth-first half by one cell, backtracking as far as it must to find a
    /// neighbour that has not been reached yet.
    /// </summary>
    /// <param name="graph">The graph being searched.</param>
    /// <param name="branch">The branch the half is following.</param>
    /// <param name="depths">The depth of every cell the half has reached.</param>
    /// <param name="cameFrom">The link from every reached cell to the one it was reached from.</param>
    /// <param name="node">When this method returns, the cell that was reached.</param>
    /// <param name="depth">When this method returns, the depth of that cell.</param>
    /// <returns><see langword="true"/> when an unreached neighbour was found.</returns>
    private static bool TryExpandDepthFirst(
        GridGraph graph,
        Stack<Branch> branch,
        Dictionary<GridNode, int> depths,
        Dictionary<GridNode, GridNode?> cameFrom,
        out GridNode node,
        out int depth)
    {
        while (branch.Count > 0)
        {
            var current = branch.Peek();

            if (!current.TryTakeNeighbour(out var neighbour))
            {
                // The branch is exhausted, so the half backtracks to the cell before it.
                branch.Pop();
                continue;
            }

            if (cameFrom.ContainsKey(neighbour))
            {
                continue;
            }

            cameFrom[neighbour] = current.Node;
            depth = depths[current.Node] + 1;
            depths[neighbour] = depth;
            branch.Push(new Branch(neighbour, graph.Neighbors(neighbour)));

            node = neighbour;
            return true;
        }

        node = null!;
        depth = 0;
        return false;
    }

    /// <summary>
    /// Joins the two halves of the path at the cell they met at.
    /// </summary>
    /// <param name="meeting">The cell both halves reached.</param>
    /// <param name="fromStart">The links from the breadth-first half, rooted at the start.</param>
    /// <param name="fromEnd">The links from the depth-first half, rooted at the end.</param>
    /// <returns>The path from the start to the end.</returns>
    private static IReadOnlyList<GridNode> BuildPath(
        GridNode meeting,
        Dictionary<GridNode, GridNode?> fromStart,
        Dictionary<GridNode, GridNode?> fromEnd)
    {
        var path = new List<GridNode>();
        GridNode? current = meeting;

        while (current is not null)
        {
            path.Add(current);
            current = fromStart[current];
        }

        path.Reverse();

        // The depth-first half's links run towards the end, so they are added in order and the
        // meeting cell is already the last cell of the first part.
        current = fromEnd[meeting];

        while (current is not null)
        {
            path.Add(current);
            current = fromEnd[current];
        }

        return path;
    }
    #endregion Private Methods
}

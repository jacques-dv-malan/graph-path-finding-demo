using GraphPathFindingDemo.Core.Graph;

namespace GraphPathFindingDemo.Core.Pathfinding.Search;

/// <summary>
/// Runs two searches towards each other and reports them as a single sequence of steps.
/// </summary>
/// <remarks>
/// <para>
/// One half grows from the start and the other from the end. Each half keeps its own frontier and
/// the search expands whichever half has the cheaper unsettled cell. When a cell is reached by
/// both halves, the two ways to it make a complete path, and the search keeps the cheapest such
/// path it has seen.
/// </para>
/// <para>
/// The search stops once either half can no longer promise a cheaper path than the best meeting
/// found: when the cheapest promise on one front reaches that cost, or when the two cheapest
/// unsettled costs, added together, do so. The first rule ends the search once a guided half has
/// run out of promising cells; the second keeps the answer correct when neither half is guided.
/// </para>
/// </remarks>
internal static class BidirectionalSearchDriver
{
    #region Constants
    /// <summary>
    /// The cost that stands for a half with no cell left to expand.
    /// </summary>
    private const double Unreachable = double.PositiveInfinity;
    #endregion Constants

    #region Internal Classes
    /// <summary>
    /// One half of a bidirectional search: a frontier growing from one endpoint towards the
    /// other.
    /// </summary>
    private sealed class Side
    {
        private readonly Dictionary<GridNode, double> _costs = [];
        private readonly Dictionary<GridNode, GridNode?> _cameFrom = [];
        private readonly HashSet<GridNode> _settled = [];
        private readonly PriorityQueue<(GridNode Node, double Cost), double> _queue = new();
        private readonly PriorityQueue<GridNode, double> _bounds = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="Side"/> class.
        /// </summary>
        /// <param name="origin">The endpoint the half grows from.</param>
        /// <param name="target">The endpoint the half grows towards.</param>
        public Side(GridNode origin, GridNode target)
        {
            Origin = origin;
            Target = target;
        }

        /// <summary>
        /// Gets the endpoint this half grows from.
        /// </summary>
        public GridNode Origin { get; }

        /// <summary>
        /// Gets the endpoint this half grows towards.
        /// </summary>
        public GridNode Target { get; }

        /// <summary>
        /// Gets the cheapest cost of a cell this half has not expanded yet, or
        /// <see cref="Unreachable"/> when no such cell is left.
        /// </summary>
        public double Bound
        {
            get
            {
                while (_bounds.TryPeek(out var node, out var cost))
                {
                    if (_settled.Contains(node)
                        || !_costs.TryGetValue(node, out var current)
                        || current != cost)
                    {
                        _bounds.Dequeue();
                        continue;
                    }

                    return cost;
                }

                return Unreachable;
            }
        }

        /// <summary>
        /// Gets the smallest promise of a cell this half has not expanded yet, or
        /// <see cref="Unreachable"/> when no such cell is left.
        /// </summary>
        /// <remarks>
        /// The promise is the cost of a cell plus the estimate of what is left, so it is the value
        /// the frontier is ordered by. An unguided half has no estimate, so its promise is the
        /// cost itself.
        /// </remarks>
        public double Top
        {
            get
            {
                while (_queue.TryPeek(out var entry, out var priority))
                {
                    if (_settled.Contains(entry.Node)
                        || !_costs.TryGetValue(entry.Node, out var current)
                        || current != entry.Cost)
                    {
                        _queue.Dequeue();
                        continue;
                    }

                    return priority;
                }

                return Unreachable;
            }
        }

        /// <summary>
        /// Returns what this half knows a cell to cost.
        /// </summary>
        /// <param name="node">The cell to look up.</param>
        /// <param name="cost">When this method returns, the cost of the cell.</param>
        /// <returns><see langword="true"/> when the half has reached the cell.</returns>
        public bool TryGetCost(GridNode node, out double cost)
        {
            return _costs.TryGetValue(node, out cost);
        }

        /// <summary>
        /// Records a cell this half has reached, or reached more cheaply, and queues it for
        /// expansion.
        /// </summary>
        /// <param name="node">The cell that was reached.</param>
        /// <param name="from">The cell it was reached from, or <see langword="null"/> for the
        /// origin.</param>
        /// <param name="cost">The cost of the cell.</param>
        /// <param name="priority">The value the frontier is ordered by.</param>
        /// <remarks>
        /// A cell that was expanded before can be reached more cheaply later when the frontier is
        /// ordered by something other than the cost. It is put back on the frontier then, so the
        /// search never closes a cell before its cost is final.
        /// </remarks>
        public void Discover(GridNode node, GridNode? from, double cost, double priority)
        {
            _costs[node] = cost;
            _cameFrom[node] = from;

            _settled.Remove(node);

            _queue.Enqueue((node, cost), priority);
            _bounds.Enqueue(node, cost);
        }

        /// <summary>
        /// Takes the next cell to expand from the frontier.
        /// </summary>
        /// <param name="node">When this method returns, the cell to expand.</param>
        /// <param name="cost">When this method returns, the cost of the cell.</param>
        /// <returns><see langword="true"/> when the frontier held a cell to expand.</returns>
        public bool TryTake(out GridNode node, out double cost)
        {
            while (_queue.TryDequeue(out var entry, out _))
            {
                if (_settled.Contains(entry.Node))
                {
                    continue;
                }

                if (!_costs.TryGetValue(entry.Node, out var current) || current != entry.Cost)
                {
                    continue;
                }

                _settled.Add(entry.Node);

                node = entry.Node;
                cost = entry.Cost;
                return true;
            }

            node = null!;
            cost = 0;
            return false;
        }

        /// <summary>
        /// Rebuilds the part of the path that runs from this half's origin to the given cell.
        /// </summary>
        /// <param name="node">The cell the path ends at.</param>
        /// <returns>The cells from the origin to <paramref name="node"/>, in order.</returns>
        public IReadOnlyList<GridNode> BuildFromOrigin(GridNode node)
        {
            var path = new List<GridNode>();
            GridNode? current = node;

            while (current is not null)
            {
                path.Add(current);
                current = _cameFrom[current];
            }

            path.Reverse();
            return path;
        }
    }
    #endregion Internal Classes

    #region Public Methods
    /// <summary>
    /// Searches the graph from both ends, reporting one <see cref="SearchStep"/> per event.
    /// </summary>
    /// <param name="graph">The graph to search.</param>
    /// <param name="start">The node one half grows from.</param>
    /// <param name="end">The node the other half grows from.</param>
    /// <param name="useWeights">Whether a passage costs what the terrain says, rather than a
    /// plain step.</param>
    /// <param name="heuristic">The estimate that orders the frontier, or <see langword="null"/>
    /// when the halves are not guided.</param>
    /// <returns>
    /// The sequence of search steps. The sequence ends once no path can beat the cheapest meeting
    /// found, or when both frontiers empty without the halves meeting.
    /// </returns>
    internal static IEnumerable<SearchStep> Search(
        GridGraph graph,
        GridNode start,
        GridNode end,
        bool useWeights,
        IHeuristic? heuristic)
    {
        if (ReferenceEquals(start, end))
        {
            yield return new SearchStep(start, SearchStepKind.Discovered, 0);
            yield return new SearchStep(start, SearchStepKind.Visited, 0);
            yield return new SearchStep(start, SearchStepKind.PathFound, 0, [start]);
            yield break;
        }

        var forward = new Side(start, end);
        var backward = new Side(end, start);

        forward.Discover(start, null, 0, Estimate(graph, heuristic, start, end));
        backward.Discover(end, null, 0, Estimate(graph, heuristic, end, start));

        yield return new SearchStep(start, SearchStepKind.Discovered, 0);
        yield return new SearchStep(end, SearchStepKind.Discovered, 0);

        var bestCost = Unreachable;
        GridNode? meeting = null;

        while (true)
        {
            var forwardBound = forward.Bound;
            var backwardBound = backward.Bound;
            var forwardTop = forward.Top;
            var backwardTop = backward.Top;

            if (forwardBound == Unreachable && backwardBound == Unreachable)
            {
                yield break;
            }

            // One half has explored everything it can reach without meeting the other, so the
            // grid holds no path between the endpoints.
            if (bestCost == Unreachable && (forwardBound == Unreachable || backwardBound == Unreachable))
            {
                yield break;
            }

            if (bestCost != Unreachable
                && (Math.Max(forwardTop, backwardTop) >= bestCost
                    || forwardBound + backwardBound >= bestCost))
            {
                break;
            }

            var forwardFirst = forwardTop <= backwardTop;
            var side = forwardFirst ? forward : backward;
            var other = forwardFirst ? backward : forward;

            if (!side.TryTake(out var node, out var cost))
            {
                continue;
            }

            yield return new SearchStep(node, SearchStepKind.Visited, cost);

            if (other.TryGetCost(node, out var otherCost) && cost + otherCost < bestCost)
            {
                bestCost = cost + otherCost;
                meeting = node;
            }

            foreach (var neighbour in graph.Neighbors(node))
            {
                var step = useWeights ? graph.WeightBetween(node, neighbour) : 1.0;
                var next = cost + step;

                if (side.TryGetCost(neighbour, out var known) && next >= known)
                {
                    continue;
                }

                var priority = next + Estimate(graph, heuristic, neighbour, side.Target);

                side.Discover(neighbour, node, next, priority);

                if (other.TryGetCost(neighbour, out otherCost) && next + otherCost < bestCost)
                {
                    bestCost = next + otherCost;
                    meeting = neighbour;
                }

                yield return new SearchStep(neighbour, SearchStepKind.Discovered, next);
            }
        }

        yield return new SearchStep(
            meeting!,
            SearchStepKind.PathFound,
            bestCost,
            BuildPath(meeting!, forward, backward));
    }
    #endregion Public Methods

    #region Private Methods
    /// <summary>
    /// Returns the estimate that orders the frontier of one half.
    /// </summary>
    /// <param name="graph">The graph being searched.</param>
    /// <param name="heuristic">The estimate, or <see langword="null"/> when the half is not
    /// guided.</param>
    /// <param name="from">The cell the estimate is made for.</param>
    /// <param name="to">The endpoint the half grows towards.</param>
    /// <returns>The estimate, or <c>0</c> when the half is not guided.</returns>
    private static double Estimate(GridGraph graph, IHeuristic? heuristic, GridNode from, GridNode to)
    {
        return heuristic is null ? 0.0 : heuristic.Estimate(graph, from, to);
    }

    /// <summary>
    /// Joins the two halves of the path at the cell they met at.
    /// </summary>
    /// <param name="meeting">The cell both halves reached.</param>
    /// <param name="forward">The half that grew from the start.</param>
    /// <param name="backward">The half that grew from the end.</param>
    /// <returns>The path from the start to the end.</returns>
    private static IReadOnlyList<GridNode> BuildPath(GridNode meeting, Side forward, Side backward)
    {
        var path = new List<GridNode>(forward.BuildFromOrigin(meeting));
        var fromEnd = backward.BuildFromOrigin(meeting);

        // The half that grew from the end knows its part backwards, and the meeting cell is
        // already the last cell of the first part.
        for (var index = fromEnd.Count - 2; index >= 0; index--)
        {
            path.Add(fromEnd[index]);
        }

        return path;
    }
    #endregion Private Methods
}

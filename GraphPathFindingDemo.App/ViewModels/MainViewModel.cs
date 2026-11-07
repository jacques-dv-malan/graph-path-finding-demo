using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GraphPathFindingDemo.Core.Factories;
using GraphPathFindingDemo.Core.Graph;
using GraphPathFindingDemo.Core.Layout;
using GraphPathFindingDemo.Core.Pathfinding;
using System.Diagnostics;
using System.Windows.Media;

namespace GraphPathFindingDemo.App.ViewModels;

/// <summary>
/// Drives the demo: builds the scenario, steps the search, and exposes the state the window and
/// the canvas bind to.
/// </summary>
/// <remarks>
/// The search runs on the UI thread. A grid holds at most a few thousand nodes, so a whole
/// search costs well under a millisecond and no background task is needed.
/// </remarks>
public partial class MainViewModel : ObservableObject
{
    #region Constants
    /// <summary>
    /// The width a new grid starts at.
    /// </summary>
    public const int DefaultWidth = 25;

    /// <summary>
    /// The height a new grid starts at.
    /// </summary>
    public const int DefaultHeight = 25;

    /// <summary>
    /// The number of milliseconds between two moves a new run starts at.
    /// </summary>
    public const int DefaultStepIntervalMilliseconds = 200;

    /// <summary>
    /// The smallest number of milliseconds between two moves.
    /// </summary>
    public const int MinStepIntervalMilliseconds = 0;

    /// <summary>
    /// The largest number of milliseconds between two moves.
    /// </summary>
    public const int MaxStepIntervalMilliseconds = 1000;

    /// <summary>
    /// The status shown while no search is running.
    /// </summary>
    public const string StatusIdle = "idle";

    /// <summary>
    /// The status shown while the search is running.
    /// </summary>
    public const string StatusRunning = "running";

    /// <summary>
    /// The status shown when the search found a path.
    /// </summary>
    public const string StatusDone = "done";

    /// <summary>
    /// The status shown when the search ended without finding a path.
    /// </summary>
    public const string StatusNoPath = "no path";
    #endregion Constants

    #region Instance Fields
    private readonly IGridFactory _factory;
    private readonly IReadOnlyDictionary<SearchAlgorithmStyle, ISearchAlgorithm> _algorithms;

    private IHeuristicSearch? _heuristicSearch;

    private GridPoint? _startPoint;
    private GridPoint? _endPoint;

    private readonly HashSet<GridNode> _discovered = [];
    private readonly List<GridNode> _visited = [];
    private readonly List<GridNode> _path = [];
    private readonly List<LayoutOption> _layoutOptions = [];
    private readonly List<SearchAlgorithmOption> _algorithmOptions = [];

    private GridScenario _scenario = null!;
    private IEnumerator<SearchStep>? _search;
    private SearchStep? _lookahead;
    private GridNode? _visitedLeaf;

    private long _lastMoveTimestamp;
    private bool _isRunning;
    #endregion Instance Fields

    #region Properties
    /// <summary>
    /// Gets or sets the number of columns of the grid. Values outside the supported range are
    /// clamped, and any change rebuilds the scenario.
    /// </summary>
    [ObservableProperty]
    private int _width = DefaultWidth;

    /// <summary>
    /// Gets or sets the number of rows of the grid. Values outside the supported range are
    /// clamped, and any change rebuilds the scenario.
    /// </summary>
    [ObservableProperty]
    private int _height = DefaultHeight;

    /// <summary>
    /// Gets or sets the seed of the layout. Any change rebuilds the scenario.
    /// </summary>
    [ObservableProperty]
    private int _seed = CurrentEpochSeconds();

    /// <summary>
    /// Gets or sets the style of the layout. Any change rebuilds the scenario.
    /// </summary>
    [ObservableProperty]
    private LayoutStyle _selectedLayout = LayoutStyle.RecursiveDivision;

    /// <summary>
    /// Gets the layouts the user can pick from.
    /// </summary>
    public IReadOnlyList<LayoutOption> LayoutOptions => _layoutOptions;

    /// <summary>
    /// Gets the algorithms the user can pick from.
    /// </summary>
    public IReadOnlyList<SearchAlgorithmOption> AlgorithmOptions => _algorithmOptions;

    /// <summary>
    /// Gets where the start of the search stands.
    /// </summary>
    public string StartPosition => $"({_scenario.Start.Row}, {_scenario.Start.Column})";

    /// <summary>
    /// Gets where the end of the search stands.
    /// </summary>
    public string EndPosition => $"({_scenario.End.Row}, {_scenario.End.Column})";

    /// <summary>
    /// Gets the heuristics the chosen search can be tuned with. It is empty for the searches that
    /// have no heuristic.
    /// </summary>
    public IReadOnlyList<IHeuristic> HeuristicOptions => _heuristicSearch?.Heuristics ?? [];

    /// <summary>
    /// Gets whether the chosen search has a heuristic to pick.
    /// </summary>
    [ObservableProperty]
    private bool _hasHeuristics;

    /// <summary>
    /// Gets whether the play button shows.
    /// </summary>
    [ObservableProperty]
    private bool _isPlayVisible = true;

    /// <summary>
    /// Gets whether the pause button shows.
    /// </summary>
    [ObservableProperty]
    private bool _isPauseVisible;

    /// <summary>
    /// Gets or sets the heuristic the search runs with. Any change restarts the search.
    /// </summary>
    [ObservableProperty]
    private IHeuristic? _selectedHeuristic;

    /// <summary>
    /// Gets or sets the algorithm the search runs with. Any change resets the search.
    /// </summary>
    [ObservableProperty]
    private SearchAlgorithmStyle _selectedAlgorithm = SearchAlgorithmStyle.BreadthFirstSearch;

    /// <summary>
    /// Gets or sets the number of milliseconds between two moves. Values outside the supported
    /// range are clamped.
    /// </summary>
    [ObservableProperty]
    private int _stepIntervalMilliseconds = DefaultStepIntervalMilliseconds;

    /// <summary>
    /// Gets the snapshot the canvas draws. A new instance is published on every update.
    /// </summary>
    [ObservableProperty]
    private RenderState _renderState = null!;

    /// <summary>
    /// Gets the number of nodes that have been expanded.
    /// </summary>
    [ObservableProperty]
    private int _visitedCount;

    /// <summary>
    /// Gets the number of nodes waiting on the frontier.
    /// </summary>
    [ObservableProperty]
    private int _frontierCount;

    /// <summary>
    /// Gets the length of the path found so far, counted in segments.
    /// </summary>
    [ObservableProperty]
    private int _pathLength;

    /// <summary>
    /// Gets what the path found so far costs to walk, counted over the passages it runs through.
    /// </summary>
    [ObservableProperty]
    private int _pathCost;

    /// <summary>
    /// Gets the number of nodes in the current grid.
    /// </summary>
    [ObservableProperty]
    private int _nodeCount;

    /// <summary>
    /// Gets the number of passages in the current grid.
    /// </summary>
    [ObservableProperty]
    private int _edgeCount;

    /// <summary>
    /// Gets the current status of the search.
    /// </summary>
    [ObservableProperty]
    private string _status = StatusIdle;

    /// <summary>
    /// Gets whether the search can be started.
    /// </summary>
    [ObservableProperty]
    private bool _canPlay;

    /// <summary>
    /// Gets whether the search can be paused.
    /// </summary>
    [ObservableProperty]
    private bool _canPause;

    /// <summary>
    /// Gets whether the search can be advanced by a single step.
    /// </summary>
    [ObservableProperty]
    private bool _canStep;

    /// <summary>
    /// Gets whether the grid and its seed can be changed. They cannot be changed while the search
    /// runs.
    /// </summary>
    [ObservableProperty]
    private bool _canEditGrid;
    #endregion Properties

    #region Constructor
    /// <summary>
    /// Initializes a new instance of the <see cref="MainViewModel"/> class and builds the first
    /// scenario, so the window is never empty.
    /// </summary>
    /// <param name="factory">The factory producing the scenarios.</param>
    /// <param name="algorithms">The algorithms the user can pick from. Every algorithm must offer
    /// a different style.</param>
    /// <exception cref="ArgumentNullException">Any of the arguments is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">No algorithm was given.</exception>
    public MainViewModel(IGridFactory factory, params ISearchAlgorithm[] algorithms)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(algorithms);

        if (algorithms.Length == 0)
        {
            throw new ArgumentException("At least one search algorithm is required.", nameof(algorithms));
        }

        _factory = factory;
        _algorithms = algorithms.ToDictionary(algorithm => algorithm.Style);

        foreach (var style in _factory.AvailableStyles)
        {
            _layoutOptions.Add(LayoutOption.For(style));
        }

        foreach (var algorithm in algorithms)
        {
            _algorithmOptions.Add(SearchAlgorithmOption.For(algorithm.Style));
        }

        CompositionTarget.Rendering += OnRendering;

        RebuildScenario();

        // The search starts with the first algorithm that was given.
        SelectedAlgorithm = algorithms[0].Style;
    }
    #endregion Constructor

    #region Public Methods
    /// <summary>
    /// Moves the given endpoint to the given cell and starts the search again.
    /// </summary>
    /// <param name="point">The cell the endpoint moves to.</param>
    /// <param name="target">The endpoint the click moves.</param>
    public void MoveEndpoint(GridPoint point, EndpointTarget target)
    {
        if (!CanEditGrid)
        {
            return;
        }

        var other = target == EndpointTarget.Start ? _scenario.End : _scenario.Start;

        // The two endpoints may never stand on the same cell.
        if (other.Position == point)
        {
            return;
        }

        if (target == EndpointTarget.Start)
        {
            _startPoint = point;
        }
        else
        {
            _endPoint = point;
        }

        _scenario = PlaceEndpoints(_scenario);

        ResetSearch();
    }

    /// <summary>
    /// Stops the animation. Called when the window closes.
    /// </summary>
    public void Stop()
    {
        CompositionTarget.Rendering -= OnRendering;
    }
    #endregion Public Methods

    #region Private Methods
    /// <summary>
    /// Rebuilds the scenario when the width changes, clamping the value first.
    /// </summary>
    /// <param name="value">The requested width.</param>
    partial void OnWidthChanged(int value)
    {
        var clamped = Math.Clamp(value, GridFactory.MinSize, GridFactory.MaxSize);

        if (clamped != value)
        {
            Width = clamped;
            return;
        }

        RebuildScenario();
    }

    /// <summary>
    /// Rebuilds the scenario when the height changes, clamping the value first.
    /// </summary>
    /// <param name="value">The requested height.</param>
    partial void OnHeightChanged(int value)
    {
        var clamped = Math.Clamp(value, GridFactory.MinSize, GridFactory.MaxSize);

        if (clamped != value)
        {
            Height = clamped;
            return;
        }

        RebuildScenario();
    }

    /// <summary>
    /// Rebuilds the scenario when the seed changes.
    /// </summary>
    /// <param name="value">The requested seed.</param>
    partial void OnSeedChanged(int value)
    {
        RebuildScenario();
    }

    /// <summary>
    /// Rebuilds the scenario when the layout changes.
    /// </summary>
    /// <param name="value">The requested layout.</param>
    partial void OnSelectedLayoutChanged(LayoutStyle value)
    {
        RebuildScenario();
    }

    /// <summary>
    /// Restarts the search when the algorithm changes, and offers the heuristics of the new
    /// search.
    /// </summary>
    /// <param name="value">The requested algorithm.</param>
    partial void OnSelectedAlgorithmChanged(SearchAlgorithmStyle value)
    {
        _heuristicSearch = _algorithms[value] as IHeuristicSearch;

        HasHeuristics = _heuristicSearch is not null;
        OnPropertyChanged(nameof(HeuristicOptions));

        SelectedHeuristic = _heuristicSearch?.Heuristic;

        ResetSearch();
    }

    /// <summary>
    /// Hands the heuristic to the search and restarts it.
    /// </summary>
    /// <param name="value">The requested heuristic.</param>
    partial void OnSelectedHeuristicChanged(IHeuristic? value)
    {
        if (_heuristicSearch is null || value is null)
        {
            return;
        }

        if (ReferenceEquals(_heuristicSearch.Heuristic, value))
        {
            return;
        }

        _heuristicSearch.Heuristic = value;

        ResetSearch();
    }

    /// <summary>
    /// Clamps the number of milliseconds between two moves when it changes. The running animation
    /// reads the interval as it goes, so a change takes effect at once.
    /// </summary>
    /// <param name="value">The requested number of milliseconds between two moves.</param>
    partial void OnStepIntervalMillisecondsChanged(int value)
    {
        var clamped = Math.Clamp(value, MinStepIntervalMilliseconds, MaxStepIntervalMilliseconds);

        if (clamped != value)
        {
            StepIntervalMilliseconds = clamped;
        }
    }

    /// <summary>
    /// Builds a new grid from a new seed, which is derived from the current one.
    /// </summary>
    [RelayCommand]
    private void NewGrid()
    {
        Seed = new Random(Seed).Next();
    }

    /// <summary>
    /// Starts the animation.
    /// </summary>
    [RelayCommand]
    private void Play()
    {
        if (_search is null)
        {
            return;
        }

        _isRunning = true;
        _lastMoveTimestamp = Stopwatch.GetTimestamp();

        PublishRenderState();

        Status = StatusRunning;
        UpdateCommandStates();
    }

    /// <summary>
    /// Stops the animation, leaving the search where it is.
    /// </summary>
    [RelayCommand]
    private void Pause()
    {
        if (!_isRunning)
        {
            return;
        }

        _isRunning = false;
        Status = StatusIdle;
        UpdateCommandStates();
    }

    /// <summary>
    /// Advances the search by exactly one move.
    /// </summary>
    [RelayCommand]
    private void Step()
    {
        if (_search is null)
        {
            return;
        }

        _isRunning = false;
        Status = StatusIdle;

        Advance();
        UpdateCommandStates();
    }

    /// <summary>
    /// Returns the current grid to its pre-search state so the run can be replayed.
    /// </summary>
    [RelayCommand]
    private void Reset()
    {
        ResetSearch();
    }

    /// <summary>
    /// Returns the current time as the number of seconds since the Unix epoch, which is the seed a
    /// new run starts at.
    /// </summary>
    /// <returns>The seed for a new run.</returns>
    private static int CurrentEpochSeconds()
    {
        return (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    /// <summary>
    /// Creates a new scenario for the current size and seed.
    /// </summary>
    private void RebuildScenario()
    {
        _scenario = PlaceEndpoints(_factory.Create(Width, Height, Seed, SelectedLayout));
        NodeCount = _scenario.Graph.Nodes.Count;
        EdgeCount = _scenario.Graph.Edges.Count;
        ResetSearch();
    }

    /// <summary>
    /// Puts the endpoints of a scenario where the user has left them, keeping them inside the
    /// grid and apart from each other.
    /// </summary>
    /// <param name="scenario">The scenario to place the endpoints in.</param>
    /// <returns>The scenario with the endpoints in place.</returns>
    private GridScenario PlaceEndpoints(GridScenario scenario)
    {
        var graph = scenario.Graph;

        var startWanted = _startPoint ?? scenario.Start.Position;
        var endWanted = _endPoint ?? scenario.End.Position;

        var startFits = IsInside(graph, startWanted);
        var endFits = IsInside(graph, endWanted);
        var clash = startFits && endFits && startWanted == endWanted;

        GridNode? start = null;
        GridNode? end = null;

        if (startFits && !clash)
        {
            start = graph.NodeAt(startWanted);
        }

        if (endFits && !clash)
        {
            end = graph.NodeAt(endWanted);
        }

        // An endpoint that could not keep its cell takes the bottom-right most cell that is free.
        start ??= BottomRightMostFree(graph, end);
        end ??= BottomRightMostFree(graph, start);

        return scenario with { Start = start, End = end };
    }

    /// <summary>
    /// Returns the bottom-right most cell of the grid that is not taken by the other endpoint.
    /// </summary>
    /// <param name="graph">The grid to search.</param>
    /// <param name="taken">The cell the other endpoint stands on, if it has one.</param>
    /// <returns>The free cell.</returns>
    /// <exception cref="InvalidOperationException">The grid holds no free cell.</exception>
    private static GridNode BottomRightMostFree(GridGraph graph, GridNode? taken)
    {
        for (var row = graph.Height - 1; row >= 0; row--)
        {
            for (var column = graph.Width - 1; column >= 0; column--)
            {
                var node = graph.NodeAt(new GridPoint(row, column));

                if (taken is null || !ReferenceEquals(node, taken))
                {
                    return node;
                }
            }
        }

        throw new InvalidOperationException("The grid holds no free cell.");
    }

    /// <summary>
    /// Determines whether a cell lies inside the grid.
    /// </summary>
    /// <param name="graph">The grid the cell has to fit in.</param>
    /// <param name="point">The cell to place.</param>
    /// <returns><see langword="true"/> when the cell lies inside the grid.</returns>
    private static bool IsInside(GridGraph graph, GridPoint point)
    {
        return point.Row >= 0
            && point.Row < graph.Height
            && point.Column >= 0
            && point.Column < graph.Width;
    }

    /// <summary>
    /// Clears the progress of the current scenario and starts a new search over it.
    /// </summary>
    private void ResetSearch()
    {
        _isRunning = false;
        DisposeSearch();

        _discovered.Clear();
        _visited.Clear();
        _path.Clear();
        _visitedLeaf = null;

        _search = _algorithms[SelectedAlgorithm].Search(_scenario.Graph, _scenario.Start, _scenario.End).GetEnumerator();

        Status = StatusIdle;
        VisitedCount = 0;
        FrontierCount = 0;
        PathLength = 0;
        PathCost = 0;

        OnPropertyChanged(nameof(StartPosition));
        OnPropertyChanged(nameof(EndPosition));

        PublishRenderState();
        UpdateCommandStates();
    }

    /// <summary>
    /// Applies the next move of the search and publishes the new state.
    /// </summary>
    /// <remarks>
    /// A move ends just before the next visit, so a visit and the neighbours it reveals share one
    /// interval. The start cell belongs to the first move, and a visit that reveals no neighbour
    /// stands alone.
    /// </remarks>
    private void Advance()
    {
        if (_search is null)
        {
            return;
        }

        var visitedInMove = false;

        while (true)
        {
            if (!TryTakeStep(out var step))
            {
                PublishRenderState();
                Complete(pathFound: false);
                return;
            }

            ApplyStep(step);

            if (step.Kind == SearchStepKind.PathFound)
            {
                PublishRenderState();
                Complete(pathFound: true);
                return;
            }

            if (step.Kind == SearchStepKind.Visited)
            {
                visitedInMove = true;
            }

            var hasNext = TryPeekStep(out var next);
            var nextIsVisit = !hasNext || next.Kind == SearchStepKind.Visited;

            // A dead end has no neighbour to hand the highlight on, so it is released at once.
            if (step.Kind == SearchStepKind.Visited && nextIsVisit && _visitedLeaf is not null)
            {
                _discovered.Remove(_visitedLeaf);
                _visitedLeaf = null;
            }

            // The move runs up to the next visit. The first move stays open until the start cell
            // has been visited.
            if (nextIsVisit && visitedInMove)
            {
                break;
            }
        }

        PublishRenderState();
        UpdateCounters();
    }

    /// <summary>
    /// Looks at the next step of the search without consuming it.
    /// </summary>
    /// <param name="step">The step that comes next.</param>
    /// <returns><see langword="true"/> when a step is available.</returns>
    private bool TryPeekStep(out SearchStep step)
    {
        if (_lookahead is null)
        {
            if (_search is null || !_search.MoveNext())
            {
                step = default;
                return false;
            }

            _lookahead = _search.Current;
        }

        step = _lookahead.Value;
        return true;
    }

    /// <summary>
    /// Takes the next step of the search, consuming it.
    /// </summary>
    /// <param name="step">The step that was taken.</param>
    /// <returns><see langword="true"/> when a step was available.</returns>
    private bool TryTakeStep(out SearchStep step)
    {
        if (!TryPeekStep(out step))
        {
            return false;
        }

        _lookahead = null;
        return true;
    }

    /// <summary>
    /// Applies a single search event to the frontier, the visited set and the path.
    /// </summary>
    /// <param name="step">The event to apply.</param>
    private void ApplyStep(SearchStep step)
    {
        switch (step.Kind)
        {
            case SearchStepKind.Discovered:
                // The previously visited leaf stays highlighted until its replacement is found.
                if (_visitedLeaf is not null)
                {
                    _discovered.Remove(_visitedLeaf);
                    _visitedLeaf = null;
                }

                _discovered.Add(step.Node);
                break;

            case SearchStepKind.Visited:
                // The cell this one replaces is released. This one stays highlighted until one of
                // its neighbours is discovered, or until the move finds it to be a dead end.
                if (_visitedLeaf is not null)
                {
                    _discovered.Remove(_visitedLeaf);
                }

                _visited.Add(step.Node);
                _visitedLeaf = step.Node;
                break;

            case SearchStepKind.PathFound:
                _path.Clear();

                if (step.Path is not null)
                {
                    _path.AddRange(step.Path);
                }

                PathCost = MeasureCost(step.Path);
                break;
        }
    }

    /// <summary>
    /// Works out what a path costs to walk, by adding up what its passages cost.
    /// </summary>
    /// <param name="path">The path to measure.</param>
    /// <returns>The cost of the path.</returns>
    private int MeasureCost(IReadOnlyList<GridNode>? path)
    {
        if (path is null || path.Count < 2)
        {
            return 0;
        }

        var cost = 0;

        for (var index = 0; index + 1 < path.Count; index++)
        {
            cost += _scenario.Graph.WeightBetween(path[index], path[index + 1]);
        }

        return cost;
    }

    /// <summary>
    /// Ends the search and reports the final status.
    /// </summary>
    /// <param name="pathFound">Whether the search found a path.</param>
    private void Complete(bool pathFound)
    {
        _isRunning = false;
        DisposeSearch();

        Status = pathFound ? StatusDone : StatusNoPath;
        UpdateCounters();
        UpdateCommandStates();
    }

    /// <summary>
    /// Discards the current search, which aborts the run.
    /// </summary>
    private void DisposeSearch()
    {
        (_search as IDisposable)?.Dispose();
        _search = null;
        _lookahead = null;
    }

    /// <summary>
    /// Refreshes the counters shown in the window.
    /// </summary>
    private void UpdateCounters()
    {
        VisitedCount = _visited.Count;
        FrontierCount = _discovered.Count;
        PathLength = _path.Count > 0 ? _path.Count - 1 : 0;
    }

    /// <summary>
    /// Publishes the current state to the canvas. The collections are handed over as they are,
    /// so that a move does not copy them.
    /// </summary>
    private void PublishRenderState()
    {
        RenderState = new RenderState(_scenario, _discovered, _visited, _path);
    }

    /// <summary>
    /// Refreshes the states that enable and disable the buttons.
    /// </summary>
    private void UpdateCommandStates()
    {
        var hasSearch = _search is not null;

        CanPlay = hasSearch && !_isRunning;
        CanPause = _isRunning;
        CanStep = hasSearch && !_isRunning;
        CanEditGrid = !_isRunning;

        IsPlayVisible = !_isRunning;
        IsPauseVisible = _isRunning;
    }

    #endregion Private Methods

    #region Events
    /// <summary>
    /// Handles a frame of the display by applying the move that is due.
    /// </summary>
    /// <remarks>
    /// A move is applied once <see cref="StepIntervalMilliseconds"/> have elapsed since the
    /// previous one, measured on the high-resolution clock. A move is applied whole, so the delay
    /// falls between moves and never between the events of a move.
    /// </remarks>
    /// <param name="sender">The composition target raising the event.</param>
    /// <param name="e">The event arguments of the frame.</param>
    private void OnRendering(object? sender, EventArgs e)
    {
        if (!_isRunning)
        {
            return;
        }

        // Republishing every frame keeps the rendering loop ticking between moves.
        PublishRenderState();

        var timestamp = Stopwatch.GetTimestamp();

        if (Stopwatch.GetElapsedTime(_lastMoveTimestamp, timestamp).TotalMilliseconds < StepIntervalMilliseconds)
        {
            return;
        }

        _lastMoveTimestamp = timestamp;
        Advance();
    }
    #endregion Events
}

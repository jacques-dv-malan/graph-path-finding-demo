using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GraphPathFindingDemo.Core.Factories;
using GraphPathFindingDemo.Core.Graph;
using GraphPathFindingDemo.Core.Pathfinding;
using System.Windows.Threading;

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
    /// The seed a new grid starts at.
    /// </summary>
    public const int DefaultSeed = 0;

    /// <summary>
    /// The number of search events per tick a new run starts at.
    /// </summary>
    public const int DefaultStepsPerTick = 10;

    /// <summary>
    /// The smallest number of search events per tick.
    /// </summary>
    public const int MinStepsPerTick = 1;

    /// <summary>
    /// The largest number of search events per tick.
    /// </summary>
    public const int MaxStepsPerTick = 50;

    /// <summary>
    /// The interval between two ticks of the playback timer, in milliseconds.
    /// </summary>
    public const int TimerIntervalMilliseconds = 40;

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
    private readonly ISearchAlgorithm _algorithm;
    private readonly DispatcherTimer _timer;

    private readonly List<GridNode> _discovered = [];
    private readonly List<GridNode> _visited = [];
    private readonly List<GridNode> _path = [];

    private GridScenario _scenario = null!;
    private IEnumerator<SearchStep>? _search;
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
    private int _seed = DefaultSeed;

    /// <summary>
    /// Gets or sets the number of search events the timer applies per tick. Values outside the
    /// supported range are clamped.
    /// </summary>
    [ObservableProperty]
    private int _stepsPerTick = DefaultStepsPerTick;

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
    #endregion Properties

    #region Constructor
    /// <summary>
    /// Initializes a new instance of the <see cref="MainViewModel"/> class and builds the first
    /// scenario, so the window is never empty.
    /// </summary>
    /// <param name="factory">The factory producing the scenarios.</param>
    /// <param name="algorithm">The algorithm searching the scenarios.</param>
    /// <exception cref="ArgumentNullException">Any of the arguments is <see langword="null"/>.</exception>
    public MainViewModel(IGridFactory factory, ISearchAlgorithm algorithm)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        _algorithm = algorithm ?? throw new ArgumentNullException(nameof(algorithm));

        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(TimerIntervalMilliseconds) };
        _timer.Tick += OnTimerTick;

        RebuildScenario();
    }
    #endregion Constructor

    #region Public Methods
    /// <summary>
    /// Stops the playback timer. Called when the window closes.
    /// </summary>
    public void Stop()
    {
        _timer.Stop();
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
    /// Clamps the number of search events per tick when it changes.
    /// </summary>
    /// <param name="value">The requested number of events per tick.</param>
    partial void OnStepsPerTickChanged(int value)
    {
        StepsPerTick = Math.Clamp(value, MinStepsPerTick, MaxStepsPerTick);
    }

    /// <summary>
    /// Builds a new grid and starts a fresh search on it.
    /// </summary>
    [RelayCommand]
    private void NewGrid()
    {
        Seed = Seed == int.MaxValue ? 0 : Seed + 1;
    }

    /// <summary>
    /// Starts the playback timer.
    /// </summary>
    [RelayCommand]
    private void Play()
    {
        if (_search is null)
        {
            return;
        }

        Status = StatusRunning;
        _timer.Start();
        UpdateCommandStates();
    }

    /// <summary>
    /// Stops the playback timer, leaving the search where it is.
    /// </summary>
    [RelayCommand]
    private void Pause()
    {
        if (!_timer.IsEnabled)
        {
            return;
        }

        _timer.Stop();
        Status = StatusIdle;
        UpdateCommandStates();
    }

    /// <summary>
    /// Advances the search by exactly one event.
    /// </summary>
    [RelayCommand]
    private void Step()
    {
        if (_search is null)
        {
            return;
        }

        _timer.Stop();
        Status = StatusIdle;
        Advance(1);
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
    /// Creates a new scenario for the current size and seed.
    /// </summary>
    private void RebuildScenario()
    {
        _scenario = _factory.Create(Width, Height, Seed);
        NodeCount = _scenario.Graph.Nodes.Count;
        EdgeCount = _scenario.Graph.Edges.Count;
        ResetSearch();
    }

    /// <summary>
    /// Clears the progress of the current scenario and starts a new search over it.
    /// </summary>
    private void ResetSearch()
    {
        _timer.Stop();
        DisposeSearch();

        _discovered.Clear();
        _visited.Clear();
        _path.Clear();

        _search = _algorithm.Search(_scenario.Graph, _scenario.Start, _scenario.End).GetEnumerator();

        Status = StatusIdle;
        VisitedCount = 0;
        FrontierCount = 0;
        PathLength = 0;

        PublishRenderState();
        UpdateCommandStates();
    }

    /// <summary>
    /// Applies up to the given number of search events and publishes the new state.
    /// </summary>
    /// <param name="stepCount">The maximum number of events to apply.</param>
    private void Advance(int stepCount)
    {
        if (_search is null)
        {
            return;
        }

        var finished = false;
        var pathFound = false;

        for (var i = 0; i < stepCount; i++)
        {
            if (!_search.MoveNext())
            {
                finished = true;
                break;
            }

            var step = _search.Current;
            ApplyStep(step);

            if (step.Kind == SearchStepKind.PathFound)
            {
                finished = true;
                pathFound = true;
                break;
            }
        }

        PublishRenderState();

        if (finished)
        {
            Complete(pathFound);
        }
        else
        {
            UpdateCounters();
        }
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
                _discovered.Add(step.Node);
                break;

            case SearchStepKind.Visited:
                _visited.Add(step.Node);
                _discovered.Remove(step.Node);
                break;

            case SearchStepKind.PathFound:
                _path.Clear();

                if (step.Path is not null)
                {
                    _path.AddRange(step.Path);
                }

                break;
        }
    }

    /// <summary>
    /// Ends the search and reports the final status.
    /// </summary>
    /// <param name="pathFound">Whether the search found a path.</param>
    private void Complete(bool pathFound)
    {
        _timer.Stop();
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
    /// Publishes the current state to the canvas.
    /// </summary>
    private void PublishRenderState()
    {
        RenderState = new RenderState(
            _scenario,
            _discovered.ToArray(),
            _visited.ToArray(),
            _path.ToArray());
    }

    /// <summary>
    /// Refreshes the states that enable and disable the buttons.
    /// </summary>
    private void UpdateCommandStates()
    {
        var hasSearch = _search is not null;
        var isRunning = _timer.IsEnabled;

        CanPlay = hasSearch && !isRunning;
        CanPause = isRunning;
        CanStep = hasSearch && !isRunning;
    }
    #endregion Private Methods

    #region Events
    /// <summary>
    /// Handles a tick of the playback timer by applying the next batch of search events.
    /// </summary>
    /// <param name="sender">The timer raising the tick.</param>
    /// <param name="e">The event arguments of the tick.</param>
    private void OnTimerTick(object? sender, EventArgs e)
    {
        Advance(StepsPerTick);
    }
    #endregion Events
}

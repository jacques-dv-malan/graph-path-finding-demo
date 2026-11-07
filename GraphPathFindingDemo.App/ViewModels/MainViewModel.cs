using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GraphPathFindingDemo.Core.Factories;
using GraphPathFindingDemo.Core.Graph;
using GraphPathFindingDemo.Core.Pathfinding;

namespace GraphPathFindingDemo.App.ViewModels;

public partial class MainViewModel : ObservableObject
{
    public const int DefaultWidth = 25;
    public const int DefaultHeight = 25;
    public const int DefaultSeed = 0;
    public const int DefaultStepsPerTick = 10;
    public const int MinStepsPerTick = 1;
    public const int MaxStepsPerTick = 50;
    public const int TimerIntervalMilliseconds = 40;

    public const string StatusIdle = "idle";
    public const string StatusRunning = "running";
    public const string StatusDone = "done";
    public const string StatusNoPath = "no path";

    private readonly IGridFactory _factory;
    private readonly ISearchAlgorithm _algorithm;
    private readonly DispatcherTimer _timer;

    private readonly List<GridNode> _discovered = [];
    private readonly List<GridNode> _visited = [];
    private readonly List<GridNode> _path = [];

    private GridScenario _scenario = null!;
    private IEnumerator<SearchStep>? _search;

    [ObservableProperty]
    private int _width = DefaultWidth;

    [ObservableProperty]
    private int _height = DefaultHeight;

    [ObservableProperty]
    private int _seed = DefaultSeed;

    [ObservableProperty]
    private int _stepsPerTick = DefaultStepsPerTick;

    [ObservableProperty]
    private RenderState _renderState = null!;

    [ObservableProperty]
    private int _visitedCount;

    [ObservableProperty]
    private int _frontierCount;

    [ObservableProperty]
    private int _pathLength;

    [ObservableProperty]
    private int _nodeCount;

    [ObservableProperty]
    private int _edgeCount;

    [ObservableProperty]
    private string _status = StatusIdle;

    [ObservableProperty]
    private bool _canPlay;

    [ObservableProperty]
    private bool _canPause;

    [ObservableProperty]
    private bool _canStep;

    public MainViewModel(IGridFactory factory, ISearchAlgorithm algorithm)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        _algorithm = algorithm ?? throw new ArgumentNullException(nameof(algorithm));

        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(TimerIntervalMilliseconds) };
        _timer.Tick += OnTimerTick;

        RebuildScenario();
    }

    public void Stop()
    {
        _timer.Stop();
    }

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

    partial void OnSeedChanged(int value)
    {
        RebuildScenario();
    }

    partial void OnStepsPerTickChanged(int value)
    {
        StepsPerTick = Math.Clamp(value, MinStepsPerTick, MaxStepsPerTick);
    }

    [RelayCommand]
    private void NewGrid()
    {
        Seed = Seed == int.MaxValue ? 0 : Seed + 1;
    }

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

    [RelayCommand]
    private void Reset()
    {
        ResetSearch();
    }

    private void RebuildScenario()
    {
        _scenario = _factory.Create(Width, Height, Seed);
        NodeCount = _scenario.Graph.Nodes.Count;
        EdgeCount = _scenario.Graph.Edges.Count;
        ResetSearch();
    }

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

    private void OnTimerTick(object? sender, EventArgs e)
    {
        Advance(StepsPerTick);
    }

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

    private void Complete(bool pathFound)
    {
        _timer.Stop();
        DisposeSearch();

        Status = pathFound ? StatusDone : StatusNoPath;
        UpdateCounters();
        UpdateCommandStates();
    }

    private void DisposeSearch()
    {
        (_search as IDisposable)?.Dispose();
        _search = null;
    }

    private void UpdateCounters()
    {
        VisitedCount = _visited.Count;
        FrontierCount = _discovered.Count;
        PathLength = _path.Count > 0 ? _path.Count - 1 : 0;
    }

    private void PublishRenderState()
    {
        RenderState = new RenderState(
            _scenario,
            _discovered.ToArray(),
            _visited.ToArray(),
            _path.ToArray());
    }

    private void UpdateCommandStates()
    {
        var hasSearch = _search is not null;
        var isRunning = _timer.IsEnabled;

        CanPlay = hasSearch && !isRunning;
        CanPause = isRunning;
        CanStep = hasSearch && !isRunning;
    }
}

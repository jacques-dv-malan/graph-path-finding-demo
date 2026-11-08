using GraphPathFindingDemo.App.Controls;
using GraphPathFindingDemo.App.ViewModels;
using GraphPathFindingDemo.Core.Factories;
using GraphPathFindingDemo.Core.Layout;
using GraphPathFindingDemo.Core.Layout.Generator;
using GraphPathFindingDemo.Core.Pathfinding.Heuristic;
using GraphPathFindingDemo.Core.Pathfinding.Search;
using System.ComponentModel;
using System.Windows;

namespace GraphPathFindingDemo.App;

/// <summary>
/// The window of the demo. It is also the composition root: it creates the factory, the
/// algorithm and the view model and hands them to the view.
/// </summary>
public partial class MainWindow : Window
{
    #region Instance Fields
    private readonly MainViewModel _viewModel;
    #endregion Instance Fields

    #region Constructor
    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class and binds it to a
    /// freshly created view model.
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();

        var factory = new GridFactory(
            new RecursiveDivisionLayoutGenerator(),
            new RecursiveBacktrackerLayoutGenerator(),
            new RandomizedPrimLayoutGenerator(),
            new EllerLayoutGenerator(),
            new WilsonLayoutGenerator(),
            new GrowingTreeLayoutGenerator(),
            new HuntAndKillLayoutGenerator(),
            new BraidedLayoutGenerator(),
            new WallDensityLayoutGenerator(LayoutStyle.SparseWalls, 0.5),
            new WallDensityLayoutGenerator(LayoutStyle.DenseWalls, 0.1),
            new RoomAndCorridorLayoutGenerator(),
            new CellularAutomataLayoutGenerator(),
            new ForestLayoutGenerator(),
            new WeightedKruskalLayoutGenerator()
        );

        _viewModel = new MainViewModel(
            factory,
            new BreadthFirstSearch(),
            new DepthFirstSearch(),
            new DijkstraSearch(),
            new AStarSearch(
                new TerrainHeuristic(),
                new ManhattanHeuristic(),
                new EuclideanHeuristic(),
                new ChebyshevHeuristic(),
                new OctileHeuristic()),
            new BidirectionalBreadthFirstSearch(),
            new BidirectionalBreadthFirstDepthFirstSearch(),
            new BidirectionalDijkstraSearch(),
            new BidirectionalAStarSearch(
                new TerrainHeuristic(),
                new ManhattanHeuristic(),
                new EuclideanHeuristic(),
                new ChebyshevHeuristic(),
                new OctileHeuristic()));

        GridCanvas.CellClicked += OnCellClicked;

        DataContext = _viewModel;
    }
    #endregion Constructor

    #region Overridden Members
    /// <summary>
    /// Stops the playback timer before the window closes.
    /// </summary>
    /// <param name="e">The event arguments of the close.</param>
    protected override void OnClosing(CancelEventArgs e)
    {
        _viewModel.Stop();

        base.OnClosing(e);
    }
    #endregion Overridden Members

    #region Private Methods
    /// <summary>
    /// Moves the endpoint the click names to the cell the user clicked on.
    /// </summary>
    /// <param name="sender">The canvas the click landed on.</param>
    /// <param name="e">The arguments of the click.</param>
    private void OnCellClicked(object? sender, CellClickedEventArgs e)
    {
        _viewModel.MoveEndpoint(e.Point, e.Target);
    }
    #endregion Private Methods
}

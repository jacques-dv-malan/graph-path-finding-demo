using GraphPathFindingDemo.App.ViewModels;
using GraphPathFindingDemo.Core.Factories;
using GraphPathFindingDemo.Core.Layout;
using GraphPathFindingDemo.Core.Pathfinding;
using System.ComponentModel;
using System.Windows;

namespace GraphPathFindingDemo.App;

/// <summary>
/// The window of the demo. It is also the composition root: it creates the factory, the
/// algorithm and the view model and hands them to the view.
/// </summary>
public partial class MainWindow : Window
{
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
            new EllerLayoutGenerator()
        );

        var viewModel = new MainViewModel(factory, new BreadthFirstSearch());

        DataContext = viewModel;
    }
    #endregion Constructor

    #region Overridden Members
    /// <summary>
    /// Stops the playback timer before the window closes.
    /// </summary>
    /// <param name="e">The event arguments of the close.</param>
    protected override void OnClosing(CancelEventArgs e)
    {
        if (DataContext is MainViewModel viewModel)
        {
            viewModel.Stop();
        }

        base.OnClosing(e);
    }
    #endregion Overridden Members
}

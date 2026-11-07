using System.ComponentModel;
using System.Windows;
using GraphPathFindingDemo.App.ViewModels;
using GraphPathFindingDemo.Core.Factories;
using GraphPathFindingDemo.Core.Layout;
using GraphPathFindingDemo.Core.Pathfinding;

namespace GraphPathFindingDemo.App;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        var factory = new GridFactory(new RecursiveDivisionLayoutGenerator());
        var viewModel = new MainViewModel(factory, new BfsSearch());

        DataContext = viewModel;
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (DataContext is MainViewModel viewModel)
        {
            viewModel.Stop();
        }

        base.OnClosing(e);
    }
}

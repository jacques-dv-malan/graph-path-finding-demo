using GraphPathFindingDemo.Core.Graph;

namespace GraphPathFindingDemo.Core.Layout;

public interface ILayoutGenerator
{
    GridGraph Generate(int width, int height, int seed);
}

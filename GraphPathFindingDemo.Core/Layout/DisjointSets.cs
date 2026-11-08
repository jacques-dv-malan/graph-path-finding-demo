namespace GraphPathFindingDemo.Core.Layout;

/// <summary>
/// The groups the cells belong to. Two cells are in the same group when they are connected
/// to each other through the passages opened so far.
/// </summary>
public sealed class DisjointSets
{
    #region Instance Fields
    private readonly int[] _parent;
    #endregion Instance Fields

    #region Constructor
    /// <summary>
    /// Initializes a new instance of the <see cref="DisjointSets"/> class with every cell in
    /// a group of its own.
    /// </summary>
    /// <param name="count">The number of cells of the grid.</param>
    public DisjointSets(int count)
    {
        _parent = new int[count];

        for (var index = 0; index < count; index++)
        {
            _parent[index] = index;
        }
    }
    #endregion Constructor

    #region Public Methods
    /// <summary>
    /// Returns the group the given cell belongs to.
    /// </summary>
    /// <param name="index">The index of the cell.</param>
    /// <returns>The root of the group the cell belongs to.</returns>
    public int Find(int index)
    {
        while (_parent[index] != index)
        {
            _parent[index] = _parent[_parent[index]];
            index = _parent[index];
        }

        return index;
    }

    /// <summary>
    /// Puts the two given cells into the same group.
    /// </summary>
    /// <param name="a">The index of the first cell.</param>
    /// <param name="b">The index of the second cell.</param>
    public void Union(int a, int b)
    {
        var rootA = Find(a);
        var rootB = Find(b);

        if (rootA != rootB)
        {
            _parent[rootB] = rootA;
        }
    }
    #endregion Public Methods
}

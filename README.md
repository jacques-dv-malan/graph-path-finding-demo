# Graph & Path Finding Demonstration

A Windows desktop demo that renders a square grid graph, animates a path finding search across it,
and draws the resulting path. Its purpose is to illustrate the graph data structure and path finding.

## What the demo shows

- **The graph model** — every cell is a node, every open passage is an edge. Walls are not a
  separate model: a wall is simply the *absence* of an edge between two orthogonal cells.
- **A perfect maze** — the layout is picked in the side panel: recursive division, a recursive
  backtracker, randomized Prim, Eller's algorithm, Wilson's algorithm, a growing tree or
  hunt-and-kill. All seven produce a spanning tree: every cell is reachable from every other
  cell, and `Edges.Count == Nodes.Count - 1` (visible in the read-outs as *Nodes* and *Edges*).
  The braided maze is the exception: it is a perfect maze with its dead ends knocked out, so it
  holds more passages than cells minus one and has loops. The sparse and dense wall mazes are
  exceptions too: they are a perfect maze with some of its spare walls knocked out. The room and
  corridor maze is a third exception: it is made of open rooms, so it holds even more
  passages. The cave maze is a fourth: it is made of open caves with rock between them, so it
  holds passages between the caves while the rock cells carry none at all. The forest maze is
  the fifth: it holds several groves that never touch, so a search from one end of the grid to
  the other ends without a path.
- **The search in motion** — the search is stepped one move at a time. A move colours in one cell:
  the cell the search reaches turns blue, and the neighbours it reveals turn amber. The cyan line
  is the path found at the end.
- **Two searches, side by side** — the algorithm is picked in the side panel: breadth-first search,
  depth-first search, Dijkstra's algorithm or A*. The breadth-first search explores the graph level by
  level, so its frontier grows and the path it finds is the shortest one. The depth-first search
  follows one branch as deep as it goes before it backtracks, so its frontier holds a single cell
  and the path it finds is often a longer way round. Dijkstra always leaves the cheapest cell it
  knows, so it finds the least costly path. A* leaves the cell that promises the cheapest way to
  the end, so it finds the same path as Dijkstra while looking at far fewer cells.
- **The ground under the maze** — the cells do not all stand at the same height. The terrain is a
  field of hills that the same seed always builds the same way, and a passage costs one for the
  step plus the climb between its two cells. The read-out *Path cost* adds up what the passages of
  the path cost, so it is worth more than the number of cells on it. In a maze with loops the
  cheapest path and the shortest one can differ: the shortest one may climb more than the cheapest
  one, and it is worth more.
- **The estimate A* trusts** — the heuristic is picked in a second picker, which only appears while
  A* is the chosen algorithm. The *Terrain* estimate is the distance across the grid plus the
  difference of the heights: every remaining step costs at least one, and the climb a walk makes
  in total is at least the difference of the two heights, so the estimate never promises more than
  the truth. A* therefore still finds the cheapest path, and it never looks at a cell twice. The
  *Manhattan*, *Euclidean*, *Chebyshev* and *Octile* estimates are there for the same reason, and
  they show what a looser estimate costs: the path comes out the same, but far more cells are
  looked at.
- **Why BFS is optimal here** — in a perfect maze the shortest path is the *only* path, so BFS's
  optimality is never confused by a tie-break between equally short routes, and the depth-first
  and the cheapest path are the very same one. The braided, sparse, dense wall, room and corridor
  and cave mazes are the exceptions: they have loops, so several routes of the same length may
  lead to the end, and the searches can come out with different paths. The forest maze goes
  further: it has no path at all, and the status reads `no path`.

## Requirements

- Windows 10 (21H2 / LTSC / Enterprise) or Windows 11 (23H2+)
- [.NET 10 SDK](https://dotnet.microsoft.com/download) (LTS)

## Run it

```powershell
dotnet run --project GraphPathFindingDemo.App
```

To produce a release build:

```powershell
dotnet build -c Release
```

## Using it

| Control | Effect |
| --- | --- |
| **Layout** | The maze generator: recursive division (boxes and corridors), recursive backtracker (long corridors), randomized Prim (blobby), Eller (row by row), Wilson (evenly drawn), growing tree (bushy), hunt-and-kill (long winding passages), braided (no dead ends), sparse walls (open), dense walls (narrow), room and corridor (open rooms joined by doors) or
cellular automata caves (organic caves with rock between them) or forest (separate groves, no
path between them). Changing it rebuilds the maze and resets the search. |
| **Algorithm** | The search: breadth-first search (level by level, shortest path), depth-first
search (one branch at a time, any path), Dijkstra (always the cheapest way out, least costly
path) or A* (guided by a heuristic, the least costly path with far fewer cells looked at).
Changing it restarts the search. |
| **Heuristic** | The estimate A* trusts: terrain (the distance across the grid plus the climb),
Manhattan, Euclidean, Chebyshev or Octile. The picker only shows while A* is the chosen algorithm.
Changing it restarts the search. |
| **Width** / **Height** | Grid size, 5–60 per axis. Changing either rebuilds the maze and resets the search. |
| **Seed** | Seed for the layout generator and for the terrain. The same seed always produces the same maze and the same ground. A new run starts with the current epoch time in seconds. |
| **New Grid** | Builds a new maze from a new seed, which is derived from the current one. |
| **Play** / **Pause** | Runs or freezes the search. |
| **Step** | Advances the search by exactly one move. |
| **Reset** | Returns the *same* maze to its pre-search state so a run can be replayed identically. |
| **Step interval** | Milliseconds between two moves of the search (0–1000, in steps of 50). Lower is faster. The moves are counted in display frames, so the time between two moves only varies by the jitter of the display. |

The read-outs show the node and edge counts, how many cells have been visited, the current
frontier size, the path length in segments, what the path costs to walk, and the status (`idle` /
`running` / `done` / `no path`).

## Solution layout

### `GraphPathFindingDemo.Core` (`net10.0`)

The graph model and the algorithms, with no UI dependencies.

```
GraphPathFindingDemo.Core/
├─ Graph/
│  ├─ GridNode.cs
│  ├─ GridEdge.cs
│  └─ GridGraph.cs
├─ Layout/
│  ├─ ILayoutGenerator.cs
│  ├─ LayoutStyle.cs
│  ├─ RecursiveDivisionLayoutGenerator.cs
│  ├─ RecursiveBacktrackerLayoutGenerator.cs
│  ├─ RandomizedPrimLayoutGenerator.cs
│  ├─ EllerLayoutGenerator.cs
│  ├─ WilsonLayoutGenerator.cs
│  ├─ GrowingTreeLayoutGenerator.cs
│  ├─ HuntAndKillLayoutGenerator.cs
│  ├─ BraidedLayoutGenerator.cs
│  ├─ WallDensityLayoutGenerator.cs
│  ├─ RoomAndCorridorLayoutGenerator.cs
│  ├─ CellularAutomataLayoutGenerator.cs
│  └─ ForestLayoutGenerator.cs
├─ Pathfinding/
│  ├─ ISearchAlgorithm.cs
│  ├─ SearchAlgorithmStyle.cs
│  ├─ SearchStep.cs
│  ├─ SearchStepKind.cs
│  ├─ PathBuilder.cs
│  ├─ IHeuristic.cs
│  ├─ IHeuristicSearch.cs
│  ├─ GridDistanceHeuristic.cs
│  ├─ TerrainHeuristic.cs
│  ├─ ManhattanHeuristic.cs
│  ├─ EuclideanHeuristic.cs
│  ├─ ChebyshevHeuristic.cs
│  ├─ OctileHeuristic.cs
│  ├─ BreadthFirstSearch.cs
│  ├─ DepthFirstSearch.cs
│  ├─ DijkstraSearch.cs
│  └─ AStarSearch.cs
├─ Terrain/
│  └─ HeightField.cs
├─ Endpoints/
│  ├─ EndpointMode.cs
│  └─ EndpointSelector.cs
└─ Factories/
   ├─ IGridFactory.cs
   ├─ GridScenario.cs
   └─ GridFactory.cs
```

`GridNode.cs` also holds `GridPoint` — a node owns its coordinate, so the two are inseparable.

### `GraphPathFindingDemo.App` (`net10.0-windows`, WPF)

The view model, the custom-drawn canvas, and the window that composes them.

```
GraphPathFindingDemo.App/
├─ ViewModels/
│  ├─ MainViewModel.cs
│  ├─ RenderState.cs
│  ├─ LayoutOption.cs
│  └─ SearchAlgorithmOption.cs
├─ Controls/
│  └─ GridCanvas.cs
├─ App.xaml
├─ App.xaml.cs
├─ MainWindow.xaml
└─ MainWindow.xaml.cs
```

`MainWindow` is the composition root: it creates the `GridFactory` and the search algorithms and hands
them to the `MainViewModel`. There is no DI container.

## Extension points

- **Another layout** (braided, sparse, dense, room and corridor, caves, forest) — implement
  `ILayoutGenerator` and return a `GridGraph` built with `Connect`. A generator names the layout
  it produces through `ILayoutGenerator.Style`; add it to the list the `GridFactory` is built
  with and it appears in the layout picker.
- **Another algorithm** — implement `ISearchAlgorithm` and name it through
  `ISearchAlgorithm.Style`. `MainViewModel` takes the searches it offers as a list, so adding one
  to the composition root in `MainWindow` puts it in the algorithm picker.
- **Another heuristic** (Manhattan, Euclidean, a perfect estimate from a backward Dijkstra run) —
  implement `IHeuristic` and hand it to the `AStarSearch` in the composition root. It lands in the
  heuristic picker on its own. An estimate has to stay under the truth, or A* can no longer promise
  the cheapest path.
- **The other endpoint modes** — implement the three remaining branches in `EndpointSelector`
  (`FixedRandom`, `RandomFixed`, `RandomRandom`); the enum values already exist and the other three
  throw `NotSupportedException` in v1.
- **Other terrain** — `HeightField` builds the hills; a different field only has to hand the
  factory another set of heights.
- **Canvas click interaction** — `GridCanvas` exposes `RenderState` only; a click handler would map
  a pixel to a node via `Graph.NodeAt`.

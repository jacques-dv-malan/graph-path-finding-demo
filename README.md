# Graph & Path Finding Demonstration

A Windows desktop demo that renders a square grid graph, animates a path finding search across it,
and draws the resulting path. Its purpose is to illustrate the graph data structure and path finding.

## Contents

- [What the demo shows](#what-the-demo-shows)
- [How it works](#how-it-works)
- [Requirements](#requirements)
- [Run it](#run-it)
- [Using it](#using-it)
- [Read-outs](#read-outs)
- [Solution layout](#solution-layout)
- [Extension points](#extension-points)

## What the demo shows

- A grid graph in which every cell is a node and every open passage is an edge.
- Thirteen generated layouts, from perfect mazes to caves and disconnected groves.
- Four searches, animated one node expansion at a time.
- Terrain heights that make a passage cost more than a plain step.
- A choice of heuristic for A*.

## How it works

### The graph model

Every cell is a node and every open passage is an edge. Walls are not a separate model: a wall is
simply the *absence* of an edge between two orthogonal cells.

`GridGraph` creates the nodes row by row and exposes `Nodes`, `Edges` and `Neighbors`. Layout
generators add passages through `Connect`. `ApplyHeights` then sets each node's height and works out
what every passage costs.

### Layouts

The layout is picked in the side panel. Most generators produce a perfect maze: a spanning tree in
which every cell is reachable and `Edges.Count == Nodes.Count - 1`.

| Layout | Appearance | Graph property |
| --- | --- | --- |
| Recursive division | Boxes joined by single openings | Perfect maze |
| Recursive backtracker | Long corridors | Perfect maze |
| Randomized Prim | Blobby tree | Perfect maze |
| Eller | Built row by row | Perfect maze |
| Wilson | Evenly drawn passages | Perfect maze |
| Growing tree | Bushy tree | Perfect maze |
| Hunt-and-kill | Long winding passages | Perfect maze |
| Braided | No dead ends | Loops |
| Sparse walls | Open | Loops |
| Dense walls | Narrow | Loops |
| Room and corridor | Open rooms joined by doors | Loops |
| Cellular automata caves | Organic caves with rock between them | Loops, with rock cells carrying no passages |
| Forest | Separate groves | Disconnected; no path between groves |

The braided layout is a perfect maze with its dead ends knocked out. The sparse and dense wall layouts
are a perfect maze with some spare walls knocked out. Because those layouts have loops, several routes
may lead to the end and the searches can return different paths.

### The cost model

The cells do not all stand at the same height. `HeightField` builds a smooth field of hills, and every
passage costs

```
1 + abs(height difference)
```

so the read-out *Path cost* is worth more than the number of cells on the path. In a layout with loops
the cheapest path and the shortest path can differ: the shortest one may climb more and cost more.

The same seed drives both the layout generator and the height field, so a seed reproduces the whole
scene.

### The searches

| Search | Strategy | Result |
| --- | --- | --- |
| Breadth-first search | Explores level by level with a queue | Shortest path |
| Depth-first search | Follows one branch as deep as it goes | Any path, often a longer way round |
| Dijkstra | Always expands the cheapest cell known | Least costly path |
| A* | Expands the cell with the cheapest cost plus estimate | Same path as Dijkstra, far fewer cells expanded |

In a perfect maze the shortest path is the only path, so the shortest and the cheapest path coincide.
In a layout with loops they can differ. In the forest layout the search ends without a path and the
status reads `no path`.

### The search in motion

A search is an `IEnumerable<SearchStep>`. The algorithm yields one step per event as it runs.

- `Discovered` — a neighbour enters the frontier and is drawn amber.
- `Visited` — a node is expanded and is drawn blue.
- `PathFound` — the path is complete and is drawn cyan.

A *move* is the expansion of one node, together with the neighbours it reveals. The view model
advances the sequence one move per display frame, subject to the step interval. `Reset` discards the
sequence and starts the same search over the same graph.

### Heuristics

The heuristic picker appears only while A* is the chosen search. Every heuristic has to stay under
the true remaining cost, or A* can no longer promise the cheapest path.

| Heuristic | Estimate |
| --- | --- |
| Terrain | `rows + columns + abs(height difference)` |
| Manhattan | `rows + columns` |
| Euclidean | `floor(sqrt(rows² + columns²))` |
| Chebyshev | `max(rows, columns)` |
| Octile | `floor(max(rows, columns) + (√2 − 1) × min(rows, columns))` |

Terrain is the tightest estimate and is consistent, so A* never has to look at a cell twice. The
others are admissible but looser: the path stays the same while more cells are expanded.

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
| **Layout** | The maze generator. Changing it rebuilds the maze and resets the search. |
| **Algorithm** | The search. Changing it restarts the search. |
| **Heuristic** | The estimate A* trusts. The picker only shows while A* is chosen. Changing it restarts the search. |
| **Endpoints** | The read-only fields show where the search starts and ends. A left-click moves the start; a right-click moves the end. |
| **Width** / **Height** | Grid size, 5–100 per axis. Changing either rebuilds the maze and resets the search. |
| **Seed** | Seed for the layout generator and the terrain. A new run starts with the current epoch time in seconds. |
| **New Grid** | Builds a new maze from a new seed, derived from the current one. |
| **Play** / **Pause** | Runs or freezes the search. Only one of the two shows at a time. |
| **Step** | Advances the search by exactly one move. |
| **Reset** | Returns the *same* maze to its pre-search state so a run can be replayed identically. |
| **Step interval** | Milliseconds between two moves, 0–1000 in steps of 50. Lower is faster. |

Endpoint rules:

- The two endpoints never share a cell. A click on the other endpoint does nothing.
- The endpoints keep their places when the layout, seed, grid size or search changes.
- A smaller grid moves an endpoint that no longer fits to the bottom-right most free cell.

The grid controls are disabled while the search runs.

## Read-outs

- **Nodes** and **Edges** — counts for the current graph.
- **Visited** — cells that have been expanded.
- **Frontier** — cells queued but not yet expanded.
- **Path length** — the number of segments on the path.
- **Path cost** — the sum of the passage weights along the path.
- **Status** — `idle`, `running`, `done` or `no path`.

## Solution layout

### `GraphPathFindingDemo.Core` (`net10.0`)

The graph model and the algorithms, with no UI dependencies.

```
GraphPathFindingDemo.Core/
├─ Endpoints/
│  └─ EndpointSelector.cs
├─ Factories/
│  ├─ GridFactory.cs
│  ├─ GridScenario.cs
│  └─ IGridFactory.cs
├─ Graph/
│  ├─ GridEdge.cs
│  ├─ GridGraph.cs
│  └─ GridNode.cs
├─ Layout/
│  ├─ DisjointSets.cs
│  ├─ Generator/
│  │  ├─ BraidedLayoutGenerator.cs
│  │  ├─ CellularAutomataLayoutGenerator.cs
│  │  ├─ EllerLayoutGenerator.cs
│  │  ├─ ForestLayoutGenerator.cs
│  │  ├─ GrowingTreeLayoutGenerator.cs
│  │  ├─ HuntAndKillLayoutGenerator.cs
│  │  ├─ RandomizedPrimLayoutGenerator.cs
│  │  ├─ RecursiveBacktrackerLayoutGenerator.cs
│  │  ├─ RecursiveDivisionLayoutGenerator.cs
│  │  ├─ RoomAndCorridorLayoutGenerator.cs
│  │  ├─ WallDensityLayoutGenerator.cs
│  │  └─ WilsonLayoutGenerator.cs
│  ├─ ILayoutGenerator.cs
│  └─ LayoutStyle.cs
├─ Pathfinding/
│  ├─ GridDistanceHeuristic.cs
│  ├─ Heuristic/
│  │  ├─ ChebyshevHeuristic.cs
│  │  ├─ EuclideanHeuristic.cs
│  │  ├─ ManhattanHeuristic.cs
│  │  ├─ OctileHeuristic.cs
│  │  └─ TerrainHeuristic.cs
│  ├─ IHeuristic.cs
│  ├─ IHeuristicSearch.cs
│  ├─ ISearchAlgorithm.cs
│  ├─ PathBuilder.cs
│  ├─ Search/
│  │  ├─ AStarSearch.cs
│  │  ├─ BreadthFirstSearch.cs
│  │  ├─ DepthFirstSearch.cs
│  │  └─ DijkstraSearch.cs
│  ├─ SearchAlgorithmStyle.cs
│  ├─ SearchStep.cs
│  └─ SearchStepKind.cs
└─ Terrain/
   └─ HeightField.cs
```

`GridNode.cs` also holds `GridPoint` — a node owns its coordinate, so the two are inseparable.
`DisjointSets` is the union-find used by the Eller and forest generators. `GridDistanceHeuristic` is
the abstract base shared by the distance-based heuristics.

### `GraphPathFindingDemo.App` (`net10.0-windows`, WPF)

The view model, the custom-drawn canvas, and the window that composes them.

```
GraphPathFindingDemo.App/
├─ App.xaml
├─ App.xaml.cs
├─ Controls/
│  ├─ CellClickedEventArgs.cs
│  └─ GridCanvas.cs
├─ MainWindow.xaml
├─ MainWindow.xaml.cs
└─ ViewModels/
   ├─ EndpointTarget.cs
   ├─ LayoutOption.cs
   ├─ MainViewModel.cs
   ├─ RenderState.cs
   └─ SearchAlgorithmOption.cs
```

`MainWindow` is the composition root: it creates the `GridFactory` and the search algorithms and hands
them to the `MainViewModel`. There is no DI container.

## Extension points

- **Another layout** — implement `ILayoutGenerator` and return a `GridGraph` built with `Connect`.
  Add the generator to the list the `GridFactory` is built with and it appears in the layout picker.
- **Another search** — implement `ISearchAlgorithm` and name it through `ISearchAlgorithm.Style`.
  Add it to the composition root in `MainWindow` and it appears in the algorithm picker.
- **Another heuristic** — implement `IHeuristic` and hand it to the `AStarSearch` in the composition
  root. It lands in the heuristic picker on its own. An estimate has to stay under the truth, or A*
  can no longer promise the cheapest path.
- **Other terrain** — `HeightField` builds the hills; a different field only has to hand the factory
  another set of heights.

using System;
using System.Collections.Generic;
using Engine;
using System.Numerics;
using Point = Engine.VectorInt2;
using Engine.TiledSharp;

namespace Engine.AI.Pathfinding
{
	/// <summary>
	/// basic static grid graph for use with A*. Add walls to the walls HashSet and weighted nodes to the weightedNodes HashSet. This provides
	/// a very simple grid graph for A* with just two weights: defaultWeight and weightedNodeWeight.
	/// </summary>
	public class AstarGridGraph : IAstarGraph<Point>
	{
		/// <summary>
		/// Default direction for tilemap
		/// </summary>
		public List<Point> Dirs { get; set; } = new List<Point>
		{
			new Point(1, 0),
			new Point(0, -1),
			new Point(-1, 0),
			new Point(0, 1)
		};

		public HashSet<Point> Walls = new HashSet<Point>();
		public HashSet<Point> WeightedNodes = new HashSet<Point>();
		public int DefaultWeight = 1;
		public int WeightedNodeWeight = 5;

		int _width, _height;
		List<Point> _neighbors = new List<Point>(4);


		public AstarGridGraph(int width, int height)
		{
			_width = width;
			_height = height;
		}

		/// <summary>
		/// creates a WeightedGridGraph from a TiledTileLayer. Present tile are walls and empty tiles are passable.
		/// </summary>
		/// <param name="tiledLayer">Tiled layer.</param>
		public AstarGridGraph(TmxLayer tiledLayer)
		{
			_width = tiledLayer.Map.Width;
			_height = tiledLayer.Map.Height;

			for (var y = 0; y < tiledLayer.Map.Height; y++)
			{
				for (var x = 0; x < tiledLayer.Map.Width; x++)
				{
					var tile = tiledLayer.GetTile(x, y);
					if (tile != null && tile.Gid != 0)
						Walls.Add(new Point(x, y));
				}
			}
		}

		/// <summary>
		/// ensures the node is in the bounds of the grid graph
		/// </summary>
		/// <returns><c>true</c>, if node in bounds was ised, <c>false</c> otherwise.</returns>
		bool IsNodeInBounds(Point node)
		{
			return 0 <= node.X && node.X < _width && 0 <= node.Y && node.Y < _height;
		}

		/// <summary>
		/// checks if the node is passable. Walls are impassable.
		/// </summary>
		/// <returns><c>true</c>, if node passable was ised, <c>false</c> otherwise.</returns>
		bool IsNodePassable(Point node) => !Walls.Contains(node);

		/// <summary>
		/// convenience shortcut for calling AStarPathfinder.search
		/// </summary>
		/// <param name="start">start location </param>
		/// <param name="goal">target location</param>
		/// <returns></returns>
		public List<Point> Search(Point start, Point goal) => AStarPathfinder.Search(this, start, goal);

		#region IAstarGraph implementation

		IEnumerable<Point> IAstarGraph<Point>.GetNeighbors(Point node)
		{
			_neighbors.Clear();

			foreach (var dir in Dirs)
			{
				var next = new Point(node.X + dir.X, node.Y + dir.Y);
				if (IsNodeInBounds(next) && IsNodePassable(next))
					_neighbors.Add(next);
			}

			return _neighbors;
		}

		int IAstarGraph<Point>.Cost(Point from, Point to)
		{
			return WeightedNodes.Contains(to) ? WeightedNodeWeight : DefaultWeight;
		}

		int IAstarGraph<Point>.Heuristic(Point node, Point goal)
		{
			return Math.Abs(node.X - goal.X) + Math.Abs(node.Y - goal.Y);
		}

		#endregion
	}
}
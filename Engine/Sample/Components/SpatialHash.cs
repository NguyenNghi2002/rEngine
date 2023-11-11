using Raylib_cs;
using System.Numerics;

namespace Engine
{
    public class SpatialHash<TCollider> 
    {
        CellStorage<TCollider> _cells = new CellStorage<TCollider>();

        public Rectangle GridBounds = default;


        private float _cellSize;

        public SpatialHash (float cellSize = 1f)
        {
            _cellSize = cellSize;
        }

        List<TCollider> GetCellAtPoint(float x, float y, bool createIfEmpty = false)
        {
            IEnumerable<TCollider> cell = null;
            if (Contain(x, y))
            {
                int locX = (int)(GridBounds.x / _cellSize);
                int locY = (int)(GridBounds.y / _cellSize);
                if (!_cells.TryGetValue(locX, locY, out cell)
                    && createIfEmpty)
                    _cells.Add(locX,locY,new List<TCollider>());
            }
            return cell.ToList();
        }

        public bool Contain(float x, float y)
            => Contain(new Vector2(x,y));
        public bool Contain(Vector2 point)
        {
            return Raylib.CheckCollisionPointRec(point,GridBounds);
        }
    }

    /// <summary>
    /// Storage hold reference to Value
    /// </summary>
    /// <typeparam name="Tvalue"></typeparam>
    public class CellStorage<Tvalue>
    {
        Dictionary<long, IEnumerable<Tvalue>> _board = new Dictionary<long, IEnumerable<Tvalue>>();

        public bool TryGetValue(int x, int y, out IEnumerable<Tvalue> value)
        {
            return _board.TryGetValue(GetKey(x, y), out value);
        }
        public void Add(int x, int y, IEnumerable<Tvalue> value)
            => _board.Add(GetKey(x, y), value);

        public bool TryAdd(int x, int y, IEnumerable<Tvalue> value)
            => _board.TryAdd(GetKey(x, y), value);

        public bool Remove(int x, int y)
            => _board.Remove(GetKey(x, y));

        public HashSet<Tvalue> GetAllValue()
        {
            var set = new HashSet<Tvalue>();

            foreach (var list in _board.Values)
                set.UnionWith(list);
            return set;
        }

        public void Clear()
        {
            _board.Clear();
        }

        long GetKey(int x, int y)
        {
            return unchecked((long)x << 32 | (uint)y);
        }


    }
}

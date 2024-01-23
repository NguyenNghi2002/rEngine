using Engine;
using Engine.UI;
using Raylib_cs;
using System.Collections;
using System.Diagnostics;
using System.Net;
using System.Numerics;

namespace Undo
{
    /// <summary>
    /// Require GridObject
    /// </summary>
    public class Character : Component
    {
        public string SocketID;
        public int Layer = 0;
        public bool Undoable = true;
        public bool Pushable = true;

        protected GameMananger _gm;
        protected GridObject _gridObj;
        public event Action<Entity>? OnMoved;

        public override void OnAddedToEntity()
        {
            Scene.TryFindComponent(out _gm);
            Entity.TryGetComponent(out _gridObj);
            Debug.Assert(_gm != null);
            Debug.Assert(_gridObj != null,$"{this} component require {typeof(GridObject) } ");

            _gridObj.SnapLocation(Transform.LocalPosition2);
            _gridObj.SnapTransform(_gridObj.Grid.CellSize / 2f);

            OnMoved += _gm.OnCharacterMoved;
        }

        protected bool IsNextCellWalkable(int dx, int dy)
        {
            var nextLoc = _gridObj.GetLocation() + new VectorInt2(dx, dy);

            if (!_gridObj.Grid.IsInside(nextLoc))
                return false;
            return _gridObj.Grid.GetCell(nextLoc.X, nextLoc.Y).Walkable;
        }
        protected bool IsNextCellEmpty(int dx,int dy)
        {
            var nextLoc = _gridObj.GetLocation() + new VectorInt2(dx, dy);

            if (!_gridObj.Grid.IsInside(nextLoc))
                return false;
            return _gridObj.Grid.GetCell(nextLoc.X, nextLoc.Y).Objects.Count == 0;
        }
     
        internal void ControlMovement(int dx, int dy)
        {
            MoveRecursive(dx,dy, true);
        } 
        internal bool MoveRecursive(int dx,int dy,bool allowSaveHistory)
        {
            //Check if next cell is floor
            //, return FALSE
            if (!IsNextCellWalkable(dx, dy)) 
                return false;


            var nextCell = _gridObj.Grid.GetCell(_gridObj.GetLocation() + new VectorInt2(dx, dy));

            //
            foreach (Character? character in nextCell.Objects.Select(o=> o.Entity.GetComponent<Character>()))
            {
                if (character.Layer != this.Layer) continue;

                if (!character.Pushable) return false;

                if (!character.MoveRecursive(MathF.Sign(dx), MathF.Sign(dy), allowSaveHistory))
                    return false;
            }

            if(Undoable && allowSaveHistory)
            {
                _gm.RequestMovement(this, new(dx, dy));
                return true;
            }
            else
            {

                ///Execute Movement
                _gridObj.Shift(dx, dy);
                _gridObj.SnapTransform(_gridObj.Grid.CellSize / 2f);

                var sprite = Entity.GetComponentInChilds<SpriteRenderer>();
                if (sprite != null)
                {
                    Core.StartCoroutine(AnimateMove(dx, dy, sprite.Entity,tweening,0.3f));
                }

                OnMoved?.Invoke(this.Entity);
                return true;
            }

        }

        static Queue<IEnumerator> tweening = new Queue<IEnumerator>();
        public IEnumerator AnimateMove(int dx,int dy ,Entity en,Queue<IEnumerator> enumerators,float duration )
        {
            float elapse = 0;
            var offX = dx * 16;
            var offY = dy * 16;

            float x = 0, y = 0;
            while (elapse < 1f)
            {
                elapse += Time.DeltaTime;

                x = Easings.EaseExpoOut(elapse,-offX,offX, duration);
                y = Easings.EaseExpoOut(elapse, -offY,offY, duration);
                en.Transform.LocalPosition2 = new Vector2(x,y );
                
                yield return null;
            }


        }

    }

    /// <summary>
    /// Controllable character
    /// </summary>
    public class Player : Character
    {
        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();

            if(Scene.TryFindComponent(out InputManager input))
            {
                input.OnLeft += ()=> ControlMovement(-1, 0);
                input.OnRight += ()=> ControlMovement(1, 0);
                input.OnUp += ()=> ControlMovement(0, -1);
                input.OnDown += ()=> ControlMovement(0, 1);
            }
        }
    }

    public class Indicator: Character
    {
        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();

            _gm.indicators.Add(this);
        }
        public override void OnRemovedFromEntity()
        {
            _gm.indicators.Remove(this);

            base.OnRemovedFromEntity();
        }

        public bool IsIndicated()
        {
            var cell = _gridObj.Grid.GetCell(_gridObj.GetLocation());
            return cell.Objects.Any(o => o != this._gridObj && o.Entity.GetComponent<Character>().SocketID == this.SocketID);
        }
    }

}

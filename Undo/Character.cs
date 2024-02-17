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
        public event Action<int,int>? OnMoved;

        public override void OnAddedToEntity()
        {
            Scene.TryFindComponent(out _gm);
            Entity.TryGetComponent(out _gridObj);
            Debug.Assert(_gm != null);
            Debug.Assert(_gridObj != null,$"{this} component require {typeof(GridObject) } ");

            _gridObj.SnapLocation(Transform.LocalPosition2);
            _gridObj.SnapTransform(_gridObj.Grid.CellSize / 2f);

            OnMoved += _gm.OnSingleCharacterMoved;
        }
        public override void OnRemovedFromEntity()
        {
            tweening.Clear();
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
            RelocateRecursive(dx,dy, true);
            _gm.ExecuteCommand();
        } 
        internal bool RelocateRecursive(int dx,int dy,bool allowSaveHistory)
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

                if (!character.RelocateRecursive(MathF.Sign(dx), MathF.Sign(dy), allowSaveHistory))
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

                var loc = _gridObj.GetLocation();
                OnMoved?.Invoke(loc.X,loc.Y);
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
            while (elapse < 1f && en != null && en.Transform != null)
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
        bool isRegisted = false;
        public override void OnEnable()
        {
            SetInputRegister(true);
        }
        public override void OnDisable()
        {
            SetInputRegister(false);
        }
        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();
            SetInputRegister(true);
        }
        #region Events 
        void MoveLeft() => ControlMovement(-1, 0);
        void MoveRight() => ControlMovement(1, 0);
        void MoveUp() => ControlMovement(0, -1);
        void MoveDown() => ControlMovement(0, 1); 
        #endregion
        void SetInputRegister(bool value)
        {
            if(value)
            {
                Console.WriteLine("registered");
                isRegisted = value;
                if (Scene.TryFindComponent(out PlaySceneInputManager input))
                {
                    input.OnLeft    += MoveLeft;
                    input.OnRight   += MoveRight;
                    input.OnUp      += MoveUp;
                    input.OnDown    += MoveDown;
                }
            }
            if (value != isRegisted && isRegisted == true)
            {
                Console.WriteLine("DEEEEEEEEregistered");
                isRegisted = value;
                if (Scene.TryFindComponent(out PlaySceneInputManager input))
                {
                    input.OnLeft    -= MoveLeft;
                    input.OnRight   -= MoveRight;
                    input.OnUp      -= MoveUp;
                    input.OnDown    -= MoveDown;
                }
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
            var result = cell.Objects.Any(o => o != this._gridObj && o.Entity.GetComponent<Character>().SocketID == this.SocketID);
            return result;
        }
    }

}

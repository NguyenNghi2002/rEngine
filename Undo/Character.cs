using Engine;
using Raylib_cs;
using System.Net;
using System.Numerics;

namespace Undo
{
    public class Character : Component
    {
        public bool Undoable = true;
        public bool Pushable = true;
        struct CellMoveCommand : CommandSystem.ICommand
        {
            readonly   GridObject _gridObj;
            readonly Character _character;
            int _dx, _dy;
            public CellMoveCommand(Character character, GridObject gridObject, int dx, int dy)
            {
                _character = character;
                this._gridObj = gridObject;
                this._dx = dx;
                this._dy = dy;
            }
            void CommandSystem.ICommand.Execute()
            {
                ExecuteMove(_dx, _dy);
            }

            void CommandSystem.ICommand.Redo()
            {
                ExecuteMove(_dx, _dy);
            }

            void CommandSystem.ICommand.Undo()
            {
                ExecuteMove(-_dx, -_dy);
            }

            void ExecuteMove(int dx, int dy)
            {
                if (!_character.IsNextCellWalkable(dx, dy)) return;

                FloorCell nextFloorCell = _gridObj.Grid.GetCell(_gridObj.GetLocation() + new VectorInt2(dx,dy));
                var gridObj = _gridObj;
                var halfCellSize = _gridObj.Grid.CellSize / 2f;


                //Check pushable in next
                var objs = nextFloorCell.Objects.Where((cellGridObj) =>
                cellGridObj.Entity.TryGetComponent<Pushable>(out _)
                && cellGridObj != gridObj
                );

                //relocate object cell
                foreach (GridObject obj in objs)
                {
                    obj.Entity.TryGetComponent<CommandSystem>(out var commander);
                    obj.Entity.TryGetComponent<Character>(out var character);
                    var moveIntX = dx == 0 ? 0 : MathF.Sign(dx);
                    var moveIntY = dy == 0 ? 0 : MathF.Sign(dy);
                    //character.Move(moveIntX,moveIntY);
                    commander.ExecuteCommand(new CellMoveCommand(character,obj,moveIntX,moveIntY));
                }
                
                if(nextFloorCell.Objects.Count == 0)
                {
                    _gridObj.Shift(dx, dy);
                    _gridObj.SnapTransform(halfCellSize);
                }
                else
                {
                    _character.Entity.TryGetComponent<CommandSystem>(out var command);
                }

            }
        }

        protected GameMananger _gm;
        protected GridObject _gridObj;

        public override void OnAddedToEntity()
        {
            Scene.TryFindComponent(out _gm);
            Entity.TryGetComponent(out _gridObj);

            _gridObj.SnapLocation(Transform.LocalPosition2);
            _gridObj.SnapTransform(_gridObj.Grid.CellSize / 2f);
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
        protected void MoveUp()
        {
            Move(0,-1);
        }
        protected void MoveDown()
        {
            Move(0,1);
        }
        protected void MoveLeft()
        {
            Move(-1,0);
        }
        protected void MoveRight()
        {
            Move(1,0);
        }
        internal void Move(int dx, int dy)
            => MoveRecursive(dx,dy, true);
        internal bool MoveRecursive(int dx,int dy,bool allowSaveHistory)
        {
            //Check if next cell is floor
            //, return FALSE
            if (!IsNextCellWalkable(dx, dy)) 
                return false;


            var nextCell = _gridObj.Grid.GetCell(_gridObj.GetLocation() + new VectorInt2(dx, dy));

            //Check if next cell is contain any non pushable objects
            // , return FALSE
            if (nextCell.Objects.Any(o => !o.Entity.GetComponent<Character>().Pushable))
                return false;

            ///Loop through pushable objects
            foreach (var obj in nextCell.Objects)
            {
                int dxObj = dx == 0 ? 0 : MathF.Sign(dx) * dx / dx;
                int dyObj = dy == 0 ? 0 : MathF.Sign(dy) * dy / dy;
                if (!obj.Entity.GetComponent<Character>().MoveRecursive(dxObj, dyObj, allowSaveHistory))
                    return false;
            }

            if((Undoable && allowSaveHistory) )
            {
                _gm.RequestMovement(this, new(dx, dy));
                return true;
            }
            else
            {
                _gridObj.Shift(dx, dy);
                _gridObj.SnapTransform(_gridObj.Grid.CellSize / 2f);
                return true;
            }

        }



    }

    public class Player : Character,IUpdatable
    {
        int IUpdatable.UpdateOrder { get; set; }
        public KeyboardKey 
            Left    = KeyboardKey.KEY_LEFT  ,
            Right   = KeyboardKey.KEY_RIGHT ,
            Up      = KeyboardKey.KEY_UP    ,
            Down    = KeyboardKey.KEY_DOWN  
            ;


        void IUpdatable.Update()
        {
            if (Input.IsKeyPressed(Left))
            {
                MoveLeft();
                _gm.ExecuteCommand();
            }
            if (Input.IsKeyPressed(Right))
            {
                MoveRight();
                _gm.ExecuteCommand();

            }
            if (Input.IsKeyPressed(Up))
            {
                MoveUp();
                _gm.ExecuteCommand();
            }
            if (Input.IsKeyPressed(Down)) 
            { 
                MoveDown();
                _gm.ExecuteCommand();
            }

            if (Input.IsKeyPressed(KeyboardKey.KEY_Z))
            {
                _gm.UndoCommand();
            }

            if (Input.IsKeyPressed(KeyboardKey.KEY_X))
            {
                _gm.RedoCommand();
            }
        }

        



    }

}

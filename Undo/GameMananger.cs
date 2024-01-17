using Engine;
using Engine.TiledSharp;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using Undo;

public class GameMananger : Component
{
    //Player
    Entity playerEn;

    //Tilemap
    Entity tilemapEn;
    Grid<FloorCell> grid;
    public override void OnAddedToEntity()
    {
        Debug.Assert(Scene.TryFind("player", out playerEn));

        Debug.Assert(Scene.TryFind("tilemap", out tilemapEn));
        tilemapEn.TryGetComponent(out grid);


        ///Generate Floor grid
        grid.HandleValues((loc) =>
        {
            var map = ContentManager.Get<TmxMap>("map");
            var layer = map.Layers[PlayScene.FLOOR_LAYER] as TmxLayer;
            return new FloorCell()
            {
                Walkable = layer.GetTile(loc.X, loc.Y).Gid != 0
            };
        });


    }


    struct CharactersMoveCommand : CommandSystem.ICommand
    {
        List<KeyValuePair<Character, VectorInt2>> movements;

        public CharactersMoveCommand( List<KeyValuePair< Character, VectorInt2>> movements)
        {
            this.movements = movements;
        }
        void CommandSystem.ICommand.Execute()
        {
            
            foreach (KeyValuePair<Character, VectorInt2> movement in movements)
            {
                var character = movement.Key;
                var delta = movement.Value ;

                character.MoveRecursive(delta.X, delta.Y, false);

            }
        }
         
        void CommandSystem.ICommand.Redo()
        {
            foreach (KeyValuePair<Character, VectorInt2> movement in movements)
            {
                var character = movement.Key;
                var delta =    movement.Value;

                character.MoveRecursive(delta.X, delta.Y, false);

            }
        }

        void CommandSystem.ICommand.Undo()
        {
            var moves = movements.Reverse<KeyValuePair<Character, VectorInt2>>();
            foreach (KeyValuePair<Character, VectorInt2> movement in moves)
            {
                var character = movement.Key;
                var delta = -movement.Value;

                character.MoveRecursive(delta.X, delta.Y, false);

            }
        }


        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var move in movements)
            {
                sb.AppendLine($"{move.Key}<{move.Value}>");
            }
            return sb.ToString();
        }
    }


    List<KeyValuePair<Character, VectorInt2>> characterMovements = new();
    public void RequestMovement(Character character,VectorInt2 movement)
    {
        characterMovements.Add(new(character, movement));
    }
    public void ExecuteCommand()
    {
        if(this.Entity.TryGetComponent(out CommandSystem cmd) && characterMovements.Count != 0)
        {
            cmd.ExecuteCommand(new CharactersMoveCommand(new (characterMovements)));
            characterMovements.Clear();
        }
    }
    public void UndoCommand()
    {
        if (this.Entity.TryGetComponent(out CommandSystem cmd))
        {
            cmd.SendUndoCommand();
            characterMovements.Clear();

        }
    }

    public void RedoCommand()
    {
        if (this.Entity.TryGetComponent(out CommandSystem cmd))
        {
            cmd.SendRedoCommand();
            characterMovements.Clear();
        }
    }

}

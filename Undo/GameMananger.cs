using Engine;
using Engine.TiledSharp;
using Engine.UI;
using ImGuiNET;
using Raylib_cs;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using Undo;

public class InputManager : Component, IUpdatable
{
    GameMananger _gm;
    int IUpdatable.UpdateOrder { get; set; }
    public KeyboardKey
            Left = KeyboardKey.KEY_LEFT,
            Right = KeyboardKey.KEY_RIGHT,
            Up = KeyboardKey.KEY_UP,
            Down = KeyboardKey.KEY_DOWN
            ;

    public event Action? OnLeft,OnRight,OnUp,OnDown;
    public override void OnAddedToEntity()
    {
        Entity.TryGetComponent(out _gm);
        base.OnAddedToEntity();
    }
    void IUpdatable.Update()
    {
        if (Input.IsKeyPressed(Left))
        {
            OnLeft?.Invoke();
            _gm.ExecuteCommand();
        }
        if (Input.IsKeyPressed(Right))
        {
            OnRight?.Invoke();
            _gm.ExecuteCommand();
        }
        if (Input.IsKeyPressed(Up))
        {
            OnUp?.Invoke();
            _gm.ExecuteCommand();
        }
        if (Input.IsKeyPressed(Down))
        {
            OnDown?.Invoke();
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
public class GameMananger : Component,ICustomInspectorImgui
{
    internal List<Indicator> indicators = new List<Indicator>();

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

        
        var defaultSkin = Skin.CreateDefaultSkin();
        defaultSkin.Get<LabelStyle>().Font = ContentManager.Get<Font>("UpheavalPro");
        defaultSkin.Get<LabelStyle>().Font = ContentManager.Get<Font>("UpheavalPro");

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

        if (Entity.TryGetComponent<UICanvas>(out var uiCanvas))
        {
            var butttonSize = 20;

            var table = new Table()
                .SetFillParent(true)
                .Top()
                //.DebugCell()
                ;
                ;
            table.PadTop(5);

            table.Add(new Container())
                .SetExpandX()
                .Size(butttonSize);
                ;

            table.Add(new Label("Undo", ContentManager.Get<Font>("UpheavalPro")))
                .SetExpandX()
                .Center()
                .Top();

            table.Add(new TextButton("p", defaultSkin))
                .SetExpandX()
                .Size(butttonSize);
                ;

            uiCanvas.Stage.AddElement(table);
        }
    }
    public void ChangeScene()
    {

    }

    public void CheckAllIndicators()
    {
        if(indicators.All(i => i.IsIndicated()))
            Console.WriteLine("you win");
        else
        {
            var undones = indicators.Where(i => !i.IsIndicated());
            Console.WriteLine($"{undones.Count()} still missing");
        }
    }
    public void OnCharacterMoved(Entity MovedEntity)
    {
#if true
        var location = MovedEntity.GetComponent<GridObject>().GetLocation();
        var cell = grid.GetCell(location);

        var indicators = from o in cell.Objects where
                         o.Entity.HasComponent<Indicator>() && 
                         o.Entity.GetComponent<Character>().Layer == PlayScene.INDICATOR_CHRACTER_LAYER 
                         select o.Entity.GetComponent<Indicator>();


        if (indicators.Any(i => i.IsIndicated()))
            CheckAllIndicators();
#endif
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

    void ICustomInspectorImgui.OnInspectorGUI()
    {
        if(ImGuiNET.ImGui.Button("Reset Scene") )
        {
            var transition = new FadeTransition(() => new PlayScene(0));
            transition.FadeInDuration = transition.FadeOutDuration = 0.1f;
            transition.HoldDuration = 1f;
            Core.StartTransition(transition);
        }

        foreach (var resource in ContentManager.Instance.Resources)
        {
            ImGui.Text(resource.Key);
            foreach (var content in resource.Value)
            {
                ImGui.Text($"{content.Key} : {content.Value}");
            }
            ImGui.Separator();
        }
    }

}

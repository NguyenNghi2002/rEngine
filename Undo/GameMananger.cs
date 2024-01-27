using Engine;
using Engine.TiledSharp;
using Engine.UI;
using ImGuiNET;
using Raylib_cs;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;
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

    public event Action? OnLeft,OnRight,OnUp,OnDown,OnEscapse;
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

        if (Input.IsKeyPressed(KeyboardKey.KEY_ESCAPE)) OnEscapse?.Invoke();

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
    TmxMap _map;
    InputManager _inputManager;
    UICanvas _ui;

    public override void OnAddedToEntity()
    {
        Debug.Assert(Scene.TryFindComponent(out _inputManager));
        Debug.Assert(Scene.TryFind("player", out playerEn));
        Debug.Assert(Scene.TryFind("tilemap", out tilemapEn));
        Debug.Assert(tilemapEn.TryGetComponent(out grid));
        Debug.Assert(ContentManager.TryGet("map", out _map));


        var defaultSkinName = UndoGame.DefaultUI;
        var skin = UndoGame.GameSkin ;

        ///****************************************
        ///Generate Tile that have id of non zero value
        ///****************************************
        grid.HandleValues((loc) =>
        {
            var layer = _map.Layers[PlayScene.WALL_LAYER] as TmxLayer;
            return new FloorCell()
            {
                Walkable = layer.GetTile(loc.X, loc.Y).Gid == 0
            };
        });


        ///****************************************
        ///                 UI SYSTEM
        ///****************************************
        if (Entity.TryGetComponent<UICanvas>(out _ui))
        {
            var butttonSize = 25;

            var table = new Table()
                .SetFillParent(true)
                .Top()
                ;
                ;
            table.PadTop(5);

            table.Add(new Container())
                .SetExpandX()
                .Size(butttonSize);
                ;

            var title = new Label($"Level {(Scene as PlayScene).levelID}", skin, UndoGame.TitleUI);
            table.Add(title)
                .SetExpandX()
                .Center()
                .Top();
            Core.Schedule(1.75f, false, null, timer =>
            {
                var duration = 1.6f;
                Core.StartCoroutine(AnimateElementY(title, title.GetY(), -title.GetHeight(), duration));
            });

            //var pauseWindow = CreatePauseMenu(skin);
            table.Add(new TextButton("<", skin).AddLeftMouseListener((btt) => PauseGame()))
                .SetExpandX()
                .Size(butttonSize)
                
                ;

            _ui.Stage.AddElement(table);
            ;
        }
    }

    public void ResumeGame()
    {
        _inputManager.SetEnable(true);
    }
    public void PauseGame()
    {
        _inputManager.SetEnable(false);
        ShowWindow(CreatePauseMenu(UndoGame.GameSkin, UndoGame.PauseUI), _ui.Stage);
    }
    public override void OnRemovedFromEntity()
    {
        //var cm = Core.Instance.Managers.Find(m => m is CoroutineManager) as CoroutineManager;
        //cm.TogglePauseAll(true);
    }
    IEnumerator AnimateElementY(Element element, float start, float offset, float duration)
    {
        float elapse = 0;

        while (elapse < duration)
        {
            elapse += Time.DeltaTime;
            element.SetY(Easings.EaseBackIn(elapse, start, offset, duration));
            yield return null;
        }
        element.SetY(start + offset);
    }
    void ShowWindow(Window window,Stage stage)
    {
        stage.AddElement(window);
        window.Pack();

        var width = window.GetWidth();
        var height = window.GetHeight();

        var w = MathF.Round((stage.GetWidth() - width) / 2);
        var h = MathF.Round((stage.GetHeight() - height) / 2);
        window.SetPosition(w, h);
    }
    Window CreatePauseMenu(Skin skin,string style)
    {
        float padTop = 10f;
        float buttonPadding = 2f;

        Window window = new Window("PAUSE",skin,style);
        window.GetTitleTable().PadLeft(10).PadBottom(10);
        window.SetMovable(false) ;
        window.SetResizable(false) ;

        window.Pad(5);
        Table optionsSection = new Table();
        Table bottomSection = new Table();

        window.Add(optionsSection);

        ///Resume button
        var resumeBtt = new TextButton("Resume",skin, style);
        resumeBtt.OnClicked += (b) =>
        {
            ResumeGame();
            window.Remove();
        };

        /// Add 
        optionsSection.Add(resumeBtt)
            .SetPadTop(padTop)
            .SetPadBottom(buttonPadding)
            .SetFillX();
        optionsSection.Row();
        optionsSection.Add(bottomSection);


        ///Bottom Section
        ///Setting Button 
        var settingBtt = new TextButton("Setting", skin, style);
        settingBtt.OnClicked += (b) => Console.WriteLine("working on it"); ;
        bottomSection.Add(settingBtt)
            .Width(Value.PercentWidth(1.2f))
            .SetPadRight(buttonPadding);

        ///Back Button
        var backBtt = new TextButton("Back", skin, style);
        backBtt.OnClicked += (b) => GotoLevelSelector();
        bottomSection.Add(backBtt)
            .Width(Value.PercentWidth(1.2f));


        return window;
        
    }

    /// <summary>
    /// Event Called from Character
    /// </summary>
    /// <param name="MovedEntity"></param>
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

    #region Utilities
    public void CheckAllIndicators()
    {
        if (indicators.All(i => i.IsIndicated()))
        {
            Console.WriteLine("you win");
            var nextLevelID = (Scene as PlayScene).levelID + 1;
            if (UndoGame.LevelsDictionary.TryGetValue(nextLevelID, out _))
            {
                if (Scene.TryFindComponent<InputManager>(out var inputManager))
                    inputManager.SetEnable(false);
                GoToLevel(nextLevelID);
            }
            else
                GotoLevelSelector();
        }
        else
        {
            var undones = indicators.Where(i => !i.IsIndicated());
            Console.WriteLine($"{undones.Count()} still missing");
        }
    }
    public void GoToLevel(int levelID)
        => Core.StartTransition(new FadeTransition(() => new PlayScene(levelID)));
    public void GotoLevelSelector()
        => Core.StartTransition(new FadeTransition(() => new LevelSelectorScene())); 
    #endregion

    #region Command System
    struct CharactersMoveCommand : CommandSystem.ICommand
    {
        List<KeyValuePair<Character, VectorInt2>> movements;

        public CharactersMoveCommand(List<KeyValuePair<Character, VectorInt2>> movements)
        {
            this.movements = movements;
        }
        void CommandSystem.ICommand.Execute()
        {

            foreach (KeyValuePair<Character, VectorInt2> movement in movements)
            {
                var character = movement.Key;
                var delta = movement.Value;

                character.MoveRecursive(delta.X, delta.Y, false);

            }
        }

        void CommandSystem.ICommand.Redo()
        {
            foreach (KeyValuePair<Character, VectorInt2> movement in movements)
            {
                var character = movement.Key;
                var delta = movement.Value;

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

    public void RequestMovement(Character character, VectorInt2 movement)
    {
        characterMovements.Add(new(character, movement));
    }
    public void ExecuteCommand()
    {
        if (this.Entity.TryGetComponent(out CommandSystem cmd) && characterMovements.Count != 0)
        {
            cmd.ExecuteCommand(new CharactersMoveCommand(new(characterMovements)));
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

    #endregion
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

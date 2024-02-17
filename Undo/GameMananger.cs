using Engine;
using Engine.Collections.Generic;
using Engine.TiledSharp;
using Engine.UI;
using ImGuiNET;
using Raylib_cs;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Dynamic;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Undo;

public class PlaySceneInputManager : Component, IUpdatable
{
    GameMananger gm;
    int IUpdatable.UpdateOrder { get; set; }
    public bool AllowCommand { set => AllowUndo = AllowRedo = value; }

    public KeyboardKey
            Left = KeyboardKey.KEY_LEFT,
            Right = KeyboardKey.KEY_RIGHT,
            Up = KeyboardKey.KEY_UP,
            Down = KeyboardKey.KEY_DOWN,
            Undo = KeyboardKey.KEY_Z,
            Redo = KeyboardKey.KEY_X,
            Back = KeyboardKey.KEY_ESCAPE,
            Restart = KeyboardKey.KEY_R
            ;
    public  bool AllowUndo = true;
    public  bool AllowRedo = true;
    public  bool AllowControl = true;

    public event Action? OnLeft,OnRight,OnUp,OnDown
        ,OnUndo,OnRedo,OnRestart,OnEsc;

    public override void OnAddedToEntity()
    {
        Entity.TryGetComponent(out gm);
        base.OnAddedToEntity();
    }
    void IUpdatable.Update()
    {
        if (Input.IsKeyPressed(Left) && AllowControl) OnLeft?.Invoke();
        if (Input.IsKeyPressed(Right) && AllowControl) OnRight?.Invoke();
        if (Input.IsKeyPressed(Up) & AllowControl) OnUp?.Invoke();
        if (Input.IsKeyPressed(Down) & AllowControl) OnDown?.Invoke();

        if (AllowUndo && Input.IsKeyPressed(Undo)) OnUndo?.Invoke();
        if (AllowRedo && Input.IsKeyPressed(Redo)) OnRedo?.Invoke();

        if (Input.IsKeyReleased(Back)) OnEsc?.Invoke();
        if (Input.IsKeyReleased(Restart)) OnRestart?.Invoke();

    }
}


/// <summary>
///                 [GAME MANAGER]<br/>
///     Control game input and manager object in game <br/>
/// NOTE: this component can must have and only single one exist in single scene
/// </summary>
public class GameMananger : Component,ICustomInspectorImgui
{
    internal List<Indicator> indicators = new List<Indicator>();

    internal int _moveCount = 15;
    internal int MoveCount
    {
        get => _moveCount;
        set => SetMoveCount(value);
    }

    //Player
    Entity playerEn;

    //Tilemap
    Entity tilemapEn;
    Grid<FloorCell> grid;
    TmxMap _map;
    PlaySceneInputManager _inputManager;
    UICanvas uiCanvas;
    Window pauseWindow;
    Button pauseBtt;

    public bool IsGamePaused => pauseBtt != null && pauseBtt.IsChecked;

    public void SetMoveCount(int moves)
    {
        _moveCount = moves;
        moveCountLabel?.SetText($"{moves}");
        if (MoveCount <= 0)
            _inputManager.AllowControl = false;

    }

    
    public override void OnAddedToEntity()
    {
        Debug.Assert(Scene.TryFindComponent(out _inputManager));
        Debug.Assert(Scene.TryFind("player", out playerEn));
        Debug.Assert(Scene.TryFind("tilemap", out tilemapEn));
        Debug.Assert(tilemapEn.TryGetComponent(out grid));
        Debug.Assert(ContentManager.TryGet("map", out _map));



        

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


        var defaultSkinName = ReUndoGame.DefaultUI;
        var gameSkin = ReUndoGame.GameSkin ;

        ///****************************************
        ///                 UI SYSTEM
        ///****************************************
        if (Entity.TryGetComponent<UICanvas>(out uiCanvas))
        {
            #region GameUI
            var butttonSize = 25;

            var topUI = new Table()
                .SetFillParent(true)
                .PadTop(5)
                .Top()
                ;

            //Left container cell
            topUI.Add(new Container())
                .SetExpandX()
                .Size(butttonSize);
                ;

            //Title Label
            bool hasName = _map.Properties.TryGetValue("name", out string mapName);
            string titleText = hasName ? mapName : $"Level {(Scene as PlayScene).levelID}";
            var titleUI = new Label(titleText, gameSkin)
                .SetWrap(false)
                .Center()
                ;
            topUI.Add(titleUI).SetFillX();
                ;

            //Animate title label UI
            Core.Schedule(1.75f, false, null, timer =>
            {
                var duration = 1.6f;
                Core.StartCoroutine(AnimateElementY(titleUI, titleUI.GetY(), -titleUI.GetHeight()-topUI.GetPadTop(), duration));
            });

            //pause button
            pauseBtt = new IconButton(gameSkin);
            pauseBtt.ProgrammaticChangeEvents = true;
            pauseBtt.OnChanged += isCheck =>
            {
                pauseBtt.ProgrammaticChangeEvents = false;
                TogglePause(isCheck);
                pauseBtt.ProgrammaticChangeEvents = true;
            };

            //Pause button cell
            topUI.Add(pauseBtt)
                .Size(Value.PercentWidth(2),Value.PercentHeight(2))
                .SetExpandX()
                .Right()
                .SetPadRight(5);
                
                ;
        
            //moveCount label
            moveCountLabel = new Label("MoveCount",gameSkin,ReUndoGame.TitleUI);
            moveCountLabel.FillParent = true;
            moveCountLabel.Bottom().MoveBy(0,-5) ;

            uiCanvas.Stage.AddElement(moveCountLabel);
            uiCanvas.Stage.AddElement(topUI); 
            #endregion

            
            pauseWindow = CreatePauseMenu(ReUndoGame.GameSkin, ReUndoGame.PlayScenePauseUI)
            ;
        }

        //Initialise move count number
        ///Reauire Score Label initialized
        if (_map != null && _map.Properties.TryGetValue("move_count", out string moveCntStr))
            SetMoveCount(int.Parse(moveCntStr));
        else
            SetMoveCount(_moveCount);

        _inputManager.OnEsc     += () =>
        {
            if (!IsGamePaused)
                PauseGame();
            else GotoLevelSelector();
        };
        _inputManager.OnUndo    += () => UndoCommand();
        _inputManager.OnRedo    += () => RedoCommand();
        
        _inputManager.OnRestart += RestartGame;

        OnExecute   += () => SetMoveCount(_moveCount - 1); //decrese movecount and disable input when move count reach zero
        OnExecute   += () => Raylib.PlaySound(ContentManager.Get<Sound>("sfx-walk"));
        OnExecute   += CheckWinCondition;
        OnUndo      += () => Raylib.PlaySound(ContentManager.Get<Sound>("sfx-undo"));
        OnUndo      += CheckWinCondition;
        OnRedo   += () => Raylib.PlaySound(ContentManager.Get<Sound>("sfx-redo"));
        OnRedo      += CheckWinCondition;
    }
    public override void  OnRemovedFromEntity()
    {
        //var cm = Core.Instance.Managers.Find(m => m is CoroutineManager) as CoroutineManager;
        //cm.TogglePauseAll(true);
    }

    #region UI Utils
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
    void ShowWindow(Window window, Stage stage)
    {
        stage.AddElement(window);
        window.Pack();

        var width = window.GetWidth();
        var height = window.GetHeight();

        var w = MathF.Round((stage.GetWidth() - width) / 2);
        var h = MathF.Round((stage.GetHeight() - height) / 2);
        window.SetPosition(w, h);
    }
    Window CreatePauseMenu(Skin skin, string style)
    {
        float buttonThrshold = 0;

        float titlePadBot = 10;
        float titlePadLeft = 10;

        float windowPadTop = 10f;

        float buttonsPadding = 1f;
        float buttonMinWidthPercent = 1.2f;
        Window window = new Window("PAUSE", skin, style)
            .SetMovable(false)
            .SetResizable(false)
            ;
        window.GetTitleTable().PadLeft(titlePadLeft).PadBottom(titlePadBot);
        window.Pad(4);

        #region Options Widget

        Table optionsWidget = new Table()
            .PadTop(windowPadTop);

        window.Add(optionsWidget);

        /// 1st row
        #region Resume Button
        //Element
        var resumeBtt = CreateOptionButton("Resume", skin, style);
        resumeBtt.OnClicked += (b) =>
        {
            ResumeGame();
            b.VerifyState();
        };

        //Cell
        optionsWidget.Add(resumeBtt)
            .SetMinWidth(Value.PercentWidth(buttonMinWidthPercent))
            .Pad(buttonsPadding)
            .SetColspan(2)
            .GrowX()
            ;
        #endregion Resume Button

        optionsWidget.Row();
        /// 2nd row

        #region Setting Button
        //Element
        var settingBtt = CreateOptionButton("setting",skin,style);
        settingBtt.OnClicked += (b) =>
        {
            var window = new Window("", skin,style);

            b.GetStage().AddElement(window);
            window.SetFillParent(true);

            window.Add(ReUndoGame.SettingWidget)
                .Pad(0);
            window.Row(); 
            window.Add(new TextButton("back", skin)).Center().SetPadBottom(20)
            .GetElement<TextButton>().AddLeftMouseListener(btt=> {
                window.Remove();
                b.VerifyState();

            });

        };

        //Cell
        optionsWidget.Add(settingBtt)
            .SetMinWidth(Value.PercentWidth(buttonMinWidthPercent))
            .Pad(buttonsPadding)
            .SetColspan(2)
            .GrowX()
            ; 
        #endregion

        optionsWidget.Row();
        /// 3rd row

        #region Restart Button

        //Element
        var restartBtt = CreateOptionButton("Restart (R)", skin, style);
        restartBtt.OnClicked += (b) => RestartGame();

        //Cell
        optionsWidget.Add(restartBtt)
            .SetMinWidth(Value.PercentWidth(buttonMinWidthPercent))
            .Pad(buttonsPadding)
            ;

        #endregion Restart Button

        #region Back Button
        //Element
        var backBtt = CreateOptionButton("Back", skin, style);
        backBtt.OnClicked += (b) =>
        {
            Raylib.PlaySound(ContentManager.Get<Sound>("sfx-click"));
            GotoLevelSelector();
        };

        //Cell
        optionsWidget.Add(backBtt)
            .SetMinWidth(Value.PercentWidth(buttonMinWidthPercent))
            .Pad(buttonsPadding)
            ; 
        #endregion

        #endregion Options Widget

        return window;

    } 

    TextButton CreateOptionButton(string text,Skin skin, string style)
    {
        var btt = new TextButton(text, skin, style);
        btt.ButtonBoundaryThreshold = 0;
        btt.OnHovered += (isOver) =>
        {
            if (isOver) Raylib.PlaySound(ContentManager.Get<Sound>("sfx-hover"));
        };

        return btt;
    }
    #endregion

    #region Utilities
    public void TogglePause(bool pause)
    {
        Console.WriteLine(pause);
            if (pause) PauseGame();
            else ResumeGame();
    }
    void ResumeGame()
    {
        Raylib.PlaySound(ContentManager.Get<Sound>("sfx-forward"));
        pauseWindow.Remove();

        pauseBtt.IsChecked = false;
        _inputManager.AllowCommand = _inputManager.AllowControl = !pauseBtt.IsChecked;

    }
    void PauseGame()
    {
        Raylib.PlaySound(ContentManager.Get<Sound>("sfx-back"));
        ShowWindow(pauseWindow, uiCanvas.Stage);

        pauseBtt.IsChecked = true;
        _inputManager.AllowCommand = _inputManager.AllowControl = !pauseBtt.IsChecked;


    }
    void OnWinGame()
    {
        Raylib.PlaySound(ContentManager.Get<Sound>("sfx-click_play"));
        Core.Schedule(0.3f, false, null, timer =>
        {
            var scene = (Scene as PlayScene);
            var nextLevelID = scene.levelID + 1;
            if (ReUndoGame.LevelsDictionary.TryGetValue(nextLevelID, out _))
            {
                if (Scene.TryFindComponent<PlaySceneInputManager>(out var inputManager))
                    inputManager.SetEnable(false);
                GoToLevel(nextLevelID);
            }
            else GotoLevelSelector();

            ///Set properties
            ReUndoGame.LevelsDictionary[nextLevelID].AllowReplay = true;
        });
    }
    public void RestartGame()
    {
        Raylib.PlaySound(ContentManager.Get<Sound>("sfx-click"));
        GoToLevel((this.Scene as PlayScene).levelID);
    }
    public void CheckWinCondition()
    {
        if (indicators.All(i => i.IsIndicated())) // Win
        {
            OnWinGame();

        }
        else /// Not win yet
        {
            var undones = indicators.Where(i => !i.IsIndicated());
            Console.WriteLine($"{undones.Count()} still missing");
        }
    }
    public void GoToLevel(int levelID)
    {
        Core.StartTransition(new SwipeTransition(() => new PlayScene(levelID)));
    }
    public void GotoLevelSelector()
    {
        Core.StartTransition(new SwipeTransition(() => new LevelSelectorScene())); 
    }
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

                character.RelocateRecursive(delta.X, delta.Y, false);

            }
        }

        void CommandSystem.ICommand.Redo()
        {
            foreach (KeyValuePair<Character, VectorInt2> movement in movements)
            {
                var character = movement.Key;
                var delta = movement.Value;

                character.RelocateRecursive(delta.X, delta.Y, false);

            }
        }

        void CommandSystem.ICommand.Undo()
        {
            var moves = movements.Reverse<KeyValuePair<Character, VectorInt2>>();
            foreach (KeyValuePair<Character, VectorInt2> movement in moves)
            {
                var character = movement.Key;
                var delta = -movement.Value;

                character.RelocateRecursive(delta.X, delta.Y, false);

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
    private Label moveCountLabel;

    public event Action? OnExecute, OnUndo, OnRedo;
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
            OnExecute?.Invoke();
        }
    }
    public void UndoCommand()
    {
        if (this.Entity.TryGetComponent(out CommandSystem cmd))
        {
            if (cmd.SendUndoCommand())
            {
                characterMovements.Clear();
                OnUndo?.Invoke();
            }
            
        }
    }
    public void RedoCommand()
    {
        if (this.Entity.TryGetComponent(out CommandSystem cmd))
        {
            if (cmd.SendRedoCommand())
            {
                characterMovements.Clear();
                OnRedo?.Invoke();
            }
        }
    }

    #endregion
    void ICustomInspectorImgui.OnInspectorGUI()
    {
        if(ImGuiNET.ImGui.Button("Reset Scene") )
        {
            var transition = new SwipeTransition(() => new PlayScene(0));
            transition.EnterDuration = transition.ExitDuration = 0.1f;
            transition.HoldDuration = 1f;
            Core.StartTransition(transition);
        }

        foreach (var sub in ContentManager.Instance.Resources)
        {
            ImGui.Text(sub.Key.ToString());
            foreach (var content in sub.Value)
            {
                ImGui.Text($"{content.Key} : {content.Value}");
            }
            ImGui.Separator();
        }
        ImGui.SliderInt("moveocunt",ref _moveCount,0,10);
    }

    /// <summary>
    /// Event Called from Character
    /// </summary>
    /// <param name="MovedEntity"></param>
    public void OnSingleCharacterMoved(int locX,int locY)
    {
        var cell = grid.GetCell(locX,locY);

        var indicators = from o in cell.Objects where
                         o.Entity.HasComponent<Indicator>() && 
                         o.Entity.GetComponent<Character>().Layer == PlayScene.INDICATOR_CHRACTER_LAYER 
                         select o.Entity.GetComponent<Indicator>();


    }
}

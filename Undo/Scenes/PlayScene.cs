using Engine;
using Engine.SceneManager;
using Engine.TiledSharp;
using Engine.TiledSharp.Extension;
using Engine.UI;
using Raylib_cs;
using Undo;

public class PlayScene : Scene
{
    public readonly int levelID = -1;
    public const string WALL_LAYER = "wall";
    public const string SHADOW_LAYER = "shadow";
    public const string FLOOR_LAYER = "floor";
    public const string ENTITIES_LAYER = "characters"; 
    public const string INDICATORS_LAYER = "indicators"; 
    public const string PLAYER_NAME = "player";

    public const string BOX_NAME = "box";
    public const string UNDO_PROPERTY = "allow_undo";
    public const string SOCKET_PROPERTY = "socketID";

    public const int OBJECT_CHRACTER_LAYER = 0;
    public const int INDICATOR_CHRACTER_LAYER = 1;

    protected PlaySceneInputManager inputManager;

    public TmxMap map;
    public PlayScene(int levelID) 
        : base($"level {levelID}",ReUndoGame.DEFAULT_SCENE_WIDTH,ReUndoGame.DEFAULT_SCENE_HEIGHT,Color.DARKBLUE,Color.BLACK)
    {
        this.levelID = levelID;
    }


    public override void OnLoad()
    {
        var a = ContentManager.Instance;
        map = ContentManager.Load<TmxMap>("map", () => new TmxMap($"{ReUndoGame.tilemapDir}\\{ReUndoGame.LevelsDictionary[levelID].Name}.tmx") );
    }
    public override void OnUnload()
    {
        map.Unload();
        ContentManager.Unload<TmxMap>("map");
    }
    public override void OnBegined()
    {
        Filter                          = Raylib_cs.TextureFilter.TEXTURE_FILTER_POINT;
        TmxObjectGroup entitiesGroup    = map.FindObjectGroup(ENTITIES_LAYER);
        TmxObjectGroup indicatorsGroup  = map.FindObjectGroup(INDICATORS_LAYER);
        PlaySceneInputManager a;

        var managers= CreateEntity("manager")
            .AddComponent<GameMananger>() 
            .AddComponent<PlaySceneInputManager>(out inputManager)
            .AddComponent<CommandSystem>()
            .AddComponent<UICanvas>()
                .SetRenderOrder(float.MaxValue)
            .Entity
            ;

        inputManager.AllowUndo = map.Properties.TryGetValue("game_AllowUndo", out string isAllowUndoStr) ?
            (bool.TryParse(isAllowUndoStr , out bool isAllowUndo) ? isAllowUndo : true) : true;
        inputManager.AllowRedo = map.Properties.TryGetValue("game_AllowRedo", out string isAllowRedoStr) ?
            (bool.TryParse(isAllowRedoStr, out bool isAllowRedo) ? isAllowRedo : true) : true;

        var tilemapEn = CreateEntity("tilemap")
            .AddComponent(new TileMapRenderer(map, FLOOR_LAYER, SHADOW_LAYER, WALL_LAYER))
            .AddComponent(new Grid<FloorCell>(map.Width, map.Height, map.TileWidth, map.TileHeight))
            .Entity
            .ScaleTo(2,2,1);
            ;
        
        #region Goals
        foreach (TmxObject obj in indicatorsGroup.Objects)
        {
            CreateChildEntity(tilemapEn,$"indicator_{obj.Tile.Gid}")
                .AddComponent<GridObject>()
#if true
                .AddComponent(new Indicator()
                {
                    Pushable = false,
                    Undoable = false,
                    Layer = 1,
                    SocketID = obj.Properties[SOCKET_PROPERTY]
                }) 
#endif
                .AddComponent<SpriteRenderer>(new(obj.Tile.GetSprite()))
                .SetAlpha((float)indicatorsGroup.Opacity)

                .Entity
                .MoveTo(obj.X + obj.Tile.Tileset.TileWidth / 2f, obj.Y - obj.Tile.Tileset.TileWidth / 2f)

                ;

        } 
        #endregion


        #region Player
        var playerObjs = entitiesGroup.Objects.Where(o => o.Name.Contains(PLAYER_NAME));
        foreach (var playerObj in playerObjs)
        {
            map.TilesetLocationFromGid(playerObj.Tile.Gid, out int startLocX, out int startLocY);
            var playerSprite = playerObj.Tile.Tileset.Image.TextureAtlas.Sprites[$"{startLocX},{startLocY}"];

            Entity player = CreateChildEntity(tilemapEn, "player")
                .AddComponent<GridObject>() /// Associate with Grid<T>
                .AddComponent<Player>(new Player()
                {
                    SocketID = playerObj.Properties[SOCKET_PROPERTY]
                })

                //.AddComponent(new CircleRenderer(7, Color.WHITE))
                //.AddComponent(new TileMapSnap(tilemapEn, TERRAIN_LAYER))

                .Entity
                .MoveTo(playerObj.X + playerObj.Tile.Tileset.TileWidth / 2f, playerObj.Y - playerObj.Tile.Tileset.TileHeight / 2f)
                ;


            CreateChildEntity(player,"player_sprite")
                .AddComponent(new SpriteRenderer(playerSprite))
            ;
        }
        #endregion


        #region Box
        var boxObjs = entitiesGroup.Objects.Where(o => o.Name.Contains(BOX_NAME));
        foreach (TmxObject obj in boxObjs)
        {
            obj.Properties.TryGetValue(UNDO_PROPERTY,out string allowUndoString);

            Entity boxEntity = CreateChildEntity(tilemapEn, obj.Name)
            .AddComponent<GridObject>()
            .AddComponent<Character>(new Character()
            {
                Pushable = true,
                Undoable = Convert.ToBoolean(allowUndoString),
                Layer = OBJECT_CHRACTER_LAYER,
                SocketID = obj.Properties[SOCKET_PROPERTY]
            })


            .Entity
            .MoveTo(obj.X + obj.Tile.Tileset.TileWidth / 2f, obj.Y - obj.Tile.Tileset.TileWidth / 2f)
            ;

            ///Render in child
            CreateChildEntity(boxEntity, $"{obj.Name}_render")
            .AddComponent(new SpriteRenderer(obj.Tile.GetSprite()))
            ;

        }
        #endregion

        //TODO: Add Teleport
        //TODO: Add pushable block
        //TODO: Add one way path
        //TODO: Add transition
        //TODO: Add main menul
        //TODO: Add level selector

    }
}


public class ModifiedPlayScene : PlayScene
{
    private readonly bool _allowUndo;
    private readonly bool _allowRedo;

    public ModifiedPlayScene(int levelID, bool allowUndo,bool allowRedo) : base(levelID)
    {
        this._allowUndo = allowUndo;
        this._allowRedo = allowRedo;
    }

    public override void OnBegined()
    {
        base.OnBegined();

    }
}
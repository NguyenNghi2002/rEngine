

using Engine;
using Engine.DefaultComponents.Render.Primitive;
using Engine.SceneManager;
using Engine.Texturepacker;
using Engine.TiledSharp;
using Engine.TiledSharp.Extension;
using Raylib_cs;
using Undo;

new UndoGame().Run();

public class UndoGame : Engine.Core
{
    public override void Initialize()
    {
        base.Initialize();

        //Create Scene
        Scene = new PlayScene(0);
        Scene.Filter = Raylib_cs.TextureFilter.TEXTURE_FILTER_POINT;

        Managers.Add(new ImguiEntityManager());
    }
}

public class PlayScene : Scene
{
    int levelID = -1;
    public const string WALL_LAYER = "wall";
    public const string SHADOW_LAYER = "shadow";
    public const string FLOOR_LAYER = "floor";
    public PlayScene(int levelID) 
        : base($"undo",160,160,Raylib_cs.Color.DARKBLUE,Raylib_cs.Color.BLACK)
    {
        this.levelID = levelID;
    }

    public TmxMap map;
    public override void OnLoad()
    {
        map = ContentManager.Load<TmxMap>("map", ()=> new TmxMap( ".contents\\tilemaps\\map\\tileset.tmx") );
    }
    public override void OnBegined()
    {
        var managers = CreateEntity("manager")
            .AddComponent<GameMananger>()
            .AddComponent<CommandSystem>()
            .Entity
            ;
        
        var tilemapEn = CreateEntity("tilemap")
            .AddComponent(new TileMapRenderer(map, FLOOR_LAYER,SHADOW_LAYER, WALL_LAYER))
            .AddComponent(new Grid<FloorCell>(map.Width,map.Height,map.TileWidth,map.TileHeight))
            .Entity
            ;

        var startObj = map.FindObjectGroup("locations").Objects["start"];
        Entity player = CreateChildEntity(tilemapEn,"player")
            .AddComponent<GridObject>() /// Associate with Grid<T>
            .AddComponent<Player>()

            .AddComponent(new CircleRenderer(7, Color.WHITE))
            //.AddComponent(new TileMapSnap(tilemapEn, TERRAIN_LAYER))


            .Entity
            .MoveTo(startObj.X,startObj.Y)
            ;
        
        var boxObjs = map.ObjectGroups["locations"].Objects.Where(o =>o.Name.Contains("box"));
        foreach (TmxObject obj in boxObjs)
        {
            var isUndoable = Convert.ToBoolean(obj.Properties["allow_undo"]);

            map.TilesetLocationFromGid(obj.Tile.Gid,out int x, out int y);
            CreateChildEntity(tilemapEn, "box")
            .AddComponent<GridObject>()
            .AddComponent<Character>(new Character()
            {
                Pushable = true,
                Undoable = isUndoable
            })
            .AddComponent<Pushable>()
            
            //.AddComponent(new CircleRenderer(5, isUndoable ? Color.BROWN : Color.DARKGRAY))
            .AddComponent(new SpriteRenderer(obj.Tile.Tileset.Image.TextureAtlas.Sprites[$"{x},{y}"]))

            .Entity
            .MoveTo(obj.X, obj.Y)
            ;

        }
        
        //TODO: Add Goal
        //TODO: Add Teleport
        //TODO: Add pushable block
        //TODO: Add one way path
        //TODO: Add transition
        //TODO: Add main menul
        //TODO: Add level selector

    }
}
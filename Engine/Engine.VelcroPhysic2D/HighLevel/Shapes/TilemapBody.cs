using Engine.TiledSharp;
using Engine.Velcro.Unit;
using Genbox.VelcroPhysics.Collision.Shapes;
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Factories;
using Genbox.VelcroPhysics.Shared;
using Genbox.VelcroPhysics.Tools.Cutting;
using Genbox.VelcroPhysics.Tools.Cutting.Simple;
using Genbox.VelcroPhysics.Tools.PolygonManipulation;
using Genbox.VelcroPhysics.Tools.Triangulation.Delaunay;
using Genbox.VelcroPhysics.Tools.Triangulation.Delaunay.Util;
using Genbox.VelcroPhysics.Tools.Triangulation.TriangulationBase;
using Genbox.VelcroPhysics.Utilities;
using Raylib_cs;
using System.Diagnostics;
using System.Numerics;
using System.Transactions;

namespace Engine.Velcro
{
    using MVec2 = Microsoft.Xna.Framework.Vector2;

    /// <summary>
    /// Require to have Tilemap
    /// </summary>
    public class TilemapBody : VGenericBody
    {

        string _colliderLayerName;
        bool _allowTransform;

        public TmxMap Map;

        private TilemapBody(Body body) : base(body)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="collideLayerName">name of tile layer/ object group layer </param>
        public TilemapBody(string collideLayerName,bool allowEntityTransform)
        {
            _colliderLayerName = collideLayerName;
            _allowTransform = allowEntityTransform;
        }

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();

            if (Entity.TryGetComponent<TileMapRenderer>(out var tilemap))
            {
                Map = tilemap.Map;
                SetColliderLayer(Map.Layers[_colliderLayerName]);
                Body.BodyType = BodyType.Static;
                //Body.Restitution = 1f;
                
            }
            
        }

        void CreateBasicFixture(TmxObject obj)
        {
            var scale = Entity.Transform.Scale.ToMVec2();

            var objPos = new MVec2((float)obj.X, (float)obj.Y) * scale;
            var objHScale = new MVec2((float)(obj.Width /2f), (float)(obj.Height/2f)) * scale;
            var objRot = ((float)obj.Rotation) * Raylib.DEG2RAD;

            var offset = (Raymath.Vector2Rotate((objHScale), objRot) + objPos).ToMVec2();

            var verts = PolygonUtils.CreateRectangle(
                objHScale.X * VConvert.DisplayToSim,
                objHScale.Y * VConvert.DisplayToSim,
                offset * VConvert.DisplayToSim,
                objRot
                );
            FixtureFactory.AttachPolygon(verts,0.01f,Body,this.Entity);
        }

        void CreatePolygonFixture(TmxObject obj,TriangulationAlgorithm triangulationMode)
        {
            var objPos = new MVec2((float)obj.X, (float)obj.Y);
            var objRot = ((float)obj.Rotation) * Raylib.DEG2RAD;

            var verts = new Vertices(obj.Points.Select(
                p =>
                {
                    var point = new Vector2((float)p.X, (float)p.Y);
                    var offsetPoint = (Raymath.Vector2Rotate(point, objRot) + objPos).ToMVec2();
                    return offsetPoint * VConvert.DisplayToSim;
                }));
            verts.Scale(Entity.Transform.Scale.ToMVec2());

            ///Split vertices to many rectangulars
            var remakeVerts = Triangulate.ConvexPartition(verts, triangulationMode);

            /// Add multiple vertices 
            var fixtureList = FixtureFactory.AttachCompoundPolygon(remakeVerts, 0.01f, Body);
            fixtureList.ForEach(f => f.UserData = this.Entity); // this component as userdata
        }
        void CreatePolygonFixture(Vector2 origin, Vector2 tileScale, bool vFlip, bool hFlip, bool dFlip, TmxObject obj)
        {
            var worldTileCenter = origin + tileScale / 2f;

            var objPos = new MVec2((float)obj.X, (float)obj.Y);
            var objRot = ((float)obj.Rotation) * Raylib.DEG2RAD;

            var verts = new Vertices(obj.Points.Select(
                p=>
                {
                    var point = new Vector2((float)p.X, (float)p.Y) ;
                    var offsetPoint =  (Raymath.Vector2Rotate(point ,objRot) + origin + objPos).ToMVec2() ;

                    if(dFlip)
                    {
                        Vector2Ext.FlipX(ref offsetPoint, worldTileCenter.ToMVec2());
                        offsetPoint = RaymathF.Vector2Rotate(worldTileCenter, offsetPoint, -90 * Raylib.DEG2RAD).ToMVec2();

                    }
                    if (hFlip)
                        Vector2Ext.FlipX(ref offsetPoint,worldTileCenter.ToMVec2());
                    if (vFlip)
                        Vector2Ext.FlipY(ref offsetPoint,worldTileCenter.ToMVec2());


                    return offsetPoint * VConvert.DisplayToSim;
                }));


            ///Split vertices to many rectangulars
            var remakeVerts = Triangulate.ConvexPartition(verts,TriangulationAlgorithm.Bayazit);

            /// Add multiple vertices 
            var fixtureList = FixtureFactory.AttachCompoundPolygon(remakeVerts,0.01f,Body);

            fixtureList.ForEach(f => f.UserData = this.Entity); // this component as userdata
        }
        void CreateBasicFixture(Vector2 origin,Vector2 tileScale,bool vFlip, bool hFlip, bool dFlip, TmxObject obj)
        {
            var worldTileCenter = origin + tileScale / 2f;

            var objHScale = new MVec2((float)(obj.Width /2f), (float)(obj.Height/2f));
            var objPos = new MVec2((float)obj.X, (float)obj.Y); //object local tile position
            var objRot = ( (float)obj.Rotation) * Raylib.DEG2RAD;

            var offset = (Raymath.Vector2Rotate((objHScale),objRot) + origin + objPos ).ToMVec2() ;

            var verts = PolygonUtils.CreateRectangle(
                objHScale.X * VConvert.DisplayToSim,
                objHScale.Y * VConvert.DisplayToSim,
                offset * VConvert.DisplayToSim,
                objRot
                ) ;

            if (vFlip || hFlip || dFlip )
            {
                var simWorldTileCenter = worldTileCenter.ToMVec2() * VConvert.DisplayToSim;
                for (int i = 0; i < verts.Count; i++)
                {
                    if (dFlip)
                    {
                        verts[i] = Vector2Ext.FlipX(verts[i], simWorldTileCenter);
                        verts[i] = RaymathF.Vector2Rotate(simWorldTileCenter.ToSVec2(), verts[i],-90 * Raylib.DEG2RAD).ToMVec2();
                    }
                    if (hFlip)
                        verts[i] = Vector2Ext.FlipX(verts[i], simWorldTileCenter);
                    if (vFlip)
                        verts[i] = Vector2Ext.FlipY(verts[i], simWorldTileCenter);
                }
            }

            //verts.Translate(offset * VConvert.DisplayToSim);

            var fix = FixtureFactory.AttachPolygon(verts,0.01f,Body,this.Entity);
        }


        /// <summary>
        /// Incase layer tile does not have tmxObjects
        /// </summary>
        /// <param name="layerTile"></param>
        void CreateDefaultFixture(TmxLayerTile layerTile)
        {
            var w = layerTile.Tileset.TileWidth;
            var h = layerTile.Tileset.TileHeight;
            var x = layerTile.X * w;
            var y = layerTile.Y * h;

            FixtureFactory.AttachRectangle(
                w * VConvert.DisplayToSim,
                h * VConvert.DisplayToSim,
                0.1f,
                (new Vector2(x,y) + new Vector2(w/2f,h/2f)).ToMVec2() * VConvert.DisplayToSim,
                Body, this);
        }

        public void SetColliderLayer(ITmxLayer tmxLayer)
        {
            if(!Map.Layers.Contains(tmxLayer))
                throw new("Invalid Layer");
            if (tmxLayer is TmxObjectGroup objGroup)  /// For object group
            {
                foreach (TmxObject obj in objGroup.Objects)
                {
                    
                    switch (obj.ObjectType )
                    {
                        case TmxObjectType.Basic:
                            if (obj.Width == 0 && obj.Height == 0) continue;
                            CreateBasicFixture(obj); break;
                        case TmxObjectType.Polygon: 
                            CreatePolygonFixture(obj, TriangulationAlgorithm.Bayazit); break;
                        default:break;
                    }
                }
            }
            else if (tmxLayer is TmxLayer layer)  /// For object group in tile layer
            {

                var rec = new Rectangle();
                foreach (TmxLayerTile layerTile in layer.Tiles.Where(t =>  t.Gid != 0 && t.GetTilesetTile() != null))
                {
                    var w = layerTile.Tileset.TileWidth;
                    var h = layerTile.Tileset.TileHeight;
                    var x = layerTile.X * w;
                    var y = layerTile.Y * h;
                    var topleft = new Vector2(x, y);
                    var tileScale = new Vector2(w,h);

                    TmxList<TmxObjectGroup> objsGroupList = layerTile.GetTilesetTile().ObjectGroups;
                    if (objsGroupList != null )
                    {
                        foreach (TmxObjectGroup og in objsGroupList)
                        {
                            foreach (TmxObject obj in og.Objects)
                            {
                                switch (obj.ObjectType)
                                {
                                    case TmxObjectType.Basic:
                                        CreateBasicFixture(topleft,tileScale,layerTile.VerticalFlip,layerTile.HorizontalFlip,layerTile.DiagonalFlip,obj);
                                        break;
                                    case TmxObjectType.Tile:
                                        break;
                                    case TmxObjectType.Ellipse:
                                        break;
                                    case TmxObjectType.Polygon:
                                        CreatePolygonFixture(topleft, tileScale, layerTile.VerticalFlip, layerTile.HorizontalFlip, layerTile.DiagonalFlip, obj);
                                        break;
                                    case TmxObjectType.Polyline:
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }
                    else
                    {
#if false
                        rec.width = layerTile.Tileset.TileWidth;
                        rec.height = layerTile.Tileset.TileHeight;
                        rec.x = layerTile.X * rec.width;
                        rec.y = layerTile.Y * rec.height;

                        FixtureFactory.AttachRectangle(
                            rec.width * VConvert.DisplayToSim,
                            rec.height * VConvert.DisplayToSim,
                            0.1f,
                            (rec.TopLeft() + rec.Scale().Half()).ToMVec2() * VConvert.DisplayToSim,
                            Body, this); 
#endif
                    }


                }
            }


#if false
            List<Vertices> triangles = new();
            if (Body.FixtureList.Count > 0)
            {
                var fix = Body.FixtureList.Where(f => f.Shape is PolygonShape);
                for (int i = fix.Count() - 1; i >= 0; i--)
                {
                    var f = fix.ElementAt(i);
                    if (f.Shape is PolygonShape poly)
                        triangles.AddRange(Triangulate.ConvexPartition(poly.Vertices, TriangulationAlgorithm.Earclip));
                    Body.RemoveFixture(f);
                }
                var unionedVerts = SimpleCombiner.PolygonizeTriangles(triangles);
                FixtureFactory.AttachCompoundPolygon(unionedVerts, 0.01f, Body);

            } 
#endif
        }
    }
}

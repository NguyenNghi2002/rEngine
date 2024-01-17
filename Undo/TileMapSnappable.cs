using Engine;
using Engine.TiledSharp;
using Engine.TiledSharp.Extension;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Undo
{
    /// <summary>
    /// This component will snap to selected tilemap 
    /// </summary>
    internal class TileMapSnap : Component,ICustomInspectorImgui
    {

        Entity _initialTilemapEntity;


        TileMapRenderer _map;
        TmxLayer? _layer;
        VectorInt2 _location;
        public VectorInt2 Location => _location;

        string _layerName;
        public TileMapSnap(Entity tilemapEntity,string tmxLayerName)
        {
            this._initialTilemapEntity = tilemapEntity;
            this._layerName = tmxLayerName;
        }

        public override void OnAddedToEntity()
        {
            if(_initialTilemapEntity.TryGetComponent<TileMapRenderer>(out _map))
            {
                _layer = _map.Map.FindLayer(_layerName) as TmxLayer;
                MoveToLocationFromWorldPoint(Transform.Position2);
            }
        }

        public override void OnTransformChanged(Transformation.Component component)
        {
            if (component is Transformation.Component.Position)
            {
                MoveToLocationFromWorldPoint(Transform.Position2);
            }
        }

        void MoveToLocationFromWorldPoint(Vector2 worldPoint)
        {
            TmxLayerTile? tile = _layer.GetTileFromWorldPoint(worldPoint);
            bool isMovable = tile != null && _map.Map.IsLocalInside(tile.X, tile.Y);
            if (isMovable)
            {
                var tileSize = new Vector2(_map.Map.TileWidth, _map.Map.TileHeight);
                var layerOffset = new Vector2((float)_layer.OffsetX, (float)_layer.OffsetY);

                Transform.Position2 = tile.GetLocalPoint() + layerOffset + tileSize / 2f;
                _location = new VectorInt2(tile.X, tile.Y);
            }
        }

        public void MoveTo(int x, int y)
        {
            if (_map.Map.IsLocalInside(x,y))
            {
                var layerOffset = new Vector2((float)_layer.OffsetX, (float)_layer.OffsetY);
                _map.Map.ToWorldPoint(x,y,out Vector2 point);

                Transform.Position2 = point + layerOffset;
                _location = new VectorInt2(x, y);
            }
        }

        public void Move(int deltaX,int deltaY)
        {
            
            MoveTo(_location.X+deltaX, _location.Y + deltaY);
        }

        public bool IsGid(int x, int y, params int[] gids)
            => _map.Map.IsLocalInside(x, y) && gids.Contains(_layer.GetTile(x, y).Gid);

        public void OnInspectorGUI()
        {
            ImGui.Text($"_lcation: {_location}");
        }
    }

    
}

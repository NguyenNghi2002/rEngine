using Engine.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Engine.TiledSharp.Extension
{
    public static class TiledUtil
    {
        public static ITmxLayer? FindLayer(this TmxMap self,string layerName)
        {
            if (self.Layers.TryGetValue(layerName, out var found))
                return found;

            foreach (TmxGroup group in self.Groups)
            {
                ITmxLayer foundLayer = FindLayer(group, layerName);
                if (foundLayer != null)
                    return foundLayer ;
            }
            return null;
        }
        /// <summary>
        /// tries to get <see cref="TmxObjectGroup"/> with layer group name.
        /// </summary>
        /// <param name="groupObjectName">name of <see cref="TmxObjectGroup"/></param>
        /// <returns>Found <see cref="TmxObjectGroup"/>, otherwise return <see langword="null"/>.</returns>
        public static TmxObjectGroup? FindObjectGroup(this TmxMap self, string groupObjectName)
        {
            if (self.ObjectGroups.TryGetValue(groupObjectName, out var found))
                return found;

            foreach (TmxGroup group in self.Groups)
            {
                TmxObjectGroup foundObjectGroup = FindObjectGroup(group, groupObjectName);
                if (foundObjectGroup != null)
                    return foundObjectGroup;
            }
            return null;
        }
        public static bool TryFindLayer<T>(this TmxMap self,string layerName,out T foundLayer) where T : ITmxLayer
        {
            foundLayer =  (T)FindLayer(self,layerName) ;
            return foundLayer != null;
        }
        public static bool TryFindObjectGroup(this TmxMap self, string groupObjectName, out TmxObjectGroup? objectGroup)
        {
            objectGroup = FindObjectGroup(self, groupObjectName);
            return objectGroup != null;
        }
        public static bool IsLocalInside(this TmxMap map,int x,int y) => (x >= 0 && x < map.Width) && (y >= 0 && y < map.Height);
        public static void ToWorldPoint(this TmxMap self,int X,int Y,out Vector2 worldPoint)
        {
            worldPoint = new Vector2(X*self.TileWidth,Y*self.TileHeight);
        }
        public static void ToLocation(this TmxMap self, Vector2 worldPoint,out int X,out int Y)
        {
            X = (int)MathF.Floor(worldPoint.X / self.TileWidth);
            Y = (int)MathF.Floor(worldPoint.Y / self.TileHeight);
        }


        /// <summary>
        /// tries to get <see cref="TmxObjectGroup"/> with layer group name.
        /// </summary>
        /// <param name="groupObjectName">name of <see cref="TmxObjectGroup"/></param>
        /// <param name="objectGroup"></param>
        /// <returns><see langword="true"/> if found <see cref="TmxObjectGroup"/>, otherwise return <see langword="false"/>.</returns>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ITmxLayer? FindLayer(this TmxGroup self, string name)
        {
            if (self.Layers.TryGetValue(name, out var layer))
                return layer;

            //Loop each group
            foreach (var group in self.Groups)
            {
                //Go inside current group
                var found = FindObjectGroup(group, name);
                if (found != null)
                    return found;
            }
            return null;
        }
        public static TmxObjectGroup FindObjectGroup(this TmxGroup self, string name)
        {
            if (self.ObjectGroups.TryGetValue(name, out var objGroup))
                return objGroup;

            //Loop each group
            foreach (var group in self.Groups)
            {
                //Go inside current group
                var found = FindObjectGroup(group, name);
                if (found != null)
                    return found;
            }
            return null;
        }


        /// <summary>
        /// Get location of selected tile from world point
        /// </summary>
        /// <param name="self"></param>
        /// <param name="worldPoint"></param>
        /// <returns></returns>
        public static VectorInt2 GetLocation(this TmxLayer self, Vector2 worldPoint)
        {
            int x = (int)MathF.Floor(worldPoint.X / self.Map.TileWidth);
            int y = (int)MathF.Floor(worldPoint.Y / self.Map.TileHeight);
            return new VectorInt2(x, y);
        }
        public static TmxLayerTile? GetTileFromWorldPoint(this TmxLayer self, Vector2 worldPoint)
        {
            int x = (int)MathF.Floor(worldPoint.X / self.Map.TileWidth);
            int y = (int)MathF.Floor(worldPoint.Y / self.Map.TileHeight);
            return self.GetTile(x,y);
        }

        /// <summary>
        /// Relative to the layer. This will return top left point
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static Vector2 GetLocalPoint(this TmxLayerTile self)
            => new Vector2(self.X * self.Map.TileWidth, self.Y * self.Map.TileHeight);
        public static Engine.Texturepacker.Sprite GetSprite(this TmxLayerTile self)
        {
            self.Map.TilesetLocationFromGid(self.Gid, out int x, out int y);
            return self.Tileset.Image.TextureAtlas.Sprites[$"{x},{y}"];
        }

    }

}
using Engine.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Engine.TiledSharp.Extension
{
    public static class TiledUtil
    {
        public static bool TryFindLayer<T>(this TmxMap map,string layerName,out T foundLayer) where T : ITmxLayer
        {
            foundLayer =  (T)FindLayer(map,layerName) ;
            return foundLayer != null;
        }
        public static ITmxLayer? FindLayer(this TmxMap map,string layerName)
        {
            if (map.Layers.TryGetValue(layerName, out var found))
                return found;

            foreach (TmxGroup group in map.Groups)
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
        /// <param name="objectGroup"></param>
        /// <returns><see langword="true"/> if found <see cref="TmxObjectGroup"/>, otherwise return <see langword="false"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ITmxLayer? FindLayer(this TmxGroup tmxGroup, string name)
        {
            if (tmxGroup.Layers.TryGetValue(name, out var layer))
                return layer;

            //Loop each group
            foreach (var group in tmxGroup.Groups)
            {
                //Go inside current group
                var found = FindObjectGroup(group, name);
                if (found != null)
                    return found;
            }
            return null;
        }


        public static bool TryFindObjectGroup(this TmxMap map, string groupObjectName, out TmxObjectGroup? objectGroup)
        {
            objectGroup = FindObjectGroup(map, groupObjectName);
            return objectGroup != null;
        }

        /// <summary>
        /// tries to get <see cref="TmxObjectGroup"/> with layer group name.
        /// </summary>
        /// <param name="groupObjectName">name of <see cref="TmxObjectGroup"/></param>
        /// <returns>Found <see cref="TmxObjectGroup"/>, otherwise return <see langword="null"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TmxObjectGroup? FindObjectGroup(this TmxMap map, string groupObjectName)
        {
            if (map.ObjectGroups.TryGetValue(groupObjectName, out var found))
                return found;

            foreach (TmxGroup group in map.Groups)
            {
                TmxObjectGroup foundObjectGroup = FindObjectGroup(group, groupObjectName);
                if (foundObjectGroup != null)
                    return foundObjectGroup;
            }
            return null;
        }
        public static TmxObjectGroup FindObjectGroup(this TmxGroup tmxGroup, string name)
        {
            if (tmxGroup.ObjectGroups.TryGetValue(name, out var objGroup))
                return objGroup;

            //Loop each group
            foreach (var group in tmxGroup.Groups)
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
        /// <param name="layer"></param>
        /// <param name="worldPoint"></param>
        /// <returns></returns>
        public static VectorInt2 GetLocation(this TmxLayer layer, Vector2 worldPoint)
        {
            int x = (int)MathF.Floor(worldPoint.X / layer.Map.TileWidth);
            int y = (int)MathF.Floor(worldPoint.Y / layer.Map.TileHeight);
            return new VectorInt2(x, y);
        }

        public static TmxLayerTile? GetTileFromWorldPoint(this TmxLayer layer, Vector2 worldPoint)
        {
            int x = (int)MathF.Floor(worldPoint.X / layer.Map.TileWidth);
            int y = (int)MathF.Floor(worldPoint.Y / layer.Map.TileHeight);
            return layer.GetTile(x,y);
        }

        /// <summary>
        /// Relative to the layer. This will return top left point
        /// </summary>
        /// <param name="tile"></param>
        /// <returns></returns>
        public static Vector2 GetLocalPoint(this TmxLayerTile tile)
            => new Vector2(tile.X * tile.Map.TileWidth, tile.Y * tile.Map.TileHeight);
        public static void ToWorldPoint(this TmxMap self,int X,int Y,out Vector2 worldPoint)
        {
            worldPoint = new Vector2(X*self.TileWidth,Y*self.TileHeight);
        }
        public static void ToLocation(this TmxMap self, Vector2 worldPoint,out int X,out int Y)
        {
            X = (int)MathF.Floor(worldPoint.X / self.TileWidth);
            Y = (int)MathF.Floor(worldPoint.Y / self.TileHeight);
        }

        public static bool IsLocalInside(this TmxMap map,int x,int y) => (x >= 0 && x < map.Width) && (y >= 0 && y < map.Height);
    }

}
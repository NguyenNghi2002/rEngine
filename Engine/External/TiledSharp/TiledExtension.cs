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

    }

}
using Raylib_cs;
using Engine;
using System.Diagnostics;
using System.Numerics;
using Engine.TiledSharp;
using System.Runtime.CompilerServices;
using Engine.Renderering;
using Engine.TiledSharp.Extension;
using Engine.AI.Pathfinding;
using ImGuiNET;
using static System.Net.Mime.MediaTypeNames;

namespace Engine
{
    public class TileMapRenderer : RenderableComponent,ICustomInspectorImgui
    {
        public TmxMap Map;

        int[] displayLayerIndices;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tiledMap"></param>
        /// <param name="renderLayerNames">in order of behind to top</param>
        public TileMapRenderer(TmxMap tiledMap,params string[] renderLayerNames)
        {
            Map = tiledMap;
            SetDisplayLayer(renderLayerNames);
        }

        public override void Render()
        {
            if(displayLayerIndices == null)
            {
                TiledRendering.DrawMap(Map, Vector2.Zero, Transform.Position.ToVec2(), Transform.Scale.ToVec2());
            }
            else
            {
                foreach (int layerIndex in displayLayerIndices)
                {
                    var layer = Map.Layers[layerIndex];
                    if (layer.Visible)
                        TiledRendering.DrawLayer(layer, Vector2.Zero, Transform.Position.ToVec2(), Transform.Scale.ToVec2());
                }
            }
            //TiledRendering.DrawMap(Map,Vector2.Zero,Transform.Position.ToVec2(),Transform.Scale.ToVec2());
            //_tilemap.Draw();

        }
        public override void OnRemovedFromEntity()
        {
            Map.Unload();
            Map = null;
        }

        /// <param name="renderLayerNames">in order of behind to top</param>
        public TileMapRenderer  SetDisplayLayer(params string[] renderLayerNames)
        {
            int[] result = new int[renderLayerNames.Length];
            for (int i = 0; i < result.Length; i++)
            {
                var name = renderLayerNames[i];
                if (Map.TryFindLayer(name, out ITmxLayer layer))
                    result[i] = Map.Layers.IndexOf(layer);
                else
                    throw new Exception($"Can't find \"{name}\" layer");
            }
            displayLayerIndices = result;
            return this;
        }



        const int COLLUM_COUNT = 2;
        void ICustomInspectorImgui.OnInspectorGUI()
        {

            if (ImGui.BeginTable($"tilesets###{Entity.ID}", COLLUM_COUNT,ImGuiTableFlags.BordersH))
            {
                ImGui.TableNextRow();
                foreach (TmxTileset tileset in Map.Tilesets)
                {
                    /// Compact row into group

                    ImGui.TableNextColumn();
                    var texture = tileset.Image.TextureAtlas.Texture.Value;
                    ImGui.Image((IntPtr)texture.id, texture.Scale() / 10f);
                    PopUpTileset(tileset);

                    ImGui.TableNextColumn();
                    ImGui.Text($"{tileset.Name}");
                }
                
            }
            ImGui.EndTable();
            ImGuiNET.ImGui.GetWindowDrawList().AddCircle(Vector2.Zero,20, (uint)Raylib.ColorToInt(Color.WHITE),50); ;
        }

        static void PopUpTileset(TmxTileset tileset)
        {
            if (ImGui.IsItemHovered(ImGuiHoveredFlags.DelayShort))
            {
                ImGui.BeginTooltip();
                var texture = tileset.Image.TextureAtlas.Texture.Value;
                ImGui.Image((IntPtr)texture.id, texture.Scale());
                ImGui.EndTooltip();
            }
        }


    }



    

}
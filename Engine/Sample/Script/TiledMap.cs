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

namespace Engine.External
{
    public class TileMap : RenderableComponent,ICustomInspectorImgui
    {
        public TmxMap Map;

        List<ITmxLayer> displayLayer = new List<ITmxLayer>();


        public TileMap(TmxMap tiledMap,params string[] renderLayerNames)
        {
            Map = tiledMap;
            SetDisplayLayer(renderLayerNames);
        }

        public override void Render()
        {
            foreach (var layer in displayLayer)
            {
                TiledRendering.DrawLayer(layer,Vector2.Zero,Transform.Position.ToVec2(),Transform.Scale.ToVec2());
            }
            //TiledRendering.DrawMap(Map,Vector2.Zero,Transform.Position.ToVec2(),Transform.Scale.ToVec2());
            //_tilemap.Draw();

        }
        public override void OnRemovedFromEntity()
        {
            Map.Unload();
            Map = null;
        }

        public TileMap  SetDisplayLayer(params string[] renderLayerNames)
        {
            if (renderLayerNames.Length <= 0) return this;
            if(displayLayer.Count > 0) displayLayer.Clear();

            foreach (var layerName in renderLayerNames)
            {
                if(TiledUtil.TryFindLayer(Map,layerName,out ITmxLayer foundLayer))
                {
                    displayLayer.Add(foundLayer);
                }
            }
            return this;
        }

        public void OnInspectorGUI()
        {
            ImGui.SetNextItemWidth(40);

            if (ImGui.BeginCombo("Tileset",String.Empty))
            {
                foreach (TmxTileset tileset in Map.Tilesets)
                {
                    ImGui.BeginGroup();
                        ImGui.Image((IntPtr)tileset.Image.Texture.ID, tileset.Image.Texture.Scale()/10f);
                        ImGui.SameLine();
                        if (ImGui.Selectable(tileset.Name))
                        {
                        }
                    ImGui.EndGroup();


                    if (ImGui.IsItemHovered(ImGuiHoveredFlags.DelayShort ))
                    {
                        ImGui.BeginTooltip();
                        ImGui.Image((IntPtr)tileset.Image.Texture.ID, tileset.Image.Texture.Scale());
                        ImGui.EndTooltip();
                    }
                    
                }
                ImGui.EndCombo();
            }
            ImGuiNET.ImGui.GetWindowDrawList().AddCircle(Vector2.Zero,20, (uint)Raylib.ColorToInt(Color.WHITE),50); ;
        }

    }



    

}
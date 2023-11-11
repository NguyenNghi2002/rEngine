using Engine.SceneManager;
using ImGuiNET;
using Raylib_cs;
using System.Numerics;

namespace Engine.UI
{

    public class UICanvas : RenderableComponent,IUpdatable,ICustomInspectorImgui
    {
        public Stage Stage { get; } = new Stage();
        public int UpdateOrder { get; set; } = 0;

        public override void OnAddedToEntity()
        {
            Stage.Entity = Entity;
        }
        public override void Render()
        {
            var off = Scene.Camera.offset ;
            var target = Scene.Camera.target ;
            var zoom = 1f/Scene.Camera.zoom;
            Rlgl.rlPushMatrix();
            
            Rlgl.rlTranslatef(-off.X,-off.Y,0f);
            Rlgl.rlTranslatef(target.X,target.Y, 0f);
            Rlgl.rlScalef(zoom,zoom,1f);

            Stage.Render(Scene.Camera);

            Rlgl.rlPopMatrix();

            //Raylib.DrawCircleV(Stage.GetMousePosition(),20,Color.RED);
            //Raylib.DrawCircleV(Input.GetScaledMousePosition(),2,Color.GREEN);
        }

        public override void OnRemovedFromEntity()
        {
            Stage.Dispose();
        }

        public void Update()
        {
            Stage.Update();
        }

        void ICustomInspectorImgui.OnInspectorGUI()
        {
            ImGui.Checkbox("Draw Debug", ref Stage.Debug);
            
            ImGui.Separator();
            ///-------------------------------------------------------------
            if(ImGui.BeginChildFrame((uint)this.GetHashCode(),Vector2.UnitX *( ImGui.GetWindowWidth()
                - ImGui.GetStyle().IndentSpacing * 4 - ImGui.GetStyle().FramePadding.X),ImGuiWindowFlags.None))
            {
                ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(5, 5));
                InspectGroup("Root",Stage.GetRoot());
                ImGui.PopStyleVar();

                ImGui.EndChildFrame();
            }
        }
        void InspectGroup(string label,Group group)
        {
            if (ImGui.TreeNodeEx(label,ImGuiTreeNodeFlags.Framed | ImGuiTreeNodeFlags.FramePadding |ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.PushStyleVar(ImGuiStyleVar.FramePadding,new Vector2(5,5));
                foreach (var ui in group.GetChildren())
                {
                    if (ui is Group groupUI)
                    {
                        InspectGroup($"{groupUI.GetType().Name}##{groupUI.GetHashCode()}",groupUI);
                        
                    }
                    else if(ui is Element element)
                    {
                        ImGui.BulletText(element.GetType().Name);
                    }
                }
                ImGui.TreePop();
            }
        }

    }



}
using Engine.Renderering;
using ImGuiNET;
using Raylib_cs;
using System.Numerics;

namespace Engine.DefaultComponents.Render.Primitive
{

    public class CircleRenderer : RenderableComponent,ICustomInspectorImgui
    {
        public float Radius;
        public Color Color;
        public float Angle;


        public CircleRenderer() : this(10, Color.WHITE) { }
        public CircleRenderer(float radius,Color color)
        {
            Radius = radius;
            Color = color;
        }

        public override void Render()
        {
            Rlgl.rlPushMatrix();

            Rlgl.rlTranslatef((int)Transform.Position.X, (int)Transform.Position.Y, (int)Transform.Position.Z);
            Rlgl.rlRotatef(Transform.EulerRotation.Z * Raylib.RAD2DEG, 0, 0, 1);
            Rlgl.rlScalef(Transform.Scale.X, Transform.Scale.Y, Transform.Scale.Z);

            Raylib.DrawCircle(0, 0, (int)Radius, Color);
            Rlgl.rlPopMatrix();
        }

        void ICustomInspectorImgui.OnInspectorGUI()
        {
            ImGui.SetNextItemWidth(40);
            ImGuiNET.ImGui.DragFloat("radius",ref Radius,1,0,int.MaxValue,"%0f");
        }
    }

    public class RingRenderer : CircleRenderer,ICustomInspectorImgui
    {
        public float HollowRadius;
        public float startAngle = 0,endAngle = 360;

        public RingRenderer(float outerRadius,float innerRadius,Color color) : base(outerRadius,color)
        {
            HollowRadius = innerRadius;
        }
        public override void Render()
        {
            Rlgl.rlPushMatrix();

            Rlgl.rlTranslatef(Transform.Position.X, Transform.Position.Y, Transform.Position.Z);
            Rlgl.rlRotatef(Transform.EulerRotation.Z * Raylib.RAD2DEG, 0, 0, 1);
            Rlgl.rlScalef(Transform.Scale.X, Transform.Scale.Y, Transform.Scale.Z);


            Raylib.DrawRing(Vector2.Zero, HollowRadius,Radius,startAngle,endAngle,100,Color);

            Rlgl.rlPopMatrix();
        }
        void ICustomInspectorImgui.OnInspectorGUI()
        {
            ImGui.SetNextItemWidth(40);
            ImGuiNET.ImGui.DragFloat("outer radius", ref Radius, 1, 0, int.MaxValue, "%0f");
            ImGuiNET.ImGui.DragFloat("inner radius", ref HollowRadius, 1, 0, int.MaxValue, "%0f");
        }
    }

    public class RectangleRenderer : RenderableComponent, ICustomInspectorImgui
    {
        public Vector2 Dimension;
        public Color Color;


        public RectangleRenderer(float width,float height, Color color)
        {
            Dimension.X = width;
            Dimension.Y = height;
            Color = color;
        }

        public override void Render()
        {
            var r = new Rectangle(Transform.Position.X, Transform.Position.Y, Dimension.X,Dimension.Y);
            Raylib.DrawRectanglePro(r,Dimension/2f,Transform.EulerRotation.Z,Color);
        }

        void ICustomInspectorImgui.OnInspectorGUI()
        {
            ImGui.SetNextItemWidth(40);
            ImGuiNET.ImGui.DragFloat("width", ref Dimension.X, 1, 0, int.MaxValue, "%0f");
            ImGuiNET.ImGui.DragFloat("height", ref Dimension.Y, 1, 0, int.MaxValue, "%0f");
        }

    }


}
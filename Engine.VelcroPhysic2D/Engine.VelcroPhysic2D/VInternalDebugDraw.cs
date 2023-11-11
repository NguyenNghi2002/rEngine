using Engine.Velcro.Unit;
using Genbox.VelcroPhysics.Collision.Shapes;
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Shared;
using Raylib_cs;
using Raylib_cs.Extension;
using System.Numerics;

namespace Engine.Velcro
{
    internal static class VSimulationDebugDraw
    {
        static float LineWidth = 1f;
        public static void DrawBox(Vector2 position, float width,float height,float angle ,float lineWidth,Color color)
        {
            Rlgl.rlPushMatrix();

            Rlgl.rlScalef(VConvert.SimToDisplay, VConvert.SimToDisplay, 1f);
            Rlgl.rlTranslatef(position.X, position.Y, 0f);
            Rlgl.rlRotatef(angle * Raylib.RAD2DEG, 0, 0, 1f);


            var scale = new Vector2(width, height);
            var pos = position * VConvert.SimToDisplay;
            var rec = RectangleExt.CreateRectangle(pos, scale);
            var org = scale / 2f;
            var rot = angle * Raylib.RAD2DEG;

            RayUtils.DrawRectangleLines(rec, org, rot, lineWidth * VConvert.DisplayToSim, color);

            Raylib.DrawLineEx(pos, pos + Raymath.Vector2Rotate(Vector2.UnitY * scale.Y / 2f, angle),lineWidth, color);


            Rlgl.rlPopMatrix();
        }
        public static void DrawCircle(CircleShape circle, Body body,float lineWidth,Color color)
        {
            Rlgl.rlPushMatrix();
            Rlgl.rlScalef(VConvert.SimToDisplay, VConvert.SimToDisplay, 1f);
            Rlgl.rlTranslatef(body.Position.X, body.Position.Y, 0f);
            Rlgl.rlRotatef(body.Rotation * Raylib.RAD2DEG, 0, 0, 1f);


            var c = circle.Position;
            var r = circle.Radius;

            RayUtils.DrawCircleLines(c,r , lineWidth * VConvert.DisplayToSim, color);
            Raylib.DrawLineEx(c, c + Vector2.UnitY * r, lineWidth, color); ;

            Rlgl.rlPopMatrix();

        }
        public static void DrawPoly(PolygonShape polygon, Body body, float lineWidth, Color color)
        {
            Rlgl.rlPushMatrix();

            Rlgl.rlScalef(VConvert.SimToDisplay, VConvert.SimToDisplay, 1f);
            Rlgl.rlTranslatef(body.Position.X ,body.Position.Y , 0f);
            Rlgl.rlRotatef(body.Rotation * Raylib.RAD2DEG ,0,0,1f);

            var verts = polygon.Vertices;
            for (int i = 0; i < verts.Count; i++)
            {

                var v0 = verts[i].ToSVec2();
                var v1 = verts[(i + 1) % verts.Count].ToSVec2();

                //var v0 = Raymath.Vector2Rotate(verts[i].ToSVec2(), rotation) ;
                //var v1 = Raymath.Vector2Rotate(verts[(i + 1) % verts.Count].ToSVec2(), rotation) + origin;

                //v0 *= VConvert.SimToDisplay;
                //v1 *= VConvert.SimToDisplay;


                Raylib.DrawLineEx(v0, v1, lineWidth * VConvert.DisplayToSim, color);
                //Raylib.DrawCircleV(v0, 5, Color.YELLOW);
            }
            Rlgl.rlPopMatrix();
        }
    }
}
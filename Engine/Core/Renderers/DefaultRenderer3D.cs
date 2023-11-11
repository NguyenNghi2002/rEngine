using Engine.SceneManager;
using Raylib_cs;
using System.Numerics;

namespace Engine.Renderering
{
    public class DefaultRenderer3D : Renderer3D
    {
        public override void OnAddedToScene(Scene scene)
        {
            base.UpdateRenderTexture(scene);
        }
        public override void Render(Scene scene)
        {
            var cam = Camera3D.Value;
            BeginRender(cam);
            {
                //Raylib.DrawCube(Vector3.Zero,2,2,1,Color.RED);
                //Raylib.DrawSphereEx(Vector3.Zero,5,3,2,Color.RED);
                //Raylib.DrawSphereWires(Vector3.Zero,1,1,1,Color.WHITE);
                foreach (var cpn in scene.Renderables)
                        cpn.Render();
                //Raylib.DrawSphere(Vector3.Zero, 3, Color.RED);
                //Raylib.DrawLine3D(Vector3.Zero, Vector3.One * 10, Color.YELLOW);
                //Raylib.DrawSphereWires(Vector3.Zero, 3, 32, 32, Color.WHITE);
                Raylib.DrawGrid(10, 1);
                //Raylib.DrawText("Text", 0, 0, 20, Color.RED);
                //Raylib.DrawCircleV(new Vector2(100, 100), 20, Color.BLUE);
            }
            EndRender();

            
        }
    }

}
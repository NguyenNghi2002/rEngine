using Engine.SceneManager;
using Engine.Timer;
using Raylib_cs;
using System.Collections;

namespace Engine
{
    public class SwipeTransition : TimeBaseTransition
    {
        Rectangle rectangle = default;
        Color color = Color.WHITE;    
        public SwipeTransition(Func<Scene> sceneLoadAction) : base(sceneLoadAction)
        {
        }


        public override void Render()
        {

            if (Core.Scene != null)
            {
                Raylib.DrawRectangleRec(rectangle, color);
            }
            else
                Raylib.DrawRectangleRec(new Rectangle(0, 0, Raylib.GetScreenWidth(), Raylib.GetScreenHeight()), color);
        }

        public override void UpdateEnterScene(float elapse)
        {
            rectangle = Core.Scene.GetFinalResolution();
            rectangle.height = Easings.EaseExpoOut(elapse, Core.Scene.GetFinalResolution().height, - Core.Scene.GetFinalResolution().height, ExitDuration);
            rectangle.y = Core.Scene.GetFinalResolution().height - rectangle.height;
        }

        public override void UpdateExitScene(float elapse)
        {
            rectangle = Core.Scene.GetFinalResolution();
            rectangle.height = Easings.EaseExpoOut(elapse,0,Core.Scene.GetFinalResolution().height,EnterDuration);
        }

    }

    public class FadeTransition : TimeBaseTransition
    {
        Color OverlayColor = Color.WHITE;

        Color _color;
        public FadeTransition(Func<Scene> sceneLoadAction) : base(sceneLoadAction)
        { }

       
        public override void Render()
        {
            if(Core.Scene != null)
            {
                var rec = Core.Scene.GetFinalResolution();
                Raylib.DrawRectangleRec(rec, _color);
            }
            else
                Raylib.DrawRectangleRec(new Rectangle(0, 0, Raylib.GetScreenWidth(), Raylib.GetScreenHeight()), _color);
        }

        public override void UpdateExitScene(float elapse)
        {
            _color = Raylib.Fade(OverlayColor, Raymath.Lerp(0f, 1f, elapse / ExitDuration));
        }

        public override void UpdateEnterScene(float elapse)
        {
            _color = Raylib.Fade(OverlayColor, Raymath.Lerp(1f, 0f, elapse / EnterDuration));
        }
    }
}

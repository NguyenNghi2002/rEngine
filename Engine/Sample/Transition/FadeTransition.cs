using Engine.SceneManager;
using Raylib_cs;
using System.Collections;

namespace Engine
{
    public class FadeTransition : Transition
    {
        Color OverlayColor = Color.WHITE;
        public float FadeOutDuration = 0.4f;
        public float FadeInDuration = 0.4f;
        public float HoldDuration = 0.1f; // Hold


        Color _color;
        public FadeTransition(Func<Scene> sceneLoadAction) : base(sceneLoadAction)
        { }

        public override IEnumerator OnBegin()
        {
            #region Onverlay
            var elapse = 0f;
            while (elapse < FadeOutDuration)
            {
                elapse += Time.UnscaledDeltaTime;
                _color = Raylib.Fade(OverlayColor, Raymath.Lerp(0f, 1f, elapse / FadeOutDuration));
                Console.WriteLine(elapse);
                yield return null;
            }
            OnFadedOut?.Invoke(); 
            #endregion

            yield return Core.StartCoroutine(LoadNewScene());

            Console.WriteLine("load new scene");

            yield return new WaitForSecond(HoldDuration);

            #region overlay
            elapse = 0f;
            while (elapse < FadeInDuration)
            {
                elapse += Time.UnscaledDeltaTime;
                _color = Raylib.Fade(OverlayColor, Raymath.Lerp(1f, 0f, elapse / FadeOutDuration));
                yield return null;
            }
            #endregion

            TransitionCompleted();
            Console.WriteLine("COMPLETED TRANSITION");
        }
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
    }
}

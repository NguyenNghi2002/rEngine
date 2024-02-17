using System.Collections;
using System.Diagnostics;

namespace Engine.SceneManager
{
    public class Transition
    {
        private bool sceneLoaded;

        protected Func<Scene> loadSceneAction;
        public bool LoadOnBackground;

        public Action OnFadedOut, OnFadedin, OnCompleted, OnBegined;


        public bool IsPlaying { get; internal set; }

        public Transition(Func<Scene> sceneLoadAction)
        {
            this.loadSceneAction = sceneLoadAction;
            sceneLoaded = false;
        }
        protected IEnumerator LoadNewScene()
        {
            // Event

            if (loadSceneAction == null)
            {
                sceneLoaded = true;
                yield break;
            }

            Insist.IsNotNull(loadSceneAction);

            if (LoadOnBackground)
            {
                Task.Run(() =>
                {
                    var newScene = loadSceneAction.Invoke();

                    Core.ScheduleNextFrame(null, (timer) =>
                    {
                        Core.Scene = newScene;
                        sceneLoaded = true;
                    });
                });
            }
            else
            {
                Core.Scene = loadSceneAction.Invoke();
                sceneLoaded = true;
            }

            //keep loop until new scene loaded
            while (!sceneLoaded)
                yield return null;
        }

        public virtual IEnumerator OnBegin()
        {
            yield return null;
            yield return LoadNewScene();

            Core.Instance._sceneTransition = null;
        }

        public virtual void Render()
        {

        }

        protected virtual void TransitionCompleted()
        {
            Core.Instance._sceneTransition = null;

            loadSceneAction = null;
            sceneLoaded = true;
            IsPlaying = false;
        }
    }


    public abstract class TimeBaseTransition : Transition
    {
        public float ExitDuration = 0.4f;
        public float EnterDuration = 0.4f;
        public float HoldDuration = 0.1f; // Hold

        public TimeBaseTransition(Func<Scene> sceneLoadAction) : base(sceneLoadAction)
        { }
        public abstract void UpdateExitScene(float elapse);
        public abstract void UpdateEnterScene(float elapse);
        sealed public override IEnumerator OnBegin()
        {
            //Begin
            OnBegined?.Invoke();

            //Fade out
            var elapse = 0f;
            while (elapse < ExitDuration)
            {
                elapse += Time.UnscaledDeltaTime;
                UpdateExitScene(elapse);
                Console.WriteLine(elapse);
                yield return null;
            }
            OnFadedOut?.Invoke();

            //LoadScene
            yield return Core.StartCoroutine(LoadNewScene());
            Console.WriteLine("loaded new scene");

            //Delay hold
            yield return new WaitForSecond(HoldDuration);

            //Fade in
            OnFadedin?.Invoke();
            elapse = 0f;
            while (elapse < EnterDuration)
            {
                elapse += Time.UnscaledDeltaTime;
                UpdateEnterScene(elapse);
                yield return null;
            }

            //Completed
            TransitionCompleted();
            OnCompleted?.Invoke();
        }
    }
}

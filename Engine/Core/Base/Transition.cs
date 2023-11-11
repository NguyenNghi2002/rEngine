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
}

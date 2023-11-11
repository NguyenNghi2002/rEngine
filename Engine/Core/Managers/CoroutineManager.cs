using System.Collections;

namespace Engine
{
    /// <summary>
    /// interface returned by startCoroutine which provides the ability to stop the coroutine mid-flight
    /// </summary>
    public interface ICoroutine
    {
        /// <summary>
        /// stops the Coroutine
        /// </summary>
        void Stop();

    }
    public interface ICoroutineCounter
    {
        public bool CountDown(float step);
    }
    public class CoroutineManager : GlobalManager,IUpdatable
    {
        private class CoroutineObj : IPoolable, ICoroutine
        {
            public bool IsPaused;
            public bool IsDone;
            public CoroutineObj? waitCoroutine;
            public IEnumerator Enumerator;
            public ICoroutineCounter counter;
            public void Setup()
            {
                IsDone = false;
            }
            public void Reset()
            {
                Stop();
            }
            public void Stop()
            {
                IsDone = true;
                waitCoroutine = null;
                counter = null;
                Enumerator = null;
            }
        }

        private List<CoroutineObj> _runnings = new List<CoroutineObj>();
        private List<CoroutineObj> _shouldRunNextFrame = new List<CoroutineObj>();

        public IEnumerable<ICoroutine> Runnings => _runnings.Select(o=>o as ICoroutine);
        public bool bIsUpdating = false;

        public int UpdateOrder { get; set; } = 0;


        public ICoroutine? StartCoroutine(IEnumerator enumerator)
        {
            var coroutine = Pool<CoroutineObj>.Obtain();
            coroutine.Setup();

            coroutine.Enumerator = enumerator;

            bool shouldContinue = Tick(coroutine, Time.DeltaTime);

            if (!shouldContinue) return null;

            if (bIsUpdating) _shouldRunNextFrame.Add(coroutine);
            else _runnings.Add(coroutine);

            return coroutine;
        }

        public void Update()
        {
            var step = Time.DeltaTime;
            bIsUpdating = true;
            for (int i = 0; i < _runnings.Count(); i++)
            {
                var coroutine = _runnings[i];

                if (coroutine.IsDone)
                {
                    Pool<CoroutineObj>.Free(coroutine);
                    continue;
                }

                /* Handle wait another coroutine */
                if (coroutine.waitCoroutine != null)
                {
                    if (coroutine.waitCoroutine.IsDone)
                        coroutine.waitCoroutine = null;
                    else
                    {
                        _shouldRunNextFrame.Add(coroutine);
                        continue;
                    }
                }


                /* Handle Delay for next coroutine */
                if (coroutine.counter != null)
                    if (coroutine.counter.CountDown(step))
                    {
                        _shouldRunNextFrame.Add(coroutine);
                        continue;
                    }

                /* Go to next Yeild return statment */
                if (Tick(coroutine, step))
                    _shouldRunNextFrame.Add(coroutine);
            }

            _runnings.Clear();
            _runnings.AddRange(_shouldRunNextFrame);
            _shouldRunNextFrame.Clear();

            bIsUpdating = false;
        }
        public void Clear()
        {
            foreach (var o in _runnings)
            {
                Pool<CoroutineObj>.Free(o);
            }

            foreach (var o in _shouldRunNextFrame)
            {
                Pool<CoroutineObj>.Free(o);
            }

            _runnings.Clear();
            _shouldRunNextFrame.Clear();
        }
        public void StopAll()
        {
            foreach (var o in _runnings)
            {
                o.Stop();
                Pool<CoroutineObj>.Free(o);
            }
            _runnings.Clear();
        }
        public void TogglePauseAll(bool isPause)
        {
            foreach (var o in _runnings)
                o.IsPaused = isPause;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="coroutine">coroutine object</param>
        /// <param name="step">delta time</param>
        /// <returns><see cref="true"/> to continue, <see cref="false"/> to break</returns>
        private bool Tick(CoroutineObj coroutine, float step)
        {
            if (coroutine.IsPaused) return true;

            var iter = coroutine.Enumerator;
            if (!iter.MoveNext())
            {
                Pool<CoroutineObj>.Free(coroutine);
                return false;
            }

            ///Next frame
            if (iter.Current == null)
                return true;

            if (iter.Current is ICoroutineCounter counter)
            {
                coroutine.counter = counter;
                return true;
            }

            if (iter.Current is ICoroutine waitCoroutine)
            {
                coroutine.waitCoroutine = (CoroutineObj)waitCoroutine;
                return true;
            }
            if (iter.Current is IEnumerator enumerator)
            {
                coroutine.waitCoroutine = (CoroutineObj)StartCoroutine(enumerator);
                return true;
            }

            return true;

        }
    }


    public class WaitForFrame : ICoroutineCounter
    {
        public readonly int Frame;
        internal int counter;
        public WaitForFrame(int frame)
        {
            Frame = frame;
            counter = frame;
        }
        public bool CountDown(float step)
        {
            counter--;
            return counter > 0;
        }
    }
    public class WaitForSecond : ICoroutineCounter
    {
        public readonly float Duration;
        internal float timer;
        public WaitForSecond(float duration)
        {
            Duration = duration;
            timer = duration;
        }
        public bool CountDown(float step)
        {
            timer -= step;
            return timer > 0;
        }
    }



}
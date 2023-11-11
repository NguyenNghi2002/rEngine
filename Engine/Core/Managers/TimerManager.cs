using Engine;
using Raylib_cs;

namespace Engine.Timer
{
    public interface ITimer
    {

        float Elapse { get; }
        void Reset();
        void Stop();
        object Context { get; }

    }

    public class rTimer : IPoolable, ITimer
    {
        public object Context { get; set; }

        public float Elapse => _elapseTime;

        public bool Done => _isDone && _duration == 0;
        public float Duration
        {
            get => _duration;
            set => _duration = value;
        }

        Action<rTimer> _timeoutAction;
        bool _isDone;
        bool _isRepeat;
        float _duration;
        float _elapseTime; // time less than duration

        void IPoolable.Reset()
        {
            _timeoutAction = null;
            _isDone = true;
            _duration = 0;
            _elapseTime = 0;
        }
        public void Reset()
        {
            _elapseTime = 0;
        }

        internal virtual bool Tick()
        {
            if (!_isDone && _elapseTime > _duration)
            {
                _elapseTime %= _duration;
                _timeoutAction.Invoke(this);

                if (!_isDone && !_isRepeat)
                    _isDone = true;
            }

            _elapseTime += Raylib.GetFrameTime();

            return _isDone;
        }

        internal virtual void Setup(float duration, bool repeat, object context, Action<rTimer> timeOutAction)
        {
            _duration = duration;
            _isRepeat = repeat;
            Context = context;
            _timeoutAction = timeOutAction;

            _isDone = false;
        }

        public void Stop()
        {
            _isDone = true;
        }

        public T GetContext<T>() => (T)Context;

    }


    public class TimerManager : GlobalManager,IUpdatable
    {
        List<rTimer> _timers = new List<rTimer>();

        public int UpdateOrder { get; set; } = 0;

        public void Update()
        {
            for (int i = _timers.Count - 1; i >= 0; i--)
            {
                rTimer timer = _timers[i];
                if (timer.Tick())
                {
                    Pool<rTimer>.Free(timer);
                    _timers.RemoveAt(i);
                }
            }
        }

        public ITimer Schedule(float duration, bool repeat, object context, Action<ITimer> onTimeout)
        {
            var timer = Pool<rTimer>.Obtain();
            timer.Setup(duration, repeat, context, onTimeout);
            _timers.Add(timer);
            return timer;
        }

    }


}
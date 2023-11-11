using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{

    public class GlobalManager 
    {
        private bool _enable = true;
        public bool Enable
        {
            get => _enable;
            set => SetEnable(value);
        }
        public void SetEnable(bool enable)
        {
            if (_enable != enable)
            {
                if (_enable) OnDisable();
                else OnEnable();
                _enable = enable;
            }
        }


        protected virtual void OnDisable() { }
        protected virtual void OnEnable() { }
        internal protected virtual void OnDrawDebug() { }
    }
}

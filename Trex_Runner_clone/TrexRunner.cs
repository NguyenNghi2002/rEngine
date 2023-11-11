using Engine;
using Raylib_cs;

namespace Trex_runner
{
    public class TrexRunner : Core
    {
        public override void Initialize()
        {
            WindowWidth = 1280;
            WindowHeight = 300;
            base.Initialize();
            Scene = new GameScene(); 
        }
    }
}
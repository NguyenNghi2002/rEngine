
using Engine.UI;
using Microsoft.VisualBasic.FileIO;
using Raylib_cs;

namespace Engine
{
    public class FollowCursor : Component,IUpdatable
    {
        public int UpdateOrder { get; set; } = 0;

        public void Update()
        {
            var m = Scene.GetMouseWorldPosition() ;
            if(Transform.Position2 != m)
                Transform.Position = new (m,Transform.Position.Z) ;
            if (Scene.TryFindComponent<UICanvas>(out var ui))
            {
                var a= ui.Stage.Hit(Input.MousePosition);
            }
        }
    }

}
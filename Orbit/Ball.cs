using Engine;
using Engine.DefaultComponents.Render.Primitive;
using Engine.Timer;
using Engine.UI;
using Microsoft.VisualBasic.FileIO;
using Microsoft.Win32.SafeHandles;
using Raylib_cs;
using System.Collections;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.Permissions;

public class test : Component
{
    public override void OnTransformChanged(Transformation.Component component)
    {
        Console.WriteLine("test");
    }
}
public class Player : Component, IUpdatable
{
    int IUpdatable.UpdateOrder { get; set; }

    public override void OnAddedToEntity()
    {
        var planet = GameSceneManager.Instance.CurrentPlanet;

          

        var centerTF = Scene.FindEntityInParent(planet.Entity, e => e.Name.Contains("center")).Transform;
        var slotTF = Scene.FindEntityInParent(centerTF.Entity, e => e.Name.Contains("slot")).Transform;

        ///If batman
        if (Transform.Parent == null)
        {
            Transform.SetParent(slotTF,false);
        }
    }

    public override void OnRemovedFromEntity()
    {
        ballReturning?.Stop();
        ballSpining?.Stop();
    }

    float TweenTimer = 0;
    float TweenTime = 1f;

    KeyboardKey[] controlKey = { KeyboardKey.KEY_SPACE ,KeyboardKey.KEY_UP};
    MouseButton[] mouseButtons = { MouseButton.MOUSE_BUTTON_LEFT};
    void IUpdatable.Update()
    {
        
        var playerball = GameSceneManager.Instance.Player;
        var curr = GameSceneManager.Instance.CurrentPlanet;
        var nextQueue = GameSceneManager.Instance.planetNextQueue;
        if (playerball != this || curr == null)
            return;

        if (Transform.Parent != null && ballSpining == null )
        {
            var originEn = Transform;
            var cursorOnGame = !ImGuiNET.ImGui.GetIO().WantCaptureMouse;
            var mouseclicked = mouseButtons.Any(m => Raylib.IsMouseButtonPressed(m)) ;
            
            if (mouseclicked)
            {
                cursorOnGame = GameSceneManager.Instance.Entity.GetComponent<UICanvas>().Stage.Hit(Input.MousePosition) == null;
                Console.WriteLine(cursorOnGame) ;
            }

            if (( controlKey.Any(k => Input.IsKeyPressed(k)) ||  mouseclicked) && cursorOnGame)
            {


                ballReturning?.Stop();

                ballReturning = null;
                originEn.LocalPosition = Vector3.Zero;
                Transform.LocalScale = Vector3.One;
            }

            if (controlKey.Any(k => Input.IsKeyDown(k)) || mouseButtons.Any(m => Raylib.IsMouseButtonDown(m)) && cursorOnGame)
            {
                originEn.LocalPosition2 += Vector2.UnitY * GameSceneManager.Instance.GameDeltaTime * GameSceneManager.Instance.BallFloatSpeed;
            }

            if ( (controlKey.Any(k => Input.IsKeyReleased(k)) || mouseButtons.Any(m => Raylib.IsMouseButtonReleased(m))) 
                && nextQueue.TryPeek(out var next) )
            {

                var ballDis = Vector2.Distance(curr.centerTF.Position2, this.Transform.Position2);
                var nextPlanetDis = Vector2.Distance(curr.centerTF.Position2, next.centerTF.Position2);
                if (ballDis >= nextPlanetDis - next.Radius && ballDis <= nextPlanetDis + next.Radius)
                {
                    ///READY TO JUMP 
                    ///

                    //Console.WriteLine("Succes");
                    var ballDisRelateToMinSurface = (ballDis - (nextPlanetDis - next.Radius) ) ;

                    bool clowise = Convert.ToBoolean(Random.Shared.Next(0, 2));
                    Console.WriteLine(Random.Shared.Next(0, 2));
                    GameSceneManager.Instance.CalculateSlotTransform(this,ballDisRelateToMinSurface,curr,next,clowise);

                    var LocaltoSlot = next.slotTF.Position2 - curr.centerTF.Position2;
                    var angleToSlot = MathF.Atan2(LocaltoSlot.Y, LocaltoSlot.X);


                    

                    startA =  curr.centerTF.EulerLocalRotation.Z ;
                    endA = angleToSlot * Raylib.RAD2DEG;
                    var pi2 = 360;
                    var offset = endA - startA;
                    if (clowise) offset = offset >= 0 ? offset : (offset + pi2) % pi2;
                    else offset = offset < 0 ? offset : (offset - pi2) % pi2;


                    ///Run animation
                    ballSpining =  Core.StartCoroutine(RotateTweeningBall(curr.centerTF.EulerRotation.Z , angleToSlot -MathF.PI/2f,clowise));

                    //Console.WriteLine($"{ballDisRelateToMinSurface}/{next.Radius}");
                }
                else
                {
                    ///FAIL TO JUMP 
                    //Console.WriteLine("fail");

                    ballReturning =  Core.StartCoroutine(TweenLocal(Transform.LocalPosition2 , Vector2.Zero, (e,f,o,d)=> Easings.EaseBounceOut(e,f,o,d) ));;
                }
            }
        }

        
    }

    float startA, endA;
    ICoroutine? ballReturning,ballSpining;
    private float stretchFactor = 4f;

    public override void OnDebugRender()
    {
        var c = GameSceneManager.Instance.CurrentPlanet;
        //Raylib.DrawRing(c.centerTF.Position2,c.Radius - 5,c.Radius,(c.centerTF.LocalRotation.Z) * Raylib.RAD2DEG  ,endA * Raylib.RAD2DEG , 100,Color.RED);
    }

    IEnumerator RotateTweeningBall(float start,float end,bool clockWise )
    {      
        var pi2 = 2 * MathF.PI;
        //start = (start % pi2 + pi2) % pi2;
        //end = (end % pi2 + pi2) % pi2;
        float speed = 5.5f;
        var offset = end - start;

        if(clockWise) offset = offset >= 0 ? offset : (offset + pi2) % pi2;
        else offset = offset < 0 ? offset : (offset - pi2) % pi2;
        startA = start;
        endA = end;

        //float duration = MathF.Abs(offset)/speed;
        float duration = MathF.Abs(offset)/speed;
        float elapse = 0;

        Vector3 prevLocalScale = Transform.LocalScale;
        float prevLocalRotZ = Transform.LocalRotation.Z;

        while (elapse < duration)
        {
            elapse += GameSceneManager.Instance.GameDeltaTime;
            //Console.WriteLine(elapse);

            var easingRotation = Easings.EaseSineIn(elapse, start, offset, duration);
            var easingScaleX = Easings.EaseSineIn(elapse, prevLocalScale.X, stretchFactor * duration, duration);

            GameSceneManager.Instance.CurrentPlanet.centerTF.SetEulerRotation(new (0,0,easingRotation));

            Transform.SetLocalScale(new Vector3(easingScaleX,Transform.LocalScale.Y,Transform.LocalScale.X));


            yield return null;
        }
        //Console.WriteLine($"{start * Raylib.RAD2DEG} >>> {end * Raylib.RAD2DEG}");
        //Console.WriteLine($"{offset * Raylib.RAD2DEG}");
        
        
        GameSceneManager.Instance.JumpNext();

        /// Return to orginal value 
        Transform.SetLocalScale(prevLocalScale);
        Transform.SetLocalRotationZ(0);
        
        //Entity.GetComponentInChilds<SpriteRenderer>().Transform.Rotation = GameSceneManager.Instance.CurrentPlanet.
        ballSpining = null;
    }
    IEnumerator TweenLocal( Vector2 from,Vector2 to,Func<float,float,float,float,float> easingAction)
    {
        var offset = to-from;
        float duration = .43f;
        float elapse = 0;

        while (elapse < duration)
        {
            elapse += GameSceneManager.Instance.GameDeltaTime;
            Console.WriteLine(elapse);


            var easingX = easingAction.Invoke(elapse,from.X,offset.X,duration);
            var easingY = easingAction.Invoke(elapse,from.Y,offset.Y,duration);


            Entity?.Transform.SetLocalPosition(new Vector2(easingX, easingY));

            yield return null;
        }

        Transform.LocalPosition2 = to;
        ballReturning = null;
    }

}
   
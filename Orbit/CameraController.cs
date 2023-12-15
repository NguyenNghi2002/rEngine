using Engine;
using ImGuiNET;
using Raylib_cs;
using System.Collections;
using System.Numerics;

internal class CameraController : Engine.Component,IUpdatable
{
    public enum Follow
    {
        Player,Planet
    }
    public Entity Planet;
    public float SmoothDamp;
    public Follow FollowMode = Follow.Player;
    public Vector2 startpos = Vector2.Zero;
    public Vector2? targetpos = null;
    public Vector2 smoothPosition;
    public CameraController() : this(null, 0.05f)
    {

    }
    public CameraController(Entity target ,float smooth = 0.01f)
    {
        this.Planet = target;
        SmoothDamp = smooth;
    }

    ICoroutine a;
    public override void OnAddedToEntity()
    {
        smoothPosition = Scene.Camera.target;
        Scene.Camera.offset = Scene.ViewPortScale/2f;
        GameSceneManager.Instance.OnJumped += ()=>
        {
            startpos = targetpos ?? Scene.Camera.target;//targetpos.HasValue ? targe :  Scene.Camera.target;
            targetpos = Planet.Transform.Position2;

            //a= Core.StartCoroutine(Animate(startpos, targetpos.Value,2f));
        };
    }
    public int UpdateOrder { get; set; }

#if true
    public void Update()
    {
        if (Planet != null )
        {
            smoothPosition = RaymathF.SmoothDamp(smoothPosition, Planet.Transform.Position2, 2 * Time.DeltaTime);
            Scene.Camera.target = smoothPosition.ToInt2();
        }
    } 
#endif

#if false
    private IEnumerator Animate(Vector2 from, Vector2 to, float duration)
    {
        var d = duration;
        var e = 0f;
        Vector2 prepos = from;
        while (e < d)
        {
            var clampedElapse = Raymath.Clamp(e, 0, d);
            var x = Easings.EaseSineOut(clampedElapse, from.X, to.X - from.X, d);
            var y = Easings.EaseSineOut(clampedElapse, from.Y, to.Y - from.Y, d);
            var newpos = new Vector2(x, y);
            e += Time.DeltaTime;
            var velocity = newpos - prepos;

            Scene.Camera.target = newpos.ToInt2();
            prepos = newpos;
            yield return null;
        }
        Scene.Camera.target = targetpos.Value.ToInt2();

    } 
#endif
}
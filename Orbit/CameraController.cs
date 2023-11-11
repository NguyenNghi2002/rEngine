using Engine;

internal class CameraController : Engine.Component,IUpdatable
{
    public enum Follow
    {
        Player,Planet
    }
    public Entity Planet;
    public float SmoothDamp;
    public Follow FollowMode = Follow.Player;

    public CameraController() : this(null, 0.05f)
    {

    }
    public CameraController(Entity target ,float smooth = 0.01f)
    {
        this.Planet = target;
        SmoothDamp = smooth;
    }


    public override void OnAddedToEntity()
    {
        Scene.Camera.offset = Scene.ViewPortScale/2f;
    }
    public int UpdateOrder { get; set; }

    public void Update()
    {
        if(Planet != null) Scene.Camera.target = RaymathF.SmoothDamp(Scene.Camera.target, Planet.Transform.Position2,2 * Time.DeltaTime) ;
    }
}
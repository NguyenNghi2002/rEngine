
using Engine;
using Raylib_cs;
using System.Numerics;

public class Trail : RenderableComponent, IUpdatable
{
    int IUpdatable.UpdateOrder { get; set; }
    public TrailSystem trail = new TrailSystem(Vector2.Zero);
    void IUpdatable.Update()
    {
        trail.SetPosition(this.Transform.Position2);
        trail.UpdateTrail(Time.DeltaTime);
    }

    public override void Render()
    {
        var a = ContentManager.GetOrNull<rTexture>("trail_01");
        trail?.DrawTrail(a,Color.WHITE);
    }
}

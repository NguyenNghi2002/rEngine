using Engine;
using Engine.DefaultComponents.Render.Primitive;
using Engine.SceneManager;
using Raylib_cs;
using System.Collections;
using System.Diagnostics;
using System.Numerics;
using System.Timers;

public class Pulsable : Component
{
    Color Color = Color.DARKGRAY;

    float duration = 1f;
    ICoroutine? c;
    public void Pulse()
    {
        if(Entity.TryGetComponent<SpriteRenderer>(out var cpn))
        c = Core.StartCoroutine(ExecuteingPulse(cpn));

        if(Entity.TryGetComponent<CircleRenderer>(out var cpnn))
        c = Core.StartCoroutine(ExecuteingPulse(cpnn));
    }
    IEnumerator ExecuteingPulse(CircleRenderer cpn)
    {
        var origin = cpn.Color;
        var elapse = 0f;


        if (duration != 0)
        {
            //Console.WriteLine(elapse);

            while (elapse < duration)
            {
                elapse += Time.DeltaTime;
                elapse = elapse.ClampMax(duration);

                var value = Vector4.Lerp(Raylib.ColorNormalize(origin), Raylib.ColorNormalize(Color), 1 - elapse / duration);
                Entity.GetComponentInHirachy<CircleRenderer>().Color = Raylib.ColorFromNormalized(value);
                yield return null;
            }
        }
        Entity.GetComponentInHirachy<CircleRenderer>().Color = origin;

        c = null;
    }
    IEnumerator ExecuteingPulse(SpriteRenderer cpn)
    {
        var origin = cpn.TintColor;
        var elapse = 0f;

        
        if(duration != 0)
        {
            //Console.WriteLine(elapse);

            while (elapse < duration)
            {
                elapse += Time.DeltaTime;
                elapse = elapse.ClampMax(duration);

                var value = Vector4.Lerp(Raylib.ColorNormalize(origin), Raylib.ColorNormalize(Color), 1 - elapse / duration);
                Entity.GetComponentInHirachy<CircleRenderer>().Color = Raylib.ColorFromNormalized(value);
                yield return null;
            } 
        }
        Entity.GetComponentInHirachy<CircleRenderer>().Color = origin;

        c = null;
    }

    public override void OnRemovedFromEntity()
    {
       c?.Stop();
        c = null;
    }
}
public class Shakable : Component
{
    float Intensity = 20;
    float Duration = 0.5f;
    ICoroutine c;
    public void Shake()
    {
        c = Core.StartCoroutine(TranslateShake());
    }

    IEnumerator TranslateShake()
    {
        
        var origin = Transform.Position2;
        var elapse = 0f;
        while (elapse < Duration)
        {
            if(Time.TimeScale != 0)
            {
                 elapse += Time.DeltaTime;
                elapse = elapse.ClampMax(Duration);
                Transform.Position = RayUtils.RandomPointInCircle(origin,Intensity * (1 - elapse/Duration)).ToVec3();
            }

            yield return null;
        }
        Transform.Position2 = origin;
        c = null;
    }

    public override void OnRemovedFromEntity()
    {
        c?.Stop();
        c = null;
    }
}
public class Planet : Component, IUpdatable,IPoolable
{
    float _radius;
    public float Radius => _radius;
    public Transformation centerTF => Scene.FindEntityInParent(Entity, e => e.Name.Contains("center")).Transform;
    public Transformation slotTF => Scene.FindEntityInParent(centerTF.Entity, e => e.Name.Contains("slot")).Transform;

    public void Shake() => Entity.GetComponentInHirachy<Shakable>().Shake();
    public void Pulse() => Entity.GetComponentInHirachy<Pulsable>().Pulse();
    int IUpdatable.UpdateOrder { get; set; }

    public Planet() : this(180)
    {
    }
    public Planet(float radius )
    {
        _radius = radius;
    }

    public override void OnAddedToEntity()
    {


        ///normalize raw scale
#if false
        var sr = Entity.GetComponentInChilds<SpriteRenderer>();
        if (sr != null)
            sr.Transform.Scale = new((_radius * 2) / sr.Sprite.SourceScale.X);

        var rr = Entity.GetComponentInChilds<RingRenderer>();
        if (rr != null)
            rr.Transform.Scale = new Vector3(new Vector2((_radius) / rr.Radius), 1f);

#endif

        slotTF.LocalPosition2 = Vector2.UnitY * _radius;

        var queue = GameSceneManager.Instance.planetNextQueue;
        if (!queue.Contains(this) && GameSceneManager.Instance.CurrentPlanet != this) /// Already did this in GameScenManager but just for the case of New Entity creation
            queue.Enqueue(this);

        
    }

    void IUpdatable.Update()
    {
    }

    void IPoolable.Reset()
    {
        Entity.Enable = false;
    }
}

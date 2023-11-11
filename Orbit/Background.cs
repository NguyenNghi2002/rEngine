using Engine;
using Raylib_cs;
using System.Numerics;

internal class Background : Component,IUpdatable
{
    public Vector2 _source;
    TiledSpriteRenderer _spriteRenderer;

    public int UpdateOrder { get; set; }

    float parallaxFactor;
    public Background(float parallaxFactor = 1f)
    {
        this.parallaxFactor = parallaxFactor;
    }
    public override void OnAddedToEntity()
    {
        _spriteRenderer = Entity.GetComponent<TiledSpriteRenderer>();
        _source.X = _spriteRenderer.SourceWidth;
        _source.Y = _spriteRenderer.SourceHeight;

        var scale = Scene.ViewPortScale / _source;
        _spriteRenderer.Transform.SetScale(scale.X,scale.Y,Transform.Scale.Z);
    }

    public override void OnTransformChanged(Transformation.Component component)
    {
        _spriteRenderer.SourceHeight = _source.Y * Transform.Scale.Y / Transform.Scale.Z;
        _spriteRenderer.SourceWidth = _source.X * Transform.Scale.X / Transform.Scale.Z;
    }

    public void Update()
    {
        Transform.Position2 = Scene.Camera.target;
        _spriteRenderer.SourceX = Transform.Position2.X / Transform.Scale.Z * parallaxFactor;
        _spriteRenderer.SourceY = Transform.Position2.Y / Transform.Scale.Z * parallaxFactor;
    }
}


public class Gradient : RenderableComponent
{
    Color TL, TR, BL, BR;
    public float Width, Height;
    public Gradient(float width,float height,Color topleft, Color topright, Color bottomleft, Color bottomright)
    {
        Width = width;
        Height = height;

        TL = topleft;
        TR = topright;
        BL = bottomleft;
        BR = bottomright;
    }
    public override void Render()
    {
        Rlgl.rlPushMatrix();

        //Rlgl.rlScalef(Transform.Scale.X, Transform.Scale.Y, Transform.Scale.Z);
        Rlgl.rlRotatef( Transform.EulerRotation.Z,0,0,1f);

        var scale = new Vector2(Width * Transform.Scale.X, Height * Transform.Scale.Y);

        var ori = Transform.Position2 -  scale/ 2f;
        Rectangle rec = new Rectangle(ori.X,ori.Y,scale.X,scale.Y);

        ///order, TL,BL,BR,TR
        Raylib_cs.Raylib.DrawRectangleGradientEx(rec,TL,BL,BR,TR);
        Rlgl.rlPopMatrix();
    }
}
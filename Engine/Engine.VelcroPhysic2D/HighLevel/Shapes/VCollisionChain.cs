using Engine.Velcro.Unit;
using Genbox.VelcroPhysics.Collision.Shapes;
using Genbox.VelcroPhysics.Definitions.Shapes;
using Genbox.VelcroPhysics.Shared;
using System.ComponentModel;
using System.Numerics;

namespace Engine.Velcro
{
    public class VCollisionChain :VCollisionShape
    {
        List<Vector2> _verts;

        public VCollisionChain()
        {
            _fixtureDef.Shape = new ChainShape(new Vertices());
        }
        public VCollisionChain(List<Vector2> vertices) : this()  
        {
            _verts = vertices;
        }

        public VCollisionChain(Vector2[] vertices) : this()
        {
            _verts = vertices.ToList();
        }

        public override void OnTransformChanged(Transformation.Component component)
        {
            if(component == Transformation.Component.Scale)
            {
                RecreateFixture();
            }
        }
        void RecreateFixture()
        {
            Insist.IsNotNull(_verts,"vertices CANNOT be null!");

            DestroyFixture();

            Vertices verts = new Vertices(_verts.Select((v)=>v.ToMVec2()));
            verts.Scale(Transform.Scale.ToMVec2() * VConvert.DisplayToSim);

            var chainShape = _fixtureDef.Shape as ChainShape;
            chainShape.Vertices = verts;

            CreateFixture();
            
        }
    }
}


using Engine;
using Engine.Velcro;
using Genbox.VelcroPhysics.Collision.ContactSystem;
using Genbox.VelcroPhysics.Dynamics;
using System.Collections;
using System.Numerics;

namespace Trex_runner
{
    public class Cactus : Component,IUpdatable
    {
        CactusManager cactusManager;
        VCollisionBox collision;
        private bool touched;

        public int UpdateOrder { get; set; }

        public override void OnAddedToEntity()
        {
            if(Entity.TryGetComponent(out collision))
            {
                collision.SetSensor(true);
                collision.OnCollision = OnContact;
            }
            if(Entity.TryGetComponent(out cactusManager))
            {
                
            }
        }

        private void OnContact(Fixture fixtureProxyA, Fixture fixtureProxyB,Contact contact)
        {
            //Entity.Destroy(this.Entity);
            touched = true;
            Console.WriteLine("game end" + fixtureProxyB.UserData);
        }

        public override void OnRemovedFromEntity()
        {
            
            if(cactusManager != null)
            {
                cactusManager.Actives.Remove(this.Entity);
            }
            cactusManager = null;
            collision = null;
            Console.WriteLine($"Destroy{this.Entity.ID}");

        }

        public override Component DeepClone()
        {
            return new Cactus();
        }

        public void Update()
        {
            Transform.Position2 -= Vector2.UnitX * 400 * Time.DeltaTime;

            if (touched)
            {
                GameManager.Instance?.StopGame();
                touched = false;
            }
        }
    }
}

using Engine;
using Engine.Timer;

namespace Trex_runner
{
    public class CactusManager : Component,IUpdatable
    {
        public List<Entity> Actives = new List<Entity>();
        Entity cactusOriginal;
        ITimer spawnTimer;

        public int UpdateOrder { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public  CactusManager(Entity origin)
        {
            this.cactusOriginal = origin;
        }



        public void BeginSpawning()
        {
            
            if (spawnTimer != null) return;
            spawnTimer = Core.Schedule(0.7f, true, Entity, (timer) =>
            {
                (timer as rTimer).Duration = Random.Shared.NextSingle() * 1f + 0.1f;
                Console.WriteLine("Spawn");
                if (cactusOriginal != null)
                {
                    var en = Entity.Instantiate(cactusOriginal);
                    en.Name = "catus";
                    Actives.Add(en);
                }
            });
        }

        public void StopSpawning()
        {
            if (spawnTimer == null) return;
            spawnTimer.Stop();
            spawnTimer = null;
        }
        public void ClearCactus()
        {
            foreach (var cactus in Actives)
            {
                Entity.Destroy(cactus);
            }
            Actives.Clear();
        }

        public override void OnAddedToEntity()
        {
            BeginSpawning();
        }


        public void Update()
        {
            for (int i = Actives.Count - 1; i >= 0; i--)
            {
                var en = Actives[i];
                if (en.Transform.Position.X < -Scene.Resolution.X / 2f)
                {
                    Actives.Remove(en);
                    Entity.Destroy(en);
                }

            }
        }

        public override void OnRemovedFromEntity()
        {
            spawnTimer?.Stop();
            cactusOriginal = null;
            Actives.Clear();
        }
    }
}
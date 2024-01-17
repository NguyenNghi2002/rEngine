using Raylib_cs;
using System.Numerics;

namespace Engine.SceneManager
{
    public partial class Scene
    {
        /** UTILITIES **/
        #region APIs / Utilities

        /// <summary>
        /// Create An Entity and automaticlly add to scene list
        /// </summary>
        /// <param name="name"></param>
        /// <returns><see cref="Entity"/></returns>
        public Entity CreateEntity(string name, Vector2 position = default)
        {
            var newEntity = new Entity(name);
            newEntity.Scene = this;
            SceneEntitiesList.RequestAdd(newEntity);
            newEntity.Transform.SetPosition(position.ToVec3());

            return newEntity;
        }
        /// <summary>
        /// Create An Entity, auto assign into parent and automaticlly add to scene list
        /// </summary>
        /// <param name="parent">parent for child attach to</param>
        /// <param name="name">name of child entity</param>
        /// <returns>created child <see cref="Entity"/></returns>
        public Entity CreateChildEntity(Entity parent, string name,Vector2 position = default, bool keepPosition = false)
        {
            var child = CreateEntity(name);
            child.Transform.SetParent(parent.Transform,keepPosition);
            return child;
        }


        /// <summary>
        /// Find <see cref="Entity"/> by name
        /// </summary>
        /// <param name="name">name to find entity</param>
        /// <returns></returns>
        public Entity? Find(string name)
            => SceneEntitiesList.FindByName(name);
        public Entity? FindEntityOfType<T>() where T : Component
            => SceneEntitiesList.FindByComponent<T>();
        public Entity[] FindEntitiesOfType<T>() where T : Component
        {
            var entities = from entity in SceneEntitiesList
                           where entity.HasComponent<T>()
                           select entity;
            return entities.ToArray();
        }
        public List<T> FindComponents<T>() where T : Component
            => SceneEntitiesList.FindAllComponents<T>();
        public Entity FindEntityInParent(Entity parent,Func<Entity,bool> predicate)
        {
            Transformation a = parent.Transform.Childs.SingleOrDefault(tf => predicate.Invoke(tf.Entity));
            return a != null ? a.Entity : null;
        }
        public Entity FindEntityInParent(Entity parent, string name)
            => FindEntityInParent(parent, en => en.Name == name);
        public bool TryFind(string name, out Entity entity)
        {
            entity = SceneEntitiesList.FindByName(name);
            return entity != null;
        }
        public bool TryFindComponent<T>(out T component) where T : Component
        {
            component = SceneEntitiesList.FindAllComponents<T>().Find((p) => true);
            return component != null;
        }
        public bool TryFindComponent<T>(Predicate<T> predicate, out T component) where T : Component
        {
            component = SceneEntitiesList.FindAllComponents<T>().Find(predicate);
            return component != null;
        }

#if true

        public Vector2 WindowToScene(Vector2 offset)
        {
            return new Vector2()
            {
                X = (offset.X - (Core.Instance.WindowWidth - (screenWidth * screenRatio)) * 0.5f) / screenRatio,
                Y = (offset.Y - (Core.Instance.WindowHeight - (screenHeight * 1/screenRatio)) * 0.5f) / 1/screenRatio,
            };
        }
        public Vector2 GetMouseLocalPosition(bool clamp = false)
    => GetMouseLocalPosition(Vector2.Zero, clamp);
        public Vector2 GetMouseLocalPosition(Vector2 offset, bool clamp = false)
        {
            Vector2 mouse = Raylib.GetMousePosition() + offset;
#if false
            if (RaySharpImgui.IsSceneWindowOpened && Core.Instance.ShowInspector)
            {
                var rec = RaySharpImgui.WindowScreenRect;
                if (rec.IsQualify())
                {
                    var mouseCoord = RayUtils.GetCoord01(rec, mouse);
                    mouse = mouseCoord * new Vector2(Core.Instance.WindowWidth, Core.Instance.WindowHeight);
                }
            } 
#endif

            Vector2 virtualMouse = WindowSpaceToViewSpace(mouse);
            return clamp ?
                RaymathF.ClampPoint(virtualMouse, Vector2.Zero, new(screenWidth, screenHeight)) :
                virtualMouse;
        } 
        public float ViewPortWidth
            => FinalRenderTexture.texture.width / Camera.zoom;
        public float ViewPortHeight
            => FinalRenderTexture.texture.height / Camera.zoom;
        public Vector2 ViewPortScale
            => new Vector2(ViewPortWidth, ViewPortHeight);
        public Vector2 GetMouseWorldPosition()
        {
            var mouseLocal = GetMouseLocalPosition();
            return Raylib.GetScreenToWorld2D(mouseLocal, Camera);

        }
        public Vector2 GetMouseWorldPosition(Vector2 offset)
            => Raylib.GetScreenToWorld2D(GetMouseLocalPosition(offset), Camera);
#endif

        public Scene SetCameraZoom(float zoom)
        {
            Camera.zoom = zoom;
            return this;
        }

        public Vector2 GetRenderTextureScale()
            => new Vector2(FinalRenderTexture.texture.width, FinalRenderTexture.texture.height);
        public override string ToString()
        {
            return String.Format($"{SceneName} - {SceneEntitiesList.Count}");
        }
        #endregion

    }
}

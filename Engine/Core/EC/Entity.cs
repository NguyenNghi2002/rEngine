using System.Diagnostics;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using Engine.SceneManager;

namespace Engine
{

    /// <summary>
    /// Entity class hold multiple <see cref="Component"/>.<br/> <br/>
    /// Create using <see cref="Scene.CreateEntity(string)"/> 
    /// will add new entity into current scene
    /// 
    /// </summary>
    public class Entity :  IComparable<Entity>
    {
#if true
        private static int _IdCounter = 0;

        private int _processOrder = 0;
        public int ProcessOrder
        {
            get => _processOrder;
            set => SetProcessOrder(value);
        }
        public Entity SetProcessOrder(int order)
        {
            _processOrder = order;
            Scene.SceneEntitiesList.Sort();
            return this;
        }

        private bool _enable;
        public bool Enable
        {
            get => _enable;
            set => SetEnable(value);
        }

        public Entity SetEnable(bool enable)
        {
            if (_enable != enable)
            {
                _enable = enable;

                foreach (var transform in Transform.Childs)
                    transform.Entity.SetEnable(enable);
            }
            return this;
        }

        public Transformation Transform;
        public Scene Scene;
        public string Name;
        public readonly int ID;

        internal ComponentCollection components;

#endif


        /// <summary>
        /// An instance HAVE NOT attach to any scene yet. 
        /// Suggest use <see cref="Scene.CreateEntity(string)"/> instead
        /// 
        /// </summary>
        /// <param name="name"></param>
        [Obsolete]
        public Entity(string name)
        {
            Name = name;
            ID = _IdCounter;
            _IdCounter++;

            Transform = new Transformation(this);
            components = new ComponentCollection(this);
            Enable = true;
        }

        /// <summary>
        /// Process all Requests list <br/>
        /// After that update every <see cref="UpdatableComponent"/> on everyframe
        /// </summary>
        internal void PopAddingAndRemovingComponentsRequest()
        {
            //Pop request buffer
            components.PopAddingAndRemoveingRequests();
        }
        internal void Start()
        {
            /** Settup Event **/
            while (ComponentCollection._requestStartComponents.Count > 0)
            {
                var cpn = ComponentCollection._requestStartComponents.Dequeue();
                cpn.OnAddedToEntity();
            }
        }
        internal void Update()
        {
#if false
            /** INVOKE Component.OnTransformChanged **/
            if (_notifyTranformChanged)
            {
                foreach (Component cpn in components)
                    cpn.OnTransformChanged(transformedFlags);
                _notifyTranformChanged = false;
            } 
#endif

            for (int i = components.Updatables.Count - 1; i >= 0; i--)
            {
                var cpn = components.Updatables[i];
                if ((cpn as Component).Enable)
                    cpn.Update();
            }
#if false
            foreach (var cpn in components.Updatables)
            {
                if ((cpn as Component).Enable)
                    cpn.Update();
            } 
#endif
        }
        internal void FixedUpdate()
        {
            foreach (var cpn in components.FixedUpdatables)
                cpn.FixedUpdate();
        }
        internal void TransformChanged(Transformation.Component transformedComponent)
        {
            foreach (Component cpn in components)
                cpn.OnTransformChanged(transformedComponent);
            //transformedFlags |= transformedComponent;
            //_notifyTranformChanged = true;
        }



        #region Request Components
        public T AddComponent<T>() where T : Component, new()
        {
            var cpn = new T();
            HandleAddComponent(cpn);
            return cpn;

        }

        /// <summary>
        /// <see cref="Component"/> will be added to request to be add on next frame
        /// </summary>
        /// <param name="component"></param>
        /// <returns><see cref="Entity"/></returns>
        public T AddComponent<T>(T component) where T : Component
        {
            HandleAddComponent(component);
            return component;
        }

        public T AddComponent<T>(T component,out T componentOut) where T : Component
        {
            HandleAddComponent(component);
            componentOut = component;
            return component;
        }

        /// <summary>
        /// <see cref="Component"/> will be added to request to be add on next frame
        /// </summary>
        /// <param name="component"></param>
        /// <returns><see cref="Entity"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddComponents(IEnumerable<Component> components)
        {
            foreach (var cpn in components)
                HandleAddComponent(cpn);
        }

        /// <summary>
        /// <see cref="Component"/> will be added to request to be add on next frame
        /// </summary>
        /// <param name="component"></param>
        /// <returns><see cref="Entity"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddComponents(Component[] components)
        {
            foreach (var cpn in components)
                HandleAddComponent(cpn);
        }

        /// <summary>
        /// <see cref="Component"/> will be added to request to be removed on next frame
        /// </summary>
        /// <param name="component"></param>
        /// <returns><see cref="Entity"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool RemoveComponent<T>() where T : Component
        {
            if(TryGetComponent<T>(out var cpn))
            {
                return RemoveComponent(cpn);
            }
            return false;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool RemoveComponent(Component component)
        {
            return HandleRemoveComponent(component);
        }

        /// <summary>
        /// Find and return component attached to this Entity
        /// </summary>
        /// <typeparam name="T">type of Component</typeparam>
        /// <returns>nullable <see cref="{T}"/></returns>
        public List<T> GetComponents<T>()
            where T : Component
        {
            var founds = components.SolidComponents.Where(c=>c is T).Select(c=>c as T);
            if(founds == null || founds.Count() == 0)
                founds = components.requestAddComponents.Where(c => c is T).Select(c => c as T);
            return founds.ToList() ;
        }
        public T? GetComponent<T>() where T : Component
        {
            T? res = (T?)components.SolidComponents.Find(cpn =>cpn is T);
            if (res == null)
                res = (T?)components.requestAddComponents.Single(cpn => cpn is T);
            return res;

        }


        /// <summary>
        /// Include parent
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetComponentInHirachy<T>() where T : Component
        {
            if (!TryGetComponent<T>(out var result))
                result = GetComponentInChilds<T>();

            return result;
        }

        /// <summary>
        /// Not include parent
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetComponentInChilds<T>() where T : Component
        {
            if (!Transform.HasChilds) return null;

            foreach (var childTF in Transform.Childs)
            {
                if(childTF.Entity.TryGetComponent<T>(out var cpn))
                    return cpn;

                cpn = childTF.Entity.GetComponentInChilds<T>();
                if (cpn != null)
                    return cpn;
            }
            return null;
        }

        /// <summary>
        /// Get all components
        /// </summary>
        public List<Component> Components
            => components.SolidComponents;

        /// <summary>
        /// Check if entity contain <see cref="Component"/>
        /// </summary>
        /// <typeparam name="T"><see cref="Component"/> Type </typeparam>
        /// <returns><see langword="true"/> on found, otherwise <see langword="false"/> </returns>
        public bool HasComponent<T>() where T : Component
        {
            T? res = (T?)components.SolidComponents.Find(c => c is T);
            return res != null;
        }

        /// <summary>
        /// Fetch <see cref="Component"/> if exist, <br/>
        /// out <see langword="null"/> if unable to find
        /// </summary>
        /// <typeparam name="T"><see cref="Component"/> Type</typeparam>
        /// <param  name="component"></param>
        /// <returns><see langword="true"/> on found, otherwise <see langword="false"/> </returns>
        public bool TryGetComponent<T>(out T component) where T : Component
        {
            component = (T?)components.SolidComponents.Find(cpn => cpn is T);
            return component != null;
             
        }

        /// <summary>
        /// Remove all both solid and request <see cref="Component"/> from this <see cref="Entity"/>
        /// </summary>
		public void RemoveAllComponents()
        {
            components.PushRemoveAll();
        } 
        #endregion

        #region Static Methods
        public static Entity Instantiate(Entity original)
            => Instantiate(original, original.Transform);
        public static Entity Instantiate(Entity original, Transformation transfrom)
        {
            //check scene exist
            if (original.Scene == null)
            {
                Console.WriteLine("Entity {0} is removed", original.Name);
                return null;
            }
            //set up entity
            var scene = original.Scene;
            var newEntity = scene.CreateEntity(original.Name + " (clone)");
            newEntity.Scene = scene;
            newEntity.Transform.Copy(transfrom);

            foreach (var originalComponent in original.components.SolidComponents)
            {
                var copiedComponent = originalComponent.DeepClone()
                    .SetEntity(newEntity);
                newEntity.AddComponent(copiedComponent);
            }
            return newEntity;
        }

        /// <summary>
        /// 
        /// Remove all childs and parent attached to this Entity<br/>
        /// <see cref="Component.OnRemovedFromEntity()"/> in entity will invoke in the next frame<br/>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        public static void Destroy(Entity entity)
        {
            Insist.IsNotNull(entity,"Why tf you pass null entity you stupid cunt");

            ///If entity has child, destroy it and deeper child
            if(entity.Transform.ChildsCount > 0)
            {
                for (int i = 0; i < entity.Transform.Childs.Count; i++)
                {
                    Entity.Destroy(entity.Transform.Childs[i].Entity);
                }
                entity.Transform.Childs.Clear();
            }
     
            entity.Scene.SceneEntitiesList.RequestRemove(entity);
        }
        #endregion


        /** HANDLE MODIDY COMPONENT  **/
        #region Handle Methods

        private void HandleAddComponent(Component component)
            => components.PushRequestAdd(component);
        private bool HandleRemoveComponent(Component component)
            => components.PushRequestRemove(component);
        #endregion

        /** OVERRIDE GENERAL METHODS  **/
        #region General Methods
        public int CompareTo(Entity? other)
        {
            return ID.CompareTo(other.ID);
        }
        public override string ToString()
        {
            return "[Entity] " + Name;
        }
        #endregion


        ~Entity()
        {
            Debugging.Log(" {0} deconstructed", Debugging.LogLevel.Comment, this);
        }

    }

	[System.AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
	public sealed class RequireComponentAttribute : Attribute
	{
		public Type Component { get; set; }
	}

}

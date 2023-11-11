using Engine.Renderering;
using System.Collections;
using System.Diagnostics;

namespace Engine
{
    /// <summary>
    /// abstract Class collection specialize for <see cref="Engine.Entity"/> class
    /// </summary>
    internal class ComponentCollection : IEnumerable<Component>
    {
        internal static Queue<Component> _requestStartComponents = new Queue<Component>();

        /// <summary>
        /// Entity collection belong to
        /// </summary>
        public Entity Entity;

        /// <summary>
        /// Solid components list
        /// </summary>
        public List<Component> SolidComponents = new List<Component>();

        //protected readonly Queue<T> requestStartComponents;
        internal HashSet<Component> requestAddComponents = new HashSet<Component>();
        internal HashSet<Component> requestRemoveComponents = new HashSet<Component>();

        public List<IUpdatable> Updatables = new List<IUpdatable>();
        public List<IFixedUpdatable> FixedUpdatables = new List<IFixedUpdatable>();

        public ComponentCollection(Entity entity)
        {
            this.Entity = entity;
        }
        public virtual void PushRequestAdd(Component component)
        {
            //If scene started then add to request list and add to solid list on next frame
            Insist.IsNotNull(component,$"Component {component} can NOT be null");

            if (!SolidComponents.Contains(component))
                requestAddComponents.Add(component);
            else
                throw new Exception($"Component {component} already exist");
            component.SetEntity(Entity);
        }
        public virtual bool PushRequestRemove(Component component)
        {
            Insist.IsNotNull(component ,"Component is null");
            var shouldRemove = SolidComponents.Contains(component) ;

            if (shouldRemove) 
                requestRemoveComponents.Add(component);

            return shouldRemove;
        }

        /// <summary>
        /// Call this to push all request component to solid list<br/>
        /// remove then follow by add
        /// </summary>
        public virtual void PopAddingAndRemoveingRequests()
        {
            
            /** [handle request remove]
             * - Called every on removing component OnRemovedFromEntity()
             * - Removed every component in request remove list from:
             *      - scene renderable list
             *      - scene and this updatable list
             *      - scene and this fixed updatable list
             *      - solid component list
             * - clear all the request
             */
            foreach (Component cpn in requestRemoveComponents)
                cpn.OnRemovedFromEntity();
            foreach (Component cpn in requestRemoveComponents)
            {
                /** Clasify component type **/
                if (cpn is IRenderable renderableComponent)
                {
                    Entity.Scene?.Renderables.Remove(renderableComponent);
                }
                if (cpn is IUpdatable updatableComponent)
                {
                    Entity.Scene?.Updates.Remove(updatableComponent);
                    this.Updatables.Remove(updatableComponent);
                }
                if (cpn is IFixedUpdatable fixedUpdatableComponent)
                {
                    Entity.Scene?.FixedUpdates.Remove(fixedUpdatableComponent);
                    this.FixedUpdatables.Remove(fixedUpdatableComponent);
                }

                requestAddComponents.Remove(cpn); 
                if (SolidComponents.Remove(cpn))
                {
                    cpn.SetEntity(null) ;
                }
            }
            requestRemoveComponents.Clear();

            /// handle request add
          
            foreach (Component cpn in requestAddComponents)
            {
                /** Clasify component type **/
                if (cpn is IRenderable renderableComponent)
                {
                    Entity.Scene?.Renderables.Add(renderableComponent);
                }
                if (cpn is IUpdatable UpdatableComponent)
                {
                    Entity.Scene?.Updates.Add(UpdatableComponent);
                    Updatables.Add(UpdatableComponent);
                }
                if (cpn is IFixedUpdatable fixedUpdatableComponent)
                {
                    Entity.Scene?.FixedUpdates.Add(fixedUpdatableComponent);
                    this.FixedUpdatables.Add(fixedUpdatableComponent);
                }

                //cpn.Entity = entity;
                SolidComponents.Add(cpn);
                cpn.SetEnable(true);
                _requestStartComponents.Enqueue(cpn);
            }


            ///Sort Render Component
            Entity.Scene?.Renderables.Sort(RenderableComponent.RenderComparer);

            requestAddComponents.Clear();
            

        }


        public void PushRemoveAll()
        {
            requestRemoveComponents.UnionWith(_requestStartComponents);
            requestRemoveComponents.UnionWith(SolidComponents);
        }

        /// <summary>
        /// Force clear all component
        /// </summary>
        public void ClearAll()
        {
            requestRemoveComponents.UnionWith(_requestStartComponents);
            requestRemoveComponents.UnionWith(SolidComponents);

            PopAddingAndRemoveingRequests();
        }

        #region IEnumerator Methods
        public IEnumerator<Component> GetEnumerator()
        {
            return SolidComponents.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)SolidComponents).GetEnumerator();
        }

        #endregion
    }
}

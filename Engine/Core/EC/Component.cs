using Engine.SceneManager;
using System.Diagnostics;

namespace Engine
{
    public abstract class Component 
    {
        Entity _entity;
        bool _bEnable  = true;
        public Entity Entity
        {
            get => _entity;
            set => SetEntity(value);
        }
        public Transformation Transform
        {
            get 
            {
                if (Entity == null) return null;
                return Entity.Transform;
            }
        }

        /// <summary>
        /// Shortcut for <see cref="Entity.Scene"/>
        /// </summary>
        protected Scene Scene => _entity.Scene;
        public bool Enable
        {
            get => _bEnable;
            set => SetEnable(value);
        }


        public Component SetEntity(Entity entity)
        {
            _entity = entity;
            return this;
        }

        /// <summary>
        /// this will call up <see cref="OnDisable()"/>
        /// </summary>
        public Component SetEnable(bool isEnable)
        {
            if (isEnable != _bEnable)
            {
                _bEnable = isEnable;
                if (isEnable)
                    OnEnable();
                else
                    OnDisable();
            }
            return this;
            
        }


        #region Life Cycle
        /// <summary>
        /// Called on <see cref="Component"/> being removed from Entity,<see cref="Component"/> deattached after this get called
        /// </summary>
        public virtual void OnRemovedFromEntity() { }

        /// <summary>
        /// Called on <see cref="Component"/> being added to Entity. Communicate script here
        /// </summary>
        public virtual void OnAddedToEntity() { }

        /// <summary>
        /// Call on Entity is Enabled
        /// </summary>
        public virtual void OnEnable() { }

        /// <summary>
        /// Called where Entity is disable
        /// </summary>
        public virtual void OnDisable() { } 

        /// <summary>
        /// Called when Transfrom from this Entity <see cref="Transform"/> being changed. 
        /// </summary>
        public virtual void OnTransformChanged(Transformation.Component component) { }

        public virtual void OnDebugRender() { }
        #endregion

        /// <summary>
        /// This must be overrided in case sub class request to make a deep copy. <br/>
        /// When cloning, Entity of the copied component will be set to Null
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public virtual Component DeepClone()
        {
            throw new NotImplementedException($"component {this.GetType()}: DeepClone is not implementted");
        }


        ~Component()
        {
            Debugging.Log("component [{0}] deconstructed from {1} ", Debugging.LogLevel.Comment, this,Entity);

        }

    }
}

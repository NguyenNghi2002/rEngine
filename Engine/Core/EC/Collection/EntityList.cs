using Engine.SceneManager;
using System.Collections;
using System.Runtime.CompilerServices;

namespace Engine
{

    public class EntityProcessComparer : IComparer<Entity>
	{
		public int Compare(Entity? x, Entity? y)
		{
			int value = y.ProcessOrder.CompareTo(x.ProcessOrder);
			value = value == 0 ? x.ID.CompareTo(y.ID) : value;
			return value;
		}
	}
	public class EntityList:IEnumerable<Entity> 
    {
        /// <summary>
        /// Scene attach to
        /// </summary>
        Scene _scene;

		internal static EntityProcessComparer ProcessDecensingOrderComparer = new EntityProcessComparer();

		public Scene Scene
		{
			get { return _scene; }
			set { _scene = value; }
		}

        List<Entity> _entities                      = new List<Entity>();
        HashSet<Entity> _entitiesRequestAdding      = new HashSet<Entity>();
        HashSet<Entity> _entitiesRequestRemoving    = new HashSet<Entity>();

		public int Count => _entities.Count;

        internal EntityList(Scene scene)
        {
            _scene = scene;
        }

        /// <summary>
        /// push new <see cref="Entity"/> to add request buffer and add in the next frame. 
        /// Automaticly assign scene to <see cref="Entity.Scene"/>
        /// </summary>
        /// <param name="entities"></param>
        public void RequestAdd(Entity entity)
        {
            entity.Scene = _scene;
            _entitiesRequestAdding.Add(entity);
        }
        public void RequestAddRange(IEnumerable<Entity> entities)
        {
            foreach (var entity in entities)
                RequestAdd(entity);
        }

        /// <summary>
        /// push <see cref="Entity"/> to remove request buffer and remove in the next frame
        /// </summary>
        /// <param name="entity"></param>
        public void RequestRemove(Entity entity)
        {
            _entitiesRequestRemoving.Add(entity);
        }

        /// <summary>
        /// clear both current entityList and request buffer
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < _entities.Count; i++)
                Entity.Destroy(_entities[i]);

            ///Flush all request
            PopRemovingEntityRequest();

            ///Clear all collection
            _entities.Clear();
            _entitiesRequestAdding.Clear();
            _entitiesRequestRemoving.Clear();

            GC.Collect();
        }

        /// <summary>
        /// Pop All Adding and Removing Components for each Entity from last frame
        /// </summary>
        public void PopAllComponentsRequest()
		{
			for (int i = 0; i < _entities.Count; i++)
				_entities[i].components.PopAddingAndRemoveingRequests();

            var requestStarts = ComponentCollection._requestStartComponents;
            while (requestStarts.Count > 0)
            {
                requestStarts.Dequeue().OnAddedToEntity();
            }

		}
        /// <summary>
        /// call <see cref="Entity.Update()"/> for each entities in non-request list
        /// </summary>
        internal void UpdateEntities()
        {
            for (int i = 0; i < _entities.Count; i++)
            {
                if (_entities[i].Enable)
                    _entities[i].Update();
            }
        }

		internal void FixedUpdateEntities()
		{
			for (int i = 0; i < _entities.Count; i++)
            {

                if (_entities[i].Enable)
				    _entities[i].FixedUpdate();
            }
		}

        /// <summary>
        /// call <see cref="Entity.Draw()"/> for each entities in non-request list
        /// </summary>
        /// 





        /// <summary>
        /// Pop All Adding and Removing Entites for each Entity from last frame
        /// Pass requesting Entity to solid list
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PopEntityRequests()
        {
            ///NOTE : clear request list after finish 

            bool requestSort =
                _entitiesRequestAdding.Count > 0 ||
                _entitiesRequestRemoving.Count > 0;

            ///Update Add list
            PopAddingEntityRequest();

            ///Update Remove list
            PopRemovingEntityRequest();


            if (requestSort) this.Sort();



            /** Handle request from list**/
            PopAllComponentsRequest();


        }

        /// <summary>
        /// Before removing Entity completely: Clear all components, remove
        /// </summary>
        private void PopRemovingEntityRequest()
        {
            if (_entitiesRequestRemoving.Count > 0)
            {
                foreach (var entity in _entitiesRequestRemoving)
                {
                    entity.components.ClearAll();
                    entity.Transform.SetParent(null);
                    _entities.Remove(entity);

                    entity.Scene = null;
                    entity.Transform = null;
                }
                _entitiesRequestRemoving.Clear();
                
            }
        }
		private void PopAddingEntityRequest()
        {
            if (_entitiesRequestAdding.Count > 0)
            {
                foreach (var entity in _entitiesRequestAdding)
                {
                    _entities.Add(entity);
                    entity.Scene = Scene;
                }
                _entitiesRequestAdding.Clear();
            }
        }

        #region Utils
        public Entity? FindByComponent<T>() where T : Component
           => _entities.Find(e => e.HasComponent<T>());
        public Entity? FindByName(string name)
           => _entities.Find(e => e.Name == name);
        public List<Entity> FindAllByName(string name)
            => _entities.FindAll(e => e.Name == name);
        public List<T> FindAllComponents<T>() where T : Component
        {
            List<T> list = new List<T>();
            foreach (var en in _entities)
            {
                if (en.TryGetComponent<T>(out var cpn))
                {
                    list.Add(cpn);
                }
            }
            return list;
        }
        public void Sort()
        {
            //TODO: sort _entities
            _entities.Sort(0, _entities.Count, ProcessDecensingOrderComparer);
            //_entities.Sort(Entity.ProcessOrderComparer);
        } 
        #endregion

        #region Enumerator Methods
        public IEnumerator<Entity> GetEnumerator()
        {
            return ((IEnumerable<Entity>)_entities).GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_entities).GetEnumerator();
        } 
        #endregion

    }
}

using Genbox.VelcroPhysics.Collision.Filtering;
using Genbox.VelcroPhysics.Collision.Handlers;
using Genbox.VelcroPhysics.Definitions;
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Factories;

namespace Engine.Velcro
{
    /// <summary>
    /// Fixture userdata will set to this
    /// </summary>
    public abstract class VCollisionShape : Component
    {
        protected static float defaultDensitiy = 100f;


        /// <summary>
        /// never null
        /// </summary>
        protected FixtureDef _fixtureDef { get; set; } = new FixtureDef();
        public Fixture RawFixture { get; protected set; }  // also control by VRigidBody2D

        event OnCollisionHandler        _onCollisionHandler;
        event BeforeCollisionHandler    _beforeCollisionHandler;
        event AfterCollisionHandler     _afterCollisionHandler;
        event OnSeparationHandler       _onSeparationHandler;

        /// <summary>
        /// Fixtue A, Fixture B, Contact
        /// </summary>
        public OnCollisionHandler OnCollision
        {
            
            get => RawFixture != null ? RawFixture.OnCollision : _onCollisionHandler;
            set
            {
                if (RawFixture != null)
                    RawFixture.OnCollision = value;
                _onCollisionHandler = value;
            }
        }
        public AfterCollisionHandler AfterCollision
        {

            get => RawFixture != null ? RawFixture.AfterCollision : _afterCollisionHandler;
            set
            {
                if (RawFixture != null)
                    RawFixture.AfterCollision = value;
                _afterCollisionHandler = value;
            }
        }
        public BeforeCollisionHandler BeforeCollision
        {

            get => RawFixture != null ? RawFixture.BeforeCollision : _beforeCollisionHandler;
            set
            {
                if (RawFixture != null)
                    RawFixture.BeforeCollision = value;
                _beforeCollisionHandler = value;
            }
        }
        public OnSeparationHandler OnSeparation
        {
            get => RawFixture != null ? RawFixture.OnSeparation : _onSeparationHandler;
            set
            {
                if (RawFixture != null)
                    RawFixture.OnSeparation = value;
                _onSeparationHandler = value;
            }
        }

        public override void OnAddedToEntity()
        {
            CreateFixture();
        }
        /// <summary>
        /// <inheritdoc/><br/><br/>
        /// <see  cref="OnRemovedFromEntity()"/> is required.
        /// </summary>
        public override void OnRemovedFromEntity()
        {
            DestroyFixture();
            _onCollisionHandler = null;
            _onSeparationHandler = null;
            _fixtureDef = null;
        }

        /// <summary>
        /// Invoke When RigidBody added to entity
        /// </summary>
        internal virtual void CreateFixture()
        {
            if (RawFixture != null) return;

            var rb = Entity.GetComponent<VRigidBody2D>();
            if (rb == null || rb.Body == null) return;

            var body = rb.Body;
            _fixtureDef.UserData = this.Entity;
            RawFixture =  FixtureFactory.CreateFromDef(body,_fixtureDef);


            RawFixture.OnCollision += _onCollisionHandler;
            RawFixture.OnSeparation += _onSeparationHandler;
        }

        /// <summary>
        /// Fixture will be set to null
        /// </summary>
        internal virtual void DestroyFixture()
        {
            if (RawFixture == null) return;

            var rb = Entity.GetComponent<VRigidBody2D>();
            if (rb == null || rb.Body == null)
                return;

            rb.Body.RemoveFixture(RawFixture);
            RawFixture.OnSeparation = null;
            RawFixture.OnCollision = null;
            RawFixture = null;
        }

        protected void WakeContactBodies()
        {
            var body = Entity.GetComponent<VRigidBody2D>().Body;
            var currEdge = body.ContactList;
            while(currEdge != null)
            {
                var contact = currEdge.Contact;
                if(contact.FixtureA == this.RawFixture 
                    || contact.FixtureB == this.RawFixture)
                {
                    contact.FixtureA.Body.Awake = true;
                    contact.FixtureB.Body.Awake = true;
                }

                currEdge = currEdge.Next;
            }
        }

        
        internal virtual void DrawShape(Body body) 
        {
        }

        public bool IsSensor
        {
            get=>RawFixture != null ? RawFixture.IsSensor : _fixtureDef.IsSensor;
            set=>SetSensor(value);
        }
        public Category CollidesWith
        {
            get => RawFixture != null ? RawFixture.CollidesWith : _fixtureDef.Filter.CategoryMask;
            set => SetCollideWith(value);
        }

        #region Configuration

        public VCollisionShape SetCollideWith(Category category)
        {
            if (RawFixture != null) RawFixture.CollidesWith = category;
            _fixtureDef.Filter.CategoryMask = category;
            return this;
        }
        public VCollisionShape SetCollideGroup(Category category)
        {
            if (RawFixture != null) RawFixture.CollisionCategories = category;
            _fixtureDef.Filter.Category = category;
            return this;
        }
        public VCollisionShape SetSensor(bool isSensor)
        {
            if (RawFixture != null) RawFixture.IsSensor = isSensor;
            _fixtureDef.IsSensor = isSensor;
            return this;
        }
        public VCollisionShape SetRestitution(float value)
        {
            if (RawFixture != null) RawFixture.Restitution = value;
            _fixtureDef.Restitution = value;
            return this;
        }
        public VCollisionShape SetRestitutionThreshold(float value)
        {
            if (RawFixture != null) RawFixture.RestitutionThreshold = value;
            _fixtureDef.RestitutionThreshold = value;
            return this;
        }
        public VCollisionShape SetFriction(float percent)
        {
            if (RawFixture != null) RawFixture.Friction = percent;
            _fixtureDef.Friction = percent;
            return this;
        }
        #endregion



    }

}
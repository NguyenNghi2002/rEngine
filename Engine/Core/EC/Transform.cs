using System.Numerics;
using System.Runtime.CompilerServices;

namespace Engine
{
    public partial class Transformation
    {
        public enum Component : byte
        {
            Position = 1 << 0,
            Rotation = 1 << 1,
            Scale = 1 << 2,
        }
        [Flags]
        internal enum DirtyFlags : byte
        {
            World = 1 << 0,
            Local = 1 << 1
        }

        [Flags]
        internal enum MatrixDirtyFlags : byte
        {
            WorldToLocal = 1 << 0,
            LocalToWorld = 1 << 1,
            Local = 1 << 2,
        }
    }
    //TODO: need optimize amount of update matrix 
    // optimize Clean transform component 
    public partial class Transformation
    {

        /// <summary>
        /// Entity this Transform attached to
        /// </summary>
        public Entity Entity;
        public Transformation(Entity entity)
        {
            Entity = entity;
            _localPosition = Vector3.Zero;
            _localRotation = Quaternion.Identity;
            _localScale = Vector3.One;
            _localToWorldMatrix = Matrix4x4.Identity;
        }

        private readonly List<Transformation> _childrens = new List<Transformation>();
        private Transformation? _parent;
        #region Parent-children
        //childs
        public Transformation AddChild(Transformation transfrom, bool keepPosition = true)
        {
            transfrom.SetParent(this,keepPosition);
            return this;
        }
        public List<Transformation> Childs => _childrens;
        public int ChildsCount => _childrens.Count;
        public bool HasChilds => _childrens.Count > 0;

        //Parent
        public Transformation? Parent
        {
            get => _parent;
            set => SetParent(value);
        }
        public Transformation SetParent(Transformation? parent,bool keepPosition = true)
        {
            //If same transfrom then skip
            if (ReferenceEquals(_parent, parent) || ReferenceEquals(this, parent))
                return this;


            //If old parent has value, remove this from old parent's child list
            _parent?._childrens.Remove(this);

            //if new parent is not null, add this to new parent's child list
            parent?._childrens.Add(this);

            Vector3 oldPos = Position;
            Vector3 oldScale = Scale;
            Quaternion oldRot = Rotation;
            _parent = parent;

            MakeHierarchyTranformDirty();
            UpdateHierarchyTranformation();

            if (keepPosition)
            {
                SetPosition(oldPos);
                SetRotation(oldRot);
                SetScale(oldScale);

                //MakeHierarchyTranformDirty();
                //UpdateHierarchyTranformation();  
            }

#if false

            _localPosition = parent != null ?
                this.Position - parent.Position
                : Position;

            _localRotation = parent != null ?
                this.GetRotation() / parent.GetRotation()
                : GetRotation();

#endif
            return this;
        }
        #endregion


        private DirtyFlags _flags = DirtyFlags.World | DirtyFlags.Local;

        #region Private Field
        /* WORLD SPACE */
        private Vector3 _position = Vector3.Zero;
        private Quaternion _rotation = Quaternion.Identity;
        private Vector3 _scale = Vector3.One;
        /* LOCAL SPACE */
        private Vector3 _localPosition = Vector3.Zero;
        private Quaternion _localRotation = Quaternion.Identity;
        private Vector3 _localEulerRotation = Vector3.Zero;
        private Vector3 _localScale = Vector3.One;
        private Matrix4x4 _localToWorldMatrix,
                            _localMatrix,
                            _worldToLocalMatrix;
        private Vector3 _eulerRotation;
        #endregion

        public Matrix4x4 LocalToWorld
        {
            get
            {
                ComputeWorldMatrix();
                return _localToWorldMatrix;
            }
        }


        public Vector3 LocalScale
        {
            get => _localScale;
            set => SetLocalScale(value);
        }
        public Transformation SetLocalScale(Vector2 localScale)
            => SetLocalScale(new Vector3(localScale,0f));
        public Transformation SetLocalScale(Vector3 localScale)
        {
            if (localScale == _localScale)
                return this;

            _localScale = localScale;

            if (_parent != null)
            {
                
                _parent.MakeHierarchyTranformDirty();
            }
            else
            {
                _scale = localScale;
                this.MakeHierarchyTranformDirty();
            }
            NotifyTransformChanged(Component.Scale);

            return this;

        }

        public Vector3 Scale
        {
            get
            {
                
                UpdateHierarchyTranformation();
                Matrix4x4.Decompose(LocalToWorld,out var scale,out _, out _);
                //return new Vector3(scale.X,scale.Y,scale.Z) ;
                return scale;
            }
            set => SetScale(value);
        }

        #region SetScale(...)
        public Transformation SetScale(float x, float y)
    => SetScale(new Vector2(x, y));
        public Transformation SetScale(float x, float y, float z)
            => SetScale(new Vector3(x, y, z));
        public Transformation SetScale(Vector2 scale)
            => SetScale(new Vector3(scale.X, scale.Y, 1f));
        public Transformation SetScale(Vector3 scale)
        {
            _scale = scale;

            if (_parent != null)
            {
                Matrix4x4.Decompose(_worldToLocalMatrix,out var worldScale,out _,out _);
                SetLocalScale(worldScale * scale);
            }
            else
                SetLocalScale(scale);

            return this;
        } 
        #endregion


        public Vector2 LocalPosition2
        {
            get => _localPosition.ToVec2() ;
            set => SetLocalPosition(value);
        }
        public Vector3 LocalPosition
        {
            get => _localPosition;
            set => SetLocalPosition(value);
        }

        public Transformation SetLocalPosition(Vector2 newLocalPosition)
        {
            return SetLocalPosition(new Vector3(newLocalPosition.X, newLocalPosition.Y, 0f));
        }
        public Transformation SetLocalPosition(Vector3 newLocalPosition)
        {
            if (newLocalPosition == _localPosition)
                return this;


            _localPosition = newLocalPosition;

            if (_parent != null)
            {

                _parent.MakeHierarchyTranformDirty();
            }
            else
            {
                _position = newLocalPosition;
                this.MakeHierarchyTranformDirty();
            }

            NotifyTransformChanged(Component.Position);

            return this;
        }

        public Vector2 Position2
        {
            get => Position.ToVec2();
            set => Position = value.ToVec3() ;
        }
        public Vector3 Position
        {
            get
            {
                UpdateHierarchyTranformation();
                return LocalToWorld.Translation;
            }
            set => SetPosition(value);
        }
        public Transformation SetPosition(float x,float y,float z)
            =>SetPosition(new Vector3(x,y,z));
        public Transformation SetPosition(Vector2 position)
            => SetPosition(new Vector3(position,0f));
        public Transformation SetPosition(Vector3 position)
        {
            /** this guard won't work for locking world position when deattach from parent**/
            //if (position == _position) return this;

            _position = position;

            if (_parent != null)
                SetLocalPosition(Vector3.Transform(position, _worldToLocalMatrix));
            else
                SetLocalPosition(position);
            
            return this;
        }

        #region Local Axis Rotate
        public Transformation SetLocalRotationZ(float radian)
        {
            var eulerRot = ToEulerAngles(_localRotation);
            eulerRot.Z = radian;
            var local = ToQuaternion(eulerRot);
            return SetLocalRotation(local);
        }
        public Transformation SetLocalRotationY(float radian)
        {
            var eulerRot = ToEulerAngles(_localRotation);
            eulerRot.Y = radian;
            var local = ToQuaternion(eulerRot);
            return SetLocalRotation(local);
        }
        public Transformation SetLocalRotationX(float radian)
        {
            var eulerRot = ToEulerAngles(_localRotation);
            eulerRot.X = radian;
            var local = ToQuaternion(eulerRot);
            return SetLocalRotation(local);
        }
        #endregion

        public Vector3 EulerLocalRotation
        {
            get => _localEulerRotation;
            set => SetLocalEulerRotation(value);
        }
        public Quaternion LocalRotation
        {
            get => _localRotation;
            set => SetLocalRotation(value);
        }
        public Transformation SetLocalRotation(Quaternion localRotation)
        {
            if (localRotation == _localRotation) return this;

            _localRotation = localRotation;
            _localEulerRotation = ToEulerAngles(localRotation);

            if (_parent != null)
            {
                _parent.MakeHierarchyTranformDirty();

            }
            else
            {
                _eulerRotation = _localEulerRotation;
                _rotation = _localRotation;
                this.MakeHierarchyTranformDirty();
            }

            NotifyTransformChanged(Component.Rotation);
            return this;
        }
        public Transformation SetLocalEulerRotation(Vector3 eulerRotation)
        {
            if (eulerRotation == _localEulerRotation) return this;

            _localRotation = ToQuaternion(eulerRotation);
            _localEulerRotation = eulerRotation;

            if (_parent != null)
            {
                _parent.MakeHierarchyTranformDirty();
            }
            else
            {
                _eulerRotation = _localEulerRotation;
                _rotation = _localRotation;
                this.MakeHierarchyTranformDirty();
            }

            NotifyTransformChanged(Component.Rotation);
            return this;
        }

        /// <summary>
        /// Roation in X,Y,Z Axis. In Radian
        /// </summary>
        public Vector3 EulerRotation
        {
            get
            {
                UpdateHierarchyTranformation();
                Matrix4x4.Decompose(_localToWorldMatrix, out _, out Quaternion rotation, out _);
                return ToEulerAngles(rotation);
            }
            set => SetEulerRotation(value);
        }
        public Quaternion Rotation
        {
            get
            {
                UpdateHierarchyTranformation();
                Matrix4x4.Decompose(_localToWorldMatrix, out _, out Quaternion rotation, out _);
                return rotation;
            }
            set => SetRotation(value);
        }

        public Transformation SetRotation(Vector3 axis, float rotation)
            => SetRotation(Quaternion.CreateFromAxisAngle(axis, rotation));
        public Transformation SetEulerRotation(Vector3 eulerRotation)
            => SetRotation(ToQuaternion(eulerRotation));
        public Transformation SetRotation(Quaternion rotation)
        {
            /** this guard won't work for locking world position when deattach from parent**/
            //if(rotation == _rotation) return this;
            _rotation = rotation;

            if (_parent != null)
                SetLocalRotation(rotation / _parent.GetRotation());
            else
                SetLocalRotation(rotation);

            return this;
        }

        private Quaternion GetRotation()
        {
            Matrix4x4.Decompose(_localToWorldMatrix, out _, out Quaternion rotation, out _);
            return rotation;
        }

        /// <summary>
        /// Copy data , not include child
        /// </summary>
        /// <param name="transformation"></param>
        public void Copy(Transformation transformation)
        {
            Position = transformation.Position;
            Rotation = transformation.Rotation;
            Scale = transformation.Scale;
        }

        #region Calculating Methods
        public Matrix4x4 ComputeLocalMatrix()
        {
            if (IsLocalClean()) return _localMatrix;


            /* Calculate each tramform matrix tranform components */
            var rotationMat = Matrix4x4.CreateFromQuaternion(_localRotation);
            var translationMat = Matrix4x4.CreateTranslation(_localPosition);
            var scalingMat = Matrix4x4.CreateScale(_localScale);

            /* TRS 
            In order of scaling -> rotation -> translation 
             */
            var mat = Matrix4x4.Multiply(scalingMat, rotationMat);
            _localMatrix = mat = Matrix4x4.Multiply(mat, translationMat);

            MakeLocalClean();

            return mat;
        }

        /// <summary>
        /// Update  <see cref="_localMatrix"/> and <see cref="_localToWorldMatrix"/>
        /// </summary>
        /// <returns>World matrix</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix4x4 ComputeWorldMatrix()
        {
            // return if _flags has no world dirty flag on
            if (IsWorldClean()) return _localToWorldMatrix;

            ComputeLocalMatrix();
            if (_parent != null)
            {
                _localToWorldMatrix = _localMatrix * _parent.ComputeWorldMatrix();
                Matrix4x4.Invert(_parent._localToWorldMatrix, out _worldToLocalMatrix);
            }
            else
            {
                _localToWorldMatrix = _localMatrix;
                _worldToLocalMatrix = Matrix4x4.Identity;
            }

            MakeWorldClean();

            return _localToWorldMatrix;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UpdateHierarchyTranformation()
        {
            var localDirty = (_flags & DirtyFlags.Local);
            var worldDirty = (_flags & DirtyFlags.Local);
            if ((localDirty & worldDirty) == 0)
                return;

            ComputeWorldMatrix();

            foreach (var child in _childrens)
            {
                child.UpdateHierarchyTranformation();
            }
        }

        #endregion

        #region Dirty Flags
        private void MakeHierarchyTranformDirty()
        {
            MakeLocalDirty();
            MakeWorldDirty();

            if (_childrens == null) return;
            if (_childrens.Count == 0) return;

            foreach (var child in _childrens)
            {
                child.MakeLocalDirty();
                child.MakeWorldDirty();
                child.MakeHierarchyTranformDirty();
            }

        }
        private void MakeChildrenLocalDirty()
        {

            foreach (var child in _childrens)
            {
                //Call entity event
                child.MakeLocalDirty();
                child.MakeChildrenLocalDirty();
            }
        }
        private void MakeChildrenWorldDirty()
        {
            foreach (var child in _childrens)
            {
                //Call entity event
                child.MakeWorldDirty();
                child.MakeChildrenWorldDirty();
            }
        }
        private void MakeLocalDirty() => _flags |= DirtyFlags.Local;
        private void MakeWorldDirty() => _flags |= DirtyFlags.World;
        private void MakeLocalClean() => _flags &= ~DirtyFlags.Local;
        private void MakeWorldClean() => _flags &= ~DirtyFlags.World;
        private bool IsLocalClean() => (_flags & DirtyFlags.Local) == 0;
        private bool IsWorldClean() => (_flags & DirtyFlags.World) == 0;
        #endregion

        private void NotifyTransformChanged(Component component)
        {
            Entity.TransformChanged(component); 
            foreach (var c in Childs)
            {
                c.NotifyTransformChanged(component);
            }
        }

        /// <summary>
        /// Credit <see href="https://stackoverflow.com/questions/70462758/c-sharp-how-to-convert-quaternions-to-euler-angles-xyz"/> 
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        public static Vector3 ToEulerAngles(Quaternion q)
        {
            Vector3 angles = new();

            // roll / x
            double sinr_cosp = 2 * (q.W * q.X + q.Y * q.Z);
            double cosr_cosp = 1 - 2 * (q.X * q.X + q.Y * q.Y);
            angles.X = (float)Math.Atan2(sinr_cosp, cosr_cosp);

            // pitch / y
            double sinp = 2 * (q.W * q.Y - q.Z * q.X);
            if (Math.Abs(sinp) >= 1)
            {
                angles.Y = (float)Math.CopySign(Math.PI / 2, sinp);
            }
            else
            {
                angles.Y = (float)Math.Asin(sinp);
            }

            // yaw / z
            double siny_cosp = 2 * (q.W * q.Z + q.X * q.Y);
            double cosy_cosp = 1 - 2 * (q.Y * q.Y + q.Z * q.Z);
            angles.Z = (float)Math.Atan2(siny_cosp, cosy_cosp);

            return angles;
        }
        public static Quaternion ToQuaternion(Vector3 v)
        {

            float cy = (float)Math.Cos(v.Z * 0.5);
            float sy = (float)Math.Sin(v.Z * 0.5);
            float cp = (float)Math.Cos(v.Y * 0.5);
            float sp = (float)Math.Sin(v.Y * 0.5);
            float cr = (float)Math.Cos(v.X * 0.5);
            float sr = (float)Math.Sin(v.X * 0.5);

            return new Quaternion
            {
                W = (cr * cp * cy + sr * sp * sy),
                X = (sr * cp * cy - cr * sp * sy),
                Y = (cr * sp * cy + sr * cp * sy),
                Z = (cr * cp * sy - sr * sp * cy)
            };

        }
        
    }

}

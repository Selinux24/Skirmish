using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public class SceneObject : IDisposable
    {
        protected object baseObject = null;

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Processing order
        /// </summary>
        public int Order { get; set; }
        /// <summary>
        /// Visible
        /// </summary>
        public bool Visible { get; set; }
        /// <summary>
        /// Active
        /// </summary>
        public bool Active { get; set; }
        /// <summary>
        /// Gets or sets whether the object is static
        /// </summary>
        public bool Static { get; set; }
        /// <summary>
        /// Gets or sets whether the object cast shadow
        /// </summary>
        public bool CastShadow { get; set; }
        /// <summary>
        /// Gets or sets whether the object is enabled to draw with the deferred renderer
        /// </summary>
        public bool DeferredEnabled { get; set; }
        /// <summary>
        /// Uses depth info
        /// </summary>
        public bool DepthEnabled { get; set; }
        /// <summary>
        /// Enables transparent blending
        /// </summary>
        public bool AlphaEnabled { get; set; }
        /// <summary>
        /// Object usage
        /// </summary>
        public SceneObjectUsageEnum Usage { get; internal set; }
        /// <summary>
        /// Maximum instance count
        /// </summary>
        public int Count
        {
            get
            {
                if (this.Is<IComposed>())
                {
                    return this.Get<IComposed>().Count;
                }

                return 1;
            }
        }

        protected SceneObject(object obj, SceneObjectDescription description)
        {
            this.baseObject = obj;

            this.Active = true;
            this.Visible = true;
            this.Order = 0;

            this.Name = description.Name;
            this.Static = description.Static;
            this.CastShadow = description.CastShadow;
            this.DeferredEnabled = description.DeferredEnabled;
            this.DepthEnabled = description.DepthEnabled;
            this.AlphaEnabled = description.AlphaEnabled;
            this.Usage = SceneObjectUsageEnum.None;
        }

        public T Get<T>()
        {
            if (this.baseObject is T)
            {
                return (T)this.baseObject;
            }

            return default(T);
        }

        public bool Is<T>()
        {
            return (this.baseObject is T);
        }

        public void Dispose()
        {
            if (this.baseObject is IDisposable)
            {
                Helper.Dispose((IDisposable)this.baseObject);
            }
        }
    }

    public class SceneObject<T> : SceneObject
    {
        public T Instance
        {
            get
            {
                return (T)base.baseObject;
            }
        }
        public IRayPickable<Triangle> Geometry
        {
            get
            {
                return this.Get<IRayPickable<Triangle>>();
            }
        }
        public Manipulator3D Transform
        {
            get
            {
                return this.Get<ITransformable3D>()?.Manipulator;
            }
        }
        public Manipulator2D ScreenTransform
        {
            get
            {
                return this.Get<ITransformable2D>()?.Manipulator;
            }
        }

        public SceneObject(T obj, SceneObjectDescription description) : base(obj, description)
        {

        }

        public Y GetComponent<Y>(int index)
        {
            var cmpObj = this.Get<IComposed>();
            if (cmpObj != null)
            {
                var components = cmpObj.GetComponents<Y>().ToArray();

                return components[index];
            }
            else
            {
                return default(Y);
            }
        }
    }

    /// <summary>
    /// Scene object usajes enumeration
    /// </summary>
    [Flags]
    public enum SceneObjectUsageEnum : byte
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0x0,
        /// <summary>
        /// Full triangle list for path finding
        /// </summary>
        FullPathFinding = 0x1,
        /// <summary>
        /// Coarse list for path finding
        /// </summary>
        CoarsePathFinding = 0x2,
        /// <summary>
        /// Scene ground
        /// </summary>
        Ground = 0x4,
        /// <summary>
        /// Scene agent
        /// </summary>
        Agent = 0x8,
    }

    /// <summary>
    /// Scene object description
    /// </summary>
    public class SceneObjectDescription
    {
        /// <summary>
        /// Name
        /// </summary>
        public string Name = null;
        /// <summary>
        /// Is Static
        /// </summary>
        public bool Static = false;
        /// <summary>
        /// Gets or sets whether the object cast shadow
        /// </summary>
        public bool CastShadow = false;
        /// <summary>
        /// Can be renderer by the deferred renderer
        /// </summary>
        public bool DeferredEnabled = true;
        /// <summary>
        /// Uses depth info
        /// </summary>
        public bool DepthEnabled = true;
        /// <summary>
        /// Enables transparent blending
        /// </summary>
        public bool AlphaEnabled = false;

        /// <summary>
        /// Use spheric volume for culling by default
        /// </summary>
        public bool SphericVolume = true;
    }
}

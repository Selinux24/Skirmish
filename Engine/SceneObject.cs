using System;
using System.Linq;

namespace Engine
{
    /// <summary>
    /// Scene object
    /// </summary>
    public class SceneObject : IDisposable
    {
        /// <summary>
        /// Internal object
        /// </summary>
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

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="description">Description</param>
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
        /// <summary>
        /// Resource dispose
        /// </summary>
        public void Dispose()
        {
            if (this.baseObject is IDisposable)
            {
                Helper.Dispose((IDisposable)this.baseObject);
            }
        }

        /// <summary>
        /// Gets the internal instance as specified type
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <returns>Returns the instance as type</returns>
        public T Get<T>()
        {
            if (this.baseObject is T)
            {
                return (T)this.baseObject;
            }

            return default(T);
        }
        /// <summary>
        /// Gets if the instance implements type
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <returns>Returns true if the instance implements the type</returns>
        public bool Is<T>()
        {
            return (this.baseObject is T);
        }

        /// <summary>
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns the instance as text</returns>
        public override string ToString()
        {
            return string.Format("SceneObject: {0}", this.Name);
        }
    }

    /// <summary>
    /// Scene object
    /// </summary>
    /// <typeparam name="T">Internal instance type</typeparam>
    public class SceneObject<T> : SceneObject
    {
        /// <summary>
        /// Gets the instance
        /// </summary>
        public T Instance
        {
            get
            {
                return (T)base.baseObject;
            }
        }
        /// <summary>
        /// Gets the geometry helper
        /// </summary>
        public IRayPickable<Triangle> Geometry
        {
            get
            {
                return this.Get<IRayPickable<Triangle>>();
            }
        }
        /// <summary>
        /// Gets the 3D manipulator helper
        /// </summary>
        public Manipulator3D Transform
        {
            get
            {
                return this.Get<ITransformable3D>()?.Manipulator;
            }
        }
        /// <summary>
        /// Gets the 2D manipulator helper
        /// </summary>
        public Manipulator2D ScreenTransform
        {
            get
            {
                return this.Get<ITransformable2D>()?.Manipulator;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="obj">Instance</param>
        /// <param name="description">Description</param>
        public SceneObject(T obj, SceneObjectDescription description) : base(obj, description)
        {

        }

        /// <summary>
        /// Gets the component by index
        /// </summary>
        /// <typeparam name="Y">Component type</typeparam>
        /// <param name="index">Index</param>
        /// <returns>Returns the component at index that implements type</returns>
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
    public enum SceneObjectUsageEnum
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,
        /// <summary>
        /// Full triangle list for path finding
        /// </summary>
        FullPathFinding = 1,
        /// <summary>
        /// Coarse list for path finding
        /// </summary>
        CoarsePathFinding = 2,
        /// <summary>
        /// Scene ground
        /// </summary>
        Ground = 1,
        /// <summary>
        /// Scene agent
        /// </summary>
        Agent = 8,
        /// <summary>
        /// User interface
        /// </summary>
        UI = 16,
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

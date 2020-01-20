using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine
{
    /// <summary>
    /// Scene object
    /// </summary>
    /// <typeparam name="T">Internal instance type</typeparam>
    public class SceneObject<T> : ISceneObject
    {
        /// <summary>
        /// Gets the instance
        /// </summary>
        public T Instance { get; private set; }
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
        public SceneObjectUsages Usage { get; set; }
        /// <summary>
        /// Maximum instance count
        /// </summary>
        public int InstanceCount
        {
            get
            {
                if (this.Is<IComposed>())
                {
                    return this.Get<IComposed>().InstanceCount;
                }

                return 1;
            }
        }
        /// <summary>
        /// Gets or sets if the current object has a parent
        /// </summary>
        public bool HasParent { get; internal set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="obj">Instance</param>
        /// <param name="description">Description</param>
        public SceneObject(T obj, SceneObjectDescription description)
        {
            this.Instance = obj;

            this.Active = true;
            this.Visible = true;
            this.Order = 0;

            this.Name = description.Name;
            this.CastShadow = description.CastShadow;
            this.DeferredEnabled = description.DeferredEnabled;
            this.DepthEnabled = description.DepthEnabled;
            this.AlphaEnabled = description.AlphaEnabled;
            this.Usage = SceneObjectUsages.None;
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~SceneObject()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        /// <param name="disposing">Free managed resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                //Dispose the instace
                if (this.Instance is IDisposable disposable)
                {
                    disposable.Dispose();
                }

                this.Instance = default;
            }
        }

        /// <summary>
        /// Gets the internal instance as specified type
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <returns>Returns the instance as type</returns>
        public TType Get<TType>()
        {
            if (this.Instance is TType typedObject)
            {
                return typedObject;
            }

            return default;
        }
        /// <summary>
        /// Gets if the instance implements type
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <returns>Returns true if the instance implements the type</returns>
        public bool Is<TType>()
        {
            return (this.Instance is TType);
        }

        /// <summary>
        /// Gets the current object triangle collection
        /// </summary>
        /// <param name="usage">Object usage</param>
        /// <returns>Returns the triangle list</returns>
        public IEnumerable<Triangle> GetTriangles(SceneObjectUsages usage)
        {
            List<Triangle> tris = new List<Triangle>();

            List<IRayPickable<Triangle>> volumes = new List<IRayPickable<Triangle>>();

            bool isComposed = this.Is<IComposed>();
            if (!isComposed)
            {
                var trn = this.Get<ITransformable3D>();
                trn?.Manipulator.UpdateInternals(true);

                var pickable = this.Get<IRayPickable<Triangle>>();
                if (pickable != null)
                {
                    volumes.Add(pickable);
                }
            }
            else
            {
                var currComposed = this.Get<IComposed>();

                var trnChilds = currComposed.GetComponents<ITransformable3D>();
                if (trnChilds.Any())
                {
                    foreach (var child in trnChilds)
                    {
                        child.Manipulator.UpdateInternals(true);
                    }
                }

                var pickableChilds = currComposed.GetComponents<IRayPickable<Triangle>>();
                if (pickableChilds.Any())
                {
                    volumes.AddRange(pickableChilds);
                }
            }

            for (int p = 0; p < volumes.Count; p++)
            {
                var full = usage == SceneObjectUsages.None || this.Usage.HasFlag(usage);

                var vTris = volumes[p].GetVolume(full);
                if (vTris.Any())
                {
                    //Use volume mesh
                    tris.AddRange(vTris);
                }
            }

            return tris;
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
}

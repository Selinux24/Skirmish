using System;
using SharpDX;

namespace Engine.Animation
{
    /// <summary>
    /// Joint
    /// </summary>
    public class Joint
    {
        /// <summary>
        /// World matrix
        /// </summary>
        private Matrix world;
        /// <summary>
        /// Local matrix
        /// </summary>
        private Matrix local;

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Parent joint
        /// </summary>
        public Joint Parent { get; set; }
        /// <summary>
        /// Child joints
        /// </summary>
        public Joint[] Childs { get; set; }
        /// <summary>
        /// World matrix
        /// </summary>
        public Matrix World
        {
            get
            {
                return this.world;
            }
            set
            {
                this.world = value;

                Vector3 scale;
                Quaternion rotation;
                Vector3 translation;
                if (this.world.Decompose(out scale, out rotation, out translation))
                {
                    this.WorldScale = scale;
                    this.WorldRotation = rotation;
                    this.WorldTranslation = translation;
                }
                else
                {
                    throw new Exception("Bad transform");
                }
            }
        }
        /// <summary>
        /// World translation
        /// </summary>
        public Vector3 WorldTranslation { get; private set; }
        /// <summary>
        /// World rotation
        /// </summary>
        public Quaternion WorldRotation { get; private set; }
        /// <summary>
        /// World scale
        /// </summary>
        public Vector3 WorldScale { get; private set; }
        /// <summary>
        /// Local matrix
        /// </summary>
        public Matrix Local
        {
            get
            {
                return this.local;
            }
            set
            {
                this.local = value;

                Vector3 scale;
                Quaternion rotation;
                Vector3 translation;
                if (this.local.Decompose(out scale, out rotation, out translation))
                {
                    this.LocalScale = scale;
                    this.LocalRotation = rotation;
                    this.LocalTranslation = translation;
                }
                else
                {
                    throw new Exception("Bad transform");
                }
            }
        }
        /// <summary>
        /// Local translation
        /// </summary>
        public Vector3 LocalTranslation { get; private set; }
        /// <summary>
        /// Local rotation
        /// </summary>
        public Quaternion LocalRotation { get; private set; }
        /// <summary>
        /// Local scale
        /// </summary>
        public Vector3 LocalScale { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public Joint()
        {
            this.World = Matrix.Identity;
            this.Local = Matrix.Identity;
        }

        /// <summary>
        /// Gets text representation
        /// </summary>
        /// <returns>Return text representation</returns>
        public override string ToString()
        {
            return string.Format("Name: {0};", string.IsNullOrEmpty(this.Name) ? "root" : this.Name);
        }
    }
}

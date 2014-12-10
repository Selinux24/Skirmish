using System;
using SharpDX;

namespace Engine.Common
{
    [Serializable]
    public class Joint
    {
        private Matrix world;
        private Matrix local;

        public string Name { get; set; }
        public Joint Parent { get; set; }
        public Joint[] Childs { get; set; }
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
            }
        }
        public Vector3 WorldTranslation { get; private set; }
        public Quaternion WorldRotation { get; private set; }
        public Vector3 WorldScale { get; private set; }
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
            }
        }
        public Vector3 LocalTranslation { get; private set; }
        public Quaternion LocalRotation { get; private set; }
        public Vector3 LocalScale { get; private set; }

        public Joint()
        {
            this.World = Matrix.Identity;
            this.Local = Matrix.Identity;
        }

        public override string ToString()
        {
            return string.Format("Name: {0};", string.IsNullOrEmpty(this.Name) ? "root" : this.Name);
        }
    }
}

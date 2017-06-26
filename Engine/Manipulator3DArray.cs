using SharpDX;
using System;
using System.Collections.Generic;

namespace Engine
{
    /// <summary>
    /// Manipulator array
    /// </summary>
    public class Manipulator3DArray : IManipulator
    {
        /// <summary>
        /// Manipulator dictionary
        /// </summary>
        private Dictionary<string, Manipulator3D> dict = new Dictionary<string, Manipulator3D>();
        /// <summary>
        /// Transform names
        /// </summary>
        private string[] names;
        /// <summary>
        /// Transform dependencies
        /// </summary>
        private int[] dependencies;

        /// <summary>
        /// Gets the root manipulator
        /// </summary>
        public Manipulator3D Root
        {
            get
            {
                return this.dict[this.names[0]];
            }
        }
        /// <summary>
        /// Gets the manipulator by name
        /// </summary>
        /// <param name="name">Transform name</param>
        /// <returns>Returns the manipulator by transform name</returns>
        public Manipulator3D this[string name]
        {
            get
            {
                return this.dict[name];
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="names">Transform names</param>
        /// <param name="dependencies">Transform dependencies</param>
        public Manipulator3DArray(string[] names, int[] dependencies)
        {
            this.names = names;
            this.dependencies = dependencies;

            for (int i = 0; i < names.Length; i++)
            {
                this.dict.Add(names[i], new Manipulator3D());
            }
        }

        /// <summary>
        /// Updates manipulator array
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public void Update(GameTime gameTime)
        {
            foreach (var item in this.dict.Values)
            {
                item.Update(gameTime);
            }
        }

        /// <summary>
        /// Gets the transform by name
        /// </summary>
        /// <param name="name">Transform name</param>
        /// <returns>Returns the transform by name</returns>
        public Matrix Transform(string name)
        {
            var trn = this.dict[name];

            Matrix transform = trn.LocalTransform;

            int index = this.dependencies[Array.IndexOf(this.names, name)];
            while (index >= 0)
            {
                var parentName = this.names[index];
                var prt = this.dict[parentName];

                transform *= prt.LocalTransform;

                index = this.dependencies[index];
            }

            return transform;
        }
    }
}

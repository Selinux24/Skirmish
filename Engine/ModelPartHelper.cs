using Engine.Common;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine
{
    /// <summary>
    /// Model part helper
    /// </summary>
    public class ModelPartHelper
    {
        /// <summary>
        /// Model parts
        /// </summary>
        private readonly List<IModelPart> modelParts = new();

        /// <summary>
        /// Model part count
        /// </summary>
        public int Count
        {
            get
            {
                return modelParts.Count;
            }
        }
        /// <summary>
        /// Gets the root part
        /// </summary>
        public IModelPart Root
        {
            get
            {
                return modelParts.Find(p => p.Parent == null);
            }
        }

        /// <summary>
        /// Add model parts
        /// </summary>
        /// <param name="names">Part names</param>
        /// <param name="dependences">Part dependences</param>
        public IEnumerable<IModelPart> AddModelParts(string[] names, int[] dependences, EventHandler manipulatorChangedEvent)
        {
            int parents = dependences.Count(i => i == -1);
            if (parents != 1)
            {
                throw new EngineException("Model with transform dependences must have one (and only one) parent mesh identified by -1");
            }

            if (Array.Exists(dependences, i => i < -1 || i > dependences.Length - 1))
            {
                throw new EngineException("Bad transform dependences indices.");
            }

            for (int i = 0; i < names.Length; i++)
            {
                modelParts.Add(new ModelPart(names[i]));
            }

            for (int i = 0; i < names.Length; i++)
            {
                var thisPart = modelParts.Find(p => p.Name == names[i]);
                if (thisPart == null)
                {
                    continue;
                }

                var parentIndex = dependences[i];
                if (parentIndex >= 0)
                {
                    thisPart.Manipulator.Updated += manipulatorChangedEvent;

                    var parentPart = modelParts.Find(p => p.Name == names[parentIndex]);
                    thisPart.SetParent(parentPart);
                }
            }

            return modelParts;
        }
        /// <summary>
        /// Sets model part transforms from original meshes
        /// </summary>
        /// <param name="drawData">Drawing data</param>
        public void SetTransforms(DrawingData drawData)
        {
            foreach (var part in modelParts)
            {
                var mesh = drawData?.GetMeshByName(part.Name);
                if (mesh == null)
                {
                    continue;
                }

                part.InitialTransform = mesh.Transform;
            }
        }
        /// <summary>
        /// Gets model part by name
        /// </summary>
        /// <param name="name">Name</param>
        public IModelPart GetModelPartByName(string name)
        {
            return modelParts.Find(p => p.Name == name);
        }
        /// <summary>
        /// Gets the part transform by name
        /// </summary>
        /// <param name="name">Name</param>
        public Matrix? GetTransformByName(string name)
        {
            return GetModelPartByName(name)?.GetTransform();
        }
        /// <summary>
        /// Updates internal state
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public void Update(GameTime gameTime)
        {
            modelParts.ForEach(p => p.Manipulator.Update(gameTime));
        }
    }
}

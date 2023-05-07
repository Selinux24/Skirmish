using Engine;
using Engine.Common;
using Engine.Physics;
using Engine.Physics.Colliders;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Physics
{
    /// <summary>
    /// Floor
    /// </summary>
    public class PhysicsFloor : IPhysicsObject
    {
        /// <inheritdoc/>
        public IRigidBody RigidBody { get; private set; }
        /// <inheritdoc/>
        public IEnumerable<ICollider> Colliders { get; private set; }
        /// <summary>
        /// Model
        /// </summary>
        public Model Model { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public PhysicsFloor(IRigidBody body, Model model)
        {
            RigidBody = body ?? throw new ArgumentNullException(nameof(body), $"Physics object must have a rigid body.");
            Model = model ?? throw new ArgumentNullException(nameof(model), $"Physics object must have a model.");

            var tris = model.GetTriangles(true);
            tris = Triangle.Transform(tris, Matrix.Invert(model.Manipulator.FinalTransform));

            var colliders = new ICollider[tris.Count()];
            for (int i = 0; i < tris.Count(); i++)
            {
                colliders[i] = new TriangleCollider(tris.ElementAt(i));
                colliders[i].Attach(body);
            }

            Colliders = colliders;
        }

        /// <inheritdoc/>
        public void Update()
        {
            if (RigidBody == null)
            {
                return;
            }

            if (Model == null)
            {
                return;
            }

            Model.Manipulator.SetRotation(RigidBody.Rotation);
            Model.Manipulator.SetPosition(RigidBody.Position);
        }
        /// <inheritdoc/>
        public bool BroadPhaseTest(IPhysicsObject obj)
        {
            if (obj == null)
            {
                return false;
            }

            return Model.Intersects(IntersectDetectionMode.Box, obj.GetBroadPhaseBounds());
        }
        /// <inheritdoc/>
        public IEnumerable<ICollider> GetBroadPhaseColliders(IPhysicsObject obj)
        {
            var cullingVolume = obj.GetBroadPhaseBounds();

            return Colliders.Where(c => IntersectionHelper.Intersects(cullingVolume, (IntersectionVolumeSphere)c.BoundingSphere));
        }
        /// <inheritdoc/>
        public ICullingVolume GetBroadPhaseBounds()
        {
            return Model.GetIntersectionVolume(IntersectDetectionMode.Box);
        }
        /// <inheritdoc/>
        public void Reset(Matrix transform)
        {
            RigidBody?.SetInitialState(transform);
        }
        /// <inheritdoc/>
        public void Reset(Vector3 position, Quaternion rotation)
        {
            RigidBody?.SetInitialState(position, rotation);
        }
    }
}

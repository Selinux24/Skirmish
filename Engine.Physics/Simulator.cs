using Engine.Collections.Generic;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Physics
{
    /// <summary>
    /// Simulator
    /// </summary>
    public sealed class Simulator
    {
        /// <summary>
        /// Maximum iterations in the update pass
        /// </summary>
        public const int MaximumSimulationIterations = 100;

        /// <summary>
        /// Collision data structure
        /// </summary>
        private readonly ContactResolver contactResolver = new();
        /// <summary>
        /// Physics object list
        /// </summary>
        private readonly List<IPhysicsObject> physicsObjects = new();
        /// <summary>
        /// Global force generator list
        /// </summary>
        private readonly List<IGlobalForceGenerator> globalForceGenerators = new();
        /// <summary>
        /// Local force generator list
        /// </summary>
        private readonly List<ILocalForceGenerator> localForceGenerators = new();
        /// <summary>
        /// Contact generator list
        /// </summary>
        private readonly List<IContactGenerator> contactGenerators = new();
        /// <summary>
        /// Space partitioning OcTree
        /// </summary>
        private readonly OcTree<IPhysicsObject> octree;
        /// <summary>
        /// Broad phase contact pair list
        /// </summary>
        private readonly List<ContactPair> contactPairs = new();

        /// <summary>
        /// Simulation velocity
        /// </summary>
        public float Velocity { get; set; } = 1f;
        /// <summary>
        /// Simulation iterations
        /// </summary>
        public int SimulationIterations { get; set; } = 24;

        /// <summary>
        /// Gets the physics objects count
        /// </summary>
        public int PhysicsObjectsCount { get => physicsObjects?.Count ?? 0; }
        /// <summary>
        /// Gets the global forces count
        /// </summary>
        public int GlobalForcesCount { get => globalForceGenerators?.Count ?? 0; }
        /// <summary>
        /// Gets the local forces count
        /// </summary>
        public int LocalForcesCount { get => localForceGenerators?.Count ?? 0; }
        /// <summary>
        /// Gets the contact generators count
        /// </summary>
        public int ContactGeneratorsCount { get => contactGenerators?.Count ?? 0; }

        /// <summary>
        /// Constructor
        /// </summary>
        public Simulator(BoundingBox worldBounds, int itemsPerNode)
        {
            octree = new OcTree<IPhysicsObject>(worldBounds, itemsPerNode);
        }

        /// <summary>
        /// Update physics
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public void Update(GameTime gameTime)
        {
            int iterations = Math.Clamp(SimulationIterations, 1, MaximumSimulationIterations);

            // Gets the simulation elapsed time
            float time = gameTime.ElapsedSeconds * Velocity / iterations;
            if (MathUtil.IsZero(time))
            {
                return;
            }

            // Iterate each time amount
            for (int i = 0; i < iterations; i++)
            {
                // Update active force generators
                UpdateForceGenerators(time);

                // Integrate bodies
                IntegrateBodies(time);

                // Reset contact data
                contactResolver.Reset();

                // Update active contact generators
                UpdateContactGenerators();

                // Broad phase
                BroadPhase();

                // Narrow phase
                NarrowPhase();

                // Resolve the contacts
                contactResolver.Resolve(time);
            }

            // Clean inactive objects
            Clean();
        }

        /// <summary>
        /// Updates the force generators
        /// </summary>
        /// <param name="time">Elapsed time</param>
        private void UpdateForceGenerators(float time)
        {
            //Update local force generators
            localForceGenerators.ForEach(f => f.UpdateForce(time));

            //Update global force generators
            globalForceGenerators.ForEach(f =>
            {
                f.UpdateForce(time);
                physicsObjects.ForEach(o => f.ApplyForce(o.RigidBody));
            });
        }
        /// <summary>
        /// Integrate bodies
        /// </summary>
        /// <param name="time">Elapsed time</param>
        private void IntegrateBodies(float time)
        {
            //Integrate forces
            physicsObjects.ForEach(o =>
            {
                if (o.RigidBody?.IsAwake == true)
                {
                    o.RigidBody.Integrate(time);
                }

                o.Update();
            });
        }
        /// <summary>
        /// Updates the contact generators
        /// </summary>
        private void UpdateContactGenerators()
        {
            // Process contact generators
            foreach (var contactGenerator in contactGenerators)
            {
                if (!contactResolver.HasFreeContacts())
                {
                    break;
                }

                contactGenerator.AddContact(contactResolver, 0);
            }
        }
        /// <summary>
        /// Detect potential contacts between objects
        /// </summary>
        private void BroadPhase()
        {
            // Populate OcTree
            octree.Clear();
            physicsObjects.ForEach(p => octree.Insert(p.GetBroadPhaseBounds(), p));

            // Clear contact pairs
            contactPairs.Clear();

            // Test physics bodies contacts
            for (int i = 0; i < physicsObjects.Count; i++)
            {
                if (!contactResolver.HasFreeContacts())
                {
                    break;
                }

                var obj1 = physicsObjects[i];

                var colliders = octree.Query(obj1.GetBroadPhaseBounds());

                for (int j = 0; j < colliders.Count(); j++)
                {
                    if (!contactResolver.HasFreeContacts())
                    {
                        break;
                    }

                    var obj2 = colliders.ElementAt(j);
                    if (obj1 == obj2)
                    {
                        continue;
                    }

                    if (!obj1.RigidBody.IsAwake && !obj2.RigidBody.IsAwake)
                    {
                        continue;
                    }

                    if (!obj1.BroadPhaseTest(obj2))
                    {
                        continue;
                    }

                    AddPair(obj1, obj2);
                }
            }
        }
        /// <summary>
        /// Adds a new contact pair to the broad phase contact collection
        /// </summary>
        /// <param name="obj1">First physics object</param>
        /// <param name="obj2">Second physics object</param>
        private void AddPair(IPhysicsObject obj1, IPhysicsObject obj2)
        {
            var contactPair = new ContactPair { Obj1 = obj1, Obj2 = obj2 };

            if (contactPairs.Contains(contactPair))
            {
                return;
            }

            if (contactPairs.Contains(new ContactPair { Obj1 = obj2, Obj2 = obj1 }))
            {
                return;
            }

            contactPairs.Add(contactPair);
        }
        /// <summary>
        /// Detect contacts between potential contact pairs
        /// </summary>
        private void NarrowPhase()
        {
            if (!contactPairs.Any())
            {
                return;
            }

            contactPairs.ForEach(EvaluateContactPair);
        }
        /// <summary>
        /// Evaluates the contact pair collision
        /// </summary>
        /// <param name="contactPair">Contact pair</param>
        private void EvaluateContactPair(ContactPair contactPair)
        {
            var colliders1 = contactPair.Obj1.GetBroadPhaseColliders(contactPair.Obj2);
            if (!colliders1.Any())
            {
                return;
            }

            var colliders2 = contactPair.Obj2.GetBroadPhaseColliders(contactPair.Obj1);
            if (!colliders2.Any())
            {
                return;
            }

            foreach (var collider1 in colliders1)
            {
                if (!contactResolver.HasFreeContacts())
                {
                    break;
                }

                foreach (var collider2 in colliders2)
                {
                    if (!contactResolver.HasFreeContacts())
                    {
                        break;
                    }

                    ContactDetector.BetweenObjects(collider1, collider2, contactResolver);
                }
            }
        }
        /// <summary>
        /// Clean inactive generators
        /// </summary>
        private void Clean()
        {
            for (int i = globalForceGenerators.Count - 1; i >= 0; i--)
            {
                if (globalForceGenerators[i].IsActive)
                {
                    continue;
                }

                globalForceGenerators.RemoveAt(i);
            }

            for (int i = localForceGenerators.Count - 1; i >= 0; i--)
            {
                if (localForceGenerators[i].IsActive)
                {
                    continue;
                }

                localForceGenerators.RemoveAt(i);
            }

            for (int i = contactGenerators.Count - 1; i >= 0; i--)
            {
                if (contactGenerators[i].IsActive)
                {
                    continue;
                }

                contactGenerators.RemoveAt(i);
            }
        }

        /// <summary>
        /// Gets the physics objects list
        /// </summary>
        public IEnumerable<IPhysicsObject> GetPhysicsObjects()
        {
            return physicsObjects.AsReadOnly();
        }
        /// <summary>
        /// Adds a new physics object list to the simulation
        /// </summary>
        /// <param name="physicsObjects">Physics object list</param>
        public void AddPhysicsObjects(IEnumerable<IPhysicsObject> physicsObjects)
        {
            if (physicsObjects?.Any() != true)
            {
                return;
            }

            foreach (var physicsObject in physicsObjects)
            {
                AddPhysicsObject(physicsObject);
            }
        }
        /// <summary>
        /// Adds a new physics object to the simulation
        /// </summary>
        /// <param name="physicsObject">Physics object</param>
        public void AddPhysicsObject(IPhysicsObject physicsObject)
        {
            if (physicsObject == null)
            {
                return;
            }

            if (physicsObjects.Contains(physicsObject))
            {
                return;
            }

            physicsObjects.Add(physicsObject);

            //Order by mass
            physicsObjects.Sort((p, b) =>
            {
                float pMass = p.RigidBody?.Mass ?? float.PositiveInfinity;
                float bMass = b.RigidBody?.Mass ?? float.PositiveInfinity;

                return pMass.CompareTo(bMass);
            });
        }
        /// <summary>
        /// Removes the physics object from simulation
        /// </summary>
        /// <param name="physicsObject">Physics object</param>
        public void RemovePhysicsObject(IPhysicsObject physicsObject)
        {
            if (physicsObject == null)
            {
                return;
            }

            if (!physicsObjects.Contains(physicsObject))
            {
                return;
            }

            physicsObjects.Remove(physicsObject);
        }
        /// <summary>
        /// Clears the physics object list
        /// </summary>
        public void ClearPhysicsObjects()
        {
            physicsObjects.Clear();
        }

        /// <summary>
        /// Gets the global force generators list
        /// </summary>
        public IEnumerable<IGlobalForceGenerator> GetGlobalForces()
        {
            return globalForceGenerators.AsReadOnly();
        }
        /// <summary>
        /// Adds a new global force generator list to the simulation
        /// </summary>
        /// <param name="forceGenerators">Force generator list</param>
        public void AddGlobalForces(IEnumerable<IGlobalForceGenerator> forceGenerators)
        {
            if (forceGenerators?.Any() != true)
            {
                return;
            }

            foreach (var forceGenerator in forceGenerators)
            {
                AddGlobalForce(forceGenerator);
            }
        }
        /// <summary>
        /// Adds a new global force generator to the simulation
        /// </summary>
        /// <param name="forceGenerator">Force generator</param>
        public void AddGlobalForce(IGlobalForceGenerator forceGenerator)
        {
            if (forceGenerator == null)
            {
                return;
            }

            if (globalForceGenerators.Contains(forceGenerator))
            {
                return;
            }

            globalForceGenerators.Add(forceGenerator);
        }
        /// <summary>
        /// Removes the global force generator from simulation
        /// </summary>
        /// <param name="forceGenerator">Force generator</param>
        public void RemoveGlobalForce(IGlobalForceGenerator forceGenerator)
        {
            if (forceGenerator == null)
            {
                return;
            }

            if (!globalForceGenerators.Contains(forceGenerator))
            {
                return;
            }

            globalForceGenerators.Remove(forceGenerator);
        }
        /// <summary>
        /// Clears the global force generators list
        /// </summary>
        public void ClearGlobalForces()
        {
            globalForceGenerators.Clear();
        }

        /// <summary>
        /// Gets the local force generators list
        /// </summary>
        public IEnumerable<ILocalForceGenerator> GetLocalForces()
        {
            return localForceGenerators.AsReadOnly();
        }
        /// <summary>
        /// Adds a new local force generator list to the simulation
        /// </summary>
        /// <param name="forceGenerators">Force generator list</param>
        public void AddLocalForces(IEnumerable<ILocalForceGenerator> forceGenerators)
        {
            if (forceGenerators?.Any() != true)
            {
                return;
            }

            foreach (var forceGenerator in forceGenerators)
            {
                AddLocalForce(forceGenerator);
            }
        }
        /// <summary>
        /// Adds a new local force generator to the simulation
        /// </summary>
        /// <param name="forceGenerator">Force generator</param>
        public void AddLocalForce(ILocalForceGenerator forceGenerator)
        {
            if (forceGenerator == null)
            {
                return;
            }

            if (localForceGenerators.Contains(forceGenerator))
            {
                return;
            }

            localForceGenerators.Add(forceGenerator);
        }
        /// <summary>
        /// Removes the local force generator from simulation
        /// </summary>
        /// <param name="forceGenerator">Force generator</param>
        public void RemoveLocalForce(ILocalForceGenerator forceGenerator)
        {
            if (forceGenerator == null)
            {
                return;
            }

            if (!localForceGenerators.Contains(forceGenerator))
            {
                return;
            }

            localForceGenerators.Remove(forceGenerator);
        }
        /// <summary>
        /// Clears the force generators list
        /// </summary>
        public void ClearLocalForces()
        {
            localForceGenerators.Clear();
        }

        /// <summary>
        /// Gets the contact generators list
        /// </summary>
        public IEnumerable<IContactGenerator> GetContactGenerators()
        {
            return contactGenerators.AsReadOnly();
        }
        /// <summary>
        /// Adds a new contact generator list to the simulation
        /// </summary>
        /// <param name="contactGenerators">Contact generator list</param>
        public void AddContactGenerators(IEnumerable<IContactGenerator> contactGenerators)
        {
            if (contactGenerators?.Any() != true)
            {
                return;
            }

            foreach (var contactGenerator in contactGenerators)
            {
                AddContactGenerator(contactGenerator);
            }
        }
        /// <summary>
        /// Adds a new contact generator to the simulation
        /// </summary>
        /// <param name="contactGenerator">Contact generator</param>
        public void AddContactGenerator(IContactGenerator contactGenerator)
        {
            if (contactGenerator == null)
            {
                return;
            }

            if (contactGenerators.Contains(contactGenerator))
            {
                return;
            }

            contactGenerators.Add(contactGenerator);
        }
        /// <summary>
        /// Removes the contact generator from simulation
        /// </summary>
        /// <param name="contactGenerator">Contact generator</param>
        public void RemoveContactGenerator(IContactGenerator contactGenerator)
        {
            if (contactGenerator == null)
            {
                return;
            }

            if (!contactGenerators.Contains(contactGenerator))
            {
                return;
            }

            contactGenerators.Remove(contactGenerator);
        }
        /// <summary>
        /// Clears the contact generators list
        /// </summary>
        public void ClearContactGenerators()
        {
            contactGenerators.Clear();
        }
    }
}

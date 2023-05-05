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
                UpdateForces(time);

                // Integrate bodies
                IntegrateBodies(time);

                // Generate contacts
                GenerateContacts();

                // Resolve the contacts
                contactResolver.Resolve(time);
            }

            // Clean inactive objects
            Clean();
        }

        /// <summary>
        /// Updates the simulation objects
        /// </summary>
        /// <param name="time">Elapsed time</param>
        private void UpdateForces(float time)
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
        /// Gets the contacts for the current moment
        /// </summary>
        private void GenerateContacts()
        {
            // Reset contact data
            contactResolver.Reset();

            // Process contact generators
            foreach (var contactGenerator in contactGenerators)
            {
                if (!contactResolver.HasFreeContacts())
                {
                    break;
                }

                contactGenerator.AddContact(contactResolver, 0);
            }

            // Test physics bodies contacts
            for (int i = 0; i < physicsObjects.Count; i++)
            {
                if (!contactResolver.HasFreeContacts())
                {
                    break;
                }

                for (int j = i + 1; j < physicsObjects.Count; j++)
                {
                    if (!contactResolver.HasFreeContacts())
                    {
                        break;
                    }

                    var collider1 = physicsObjects[i].Collider;
                    var collider2 = physicsObjects[j].Collider;

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
        /// Adds a new contact generator list to the simulation
        /// </summary>
        /// <param name="contactGenerators">Contact generator list</param>
        public void AddContacts(IEnumerable<IContactGenerator> contactGenerators)
        {
            if (contactGenerators?.Any() != true)
            {
                return;
            }

            foreach (var contactGenerator in contactGenerators)
            {
                AddContact(contactGenerator);
            }
        }
        /// <summary>
        /// Adds a new contact generator to the simulation
        /// </summary>
        /// <param name="contactGenerator">Contact generator</param>
        public void AddContact(IContactGenerator contactGenerator)
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
        public void RemoveContact(IContactGenerator contactGenerator)
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
        public void ClearContacts()
        {
            contactGenerators.Clear();
        }
    }
}

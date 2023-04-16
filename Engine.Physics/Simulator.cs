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
        private readonly ContactResolver contactResolver = new ContactResolver();
        /// <summary>
        /// Physics object list
        /// </summary>
        private readonly List<IPhysicsObject> physicsObjects = new List<IPhysicsObject>();
        /// <summary>
        /// Force generator list
        /// </summary>
        private readonly List<IForceGenerator> forceGenerators = new List<IForceGenerator>();
        /// <summary>
        /// Contact generator list
        /// </summary>
        private readonly List<IContactGenerator> contactGenerators = new List<IContactGenerator>();

        /// <summary>
        /// Simulation velocity
        /// </summary>
        public float Velocity { get; set; } = 1f;
        /// <summary>
        /// Simulation iterations
        /// </summary>
        public int SimulationIterations { get; set; } = 24;

        /// <summary>
        /// Update physics
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public void Update(GameTime gameTime)
        {
            int iterations = Math.Clamp(SimulationIterations, 1, MaximumSimulationIterations);

            // Get time simulation
            float time = gameTime.ElapsedSeconds * Velocity / iterations;
            if (MathUtil.IsZero(time))
            {
                return;
            }

            for (int i = 0; i < iterations; i++)
            {
                // Update simulation objects
                UpdateObjects(time);

                // Generate contacts
                GenerateContacts();

                // Resolve the contacts
                contactResolver.Resolve(time);
            }
        }

        /// <summary>
        /// Updates the simulation objects
        /// </summary>
        /// <param name="time">Time</param>
        private void UpdateObjects(float time)
        {
            //Apply force generators
            var bodies = physicsObjects
                .Select(o => o.RigidBody)
                .ToArray();

            foreach (var body in bodies)
            {
                foreach (var forceGenerator in forceGenerators)
                {
                    forceGenerator.UpdateForce(body, time);
                }
            }

            //Integrate forces
            foreach (var obj in physicsObjects)
            {
                if (obj.RigidBody?.IsAwake == true)
                {
                    obj.RigidBody.Integrate(time);
                }

                obj.Update();
            }
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
        /// Adds a new force generator list to the simulation
        /// </summary>
        /// <param name="forceGenerators">Force generator list</param>
        public void AddForces(IEnumerable<IForceGenerator> forceGenerators)
        {
            if (forceGenerators?.Any() != true)
            {
                return;
            }

            foreach (var forceGenerator in forceGenerators)
            {
                AddForce(forceGenerator);
            }
        }
        /// <summary>
        /// Adds a new force generator to the simulation
        /// </summary>
        /// <param name="forceGenerator">Force generator</param>
        public void AddForce(IForceGenerator forceGenerator)
        {
            if (forceGenerator == null)
            {
                return;
            }

            if (forceGenerators.Contains(forceGenerator))
            {
                return;
            }

            forceGenerators.Add(forceGenerator);
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
    }
}

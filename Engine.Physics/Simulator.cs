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
        /// Maximum number of contacts to generate in the simulation
        /// </summary>
        private const int MaxContacts = 1024;

        /// <summary>
        /// Contact resolver
        /// </summary>
        private ContactResolver contactResolver = new ContactResolver();
        /// <summary>
        /// Collision data structure
        /// </summary>
        private readonly CollisionData contactData = new CollisionData(MaxContacts);
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
        /// Constructor
        /// </summary>
        public Simulator()
        {

        }

        /// <summary>
        /// Update physics
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public void Update(GameTime gameTime)
        {
            // Encontrar la duración de este intervalo para las físicas
            float time = gameTime.ElapsedSeconds;
            if (time <= 0.0f)
            {
                return;
            }

            if (time > 0.05f)
            {
                time = 0.05f;
            }

            // Actualizar los objetos
            UpdateObjects(time);

            // Generar los contactos
            GenerateContacts();

            // Resolver los contactos
            ResolveContacts(time);
        }

        /// <summary>
        /// Update the state of objects
        /// </summary>
        /// <param name="time">Time</param>
        private void UpdateObjects(float time)
        {
            foreach (var obj in physicsObjects)
            {
                foreach (var forceGenerator in forceGenerators)
                {
                    forceGenerator.UpdateForce(obj.Body, time);
                }
            }

            foreach (var obj in physicsObjects)
            {
                if (!obj.Body.IsAwake)
                {
                    continue;
                }

                obj.Body.Integrate(time);
            }
        }
        /// <summary>
        /// Gets the contacts for the current moment
        /// </summary>
        private void GenerateContacts()
        {
            // Prepare collision data
            contactData.Reset();
            contactData.Friction = 0.75f;
            contactData.Restitution = 0.1f;
            contactData.Tolerance = 0.1f;

            // Generate contacs
            foreach (var contactGenerator in contactGenerators)
            {
                if (!contactData.HasFreeContacts())
                {
                    break;
                }

                contactGenerator.AddContact(contactData, 0);
            }

            for (int i = 0; i < physicsObjects.Count; i++)
            {
                if (!contactData.HasFreeContacts())
                {
                    break;
                }

                for (int j = i + 1; j < physicsObjects.Count; j++)
                {
                    if (!contactData.HasFreeContacts())
                    {
                        break;
                    }

                    var collider1 = physicsObjects[i].Collider;
                    var collider2 = physicsObjects[j].Collider;

                    CollisionDetector.BetweenObjects(collider1, collider2, contactData);
                }
            }
        }
        /// <summary>
        /// Contact resolution
        /// </summary>
        /// <param name="time">Time</param>
        private void ResolveContacts(float time)
        {
            if (contactData.ContactCount <= 0)
            {
                return;
            }

            contactData.Resolve(contactResolver, time);
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
                float pMass = p.Body?.Mass ?? float.PositiveInfinity;
                float bMass = b.Body?.Mass ?? float.PositiveInfinity;

                return pMass.CompareTo(bMass);
            });
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

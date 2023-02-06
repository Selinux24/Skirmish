using System.Collections.Generic;

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
        /// Rigid body list
        /// </summary>
        private readonly List<IRigidBody> rigidBodies = new List<IRigidBody>();
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
            float time = gameTime.ElapsedMilliseconds * 0.001f;
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
            for (int i = 0; i < rigidBodies.Count; i++)
            {
                for (int f = 0; f < forceGenerators.Count; f++)
                {
                    forceGenerators[f].UpdateForce(rigidBodies[i], time);
                }
            }

            for (int i = 0; i < rigidBodies.Count; i++)
            {
                if (!rigidBodies[i].IsAwake)
                {
                    continue;
                }

                rigidBodies[i].Integrate(time);
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
        /// Adds a new rigid body to the simulation
        /// </summary>
        /// <param name="rigidBody">Rigid body</param>
        public void AddRigidBody(IRigidBody rigidBody)
        {
            if (rigidBody == null)
            {
                return;
            }

            if (rigidBodies.Contains(rigidBody))
            {
                return;
            }

            rigidBodies.Add(rigidBody);
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

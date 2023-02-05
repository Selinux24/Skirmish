using SharpDX;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Physics
{
    /// <summary>
    /// Collision data
    /// </summary>
    public class CollisionData
    {
        /// <summary>
        /// Maximum contacts
        /// </summary>
        protected const int MaxContacts = 256;

        /// <summary>
        /// Current contact index
        /// </summary>
        private int currentContactIndex = 0;
        /// <summary>
        /// Contact list
        /// </summary>
        private Contact[] contactArray;

        /// <summary> 
        /// Friction factor to add in all collisions
        /// </summary>
        public float Friction = 0f;
        /// <summary> 
        /// Restitution factor to add on all collisions
        /// </summary>
        public float Restitution = 0f;
        /// <summary>
        /// Tolerance
        /// </summary>
        public float Tolerance = 0f;

        /// <summary>
        /// Gets the contact list
        /// </summary>
        public IEnumerable<Contact> ContactArray
        {
            get
            {
                return contactArray;
            }
        }
        /// <summary> 
        /// Gets the current contact
        /// </summary>
        public Contact CurrentContact
        {
            get
            {
                return contactArray[currentContactIndex];
            }
        }
        /// <summary> 
        /// Gets the number of used contacts
        /// </summary>
        public int ContactCount
        {
            get
            {
                return currentContactIndex;
            }
        }
        /// <summary> 
        /// Gets the number of free contacts
        /// </summary>
        public int ContactsLeft
        {
            get
            {
                return contactArray.Length - currentContactIndex;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public CollisionData()
            : this(MaxContacts)
        {

        }
        /// <summary>
        /// Constructor
        /// </summary>
        public CollisionData(int maxContacts)
        {
            InitializeContactArray(maxContacts);
        }

        /// <summary>
        /// Gets if there are more contacts available in the contact list
        /// </summary>
        public bool HasFreeContacts()
        {
            return currentContactIndex < contactArray.Length;
        }
        /// <summary>
        /// Reset contact list
        /// </summary>
        public void Reset()
        {
            currentContactIndex = 0;
        }
        /// <summary>
        /// Resets the contact list to the specified size
        /// </summary>
        /// <param name="maxContacts">Number of contacts in the contact list</param>
        public void Reset(int maxContacts)
        {
            if (contactArray.Length != maxContacts)
            {
                InitializeContactArray(maxContacts);
            }

            Reset();
        }
        /// <summary>
        /// Notifies the instance that a contact has been added.
        /// </summary>
        public void AddContact()
        {
            currentContactIndex++;
        }

        /// <summary>
        /// Initializes the contact list to the specified number
        /// </summary>
        /// <param name="maxContacts">Number of contacts in the contact list</param>
        private void InitializeContactArray(int maxContacts)
        {
            contactArray = new Contact[maxContacts];

            for (int i = 0; i < contactArray.Length; i++)
            {
                contactArray[i] = new Contact();
            }
        }

        /// <summary>
        /// Solve a list of contacts by penetration and speed
        /// </summary>
        /// <param name="contactResolver">Contact resolve parameters</param>
        /// <param name="duration">The duration of the previous frame to compensate for the applied forces</param>
        /// <remarks>
        /// Contacts that cannot interact with the rest should be resolved in separate calls, as the resolution algorithm works better with small lists of contacts.
        /// </remarks>
        public void Resolve(ContactResolver contactResolver, float duration)
        {
            if (ContactCount <= 0)
            {
                return;
            }

            if (!contactResolver.IsValid())
            {
                return;
            }

            // Prepare contacts for processing
            Prepare(duration);

            // Solve interpenetration problems with contacts
            AdjustPositions(contactResolver.PositionIterations, contactResolver.PositionEpsilon);

            // Resolve speed issues with contacts
            AdjustVelocities(duration, contactResolver.VelocityIterations, contactResolver.VelocityEpsilon);
        }
        /// <summary>
        /// Prepare contacts for processing
        /// </summary>
        /// <param name="duration">Duration of the previous frame</param>
        /// <remarks>
        /// This function simply makes sure that the contacts are active and their internal data is up to date.
        /// </remarks>
        private void Prepare(float duration)
        {
            foreach (var contact in contactArray)
            {
                contact.CalculateInternals(duration);
            }
        }
        /// <summary>
        /// Solve contact list positional problems
        /// </summary>
        private void AdjustPositions(int positionIterations, float positionEpsilon)
        {
            int positionIterationsUsed = 0;

            // Resolve interpenetrations in order of severity
            while (positionIterationsUsed < positionIterations)
            {
                // Find the greatest penetration
                float max = positionEpsilon;
                int index = ContactCount;
                for (int i = 0; i < ContactCount; i++)
                {
                    if (contactArray[i].Penetration > max)
                    {
                        max = contactArray[i].Penetration;
                        index = i;
                    }
                }

                if (index == ContactCount)
                {
                    break;
                }

                var contact = contactArray[index];

                // Update contact status
                contact.MatchAwakeState();

                // Solve penetration
                contact.ApplyPositionChange(max, out var linearChange, out var angularChange);

                foreach (var c in EnumerateContactsWithContact(contact, linearChange, angularChange))
                {
                    c.Contact.AdjustPosition(
                        c.LinearChange,
                        c.AngularChange,
                        c.RelativeContactPositionsWorld,
                        c.PenetrationDirection);
                }

                positionIterationsUsed++;
            }
        }
        /// <summary>
        /// Solve speed problems with the contact list
        /// </summary>
        /// <param name="duration">Duration of the previous frame</param>
        private void AdjustVelocities(float duration, int velocityIterations, float velocityEpsilon)
        {
            int velocityIterationsUsed = 0;

            // Resolve impacts in order of severity
            while (velocityIterationsUsed < velocityIterations)
            {
                // Find contacts with maximum magnitude of probable velocity change
                float max = velocityEpsilon;
                int index = ContactCount;
                for (int i = 0; i < ContactCount; i++)
                {
                    if (contactArray[i].DesiredDeltaVelocity > max)
                    {
                        max = contactArray[i].DesiredDeltaVelocity;
                        index = i;
                    }
                }

                if (index == ContactCount)
                {
                    break;
                }

                var contact = contactArray[index];

                // Update contact status
                contact.MatchAwakeState();

                // Resolve contact
                contact.ApplyVelocityChange(out var linearChange, out var angularChange);

                foreach (var c in EnumerateContactsWithContact(contact, linearChange, angularChange))
                {
                    c.Contact.AdjustVelocities(
                        c.LinearChange,
                        c.AngularChange,
                        c.RelativeContactPositionsWorld,
                        c.PenetrationDirection,
                        duration);
                }

                velocityIterationsUsed++;
            }
        }
        /// <summary>
        /// Enumerate multi-collision contacts
        /// </summary>
        /// <param name="contact">Contact</param>
        /// <param name="linearChange">Linear velocity change</param>
        /// <param name="angularChange">Angular velocity change</param>
        private IEnumerable<(Contact Contact, Vector3 LinearChange, Vector3 AngularChange, Vector3 RelativeContactPositionsWorld, int PenetrationDirection)> EnumerateContactsWithContact(Contact contact, IEnumerable<Vector3> linearChange, IEnumerable<Vector3> angularChange)
        {
            foreach (var other in contactArray)
            {
                for (int i = 0; i < other.Bodies.Count(); i++)
                {
                    if (other.Bodies.ElementAt(i) == null)
                    {
                        continue;
                    }

                    for (int o = 0; o < contact.Bodies.Count(); o++)
                    {
                        if (other.Bodies.ElementAt(i) != contact.Bodies.ElementAt(o))
                        {
                            continue;
                        }

                        yield return (other, linearChange.ElementAtOrDefault(o), angularChange.ElementAtOrDefault(o), other.RelativeContactPositionsWorld.ElementAtOrDefault(i), i != 0 ? 1 : -1);
                    }
                }
            }
        }
    }
}

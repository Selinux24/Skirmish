using Engine.Mathematics;
using System;
using System.Linq;

namespace Engine.Physics
{
    /// <summary>
    /// Contact resolver
    /// </summary>
    public class ContactResolver
    {
        /// <summary>
        /// Resolver settings
        /// </summary>
        private readonly ContactResolverSettings settings;
        /// <summary>
        /// Contact list
        /// </summary>
        private readonly Contact[] contacts;
        /// <summary>
        /// Current contact index
        /// </summary>
        private int currentContactIndex = 0;

        /// <summary> 
        /// Gets the current contact
        /// </summary>
        public Contact CurrentContact
        {
            get
            {
                return contacts[currentContactIndex] ??= new Contact();
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
                return contacts.Length - currentContactIndex;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public ContactResolver() : this(ContactResolverSettings.Default)
        {

        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="settings">Resolver settings</param>
        public ContactResolver(ContactResolverSettings settings)
        {
            if (!settings.IsValid())
            {
                throw new ArgumentException($"Incorrect settings configuration.", nameof(settings));
            }

            this.settings = settings;

            contacts = new Contact[settings.MaxContacts];
        }

        /// <summary>
        /// Gets if there are more contacts available in the contact list
        /// </summary>
        public bool HasFreeContacts()
        {
            return ContactsLeft > 0;
        }
        /// <summary>
        /// Reset contact list
        /// </summary>
        public void Reset()
        {
            currentContactIndex = 0;
        }
        /// <summary>
        /// Notifies the instance that a contact has been added.
        /// </summary>
        /// <param name="one">First body</param>
        /// <param name="two">Second body</param>
        /// <param name="position">Contact position</param>
        /// <param name="normal">Contact normal</param>
        /// <param name="penetration">Contact penetration</param>
        public void AddContact(IRigidBody one, IRigidBody two, ref Vector3 position, ref Vector3 normal, float penetration)
        {
            float restitution = (one?.Restitution ?? 1f + two?.Restitution ?? 1f) * 0.5f;
            float friction = MathF.Max(one?.Friction ?? 0f, two?.Friction ?? 0f);

            AddContact(one, two, ref position, ref normal, penetration, restitution, friction);
        }
        /// <summary>
        /// Notifies the instance that a contact has been added.
        /// </summary>
        /// <param name="one">First body</param>
        /// <param name="two">Second body</param>
        /// <param name="position">Contact position</param>
        /// <param name="normal">Contact normal</param>
        /// <param name="penetration">Contact penetration</param>
        /// <param name="restitution">Restitution</param>
        /// <param name="friction">Friction</param>
        public void AddContact(IRigidBody one, IRigidBody two, ref Vector3 position, ref Vector3 normal, float penetration, float restitution, float friction)
        {
            if (!HasFreeContacts())
            {
                return;
            }

            if (CurrentContact?.SetContactData(one, two, ref position, ref normal, penetration, restitution, friction) == true)
            {
                currentContactIndex++;
            }
        }
        /// <summary>
        /// Gets the contact by index
        /// </summary>
        /// <param name="contactIndex">Contact index</param>
        public Contact GetContact(int contactIndex)
        {
            if (contactIndex >= ContactCount)
            {
                return null;
            }

            return contacts[contactIndex];
        }
        /// <summary>
        /// Gets the contact list
        /// </summary>
        public Contact[] GetContacts()
        {
            if (ContactCount <= 0)
            {
                return [];
            }

            return Helper.Take(contacts, ContactCount);
        }

        /// <summary>
        /// Solve a list of contacts by penetration and speed
        /// </summary>
        /// <param name="time">The duration of the previous frame to compensate for the applied forces</param>
        /// <remarks>
        /// Contacts that cannot interact with the rest should be resolved in separate calls, as the resolution algorithm works better with small lists of contacts.
        /// </remarks>
        public void Resolve(float time)
        {
            if (ContactCount <= 0)
            {
                return;
            }

            // Prepare contacts for processing
            Prepare(time);

            // Solve inter-penetration problems with contacts
            AdjustPositions();

            // Resolve speed issues with contacts
            AdjustVelocities(time);
        }
        /// <summary>
        /// Prepare contacts for processing
        /// </summary>
        /// <param name="time">Duration of the previous frame</param>
        /// <remarks>
        /// This function simply makes sure that the contacts are active and their internal data is up to date.
        /// </remarks>
        private void Prepare(float time)
        {
            var contactList = Helper.Take(contacts, ContactCount);
            contactList
                .AsParallel()
                .ForAll(c => c.CalculateInternals(time));
        }

        /// <summary>
        /// Solve contact list positional problems
        /// </summary>
        private void AdjustPositions()
        {
            int positionIterations = settings.PositionIterations;
            int positionIterationsUsed = 0;

            // Resolve inter-penetrations in order of severity
            while (positionIterationsUsed < positionIterations)
            {
                // Find the greatest penetration
                if (!FindMaxPositionChangeContact(out var contact, out var max))
                {
                    break;
                }

                // Update contact status
                contact.MatchAwakeState();

                // Solve penetration
                contact.ApplyPositionChange(max, out var linearChange1, out var linearChange2, out var angularChange1, out var angularChange2);

                for (int i = 0; i < ContactCount; i++)
                {
                    var other = contacts[i];

                    for (int o = 0; o < 2; o++)
                    {
                        var otherBody = other.GetBody(o);

                        for (int c = 0; c < 2; c++)
                        {
                            if (contact.GetBody(c) != otherBody)
                            {
                                continue;
                            }

                            var relativeContactPosition = other.GetRelativeContactPosition(o);
                            int direction = o != 0 ? 1 : -1;
                            var linearChange = c == 0 ? linearChange1 : linearChange2;
                            var angularChange = c == 0 ? angularChange1 : angularChange2;
                            var delta = linearChange + Vector3.Cross(angularChange, relativeContactPosition);
                            other.AdjustPosition(delta, direction);
                        }
                    }
                }

                positionIterationsUsed++;
            }
        }
        /// <summary>
        /// Finds the contact with greatest penetration
        /// </summary>
        /// <param name="contact">Contact</param>
        /// <param name="maxPenetration">Returns the maximum penetration value</param>
        /// <returns>Returns true if a contact was found</returns>
        private bool FindMaxPositionChangeContact(out Contact contact, out float maxPenetration)
        {
            contact = null;
            maxPenetration = settings.PositionEpsilon;

            bool found = false;
            for (int i = 0; i < ContactCount; i++)
            {
                float penetration = contacts[i].Penetration;

                if (penetration > maxPenetration)
                {
                    maxPenetration = penetration;
                    contact = contacts[i];

                    found = true;
                }
            }

            return found;
        }

        /// <summary>
        /// Solve speed problems with the contact list
        /// </summary>
        /// <param name="time">Duration of the previous frame</param>
        private void AdjustVelocities(float time)
        {
            int velocityIterations = settings.VelocityIterations;
            int velocityIterationsUsed = 0;

            // Resolve impacts in order of severity
            while (velocityIterationsUsed < velocityIterations)
            {
                // Find contacts with maximum magnitude of probable velocity change
                if (!FindMaxVelocityChangeContact(out var contact))
                {
                    return;
                }

                // Update contact status
                contact.MatchAwakeState();

                // Resolve contact
                contact.ApplyVelocityChange(out var linearChange1, out var linearChange2, out var angularChange1, out var angularChange2);

                for (int i = 0; i < ContactCount; i++)
                {
                    var other = contacts[i];

                    for (int o = 0; o < 2; o++)
                    {
                        var otherBody = other.GetBody(o);

                        for (int c = 0; c < 2; c++)
                        {
                            if (contact.GetBody(c) != otherBody)
                            {
                                continue;
                            }

                            var relativeContactPosition = other.GetRelativeContactPosition(o);
                            int direction = o != 0 ? 1 : -1;
                            var linearChange = c == 0 ? linearChange1 : linearChange2;
                            var angularChange = c == 0 ? angularChange1 : angularChange2;
                            var delta = linearChange + Vector3.Cross(angularChange, relativeContactPosition);
                            other.AdjustVelocities(delta, direction);
                            other.CalculateDesiredDeltaVelocity(time);
                        }
                    }
                }

                velocityIterationsUsed++;
            }
        }
        /// <summary>
        /// Finds the contact with maximum magnitude of probable velocity change
        /// </summary>
        /// <param name="contact">Returns the maximum velocity change contact</param>
        /// <returns>Returns true if a contact was found</returns>
        private bool FindMaxVelocityChangeContact(out Contact contact)
        {
            contact = null;
            float max = settings.VelocityEpsilon;

            bool found = false;
            for (int i = 0; i < ContactCount; i++)
            {
                float desiredDeltaVelocity = contacts[i].DesiredDeltaVelocity;

                if (desiredDeltaVelocity > max)
                {
                    max = desiredDeltaVelocity;
                    contact = contacts[i];

                    found = true;
                }
            }

            return found;
        }
    }
}

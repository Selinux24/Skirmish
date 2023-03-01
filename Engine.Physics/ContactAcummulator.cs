using SharpDX;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Physics
{
    /// <summary>
    /// Contact acummulator
    /// </summary>
    class ContactAcummulator
    {
        /// <summary>
        /// Contact collection
        /// </summary>
        private readonly List<ContactAcummulatorData> contacts = new List<ContactAcummulatorData>();

        /// <summary>
        /// Adds new contacts to the collection
        /// </summary>
        /// <param name="contact">Contact</param>
        public void Add(ContactAcummulatorData contact)
        {
            // Find edge contact
            var index = contacts.FindIndex(c => c.Edge == contact.Edge);
            if (index < 0)
            {
                // Add edge contact to the collection
                contacts.Add(contact);

                return;
            }

            // Test contact penetration
            if (contacts[index].Penetration >= contact.Penetration)
            {
                // Minor penetrations ignored
                return;
            }

            // Store contact
            contacts[index] = contact;
        }

        /// <summary>
        /// Contact count
        /// </summary>
        public int Count
        {
            get
            {
                return contacts.Count;
            }
        }
        /// <summary>
        /// Gets the contact list
        /// </summary>
        public IEnumerable<ContactAcummulatorData> GetContacts()
        {
            return contacts.ToArray();
        }
        /// <summary>
        /// Adds contacts to the contact resolver
        /// </summary>
        /// <param name="contacts">Contacts</param>
        /// <param name="body1">Fist body</param>
        /// <param name="body2">Second body</param>
        /// <param name="trn">Transform</param>
        /// <param name="data">Contact resolver</param>
        public static void AddToResolver(IEnumerable<ContactAcummulatorData> contacts, IRigidBody body1, IRigidBody body2, Matrix trn, ContactResolver data)
        {
            if (contacts?.Any() != true)
            {
                return;
            }

            if (!data.HasFreeContacts())
            {
                return;
            }

            foreach (var contact in contacts)
            {
                if (contact.Direction != 1)
                {
                    (body1, body2) = (body2, body1);
                }

                var contactPosition = contact.Point;
                var contactNormal = contact.Normal;
                var contactPenetration = contact.Penetration;

                if (!trn.IsIdentity)
                {
                    contactPosition = Vector3.TransformCoordinate(contact.Point, trn);
                    contactNormal = Vector3.TransformNormal(contact.Normal, trn);
                }

                data.AddContact(body1, body2, contactPosition, contactNormal, contactPenetration);
                if (!data.HasFreeContacts())
                {
                    break;
                }
            }
        }
    }
}

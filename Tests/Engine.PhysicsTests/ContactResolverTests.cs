using Engine.Physics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SharpDX;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Engine.PhysicsTests
{
    [ExcludeFromCodeCoverage]
    [TestClass()]
    public class ContactResolverTests
    {
        static TestContext _testContext;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _testContext = context;
        }

        [TestInitialize]
        public void SetupTest()
        {
            Console.WriteLine($"TestContext.TestName='{_testContext.TestName}'");
        }

        [TestMethod()]
        public void ContactResolverCollectionCreationTest()
        {
            ContactResolverSettings settings = new ContactResolverSettings();
            ContactResolver resolver = new ContactResolver(settings);

            Assert.IsTrue(resolver.ContactCount == 0);
            Assert.IsTrue(resolver.ContactsLeft > 0);
            Assert.IsTrue(resolver.HasFreeContacts());
        }

        [TestMethod()]
        public void ContactResolverCollectionAddContactTest()
        {
            ContactResolverSettings settings = new ContactResolverSettings();
            ContactResolver resolver = new ContactResolver(settings);

            int count = resolver.ContactCount;
            int left = resolver.ContactsLeft;
            resolver.AddContact(null, null, Vector3.Zero, Vector3.Zero, 0);
            Assert.IsTrue(resolver.ContactCount == count + 1);
            Assert.IsTrue(resolver.ContactsLeft == left - 1);
            Assert.IsTrue(resolver.HasFreeContacts());
        }

        [TestMethod()]
        public void ContactResolverCollectionAddResetTest()
        {
            ContactResolverSettings settings = new ContactResolverSettings();
            ContactResolver resolver = new ContactResolver(settings);

            int count = resolver.ContactCount;
            int left = resolver.ContactsLeft;
            resolver.AddContact(null, null, Vector3.Zero, Vector3.Zero, 0);
            Assert.IsTrue(resolver.ContactCount == count + 1);
            Assert.IsTrue(resolver.ContactsLeft == left - 1);
            Assert.IsTrue(resolver.HasFreeContacts());

            resolver.Reset();
            Assert.IsTrue(resolver.ContactCount == 0);
            Assert.IsTrue(resolver.ContactsLeft > 0);
            Assert.IsTrue(resolver.HasFreeContacts());
        }

        [TestMethod()]
        public void ContactResolverCollectionOverflowTest()
        {
            ContactResolverSettings settings = new ContactResolverSettings();
            ContactResolver resolver = new ContactResolver(settings);

            int left = resolver.ContactsLeft;
            for (int i = 0; i < left; i++)
            {
                resolver.AddContact(null, null, Vector3.Zero, Vector3.Zero, 0);
            }
            Assert.IsTrue(resolver.ContactCount == left);
            Assert.IsTrue(resolver.ContactsLeft == 0);
            Assert.IsFalse(resolver.HasFreeContacts());

            resolver.Reset();
            Assert.IsTrue(resolver.ContactCount == 0);
            Assert.IsTrue(resolver.ContactsLeft > 0);
            Assert.IsTrue(resolver.HasFreeContacts());
        }

        [TestMethod()]
        public void ContactResolverCollectionGetContactTest()
        {
            ContactResolverSettings settings = new ContactResolverSettings();
            ContactResolver resolver = new ContactResolver(settings);

            var curr = resolver.CurrentContact;
            Mock<IRigidBody> body1 = new Mock<IRigidBody>();
            Mock<IRigidBody> body2 = new Mock<IRigidBody>();
            Vector3 pos = new Vector3(1, 2, 3);
            Vector3 norm = new Vector3(4, 5, 6);
            float pen = 7;

            resolver.AddContact(body1.Object, body2.Object, pos, norm, pen);
            Assert.AreEqual(curr.GetBody(0), body1.Object);
            Assert.AreEqual(curr.GetBody(1), body2.Object);
            Assert.AreEqual(curr.Position, pos);
            Assert.AreEqual(curr.Normal, norm);
            Assert.AreEqual(curr.Penetration, pen);

            Assert.AreNotSame(resolver.CurrentContact, curr);
        }

        [TestMethod()]
        public void ContactResolverResolveTest()
        {
            ContactResolverSettings settings = new ContactResolverSettings();
            ContactResolver resolver = new ContactResolver(settings);
            float time = 1f / 60f;

            RigidBody body1 = new RigidBody(1, Matrix.Identity);

            RigidBody body2 = new RigidBody(1, Matrix.Translation(0, 1, 0));
            body2.AddLinearVelocity(new Vector3(0, -1, 0));

            Vector3 pos = new Vector3(0, 0, 0);
            Vector3 norm = new Vector3(0, -1, 0);
            float pen = 0.5f;

            resolver.AddContact(body1, body2, pos, norm, pen);
            resolver.Resolve(time);

            Assert.IsTrue(body1.Position.Y < 0);
            Assert.IsTrue(body2.Position.Y > 1);
        }
    }
}

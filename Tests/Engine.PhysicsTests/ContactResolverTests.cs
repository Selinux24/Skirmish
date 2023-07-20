using Engine.Physics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            var settings = new ContactResolverSettings();
            var resolver = new ContactResolver(settings);

            Assert.IsTrue(resolver.ContactCount == 0);
            Assert.IsTrue(resolver.ContactsLeft > 0);
            Assert.IsTrue(resolver.HasFreeContacts());
        }

        [TestMethod()]
        public void ContactResolverCollectionAddContactTest()
        {
            var settings = new ContactResolverSettings();
            var resolver = new ContactResolver(settings);
            IRigidBody r1 = new RigidBody(new() { Mass = 1f, InitialTransform = Matrix.Identity });
            IRigidBody r2 = new RigidBody(new() { Mass = 1f, InitialTransform = Matrix.Identity });

            int count = resolver.ContactCount;
            int left = resolver.ContactsLeft;
            resolver.AddContact(r1, r2, Vector3.Zero, Vector3.Zero, 0);
            Assert.IsTrue(resolver.ContactCount == count + 1);
            Assert.IsTrue(resolver.ContactsLeft == left - 1);
            Assert.IsTrue(resolver.HasFreeContacts());
        }

        [TestMethod()]
        public void ContactResolverCollectionAddResetTest()
        {
            var settings = new ContactResolverSettings();
            var resolver = new ContactResolver(settings);
            IRigidBody r1 = new RigidBody(new() { Mass = 1f, InitialTransform = Matrix.Identity });
            IRigidBody r2 = new RigidBody(new() { Mass = 1f, InitialTransform = Matrix.Identity });

            int count = resolver.ContactCount;
            int left = resolver.ContactsLeft;
            resolver.AddContact(r1, r2, Vector3.Zero, Vector3.Zero, 0);
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
            var settings = new ContactResolverSettings();
            var resolver = new ContactResolver(settings);
            IRigidBody r1 = new RigidBody(new() { Mass = 1f, InitialTransform = Matrix.Identity });
            IRigidBody r2 = new RigidBody(new() { Mass = 1f, InitialTransform = Matrix.Identity });

            int left = resolver.ContactsLeft;
            for (int i = 0; i < left; i++)
            {
                resolver.AddContact(r1, r2, Vector3.Zero, Vector3.Zero, 0);
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
            var settings = new ContactResolverSettings();
            var resolver = new ContactResolver(settings);

            var curr = resolver.CurrentContact;
            IRigidBody r1 = new RigidBody(new() { Mass = 1f, InitialTransform = Matrix.Identity });
            IRigidBody r2 = new RigidBody(new() { Mass = 1f, InitialTransform = Matrix.Identity });
            var pos = new Vector3(1, 2, 3);
            var norm = new Vector3(4, 5, 6);
            float pen = 7;

            resolver.AddContact(r1, r2, pos, norm, pen);
            Assert.AreEqual(curr.GetBody(0), r1);
            Assert.AreEqual(curr.GetBody(1), r2);
            Assert.AreEqual(curr.Position, pos);
            Assert.AreEqual(curr.Normal, norm);
            Assert.AreEqual(curr.Penetration, pen);

            Assert.AreNotSame(resolver.CurrentContact, curr);
        }

        [TestMethod()]
        public void ContactResolverResolveTest()
        {
            var settings = new ContactResolverSettings();
            var resolver = new ContactResolver(settings);
            float time = 1f / 60f;

            IRigidBody r1 = new RigidBody(new() { Mass = 1f, InitialTransform = Matrix.Identity });

            IRigidBody r2 = new RigidBody(new() { Mass = 1f, InitialTransform = Matrix.Translation(0, 1, 0) });
            r2.AddLinearVelocity(new Vector3(0, -1, 0));

            var pos = new Vector3(0, 0, 0);
            var norm = new Vector3(0, -1, 0);
            float pen = 0.5f;

            resolver.AddContact(r1, r2, pos, norm, pen);
            resolver.Resolve(time);

            Assert.IsTrue(r1.Position.Y < 0);
            Assert.IsTrue(r2.Position.Y > 1);
        }
    }
}

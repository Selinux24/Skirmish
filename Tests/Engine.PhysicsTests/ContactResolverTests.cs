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
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void SetupTest()
        {
            Console.WriteLine($"TestContext.TestName='{TestContext.TestName}'");
        }

        [TestMethod()]
        public void ContactResolverCollectionCreationTest()
        {
            var settings = new ContactResolverSettings();
            var resolver = new ContactResolver(settings);

            Assert.AreEqual(0, resolver.ContactCount);
            Assert.IsGreaterThan(0, resolver.ContactsLeft);
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
            Assert.AreEqual(count + 1, resolver.ContactCount);
            Assert.AreEqual(left - 1, resolver.ContactsLeft);
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
            Assert.AreEqual(count + 1, resolver.ContactCount);
            Assert.AreEqual(left - 1, resolver.ContactsLeft);
            Assert.IsTrue(resolver.HasFreeContacts());

            resolver.Reset();
            Assert.AreEqual(0, resolver.ContactCount);
            Assert.IsGreaterThan(0, resolver.ContactsLeft);
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
            Assert.AreEqual(left, resolver.ContactCount);
            Assert.AreEqual(0, resolver.ContactsLeft);
            Assert.IsFalse(resolver.HasFreeContacts());

            resolver.Reset();
            Assert.AreEqual(0, resolver.ContactCount);
            Assert.IsGreaterThan(0, resolver.ContactsLeft);
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

            RigidBody r1 = new(new() { Mass = 1f, InitialTransform = Matrix.Identity });

            RigidBody r2 = new(new() { Mass = 1f, InitialTransform = Matrix.Translation(0, 1, 0) });
            r2.AddLinearVelocity(new Vector3(0, -1, 0));

            var pos = new Vector3(0, 0, 0);
            var norm = new Vector3(0, -1, 0);
            float pen = 0.5f;

            resolver.AddContact(r1, r2, pos, norm, pen);
            resolver.Resolve(time);

            Assert.IsLessThan(0, r1.Position.Y);
            Assert.IsGreaterThan(1, r2.Position.Y);
        }
    }
}

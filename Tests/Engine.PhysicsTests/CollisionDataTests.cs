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
    public class CollisionDataTests
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
        public void CollisionDataCollectionCreationTest()
        {
            CollisionData data = new CollisionData();
            Assert.IsTrue(data.ContactCount == 0);
            Assert.IsTrue(data.ContactsLeft > 0);
            Assert.IsTrue(data.HasFreeContacts());
        }

        [TestMethod()]
        public void CollisionDataCollectionAddContactTest()
        {
            CollisionData data = new CollisionData();

            int count = data.ContactCount;
            int left = data.ContactsLeft;
            data.AddContact(null, null, Vector3.Zero, Vector3.Zero, 0);
            Assert.IsTrue(data.ContactCount == count + 1);
            Assert.IsTrue(data.ContactsLeft == left - 1);
            Assert.IsTrue(data.HasFreeContacts());
        }

        [TestMethod()]
        public void CollisionDataCollectionAddResetTest()
        {
            CollisionData data = new CollisionData();

            int count = data.ContactCount;
            int left = data.ContactsLeft;
            data.AddContact(null, null, Vector3.Zero, Vector3.Zero, 0);
            Assert.IsTrue(data.ContactCount == count + 1);
            Assert.IsTrue(data.ContactsLeft == left - 1);
            Assert.IsTrue(data.HasFreeContacts());

            data.Reset();
            Assert.IsTrue(data.ContactCount == 0);
            Assert.IsTrue(data.ContactsLeft > 0);
            Assert.IsTrue(data.HasFreeContacts());
        }

        [TestMethod()]
        public void CollisionDataCollectionOverflowTest()
        {
            CollisionData data = new CollisionData();

            int left = data.ContactsLeft;
            for (int i = 0; i < left; i++)
            {
                data.AddContact(null, null, Vector3.Zero, Vector3.Zero, 0);
            }
            Assert.IsTrue(data.ContactCount == left);
            Assert.IsTrue(data.ContactsLeft == 0);
            Assert.IsFalse(data.HasFreeContacts());

            data.Reset();
            Assert.IsTrue(data.ContactCount == 0);
            Assert.IsTrue(data.ContactsLeft > 0);
            Assert.IsTrue(data.HasFreeContacts());
        }

        [TestMethod()]
        public void CollisionDataCollectionGetContactTest()
        {
            CollisionData data = new CollisionData();

            var curr = data.CurrentContact;
            Mock<IRigidBody> body1 = new Mock<IRigidBody>();
            Mock<IRigidBody> body2 = new Mock<IRigidBody>();
            Vector3 pos = new Vector3(1, 2, 3);
            Vector3 norm = new Vector3(4, 5, 6);
            float pen = 7;

            data.AddContact(body1.Object, body2.Object, pos, norm, pen);
            Assert.AreEqual(curr.GetBody(0), body1.Object);
            Assert.AreEqual(curr.GetBody(1), body2.Object);
            Assert.AreEqual(curr.ContactPositionWorld, pos);
            Assert.AreEqual(curr.ContactNormalWorld, norm);
            Assert.AreEqual(curr.Penetration, pen);

            Assert.AreNotSame(data.CurrentContact, curr);
        }

        [TestMethod()]
        public void CollisionDataResolveTest()
        {
            ContactResolver resolver = new ContactResolver();
            CollisionData data = new CollisionData();
            float time = 1f / 60f;

            RigidBody body1 = new RigidBody(1, Matrix.Identity);
            
            RigidBody body2 = new RigidBody(1, Matrix.Translation(0, 1, 0));
            body2.AddLinearVelocity(new Vector3(0, -1, 0));

            Vector3 pos = new Vector3(0, 0, 0);
            Vector3 norm = new Vector3(0, -1, 0);
            float pen = 0.5f;

            data.AddContact(body1, body2, pos, norm, pen);
            data.Resolve(resolver, time);

            Assert.IsTrue(body1.Position.Y < 0);
            Assert.IsTrue(body2.Position.Y > 1);
        }
    }
}

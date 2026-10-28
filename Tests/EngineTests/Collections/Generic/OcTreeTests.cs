using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using Vortice.Mathematics;

namespace EngineTests.Collections.Generic
{
    using Engine;
    using Engine.Collections.Generic;
    using Engine.Mathematics;

    [ExcludeFromCodeCoverage]
    [TestClass]
    [OSCondition(OperatingSystems.Windows)]
    public class OcTreeTests
    {
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void SetupTest()
        {
            Console.WriteLine($"TestContext.TestName='{TestContext.TestName}'");
        }

        private static (ICullingVolume Box, Vector3 Obj) GenerateItem(Vector3 position, float size)
        {
            Vector3 extents = new(size * 0.5f);
            BoundingBox box = new(-extents + position, extents + position);

            return ((IntersectionVolumeAxisAlignedBox)box, position);
        }

        [TestMethod]
        public void TestConstructor()
        {
            float size = 50f;
            BoundingBox bbox = new(new Vector3(-size), new Vector3(size));

            OcTree<Vector3> q = new(bbox, 1);
            Assert.IsNotNull(q);

            int count = q.CountItems();
            Assert.AreEqual(0, count);
        }

        [TestMethod]
        public void TestInsert()
        {
            float size = 50f;
            BoundingBox bbox = new(new Vector3(-size), new Vector3(size));

            OcTree<Vector3> q = new(bbox, 1);

            var item = GenerateItem(Vector3.Zero, 1);
            q.Insert(item.Box, item.Obj);
            int count = q.CountItems();
            Assert.AreEqual(1, count);

            item = GenerateItem(new(10), 1);
            q.Insert(item.Box, item.Obj);
            count = q.CountItems();
            Assert.AreEqual(2, count);
        }

        [TestMethod]
        public void TestQuery()
        {
            float size = 50f;
            BoundingBox bbox = new(new Vector3(-size), new Vector3(size));

            OcTree<Vector3> q = new(bbox, 1);

            var (b1, p1) = GenerateItem(Vector3.Zero, 1);
            q.Insert(b1, p1);
            int count = q.CountItems();
            Assert.AreEqual(1, count);

            var (b2, p2) = GenerateItem(new(10), 1);
            q.Insert(b2, p2);
            count = q.CountItems();
            Assert.AreEqual(2, count);

            var (b3, p3) = GenerateItem(new(-10), 1);
            q.Insert(b3, p3);
            count = q.CountItems();
            Assert.AreEqual(3, count);

            var res = q.Query((IntersectionVolumeAxisAlignedBox)bbox);
            Assert.HasCount(3, res);

            res = q.Query(b1);
            Assert.HasCount(1, res);
            Assert.AreEqual(p1, res.ElementAt(0));

            res = q.Query(b2);
            Assert.HasCount(1, res);
            Assert.AreEqual(p2, res.ElementAt(0));

            res = q.Query(b3);
            Assert.HasCount(1, res);
            Assert.AreEqual(p3, res.ElementAt(0));

            var d = (IntersectionVolumeAxisAlignedBox)BoundingBox.CreateFromPoints([p1, p2]);
            res = q.Query(d);
            Assert.HasCount(2, res);
            Assert.AreSequenceEqual([p1, p2], res, SequenceOrder.InAnyOrder);

            d = (IntersectionVolumeAxisAlignedBox)BoundingBox.CreateFromPoints([p1, p3]);
            res = q.Query(d);
            Assert.HasCount(2, res);
            Assert.AreSequenceEqual([p1, p3], res, SequenceOrder.InAnyOrder);

            d = (IntersectionVolumeAxisAlignedBox)BoundingBox.CreateFromPoints([p2, p3]);
            res = q.Query(d);
            Assert.HasCount(3, res);
            Assert.AreSequenceEqual([p1, p2, p3], res, SequenceOrder.InAnyOrder);
        }

        [TestMethod]
        public void TestClear()
        {
            float size = 50f;
            BoundingBox bbox = new(new Vector3(-size), new Vector3(size));

            OcTree<Vector3> q = new(bbox, 1);

            var (b, o) = GenerateItem(Vector3.Zero, 1);
            q.Insert(b, o);
            int count = q.CountItems();
            Assert.AreEqual(1, count);

            q.Clear();
            count = q.CountItems();
            Assert.AreEqual(0, count);
        }
    }
}

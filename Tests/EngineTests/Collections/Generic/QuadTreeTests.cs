using Engine;
using Engine.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpDX;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace EngineTests.Collections.Generic
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class QuadTreeTests
    {
        static TestContext _testContext;

        static BoundingBox tenBox;
        static (BoundingBox, int)[] items;
        static int[] expectedArray0 = [1, 2];
        static int[] expectedArray1 = [3, 4];
        static int[] expectedArray2 = [5, 6];
        static int[] expectedArray3 = [7, 8];
        static Vector3 topLeft;
        static Vector3 topRight;
        static Vector3 bottomLeft;
        static Vector3 bottomRight;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _testContext = context;
        }

        [TestInitialize]
        public void SetupTest()
        {
            Console.WriteLine($"TestContext.TestName='{_testContext.TestName}'");

            Vector3 min = new(-10, -10, -10);
            Vector3 max = new(10, 10, 10);
            tenBox = new(min, max);

            topLeft = new(-5, 0, -5);
            topRight = new(5, 0, -5);
            bottomLeft = new(-5, 0, 5);
            bottomRight = new(5, 0, 5);

            var qBoxes = tenBox.SubdivideQuadtree().ToArray();
            var bbox0 = qBoxes[0];
            var bbox1 = qBoxes[1];
            var bbox2 = qBoxes[2];
            var bbox3 = qBoxes[3];

            items =
            [
                (bbox0, 1), (bbox0, 2),
                (bbox1, 3), (bbox1, 4),
                (bbox2, 5), (bbox2, 6),
                (bbox3, 7), (bbox3, 8),
            ];

            expectedArray0 = [1, 2];
            expectedArray1 = [3, 4];
            expectedArray2 = [5, 6];
            expectedArray3 = [7, 8];
        }

        [TestMethod]
        public void TestConstructor()
        {
            QuadTree<int> q = new(new(), [], 1);

            Assert.IsNotNull(q);
            Assert.IsNotNull(q.Root);

            var children = q.Root.Children.ToArray();
            Assert.IsNotNull(children);

            Assert.AreEqual(4, children.Length);
            Assert.IsNotNull(children[0]);
            Assert.IsNotNull(children[1]);
            Assert.IsNotNull(children[2]);
            Assert.IsNotNull(children[3]);

            CollectionAssert.AreEqual(Array.Empty<QuadTreeNode<int>>(), children[0].Children.ToArray());
            CollectionAssert.AreEqual(Array.Empty<QuadTreeNode<int>>(), children[1].Children.ToArray());
            CollectionAssert.AreEqual(Array.Empty<QuadTreeNode<int>>(), children[2].Children.ToArray());
            CollectionAssert.AreEqual(Array.Empty<QuadTreeNode<int>>(), children[3].Children.ToArray());
        }

        [TestMethod]
        public void TestParent()
        {
            QuadTree<int> q = new(new(), [], 2);
            Assert.IsNull(q.Root.Parent);

            var children = q.Root.Children.ToArray();
            Assert.AreEqual(q.Root, children[0].Parent);
            Assert.AreEqual(q.Root, children[1].Parent);
            Assert.AreEqual(q.Root, children[2].Parent);
            Assert.AreEqual(q.Root, children[3].Parent);

            var children0 = children[0].Children.ToArray();
            Assert.AreEqual(children[0], children0[0].Parent);
            Assert.AreEqual(children[0], children0[1].Parent);
            Assert.AreEqual(children[0], children0[2].Parent);
            Assert.AreEqual(children[0], children0[3].Parent);

            var children1 = children[1].Children.ToArray();
            Assert.AreEqual(children[1], children1[0].Parent);
            Assert.AreEqual(children[1], children1[1].Parent);
            Assert.AreEqual(children[1], children1[2].Parent);
            Assert.AreEqual(children[1], children1[3].Parent);

            var children2 = children[2].Children.ToArray();
            Assert.AreEqual(children[2], children2[0].Parent);
            Assert.AreEqual(children[2], children2[1].Parent);
            Assert.AreEqual(children[2], children2[2].Parent);
            Assert.AreEqual(children[2], children2[3].Parent);

            var children3 = children[3].Children.ToArray();
            Assert.AreEqual(children[3], children3[0].Parent);
            Assert.AreEqual(children[3], children3[1].Parent);
            Assert.AreEqual(children[3], children3[2].Parent);
            Assert.AreEqual(children[3], children3[3].Parent);
        }

        [TestMethod]
        public void TestLeaf()
        {
            QuadTree<int> q = new(new(), [], 2);
            Assert.IsFalse(q.Root.IsLeaf);

            var children = q.Root.Children.ToArray();
            Assert.IsFalse(children[0].IsLeaf);
            Assert.IsFalse(children[1].IsLeaf);
            Assert.IsFalse(children[2].IsLeaf);
            Assert.IsFalse(children[3].IsLeaf);

            var children0 = children[0].Children.ToArray();
            Assert.IsTrue(children0[0].IsLeaf);
            Assert.IsTrue(children0[1].IsLeaf);
            Assert.IsTrue(children0[2].IsLeaf);
            Assert.IsTrue(children0[3].IsLeaf);

            var children1 = children[1].Children.ToArray();
            Assert.IsTrue(children1[0].IsLeaf);
            Assert.IsTrue(children1[1].IsLeaf);
            Assert.IsTrue(children1[2].IsLeaf);
            Assert.IsTrue(children1[3].IsLeaf);

            var children2 = children[2].Children.ToArray();
            Assert.IsTrue(children2[0].IsLeaf);
            Assert.IsTrue(children2[1].IsLeaf);
            Assert.IsTrue(children2[2].IsLeaf);
            Assert.IsTrue(children2[3].IsLeaf);

            var children3 = children[3].Children.ToArray();
            Assert.IsTrue(children3[0].IsLeaf);
            Assert.IsTrue(children3[1].IsLeaf);
            Assert.IsTrue(children3[2].IsLeaf);
            Assert.IsTrue(children3[3].IsLeaf);
        }

        [TestMethod]
        public void TestIds()
        {
            QuadTree<int> q = new(new(), [], 2);
            Assert.AreEqual(-1, q.Root.Id);

            var children = q.Root.Children.ToArray();
            Assert.AreEqual(-1, children[0].Id);
            Assert.AreEqual(-1, children[1].Id);
            Assert.AreEqual(-1, children[2].Id);
            Assert.AreEqual(-1, children[3].Id);

            var children0 = children[0].Children.ToArray();
            Assert.AreEqual(0, children0[0].Id);
            Assert.AreEqual(1, children0[1].Id);
            Assert.AreEqual(2, children0[2].Id);
            Assert.AreEqual(3, children0[3].Id);

            var children1 = children[1].Children.ToArray();
            Assert.AreEqual(4, children1[0].Id);
            Assert.AreEqual(5, children1[1].Id);
            Assert.AreEqual(6, children1[2].Id);
            Assert.AreEqual(7, children1[3].Id);

            var children2 = children[2].Children.ToArray();
            Assert.AreEqual(8, children2[0].Id);
            Assert.AreEqual(9, children2[1].Id);
            Assert.AreEqual(10, children2[2].Id);
            Assert.AreEqual(11, children2[3].Id);

            var children3 = children[3].Children.ToArray();
            Assert.AreEqual(12, children3[0].Id);
            Assert.AreEqual(13, children3[1].Id);
            Assert.AreEqual(14, children3[2].Id);
            Assert.AreEqual(15, children3[3].Id);
        }

        [TestMethod]
        public void TestLevels()
        {
            QuadTree<int> q = new(new(), [], 2);
            Assert.AreEqual(0, q.Root.Level);

            var children = q.Root.Children.ToArray();
            Assert.AreEqual(1, children[0].Level);
            Assert.AreEqual(1, children[1].Level);
            Assert.AreEqual(1, children[2].Level);
            Assert.AreEqual(1, children[3].Level);

            var children2 = children[0].Children.ToArray();
            Assert.AreEqual(2, children2[0].Level);
            Assert.AreEqual(2, children2[1].Level);
            Assert.AreEqual(2, children2[2].Level);
            Assert.AreEqual(2, children2[3].Level);
        }

        [TestMethod]
        public void TestBoundaries()
        {
            QuadTree<int> q = new(tenBox, [], 1);

            Assert.AreEqual(tenBox, q.BoundingBox);
            Assert.AreEqual(q.Root.BoundingBox, q.BoundingBox);
            Assert.AreEqual(Vector3.Zero, q.Root.Center);

            var children = q.Root.Children.ToArray();

            Assert.IsTrue(tenBox.Contains(children[0].BoundingBox) == ContainmentType.Contains);
            Assert.IsTrue(tenBox.Contains(children[1].BoundingBox) == ContainmentType.Contains);
            Assert.IsTrue(tenBox.Contains(children[2].BoundingBox) == ContainmentType.Contains);
            Assert.IsTrue(tenBox.Contains(children[3].BoundingBox) == ContainmentType.Contains);

            Assert.IsTrue(children[0].BoundingBox.Contains(children[1].BoundingBox) != ContainmentType.Contains);
            Assert.IsTrue(children[0].BoundingBox.Contains(children[2].BoundingBox) != ContainmentType.Contains);
            Assert.IsTrue(children[0].BoundingBox.Contains(children[3].BoundingBox) != ContainmentType.Contains);

            Assert.AreEqual(topLeft, q.Root.TopLeftChild.BoundingBox.Center);
            Assert.AreEqual(topRight, q.Root.TopRightChild.BoundingBox.Center);
            Assert.AreEqual(bottomLeft, q.Root.BottomLeftChild.BoundingBox.Center);
            Assert.AreEqual(bottomRight, q.Root.BottomRightChild.BoundingBox.Center);
        }

        [TestMethod]
        public void TestNeighbors()
        {
            QuadTree<int> q = new(tenBox, [], 1);

            Assert.IsNull(q.Root.TopLeftChild.LeftNeighbor);
            Assert.IsNull(q.Root.TopLeftChild.TopLeftNeighbor);
            Assert.IsNull(q.Root.TopLeftChild.TopNeighbor);
            Assert.IsNull(q.Root.TopLeftChild.TopRightNeighbor);
            Assert.AreEqual(q.Root.TopLeftChild.RightNeighbor, q.Root.TopRightChild);
            Assert.AreEqual(q.Root.TopLeftChild.BottomRightNeighbor, q.Root.BottomRightChild);
            Assert.AreEqual(q.Root.TopLeftChild.BottomNeighbor, q.Root.BottomLeftChild);
            Assert.IsNull(q.Root.TopLeftChild.BottomLeftNeighbor);

            Assert.AreEqual(q.Root.TopRightChild.LeftNeighbor, q.Root.TopLeftChild);
            Assert.IsNull(q.Root.TopRightChild.TopLeftNeighbor);
            Assert.IsNull(q.Root.TopRightChild.TopNeighbor);
            Assert.IsNull(q.Root.TopRightChild.TopRightNeighbor);
            Assert.IsNull(q.Root.TopRightChild.RightNeighbor);
            Assert.IsNull(q.Root.TopRightChild.BottomRightNeighbor);
            Assert.AreEqual(q.Root.TopRightChild.BottomNeighbor, q.Root.BottomRightChild);
            Assert.AreEqual(q.Root.TopRightChild.BottomLeftNeighbor, q.Root.BottomLeftChild);

            Assert.IsNull(q.Root.BottomLeftChild.LeftNeighbor);
            Assert.IsNull(q.Root.BottomLeftChild.TopLeftNeighbor);
            Assert.AreEqual(q.Root.BottomLeftChild.TopNeighbor, q.Root.TopLeftChild);
            Assert.AreEqual(q.Root.BottomLeftChild.TopRightNeighbor, q.Root.TopRightChild);
            Assert.AreEqual(q.Root.BottomLeftChild.RightNeighbor, q.Root.BottomRightChild);
            Assert.IsNull(q.Root.BottomLeftChild.BottomRightNeighbor);
            Assert.IsNull(q.Root.BottomLeftChild.BottomNeighbor);
            Assert.IsNull(q.Root.BottomLeftChild.BottomLeftNeighbor);

            Assert.AreEqual(q.Root.BottomRightChild.LeftNeighbor, q.Root.BottomLeftChild);
            Assert.AreEqual(q.Root.BottomRightChild.TopLeftNeighbor, q.Root.TopLeftChild);
            Assert.AreEqual(q.Root.BottomRightChild.TopNeighbor, q.Root.TopRightChild);
            Assert.IsNull(q.Root.BottomRightChild.TopRightNeighbor);
            Assert.IsNull(q.Root.BottomRightChild.RightNeighbor);
            Assert.IsNull(q.Root.BottomRightChild.BottomRightNeighbor);
            Assert.IsNull(q.Root.BottomRightChild.BottomNeighbor);
            Assert.IsNull(q.Root.BottomRightChild.BottomLeftNeighbor);
        }

        [TestMethod]
        public void TestGetBoundingBoxes()
        {
            QuadTree<int> q = new(new(), [], 2);
            var boxes = q.GetBoundingBoxes();
            Assert.AreEqual(16, boxes.Count());

            boxes = q.GetBoundingBoxes(1);
            Assert.AreEqual(4, boxes.Count());

            boxes = q.GetBoundingBoxes(2);
            Assert.AreEqual(16, boxes.Count());

            boxes = q.GetBoundingBoxes(-1);
            Assert.AreEqual(0, boxes.Count());
        }

        [TestMethod]
        public void TestLeafNodes()
        {
            QuadTree<int> q = new(new(), [], 2);
            var nodes = q.GetLeafNodes().ToArray();
            Assert.AreEqual(16, nodes.Length);

            QuadTreeNode<int>[] lNodes =
            [
                .. q.Root.Children.ToArray()[0].Children,
                .. q.Root.Children.ToArray()[1].Children,
                .. q.Root.Children.ToArray()[2].Children,
                .. q.Root.Children.ToArray()[3].Children
            ];

            CollectionAssert.AreEquivalent(lNodes, nodes);
        }

        [TestMethod]
        public void TestTraversePosition()
        {
            QuadTree<int> q = new(tenBox, [], 1);
            var cn = q.FindClosestNode(Vector3.Zero);
            Assert.AreEqual(cn, q.Root.TopLeftChild);
            Assert.IsTrue(cn.IsLeaf);

            cn = q.FindClosestNode(new(-1, 0, -1));
            Assert.AreEqual(cn, q.Root.TopLeftChild);
            Assert.IsTrue(cn.IsLeaf);
            cn = q.FindClosestNode(new(-10, 0, -10));
            Assert.AreEqual(cn, q.Root.TopLeftChild);
            Assert.IsTrue(cn.IsLeaf);
            cn = q.FindClosestNode(new(-100, 0, -100));
            Assert.AreEqual(cn, q.Root.TopLeftChild);
            Assert.IsTrue(cn.IsLeaf);

            cn = q.FindClosestNode(new(1, 0, -1));
            Assert.AreEqual(cn, q.Root.TopRightChild);
            Assert.IsTrue(cn.IsLeaf);
            cn = q.FindClosestNode(new(10, 0, -10));
            Assert.AreEqual(cn, q.Root.TopRightChild);
            Assert.IsTrue(cn.IsLeaf);
            cn = q.FindClosestNode(new(100, 0, -100));
            Assert.AreEqual(cn, q.Root.TopRightChild);
            Assert.IsTrue(cn.IsLeaf);

            cn = q.FindClosestNode(new(1, 0, 1));
            Assert.AreEqual(cn, q.Root.BottomRightChild);
            Assert.IsTrue(cn.IsLeaf);
            cn = q.FindClosestNode(new(10, 0, 10));
            Assert.AreEqual(cn, q.Root.BottomRightChild);
            Assert.IsTrue(cn.IsLeaf);
            cn = q.FindClosestNode(new(100, 0, 100));
            Assert.AreEqual(cn, q.Root.BottomRightChild);
            Assert.IsTrue(cn.IsLeaf);

            cn = q.FindClosestNode(new(-1, 0, 1));
            Assert.AreEqual(cn, q.Root.BottomLeftChild);
            Assert.IsTrue(cn.IsLeaf);
            cn = q.FindClosestNode(new(-10, 0, 10));
            Assert.AreEqual(cn, q.Root.BottomLeftChild);
            Assert.IsTrue(cn.IsLeaf);
            cn = q.FindClosestNode(new(-100, 0, 100));
            Assert.AreEqual(cn, q.Root.BottomLeftChild);
            Assert.IsTrue(cn.IsLeaf);
        }

        [TestMethod]
        public void TestTraverseVolume()
        {
            QuadTree<int> q = new(tenBox, [], 1);

            IntersectionVolumeAxisAlignedBox volume = new BoundingBox(Vector3.One * -100, Vector3.One * -90);
            var ln = q.FindNodesInVolume(volume);
            CollectionAssert.AreEquivalent(Array.Empty<QuadTreeNode<int>>(), ln.ToArray());

            volume = new BoundingBox(new(-5, -5, -5), new(-1, 5, -1));
            ln = q.FindNodesInVolume(volume);
            CollectionAssert.AreEquivalent(new QuadTreeNode<int>[] { q.Root.TopLeftChild }, ln.ToArray());

            volume = new BoundingBox(new(1, -5, -5), new(5, 5, -1));
            ln = q.FindNodesInVolume(volume);
            CollectionAssert.AreEquivalent(new QuadTreeNode<int>[] { q.Root.TopRightChild }, ln.ToArray());

            volume = new BoundingBox(new(-5, -5, 1), new(-1, 5, 5));
            ln = q.FindNodesInVolume(volume);
            CollectionAssert.AreEquivalent(new QuadTreeNode<int>[] { q.Root.BottomLeftChild }, ln.ToArray());

            volume = new BoundingBox(new(1, -5, 1), new(5, 5, 5));
            ln = q.FindNodesInVolume(volume);
            CollectionAssert.AreEquivalent(new QuadTreeNode<int>[] { q.Root.BottomRightChild }, ln.ToArray());

            volume = new BoundingBox(new(-5, -5, -5), new(5, 5, 5));
            ln = q.FindNodesInVolume(volume);
            CollectionAssert.AreEquivalent(q.GetLeafNodes().ToArray(), ln.ToArray());
        }

        [TestMethod]
        public void TestItems()
        {
            QuadTree<int> q = new(tenBox, items, 1);

            var nodes = q.GetLeafNodes().ToArray();
            Assert.AreEqual(4, nodes.Length);

            Assert.AreEqual(2, nodes[0].Items.Count());
            Assert.AreEqual(2, nodes[1].Items.Count());
            Assert.AreEqual(2, nodes[2].Items.Count());
            Assert.AreEqual(2, nodes[3].Items.Count());

            CollectionAssert.AreEquivalent(expectedArray0, nodes[0].Items.ToArray());
            CollectionAssert.AreEquivalent(expectedArray1, nodes[1].Items.ToArray());
            CollectionAssert.AreEquivalent(expectedArray2, nodes[2].Items.ToArray());
            CollectionAssert.AreEquivalent(expectedArray3, nodes[3].Items.ToArray());
        }
    }
}

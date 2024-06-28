using Engine;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace EngineTests
{
    [ExcludeFromCodeCoverage]
    [TestClass()]
    public class SceneComponentCollectionTest
    {
        static TestContext _testContext;

        static Mock<ISceneObject> obj1;
        static Mock<ISceneObject> obj1b;
        static Mock<ISceneObject> obj2;
        static Mock<ISceneObject> obj3;

        static Mock<ISceneObject> objUsageNone;
        static Mock<ISceneObject> objUsageObject;
        static Mock<ISceneObject> objUsageGround;
        static Mock<ISceneObject> objUsageAgent;
        static Mock<ISceneObject> objUsageUI;

        static Mock<IMockModel> mdlUsageNone;
        static Mock<IMockModel> mdlUsageObject;
        static Mock<IMockModel> mdlUsageGround;
        static Mock<IMockModel> mdlUsageAgent;
        static Mock<IMockModel> mdlUsageUI;

        static Mock<IMockDrawable> mdlDrawableOpaqueDeph1a;
        static Mock<IMockDrawable> mdlDrawableOpaqueDeph1b;
        static Mock<IMockDrawable> mdlDrawableOpaqueDeph2;
        static Mock<IMockDrawable> mdlDrawableOpaqueDeph3;
        static Mock<IMockDrawable> mdlDrawableAlphaDeph1a;
        static Mock<IMockDrawable> mdlDrawableAlphaDeph1b;
        static Mock<IMockDrawable> mdlDrawableAlphaDeph2;
        static Mock<IMockDrawable> mdlDrawableAlphaDeph3;
        static Mock<IMockDrawable> mdlDrawableOpaqueNoDeph1a;
        static Mock<IMockDrawable> mdlDrawableOpaqueNoDeph1b;
        static Mock<IMockDrawable> mdlDrawableOpaqueNoDeph2;
        static Mock<IMockDrawable> mdlDrawableOpaqueNoDeph3;
        static Mock<IMockDrawable> mdlDrawableAlphaNoDeph1a;
        static Mock<IMockDrawable> mdlDrawableAlphaNoDeph1b;
        static Mock<IMockDrawable> mdlDrawableAlphaNoDeph2;
        static Mock<IMockDrawable> mdlDrawableAlphaNoDeph3;

        static Mock<IMockUpdatable> mdlUpdatable1a;
        static Mock<IMockUpdatable> mdlUpdatable1b;
        static Mock<IMockUpdatable> mdlUpdatable2;
        static Mock<IMockUpdatable> mdlUpdatable3;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _testContext = context;

            obj1 = Setup("obj1");
            obj1b = Setup("obj1");
            obj2 = Setup("obj2");
            obj3 = Setup("obj3");

            objUsageNone = Setup("objUsageNone", SceneObjectUsages.None);
            objUsageObject = Setup("objUsageObject", SceneObjectUsages.Object);
            objUsageGround = Setup("objUsageGround", SceneObjectUsages.Ground);
            objUsageAgent = Setup("objUsageAgent", SceneObjectUsages.Agent);
            objUsageUI = Setup("objUsageUI", SceneObjectUsages.UI);

            mdlUsageNone = SetupModel("mdlUsageNone", SceneObjectUsages.None);
            mdlUsageObject = SetupModel("mdlUsageObject", SceneObjectUsages.Object);
            mdlUsageGround = SetupModel("mdlUsageGround", SceneObjectUsages.Ground);
            mdlUsageAgent = SetupModel("mdlUsageAgent", SceneObjectUsages.Agent);
            mdlUsageUI = SetupModel("mdlUsageUI", SceneObjectUsages.UI);

            mdlUsageObject.Object.Owner = mdlUsageNone.Object;
            mdlUsageGround.Object.Owner = mdlUsageNone.Object;
            mdlUsageAgent.Object.Owner = mdlUsageNone.Object;
            mdlUsageUI.Object.Owner = mdlUsageNone.Object;

            mdlDrawableOpaqueDeph1a = SetupDrawable("mdlDrawableOpaqueDeph1a", BlendModes.Opaque, true);
            mdlDrawableOpaqueDeph1b = SetupDrawable("mdlDrawableOpaqueDeph1b", BlendModes.Opaque, true);
            mdlDrawableOpaqueDeph2 = SetupDrawable("mdlDrawableOpaqueDeph2", BlendModes.Opaque, true);
            mdlDrawableOpaqueDeph3 = SetupDrawable("mdlDrawableOpaqueDeph3", BlendModes.Opaque, true);

            mdlDrawableAlphaDeph1a = SetupDrawable("mdlDrawableAlphaDeph1a", BlendModes.Alpha, true);
            mdlDrawableAlphaDeph1b = SetupDrawable("mdlDrawableAlphaDeph1b", BlendModes.Alpha, true);
            mdlDrawableAlphaDeph2 = SetupDrawable("mdlDrawableAlphaDeph2", BlendModes.Alpha, true);
            mdlDrawableAlphaDeph3 = SetupDrawable("mdlDrawableAlphaDeph3", BlendModes.Alpha, true);

            mdlDrawableOpaqueNoDeph1a = SetupDrawable("mdlDrawableOpaqueNoDeph1a", BlendModes.Opaque, false);
            mdlDrawableOpaqueNoDeph1b = SetupDrawable("mdlDrawableOpaqueNoDeph1b", BlendModes.Opaque, false);
            mdlDrawableOpaqueNoDeph2 = SetupDrawable("mdlDrawableOpaqueNoDeph2", BlendModes.Opaque, false);
            mdlDrawableOpaqueNoDeph3 = SetupDrawable("mdlDrawableOpaqueNoDeph3", BlendModes.Opaque, false);

            mdlDrawableAlphaNoDeph1a = SetupDrawable("mdlDrawableDepthNoDeph1a", BlendModes.Alpha, false);
            mdlDrawableAlphaNoDeph1b = SetupDrawable("mdlDrawableDepthNoDeph1b", BlendModes.Alpha, false);
            mdlDrawableAlphaNoDeph2 = SetupDrawable("mdlDrawableDepthNoDeph2", BlendModes.Alpha, false);
            mdlDrawableAlphaNoDeph3 = SetupDrawable("mdlDrawableDepthNoDeph3", BlendModes.Alpha, false);

            mdlUpdatable1a = SetupUpdatable("mdlUpdatable1a");
            mdlUpdatable1b = SetupUpdatable("mdlUpdatable1b");
            mdlUpdatable2 = SetupUpdatable("mdlUpdatable2");
            mdlUpdatable3 = SetupUpdatable("mdlUpdatable3");
        }

        private static Mock<ISceneObject> Setup(string id, SceneObjectUsages usage = SceneObjectUsages.None, int layer = 0)
        {
            var obj = new Mock<ISceneObject>();
            obj.SetupAllProperties();

            obj.Setup(o => o.Id).Returns(id);
            obj.Setup(o => o.Name).Returns(id);

            obj.Object.Usage = usage;
            obj.Object.Layer = layer;

            return obj;
        }
        private static Mock<IMockModel> SetupModel(string id, SceneObjectUsages usage = SceneObjectUsages.None, int layer = 0)
        {
            var obj = new Mock<IMockModel>();
            obj.SetupAllProperties();

            obj.Setup(c => c.Id).Returns(id);

            obj.Object.Name = id;
            obj.Object.Usage = usage;
            obj.Object.Layer = layer;

            return obj;
        }
        private static Mock<IMockDrawable> SetupDrawable(string id, BlendModes blendMode, bool depthEnabled)
        {
            var obj = new Mock<IMockDrawable>();
            obj.SetupAllProperties();

            obj.Setup(c => c.Id).Returns(id);
            obj.Setup(c => c.BlendMode).Returns(blendMode);
            obj.Setup(c => c.DepthEnabled).Returns(depthEnabled);

            obj.Object.Name = id;
            obj.Object.Usage = SceneObjectUsages.Object;

            return obj;
        }
        private static Mock<IMockUpdatable> SetupUpdatable(string id)
        {
            var obj = new Mock<IMockUpdatable>();
            obj.SetupAllProperties();

            obj.Setup(c => c.Id).Returns(id);

            obj.Object.Name = id;
            obj.Object.Usage = SceneObjectUsages.Object;

            return obj;
        }

        [TestInitialize]
        public void SetupTest()
        {
            Console.WriteLine($"TestContext.TestName='{_testContext.TestName}'");
        }

        [TestMethod()]
        public void SceneComponentCollectionAddOneTest()
        {
            var coll = new SceneComponentCollection();
            coll.AddComponent(obj1.Object, SceneObjectUsages.None, 0);

            Assert.AreEqual(1, coll.Count);

            var components = coll.Get()?.ToArray();
            Assert.IsNotNull(components);
            Assert.AreEqual(obj1.Object, components[0]);
        }
        [TestMethod()]
        public void SceneComponentCollectionAddTwoTest()
        {
            var coll = new SceneComponentCollection();
            coll.AddComponent(obj1.Object, SceneObjectUsages.None, 0);
            coll.AddComponent(obj2.Object, SceneObjectUsages.None, 0);

            Assert.AreEqual(2, coll.Count);

            var components = coll.Get()?.ToArray();
            Assert.IsNotNull(components);
            Assert.AreEqual(obj1.Object, components[0]);
            Assert.AreEqual(obj2.Object, components[1]);
        }
        [TestMethod()]
        public void SceneComponentCollectionAddSameTest()
        {
            var coll = new SceneComponentCollection();
            coll.AddComponent(obj1.Object, SceneObjectUsages.None, 0);
            coll.AddComponent(obj1.Object, SceneObjectUsages.None, 0);

            Assert.AreEqual(1, coll.Count);

            var components = coll.Get()?.ToArray();
            Assert.IsNotNull(components);
            Assert.AreEqual(obj1.Object, components[0]);
        }
        [TestMethod()]
        public void SceneComponentCollectionAddOtherSameIdTest()
        {
            var coll = new SceneComponentCollection();
            coll.AddComponent(obj1.Object, SceneObjectUsages.None, 0);
            Assert.ThrowsException<EngineException>(() => coll.AddComponent(obj1b.Object, SceneObjectUsages.None, 0));

            Assert.AreEqual(1, coll.Count);

            var components = coll.Get()?.ToArray();
            Assert.IsNotNull(components);
            Assert.AreEqual(obj1.Object, components[0]);
        }
        [TestMethod()]
        public void SceneComponentCollectionAddNullTest()
        {
            var coll = new SceneComponentCollection();
            coll.AddComponent(null, SceneObjectUsages.None, 0);

            Assert.AreEqual(0, coll.Count);

            var components = coll.Get()?.ToArray();
            Assert.IsNotNull(components);
        }

        [TestMethod()]
        public void SceneComponentCollectionRemoveOneTest()
        {
            var coll = new SceneComponentCollection();
            coll.AddComponent(obj1.Object, SceneObjectUsages.None, 0);

            Assert.AreEqual(1, coll.Count);

            var components = coll.Get()?.ToArray();
            Assert.IsNotNull(components);
            Assert.AreEqual(obj1.Object, components[0]);

            coll.RemoveComponent(obj1.Object);

            Assert.AreEqual(0, coll.Count);
        }
        [TestMethod()]
        public void SceneComponentCollectionRemoveTwoTest()
        {
            var coll = new SceneComponentCollection();
            coll.AddComponent(obj1.Object, SceneObjectUsages.None, 0);
            coll.AddComponent(obj2.Object, SceneObjectUsages.None, 0);

            Assert.AreEqual(2, coll.Count);

            var components = coll.Get()?.ToArray();
            Assert.IsNotNull(components);
            Assert.AreEqual(obj1.Object, components[0]);
            Assert.AreEqual(obj2.Object, components[1]);

            coll.RemoveComponent(obj1.Object);
            Assert.AreEqual(1, coll.Count);

            components = coll.Get()?.ToArray();
            Assert.IsNotNull(components);
            Assert.AreEqual(obj2.Object, components[0]);

            coll.RemoveComponent(obj1.Object);
            Assert.AreEqual(1, coll.Count);

            components = coll.Get()?.ToArray();
            Assert.IsNotNull(components);
            Assert.AreEqual(obj2.Object, components[0]);

            coll.RemoveComponent(obj2.Object);
            Assert.AreEqual(0, coll.Count);
        }
        [TestMethod()]
        public void SceneComponentCollectionRemoveCollectionTest()
        {
            var coll = new SceneComponentCollection();
            coll.AddComponent(obj1.Object, SceneObjectUsages.None, 0);
            coll.AddComponent(obj2.Object, SceneObjectUsages.None, 0);
            coll.AddComponent(obj3.Object, SceneObjectUsages.None, 0);

            Assert.AreEqual(3, coll.Count);

            var components = coll.Get()?.ToArray();
            Assert.IsNotNull(components);
            Assert.AreEqual(obj1.Object, components[0]);
            Assert.AreEqual(obj2.Object, components[1]);
            Assert.AreEqual(obj3.Object, components[2]);

            coll.RemoveComponents([obj1.Object, obj1b.Object]);
            Assert.AreEqual(2, coll.Count);

            components = coll.Get()?.ToArray();
            Assert.IsNotNull(components);
            Assert.AreEqual(obj2.Object, components[0]);
            Assert.AreEqual(obj3.Object, components[1]);

            coll.RemoveComponents([obj2.Object, obj3.Object]);
            Assert.AreEqual(0, coll.Count);
        }

        [TestMethod()]
        public void SceneComponentCollectionGetTest()
        {
            var coll = new SceneComponentCollection();
            coll.AddComponent(objUsageNone.Object, SceneObjectUsages.None, 0);
            coll.AddComponent(objUsageObject.Object, SceneObjectUsages.Object, 0);
            coll.AddComponent(objUsageGround.Object, SceneObjectUsages.Ground, 0);
            coll.AddComponent(objUsageAgent.Object, SceneObjectUsages.Agent, 0);
            coll.AddComponent(objUsageUI.Object, SceneObjectUsages.UI, 0);

            Assert.AreEqual(5, coll.Count);

            var components = coll.Get()?.ToArray();
            Assert.IsNotNull(components);
            Assert.AreEqual(5, components.Length);
        }
        [TestMethod()]
        public void SceneComponentCollectionGetUsageTest()
        {
            var coll = new SceneComponentCollection();
            coll.AddComponent(objUsageNone.Object, SceneObjectUsages.None, 0);
            coll.AddComponent(objUsageObject.Object, SceneObjectUsages.Object, 0);
            coll.AddComponent(objUsageGround.Object, SceneObjectUsages.Ground, 0);
            coll.AddComponent(objUsageAgent.Object, SceneObjectUsages.Agent, 0);
            coll.AddComponent(objUsageUI.Object, SceneObjectUsages.UI, 0);

            Assert.AreEqual(5, coll.Count);

            var components = coll.Get(SceneObjectUsages.Ground)?.ToArray();
            Assert.IsNotNull(components);
            Assert.AreEqual(1, components.Length);
            Assert.AreEqual(objUsageGround.Object, components[0]);

            components = coll.Get(SceneObjectUsages.None)?.ToArray();
            Assert.IsNotNull(components);
            Assert.AreEqual(5, components.Length);
        }
        [TestMethod()]
        public void SceneComponentCollectionGetPredicateTest()
        {
            var coll = new SceneComponentCollection();
            coll.AddComponent(objUsageNone.Object, SceneObjectUsages.None, 0);
            coll.AddComponent(objUsageObject.Object, SceneObjectUsages.Object, 0);
            coll.AddComponent(objUsageGround.Object, SceneObjectUsages.Ground, 0);
            coll.AddComponent(objUsageAgent.Object, SceneObjectUsages.Agent, 0);
            coll.AddComponent(objUsageUI.Object, SceneObjectUsages.UI, 0);

            Assert.AreEqual(5, coll.Count);

            var components = coll.Get(c => c.Id != "objUsageNone")?.ToArray();
            Assert.IsNotNull(components);
            Assert.AreEqual(4, components.Length);

            Assert.ThrowsException<ArgumentNullException>(() => coll.Get((Func<ISceneObject, bool>)null));
        }
        [TestMethod()]
        public void SceneComponentCollectionGetUsagePredicateTest()
        {
            var coll = new SceneComponentCollection();
            coll.AddComponent(objUsageNone.Object, SceneObjectUsages.None, 0);
            coll.AddComponent(objUsageObject.Object, SceneObjectUsages.Object, 0);
            coll.AddComponent(objUsageGround.Object, SceneObjectUsages.Ground, 0);
            coll.AddComponent(objUsageAgent.Object, SceneObjectUsages.Agent, 0);
            coll.AddComponent(objUsageUI.Object, SceneObjectUsages.UI, 0);

            Assert.AreEqual(5, coll.Count);

            var components = coll.Get(SceneObjectUsages.Ground, c => c.Id != "objUsageNone")?.ToArray();
            Assert.IsNotNull(components);
            Assert.AreEqual(1, components.Length);
            Assert.AreEqual(objUsageGround.Object, components[0]);

            components = coll.Get(SceneObjectUsages.None, c => c.Id != "objUsageNone")?.ToArray();
            Assert.IsNotNull(components);
            Assert.AreEqual(4, components.Length);

            components = coll.Get(SceneObjectUsages.Ground, c => c.Id == "objUsageAgent")?.ToArray();
            Assert.IsNotNull(components);
            Assert.AreEqual(0, components.Length);

            Assert.ThrowsException<ArgumentNullException>(() => coll.Get(SceneObjectUsages.None, (Func<ISceneObject, bool>)null));
        }

        [TestMethod()]
        public void SceneComponentCollectionFirstTest()
        {
            var coll = new SceneComponentCollection();
            coll.AddComponent(objUsageNone.Object, SceneObjectUsages.None, 0);
            coll.AddComponent(objUsageObject.Object, SceneObjectUsages.Object, 0);
            coll.AddComponent(objUsageGround.Object, SceneObjectUsages.Ground, 0);
            coll.AddComponent(objUsageAgent.Object, SceneObjectUsages.Agent, 0);
            coll.AddComponent(objUsageUI.Object, SceneObjectUsages.UI, 0);

            Assert.AreEqual(5, coll.Count);

            var component = coll.First();
            Assert.IsNotNull(component);
            Assert.AreEqual(objUsageAgent.Object, component);
        }
        [TestMethod()]
        public void SceneComponentCollectionFirstUsageTest()
        {
            var coll = new SceneComponentCollection();
            coll.AddComponent(objUsageNone.Object, SceneObjectUsages.None, 0);
            coll.AddComponent(objUsageObject.Object, SceneObjectUsages.Object, 0);
            coll.AddComponent(objUsageGround.Object, SceneObjectUsages.Ground, 0);
            coll.AddComponent(objUsageAgent.Object, SceneObjectUsages.Agent, 0);
            coll.AddComponent(objUsageUI.Object, SceneObjectUsages.UI, 0);

            Assert.AreEqual(5, coll.Count);

            var component = coll.First(SceneObjectUsages.Ground);
            Assert.IsNotNull(component);
            Assert.AreEqual(objUsageGround.Object, component);

            component = coll.First(SceneObjectUsages.None);
            Assert.IsNotNull(component);
            Assert.AreEqual(objUsageAgent.Object, component);
        }
        [TestMethod()]
        public void SceneComponentCollectionFirstPredicateTest()
        {
            var coll = new SceneComponentCollection();
            coll.AddComponent(objUsageNone.Object, SceneObjectUsages.None, 0);
            coll.AddComponent(objUsageObject.Object, SceneObjectUsages.Object, 0);
            coll.AddComponent(objUsageGround.Object, SceneObjectUsages.Ground, 0);
            coll.AddComponent(objUsageAgent.Object, SceneObjectUsages.Agent, 0);
            coll.AddComponent(objUsageUI.Object, SceneObjectUsages.UI, 0);

            Assert.AreEqual(5, coll.Count);

            var component = coll.First(c => c.Id != "objUsageNone");
            Assert.IsNotNull(component);
            Assert.AreEqual(objUsageAgent.Object, component);

            Assert.ThrowsException<ArgumentNullException>(() => coll.First(null));
        }
        [TestMethod()]
        public void SceneComponentCollectionFirstUsagePredicateTest()
        {
            var coll = new SceneComponentCollection();
            coll.AddComponent(objUsageNone.Object, SceneObjectUsages.None, 0);
            coll.AddComponent(objUsageObject.Object, SceneObjectUsages.Object, 0);
            coll.AddComponent(objUsageGround.Object, SceneObjectUsages.Ground, 0);
            coll.AddComponent(objUsageAgent.Object, SceneObjectUsages.Agent, 0);
            coll.AddComponent(objUsageUI.Object, SceneObjectUsages.UI, 0);

            Assert.AreEqual(5, coll.Count);

            var component = coll.First(SceneObjectUsages.Ground, c => c.Id != "objUsageNone");
            Assert.IsNotNull(component);
            Assert.AreEqual(objUsageGround.Object, component);

            component = coll.First(SceneObjectUsages.None, c => c.Id != "objUsageNone");
            Assert.IsNotNull(component);
            Assert.AreEqual(objUsageAgent.Object, component);

            component = coll.First(SceneObjectUsages.Ground, c => c.Id == "objUsageAgent");
            Assert.IsNull(component);

            Assert.ThrowsException<ArgumentNullException>(() => coll.First(SceneObjectUsages.None, null));
        }

        [TestMethod()]
        public void SceneComponentCollectionGetGenericTest()
        {
            var coll = new SceneComponentCollection();
            coll.AddComponent(mdlUsageNone.Object, SceneObjectUsages.None, 0);
            coll.AddComponent(mdlUsageObject.Object, SceneObjectUsages.Object, 0);
            coll.AddComponent(mdlUsageGround.Object, SceneObjectUsages.Ground, 0);
            coll.AddComponent(mdlUsageAgent.Object, SceneObjectUsages.Agent, 0);
            coll.AddComponent(mdlUsageUI.Object, SceneObjectUsages.UI, 0);

            Assert.AreEqual(5, coll.Count);

            var components = coll.Get<IMockModel>()?.ToArray();
            Assert.IsNotNull(components);
            Assert.AreEqual(5, components.Length);
        }
        [TestMethod()]
        public void SceneComponentCollectionGetGenericUsageTest()
        {
            var coll = new SceneComponentCollection();
            coll.AddComponent(mdlUsageNone.Object, SceneObjectUsages.None, 0);
            coll.AddComponent(mdlUsageObject.Object, SceneObjectUsages.Object, 0);
            coll.AddComponent(mdlUsageGround.Object, SceneObjectUsages.Ground, 0);
            coll.AddComponent(mdlUsageAgent.Object, SceneObjectUsages.Agent, 0);
            coll.AddComponent(mdlUsageUI.Object, SceneObjectUsages.UI, 0);

            Assert.AreEqual(5, coll.Count);

            var components = coll.Get<IMockModel>(SceneObjectUsages.Ground)?.ToArray();
            Assert.IsNotNull(components);
            Assert.AreEqual(1, components.Length);
            Assert.AreEqual(mdlUsageGround.Object, components[0]);

            components = coll.Get<IMockModel>(SceneObjectUsages.None)?.ToArray();
            Assert.IsNotNull(components);
            Assert.AreEqual(5, components.Length);
        }
        [TestMethod()]
        public void SceneComponentCollectionGetGenericPredicateTest()
        {
            var coll = new SceneComponentCollection();
            coll.AddComponent(mdlUsageNone.Object, SceneObjectUsages.None, 0);
            coll.AddComponent(mdlUsageObject.Object, SceneObjectUsages.Object, 0);
            coll.AddComponent(mdlUsageGround.Object, SceneObjectUsages.Ground, 0);
            coll.AddComponent(mdlUsageAgent.Object, SceneObjectUsages.Agent, 0);
            coll.AddComponent(mdlUsageUI.Object, SceneObjectUsages.UI, 0);

            Assert.AreEqual(5, coll.Count);

            var components = coll.Get<IMockModel>(c => c.Id != "mdlUsageNone")?.ToArray();
            Assert.IsNotNull(components);
            Assert.AreEqual(4, components.Length);

            Assert.ThrowsException<ArgumentNullException>(() => coll.Get<IMockModel>(null));
        }
        [TestMethod()]
        public void SceneComponentCollectionGetGenericUsagePredicateTest()
        {
            var coll = new SceneComponentCollection();
            coll.AddComponent(mdlUsageNone.Object, SceneObjectUsages.None, 0);
            coll.AddComponent(mdlUsageObject.Object, SceneObjectUsages.Object, 0);
            coll.AddComponent(mdlUsageGround.Object, SceneObjectUsages.Ground, 0);
            coll.AddComponent(mdlUsageAgent.Object, SceneObjectUsages.Agent, 0);
            coll.AddComponent(mdlUsageUI.Object, SceneObjectUsages.UI, 0);

            Assert.AreEqual(5, coll.Count);

            var components = coll.Get<IMockModel>(SceneObjectUsages.Ground, c => c.Id != "mdlUsageNone")?.ToArray();
            Assert.IsNotNull(components);
            Assert.AreEqual(1, components.Length);
            Assert.AreEqual(mdlUsageGround.Object, components[0]);

            components = coll.Get<IMockModel>(SceneObjectUsages.None, c => c.Id != "mdlUsageNone")?.ToArray();
            Assert.IsNotNull(components);
            Assert.AreEqual(4, components.Length);

            components = coll.Get<IMockModel>(SceneObjectUsages.Ground, c => c.Id == "mdlUsageAgent")?.ToArray();
            Assert.IsNotNull(components);
            Assert.AreEqual(0, components.Length);

            Assert.ThrowsException<ArgumentNullException>(() => coll.Get<IMockModel>(SceneObjectUsages.None, null));
        }

        [TestMethod()]
        public void SceneComponentCollectionFirstGenericTest()
        {
            var coll = new SceneComponentCollection();
            coll.AddComponent(mdlUsageNone.Object, SceneObjectUsages.None, 0);
            coll.AddComponent(mdlUsageObject.Object, SceneObjectUsages.Object, 0);
            coll.AddComponent(mdlUsageGround.Object, SceneObjectUsages.Ground, 0);
            coll.AddComponent(mdlUsageAgent.Object, SceneObjectUsages.Agent, 0);
            coll.AddComponent(mdlUsageUI.Object, SceneObjectUsages.UI, 0);

            Assert.AreEqual(5, coll.Count);

            var component = coll.First<IMockModel>();
            Assert.IsNotNull(component);
            Assert.AreEqual(mdlUsageAgent.Object, component);
        }
        [TestMethod()]
        public void SceneComponentCollectionFirstGenericUsageTest()
        {
            var coll = new SceneComponentCollection();
            coll.AddComponent(mdlUsageNone.Object, SceneObjectUsages.None, 0);
            coll.AddComponent(mdlUsageObject.Object, SceneObjectUsages.Object, 0);
            coll.AddComponent(mdlUsageGround.Object, SceneObjectUsages.Ground, 0);
            coll.AddComponent(mdlUsageAgent.Object, SceneObjectUsages.Agent, 0);
            coll.AddComponent(mdlUsageUI.Object, SceneObjectUsages.UI, 0);

            Assert.AreEqual(5, coll.Count);

            var component = coll.First<IMockModel>(SceneObjectUsages.Ground);
            Assert.IsNotNull(component);
            Assert.AreEqual(mdlUsageGround.Object, component);

            component = coll.First<IMockModel>(SceneObjectUsages.None);
            Assert.IsNotNull(component);
            Assert.AreEqual(mdlUsageAgent.Object, component);
        }
        [TestMethod()]
        public void SceneComponentCollectionFirstGenericPredicateTest()
        {
            var coll = new SceneComponentCollection();
            coll.AddComponent(mdlUsageNone.Object, SceneObjectUsages.None, 0);
            coll.AddComponent(mdlUsageObject.Object, SceneObjectUsages.Object, 0);
            coll.AddComponent(mdlUsageGround.Object, SceneObjectUsages.Ground, 0);
            coll.AddComponent(mdlUsageAgent.Object, SceneObjectUsages.Agent, 0);
            coll.AddComponent(mdlUsageUI.Object, SceneObjectUsages.UI, 0);

            Assert.AreEqual(5, coll.Count);

            var component = coll.First<IMockModel>(c => c.Id != "mdlUsageNone");
            Assert.IsNotNull(component);
            Assert.AreEqual(mdlUsageAgent.Object, component);

            Assert.ThrowsException<ArgumentNullException>(() => coll.First<IMockModel>(null));
        }
        [TestMethod()]
        public void SceneComponentCollectionFirstGenericUsagePredicateTest()
        {
            var coll = new SceneComponentCollection();
            coll.AddComponent(mdlUsageNone.Object, SceneObjectUsages.None, 0);
            coll.AddComponent(mdlUsageObject.Object, SceneObjectUsages.Object, 0);
            coll.AddComponent(mdlUsageGround.Object, SceneObjectUsages.Ground, 0);
            coll.AddComponent(mdlUsageAgent.Object, SceneObjectUsages.Agent, 0);
            coll.AddComponent(mdlUsageUI.Object, SceneObjectUsages.UI, 0);

            Assert.AreEqual(5, coll.Count);

            var component = coll.First<IMockModel>(SceneObjectUsages.Ground, c => c.Id != "mdlUsageNone");
            Assert.IsNotNull(component);
            Assert.AreEqual(mdlUsageGround.Object, component);

            component = coll.First<IMockModel>(SceneObjectUsages.None, c => c.Id != "mdlUsageNone");
            Assert.IsNotNull(component);
            Assert.AreEqual(mdlUsageAgent.Object, component);

            component = coll.First<IMockModel>(SceneObjectUsages.Ground, c => c.Id == "mdlUsageAgent");
            Assert.IsNull(component);

            Assert.ThrowsException<ArgumentNullException>(() => coll.First<IMockModel>(SceneObjectUsages.None, null));
        }


        [TestMethod()]
        public void SceneComponentCollectionGetbyIdTest()
        {
            var coll = new SceneComponentCollection();
            coll.AddComponent(mdlUsageNone.Object, SceneObjectUsages.None, 0);
            coll.AddComponent(mdlUsageObject.Object, SceneObjectUsages.Object, 0);
            coll.AddComponent(mdlUsageGround.Object, SceneObjectUsages.Ground, 0);
            coll.AddComponent(mdlUsageAgent.Object, SceneObjectUsages.Agent, 0);
            coll.AddComponent(mdlUsageUI.Object, SceneObjectUsages.UI, 0);

            Assert.AreEqual(5, coll.Count);

            var component = coll.ById("mdlUsageObject");
            Assert.IsNotNull(component);
            Assert.AreEqual(mdlUsageObject.Object, component);
        }
        [TestMethod()]
        public void SceneComponentCollectionGetbyNameTest()
        {
            var coll = new SceneComponentCollection();
            coll.AddComponent(mdlUsageNone.Object, SceneObjectUsages.None, 0);
            coll.AddComponent(mdlUsageObject.Object, SceneObjectUsages.Object, 0);
            coll.AddComponent(mdlUsageGround.Object, SceneObjectUsages.Ground, 0);
            coll.AddComponent(mdlUsageAgent.Object, SceneObjectUsages.Agent, 0);
            coll.AddComponent(mdlUsageUI.Object, SceneObjectUsages.UI, 0);

            Assert.AreEqual(5, coll.Count);

            var component = coll.ByName("mdlUsageObject");
            Assert.IsNotNull(component);
            Assert.AreEqual(mdlUsageObject.Object, component);
        }
        [TestMethod()]
        public void SceneComponentCollectionGetbyOwnerTest()
        {
            var coll = new SceneComponentCollection();
            coll.AddComponent(mdlUsageNone.Object, SceneObjectUsages.None, 0);
            coll.AddComponent(mdlUsageObject.Object, SceneObjectUsages.Object, 0);
            coll.AddComponent(mdlUsageGround.Object, SceneObjectUsages.Ground, 0);
            coll.AddComponent(mdlUsageAgent.Object, SceneObjectUsages.Agent, 0);
            coll.AddComponent(mdlUsageUI.Object, SceneObjectUsages.UI, 0);

            Assert.AreEqual(5, coll.Count);

            var components = coll.ByOwner(mdlUsageNone.Object)?.ToArray();
            Assert.IsNotNull(components);
            Assert.AreEqual(4, components.Length);

            components = coll.ByOwner(null)?.ToArray();
            Assert.IsNotNull(components);
            Assert.AreEqual(0, components.Length);
        }

        [TestMethod()]
        public void SceneComponentCollectionSortTest()
        {
            var coll = new SceneComponentCollection();

            coll.AddComponent(mdlUpdatable1a.Object, SceneObjectUsages.Object, 1);
            coll.AddComponent(mdlUpdatable3.Object, SceneObjectUsages.Object, 3);
            coll.AddComponent(mdlUpdatable2.Object, SceneObjectUsages.Object, 2);
            coll.AddComponent(mdlUpdatable1b.Object, SceneObjectUsages.Object, 1);

            coll.AddComponent(mdlDrawableOpaqueDeph1a.Object, SceneObjectUsages.Object, 1);
            coll.AddComponent(mdlDrawableOpaqueDeph3.Object, SceneObjectUsages.Object, 3);
            coll.AddComponent(mdlDrawableOpaqueDeph2.Object, SceneObjectUsages.Object, 2);
            coll.AddComponent(mdlDrawableOpaqueDeph1b.Object, SceneObjectUsages.Object, 1);

            coll.AddComponent(mdlDrawableAlphaDeph1a.Object, SceneObjectUsages.Object, 1);
            coll.AddComponent(mdlDrawableAlphaDeph1b.Object, SceneObjectUsages.Object, 1);
            coll.AddComponent(mdlDrawableAlphaDeph2.Object, SceneObjectUsages.Object, 2);
            coll.AddComponent(mdlDrawableAlphaDeph3.Object, SceneObjectUsages.Object, 3);

            coll.AddComponent(mdlDrawableOpaqueNoDeph1b.Object, SceneObjectUsages.Object, 1);
            coll.AddComponent(mdlDrawableOpaqueNoDeph3.Object, SceneObjectUsages.Object, 3);
            coll.AddComponent(mdlDrawableOpaqueNoDeph2.Object, SceneObjectUsages.Object, 2);
            coll.AddComponent(mdlDrawableOpaqueNoDeph1a.Object, SceneObjectUsages.Object, 1);

            coll.AddComponent(mdlDrawableAlphaNoDeph1a.Object, SceneObjectUsages.Object, 1);
            coll.AddComponent(mdlDrawableAlphaNoDeph3.Object, SceneObjectUsages.Object, 3);
            coll.AddComponent(mdlDrawableAlphaNoDeph2.Object, SceneObjectUsages.Object, 2);
            coll.AddComponent(mdlDrawableAlphaNoDeph1b.Object, SceneObjectUsages.Object, 1);

            Assert.AreEqual(20, coll.Count);

            var components = coll.Get()?.ToArray();
            Assert.IsNotNull(components);

            Assert.AreEqual(mdlDrawableOpaqueDeph1a.Object, components[0]);
            Assert.AreEqual(mdlDrawableOpaqueDeph1b.Object, components[1]);
            Assert.AreEqual(mdlDrawableOpaqueNoDeph1a.Object, components[2]);
            Assert.AreEqual(mdlDrawableOpaqueNoDeph1b.Object, components[3]);
            Assert.AreEqual(mdlDrawableAlphaDeph1a.Object, components[4]);
            Assert.AreEqual(mdlDrawableAlphaDeph1b.Object, components[5]);
            Assert.AreEqual(mdlDrawableAlphaNoDeph1a.Object, components[6]);
            Assert.AreEqual(mdlDrawableAlphaNoDeph1b.Object, components[7]);
            Assert.AreEqual(mdlUpdatable1a.Object, components[8]);
            Assert.AreEqual(mdlUpdatable1b.Object, components[9]);

            Assert.AreEqual(mdlDrawableOpaqueDeph2.Object, components[10]);
            Assert.AreEqual(mdlDrawableOpaqueNoDeph2.Object, components[11]);
            Assert.AreEqual(mdlDrawableAlphaDeph2.Object, components[12]);
            Assert.AreEqual(mdlDrawableAlphaNoDeph2.Object, components[13]);
            Assert.AreEqual(mdlUpdatable2.Object, components[14]);

            Assert.AreEqual(mdlDrawableOpaqueDeph3.Object, components[15]);
            Assert.AreEqual(mdlDrawableOpaqueNoDeph3.Object, components[16]);
            Assert.AreEqual(mdlDrawableAlphaDeph3.Object, components[17]);
            Assert.AreEqual(mdlDrawableAlphaNoDeph3.Object, components[18]);
            Assert.AreEqual(mdlUpdatable3.Object, components[19]);
        }
    }

    public interface IMockModel : ISceneObject
    {

    }

    public interface IMockDrawable : ISceneObject, IDrawable
    {

    }
    public interface IMockUpdatable : ISceneObject, IUpdatable
    {

    }
}

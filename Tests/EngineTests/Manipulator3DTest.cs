﻿using Engine;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SharpDX;
using System;
using System.Diagnostics.CodeAnalysis;

namespace EngineTests
{
    [ExcludeFromCodeCoverage]
    [TestClass()]
    public class Manipulator3DTest
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
        public void ConstructorTest()
        {
            Manipulator3D man = new();

            Assert.AreEqual(Vector3.ForwardLH, man.Forward, "Forward direction vector error");
            Assert.AreEqual(Vector3.BackwardLH, man.Backward, "Backward direction vector error");
            Assert.AreEqual(Vector3.Left, man.Left, "Left direction vector error");
            Assert.AreEqual(Vector3.Right, man.Right, "Right direction vector error");
            Assert.AreEqual(Vector3.Up, man.Up, "Up direction vector error");
            Assert.AreEqual(Vector3.Down, man.Down, "Down direction vector error");

            Assert.AreEqual(Vector3.Zero, man.Position, "Position point error");
            Assert.AreEqual(Quaternion.Identity, man.Rotation, "Rotation quaternion error");
            Assert.AreEqual(Vector3.One, man.Scaling, "Scaling error");

            Assert.AreEqual(Matrix.Identity, man.LocalTransform, "LocalTransform error");
            Assert.AreEqual(Matrix.Identity, man.GlobalTransform, "GlobalTransform error");
        }

        [TestMethod()]
        public void SetPositionTest()
        {
            Manipulator3D man = new();
            var point = Vector3.One;
            var trn = Matrix.Translation(point);

            man.Reset();
            man.SetPosition(point.X, point.Y, point.Z);
            ValidateSetPosition(point, trn, man);

            man.Reset();
            man.SetPosition(point);
            ValidateSetPosition(point, trn, man);
        }
        private static void ValidateSetPosition(Vector3 point, Matrix trn, Manipulator3D man)
        {
            Assert.AreEqual(Vector3.ForwardLH, man.Forward, "Forward direction vector error");
            Assert.AreEqual(Vector3.BackwardLH, man.Backward, "Backward direction vector error");
            Assert.AreEqual(Vector3.Left, man.Left, "Left direction vector error");
            Assert.AreEqual(Vector3.Right, man.Right, "Right direction vector error");
            Assert.AreEqual(Vector3.Up, man.Up, "Up direction vector error");
            Assert.AreEqual(Vector3.Down, man.Down, "Down direction vector error");

            Assert.AreEqual(point, man.Position, "Position point error");
            Assert.AreEqual(Quaternion.Identity, man.Rotation, "Rotation quaternion error");
            Assert.AreEqual(Vector3.One, man.Scaling, "Scaling error");

            Assert.AreEqual(trn, man.LocalTransform, "LocalTransform error");
            Assert.AreEqual(trn, man.GlobalTransform, "GlobalTransform error");
        }
        [TestMethod()]
        public void SetRotationTest()
        {
            Manipulator3D man = new();
            var angle = MathUtil.PiOverTwo;
            var rot = Quaternion.RotationYawPitchRoll(angle, 0, 0);
            var trn = Matrix.RotationQuaternion(rot);

            man.Reset();
            man.SetRotation(Vector3.Up, angle);
            ValidateSetRotation(rot, trn, man);

            man.Reset();
            man.SetRotation(angle, 0, 0);
            ValidateSetRotation(rot, trn, man);

            man.Reset();
            man.SetRotation(rot);
            ValidateSetRotation(rot, trn, man);
        }
        private static void ValidateSetRotation(Quaternion rot, Matrix trn, Manipulator3D man)
        {
            Assert.AreEqual(-trn.Forward, man.Forward, "Forward direction vector error");
            Assert.AreEqual(-trn.Backward, man.Backward, "Backward direction vector error");
            Assert.AreEqual(trn.Left, man.Left, "Left direction vector error");
            Assert.AreEqual(trn.Right, man.Right, "Right direction vector error");
            Assert.AreEqual(trn.Up, man.Up, "Up direction vector error");
            Assert.AreEqual(trn.Down, man.Down, "Down direction vector error");

            Assert.AreEqual(Vector3.Zero, man.Position, "Position point error");
            Assert.AreEqual(rot, man.Rotation, "Rotation quaternion error");
            Assert.AreEqual(Vector3.One, man.Scaling, "Scaling error");

            Assert.AreEqual(trn, man.LocalTransform, "LocalTransform error");
            Assert.AreEqual(trn, man.GlobalTransform, "GlobalTransform error");
        }
        [TestMethod()]
        public void SetScalingTest()
        {
            Manipulator3D man = new();
            float scale = 2f;
            var scaling = new Vector3(scale);
            var trn = Matrix.Scaling(scaling);

            man.Reset();
            man.SetScaling(scale);
            ValidateSetScaling(scaling, trn, man);

            man.Reset();
            man.SetScaling(scale, scale, scale);
            ValidateSetScaling(scaling, trn, man);

            man.Reset();
            man.SetScaling(scaling);
            ValidateSetScaling(scaling, trn, man);
        }
        private static void ValidateSetScaling(Vector3 scaling, Matrix trn, Manipulator3D man)
        {
            Assert.AreEqual(Vector3.ForwardLH, man.Forward, "Forward direction vector error");
            Assert.AreEqual(Vector3.BackwardLH, man.Backward, "Backward direction vector error");
            Assert.AreEqual(Vector3.Left, man.Left, "Left direction vector error");
            Assert.AreEqual(Vector3.Right, man.Right, "Right direction vector error");
            Assert.AreEqual(Vector3.Up, man.Up, "Up direction vector error");
            Assert.AreEqual(Vector3.Down, man.Down, "Down direction vector error");

            Assert.AreEqual(Vector3.Zero, man.Position, "Position point error");
            Assert.AreEqual(Quaternion.Identity, man.Rotation, "Rotation quaternion error");
            Assert.AreEqual(scaling, man.Scaling, "Scaling error");

            Assert.AreEqual(trn, man.LocalTransform, "LocalTransform error");
            Assert.AreEqual(trn, man.GlobalTransform, "GlobalTransform error");
        }
        [TestMethod()]
        public void SetTransformTest()
        {
            Manipulator3D man = new();
            var point = Vector3.One;
            var angle = MathUtil.PiOverTwo;
            float scale = 2f;
            var scaling = new Vector3(scale);
            var rot = Quaternion.RotationYawPitchRoll(angle, 0, 0);
            var rotTrn = Matrix.RotationQuaternion(rot);
            var trn = Matrix.Scaling(scaling) * rotTrn * Matrix.Translation(point);

            man.Reset();
            man.SetTransform(point, Vector3.Up, angle, scale);
            ValidateSetTransform(point, rot, scaling, trn, man);

            man.Reset();
            man.SetTransform(point, Vector3.Up, angle, scaling);
            ValidateSetTransform(point, rot, scaling, trn, man);

            man.Reset();
            man.SetTransform(point, angle, 0, 0, scale);
            ValidateSetTransform(point, rot, scaling, trn, man);

            man.Reset();
            man.SetTransform(point, angle, 0, 0, scaling);
            ValidateSetTransform(point, rot, scaling, trn, man);

            man.Reset();
            man.SetTransform(point, rot, scale);
            ValidateSetTransform(point, rot, scaling, trn, man);

            man.Reset();
            man.SetTransform(point, rot, scaling);
            ValidateSetTransform(point, rot, scaling, trn, man);
        }
        private static void ValidateSetTransform(Vector3 point, Quaternion rot, Vector3 scaling, Matrix trn, Manipulator3D man)
        {
            var rotTrn = Matrix.RotationQuaternion(rot);

            Assert.AreEqual(-rotTrn.Forward, man.Forward, "Forward direction vector error");
            Assert.AreEqual(-rotTrn.Backward, man.Backward, "Backward direction vector error");
            Assert.AreEqual(rotTrn.Left, man.Left, "Left direction vector error");
            Assert.AreEqual(rotTrn.Right, man.Right, "Right direction vector error");
            Assert.AreEqual(rotTrn.Up, man.Up, "Up direction vector error");
            Assert.AreEqual(rotTrn.Down, man.Down, "Down direction vector error");

            Assert.AreEqual(point, man.Position, "Position point error");
            Assert.AreEqual(rot, man.Rotation, "Rotation quaternion error");
            Assert.AreEqual(scaling, man.Scaling, "Scaling error");

            Assert.AreEqual(trn, man.LocalTransform, "LocalTransform error");
            Assert.AreEqual(trn, man.GlobalTransform, "GlobalTransform error");
        }
        [TestMethod()]
        public void LookAtTest()
        {
            Manipulator3D man = new();
            var target = Vector3.One * 2f;
            var dir = Vector3.Normalize(Vector3.Zero - target);

            man.Reset();
            man.LookAt(target);
            ValidateLookAt(Vector3.Zero, dir, man);

            man.Reset();
            man.LookAt(target, Vector3.Up);
            ValidateLookAt(Vector3.Zero, dir, man);

            var point = Vector3.One;
            dir = Vector3.Normalize(point - target);

            man.Reset();
            man.SetPosition(point);
            man.LookAt(target);
            ValidateLookAt(point, dir, man);

            man.Reset();
            man.SetPosition(point);
            man.LookAt(target, Vector3.Up);
            ValidateLookAt(point, dir, man);

            point = Vector3.One * 2f;
            dir = Vector3.ForwardLH;

            man.Reset();
            man.SetPosition(point);
            man.LookAt(target);
            ValidateLookAt(point, dir, man);

            point = Vector3.One;
            dir = Vector3.Normalize(Vector3.Zero - target);

            man.Reset();
            man.LookAt(target);
            man.SetPosition(point);
            ValidateLookAt(point, dir, man);
        }
        private static void ValidateLookAt(Vector3 point, Vector3 dir, Manipulator3D man)
        {
            Assert.AreEqual(dir, man.Forward, "Forward direction vector error");

            Assert.AreEqual(point, man.Position, "Position point error");
            Assert.AreEqual(Vector3.One, man.Scaling, "Scaling error");
        }
        [TestMethod()]
        public void RotateToTest()
        {
            Manipulator3D man = new();
            var target = new Vector3(0, 2, -2);

            //Rotate freely
            var dir = Vector3.Normalize(Vector3.Zero - target);
            man.Reset();
            man.RotateTo(target, Axis.None);
            ValidateRotateTo(Vector3.Zero, dir, man);

            //Rotate on Y axis (No rotation at all)
            dir = Vector3.Normalize(Vector3.Zero - new Vector3(target.X, 0, target.Z));
            man.Reset();
            man.RotateTo(target, Axis.Y);
            ValidateRotateTo(Vector3.Zero, dir, man);

            //Rotate on X axis (Pitch 45º)
            dir = Vector3.Normalize(Vector3.Zero - new Vector3(0, target.Y, target.Z));
            man.Reset();
            man.RotateTo(target, Axis.X);
            ValidateRotateTo(Vector3.Zero, dir, man);

            //Rotate on Z axis (No rotation at all)
            dir = Vector3.ForwardLH;
            man.Reset();
            man.RotateTo(target, Axis.Z);
            ValidateRotateTo(Vector3.Zero, dir, man);

            target = new Vector3(2, 0, -2);

            //Rotate freely
            dir = Vector3.Normalize(Vector3.Zero - target);
            man.Reset();
            man.RotateTo(target, Axis.None);
            ValidateRotateTo(Vector3.Zero, dir, man);

            //Rotate on Y axis (Yaw 45º)
            dir = Vector3.Normalize(Vector3.Zero - new Vector3(target.X, 0, target.Z));
            man.Reset();
            man.RotateTo(target, Axis.Y);
            ValidateRotateTo(Vector3.Zero, dir, man);

            //Rotate on X axis (Pitch 45º)
            dir = Vector3.Normalize(Vector3.Zero - new Vector3(0, target.Y, target.Z));
            man.Reset();
            man.RotateTo(target, Axis.X);
            ValidateRotateTo(Vector3.Zero, dir, man);

            //Rotate on Z axis (No rotation at all)
            dir = Vector3.ForwardLH;
            man.Reset();
            man.RotateTo(target, Axis.Z);
            ValidateRotateTo(Vector3.Zero, dir, man);
        }
        private static void ValidateRotateTo(Vector3 point, Vector3 dir, Manipulator3D man)
        {
            Assert.AreEqual(dir, man.Forward, "Forward direction vector error");

            Assert.AreEqual(point, man.Position, "Position point error");
            Assert.AreEqual(Vector3.One, man.Scaling, "Scaling error");
        }
        [TestMethod()]
        public void SetNormalTest()
        {
            Manipulator3D man = new();

            var point = Vector3.Zero;
            var normal = Vector3.Up;
            man.Reset();
            man.SetNormal(normal);
            ValidateSetNormal(point, normal, man);

            normal = Vector3.Normalize(Vector3.One);
            man.Reset();
            man.SetNormal(normal);
            ValidateSetNormal(point, normal, man);

            normal = Vector3.Normalize(Vector3.Left);
            man.Reset();
            man.SetNormal(normal);
            ValidateSetNormal(point, normal, man);

            normal = Vector3.Normalize(Vector3.Down);
            man.Reset();
            man.SetNormal(normal);
            ValidateSetNormal(point, normal, man);

            point = Vector3.One;
            normal = Vector3.Up;
            man.Reset();
            man.SetPosition(point);
            man.SetNormal(normal);
            ValidateSetNormal(point, normal, man);

            normal = Vector3.Normalize(Vector3.One);
            man.Reset();
            man.SetPosition(point);
            man.SetNormal(normal);
            ValidateSetNormal(point, normal, man);

            normal = Vector3.Normalize(Vector3.Left);
            man.Reset();
            man.SetPosition(point);
            man.SetNormal(normal);
            ValidateSetNormal(point, normal, man);

            normal = Vector3.Normalize(Vector3.Down);
            man.Reset();
            man.SetPosition(point);
            man.SetNormal(normal);
            ValidateSetNormal(point, normal, man);
        }
        private static void ValidateSetNormal(Vector3 point, Vector3 normal, Manipulator3D man)
        {
            Assert.AreEqual(normal, man.Up, "Up direction vector error");

            Assert.AreEqual(point, man.Position, "Position vector error");
        }

        [TestMethod()]
        public void MoveTest()
        {
            float timeSeconds = 1f / 60f;

            Manipulator3D man = new();

            //Time == zero
            Mock<IGameTime> time = new();
            time.SetupAllProperties();
            man.Reset();
            man.Move(time.Object, Vector3.One, 1);
            ValidateMove(Vector3.Zero, Matrix.Identity, man);

            //Direction == zero
            time = new();
            time.SetupAllProperties();
            time.Setup(x => x.ElapsedSeconds).Returns(() => timeSeconds);
            man.Reset();
            man.Move(time.Object, Vector3.Zero, 1);
            ValidateMove(Vector3.Zero, Matrix.Identity, man);

            //Velocity == zero
            time = new();
            time.SetupAllProperties();
            time.Setup(x => x.ElapsedSeconds).Returns(() => timeSeconds);
            man.Reset();
            man.Move(time.Object, Vector3.One, 0);
            ValidateMove(Vector3.Zero, Matrix.Identity, man);

            time = new();
            time.SetupAllProperties();
            time.Setup(x => x.ElapsedSeconds).Returns(() => timeSeconds);
            var point = new Vector3(10, 0, 10);
            float velocity = 2f;

            var direction = Vector3.One;
            var translation = Vector3.Normalize(direction) * velocity * timeSeconds;
            var trn = Matrix.Translation(point + translation);
            man.Reset();
            man.SetPosition(point);
            man.Move(time.Object, direction, velocity);
            ValidateMove(point + translation, trn, man);

            direction = Matrix.Identity.Forward;
            translation = Vector3.Normalize(direction) * velocity * timeSeconds;
            trn = Matrix.Translation(point + translation);
            man.Reset();
            man.SetPosition(point);
            man.MoveForward(time.Object, velocity);
            ValidateMove(point + translation, trn, man);

            direction = Matrix.Identity.Backward;
            translation = Vector3.Normalize(direction) * velocity * timeSeconds;
            trn = Matrix.Translation(point + translation);
            man.Reset();
            man.SetPosition(point);
            man.MoveBackward(time.Object, velocity);
            ValidateMove(point + translation, trn, man);

            direction = Matrix.Identity.Left;
            translation = Vector3.Normalize(direction) * velocity * timeSeconds;
            trn = Matrix.Translation(point + translation);
            man.Reset();
            man.SetPosition(point);
            man.MoveLeft(time.Object, -velocity);
            ValidateMove(point + translation, trn, man);

            direction = Matrix.Identity.Right;
            translation = Vector3.Normalize(direction) * velocity * timeSeconds;
            trn = Matrix.Translation(point + translation);
            man.Reset();
            man.SetPosition(point);
            man.MoveRight(time.Object, -velocity);
            ValidateMove(point + translation, trn, man);

            direction = Matrix.Identity.Up;
            translation = Vector3.Normalize(direction) * velocity * timeSeconds;
            trn = Matrix.Translation(point + translation);
            man.Reset();
            man.SetPosition(point);
            man.MoveUp(time.Object, velocity);
            ValidateMove(point + translation, trn, man);

            direction = Matrix.Identity.Down;
            translation = Vector3.Normalize(direction) * velocity * timeSeconds;
            trn = Matrix.Translation(point + translation);
            man.Reset();
            man.SetPosition(point);
            man.MoveDown(time.Object, velocity);
            ValidateMove(point + translation, trn, man);

            trn = Matrix.Translation(point);
            man.Reset();
            man.SetPosition(point);
            man.MoveForward(time.Object, velocity);
            man.MoveBackward(time.Object, velocity);
            man.MoveLeft(time.Object, velocity);
            man.MoveRight(time.Object, velocity);
            man.MoveUp(time.Object, velocity);
            man.MoveDown(time.Object, velocity);
            ValidateMove(point, trn, man);
        }
        private static void ValidateMove(Vector3 point, Matrix trn, Manipulator3D man)
        {
            Assert.AreEqual(Vector3.ForwardLH, man.Forward, "Forward direction vector error");
            Assert.AreEqual(Vector3.BackwardLH, man.Backward, "Backward direction vector error");
            Assert.AreEqual(Vector3.Left, man.Left, "Left direction vector error");
            Assert.AreEqual(Vector3.Right, man.Right, "Right direction vector error");
            Assert.AreEqual(Vector3.Up, man.Up, "Up direction vector error");
            Assert.AreEqual(Vector3.Down, man.Down, "Down direction vector error");

            Assert.AreEqual(point, man.Position, "Position point error");
            Assert.AreEqual(Quaternion.Identity, man.Rotation, "Rotation quaternion error");
            Assert.AreEqual(Vector3.One, man.Scaling, "Scaling error");

            Assert.AreEqual(trn, man.LocalTransform, "LocalTransform error");
            Assert.AreEqual(trn, man.GlobalTransform, "GlobalTransform error");
        }

        [TestMethod()]
        public void RotateTest()
        {
            float timeSeconds = 1f / 60f;

            Manipulator3D man = new();

            //Time == zero
            Mock<IGameTime> time = new();
            time.SetupAllProperties();
            man.Reset();
            man.Rotate(time.Object, MathUtil.PiOverTwo, 0, 0);
            ValidateRotate(Vector3.Zero, Quaternion.Identity, Matrix.Identity, man);

            //Rotation == zero
            time = new();
            time.SetupAllProperties();
            time.Setup(x => x.ElapsedSeconds).Returns(() => timeSeconds);
            man.Reset();
            man.Rotate(time.Object, 0, 0, 0);
            ValidateRotate(Vector3.Zero, Quaternion.Identity, Matrix.Identity, man);

            time = new();
            time.SetupAllProperties();
            time.Setup(x => x.ElapsedSeconds).Returns(() => timeSeconds);
            var point = new Vector3(10, 0, 10);
            float yaw = MathUtil.PiOverTwo;
            float pitch = MathUtil.PiOverTwo;
            float roll = MathUtil.PiOverTwo;

            var rot = Quaternion.RotationYawPitchRoll(yaw * timeSeconds, pitch * timeSeconds, roll * timeSeconds);
            var trn = Matrix.RotationQuaternion(rot) * Matrix.Translation(point);
            man.Reset();
            man.SetPosition(point);
            man.Rotate(time.Object, yaw, pitch, roll);
            ValidateRotate(point, rot, trn, man);

            rot = Quaternion.RotationYawPitchRoll(-yaw * timeSeconds, 0, 0);
            trn = Matrix.RotationQuaternion(rot) * Matrix.Translation(point);
            man.Reset();
            man.SetPosition(point);
            man.YawLeft(time.Object, yaw);
            ValidateRotate(point, rot, trn, man);

            rot = Quaternion.RotationYawPitchRoll(yaw * timeSeconds, 0, 0);
            trn = Matrix.RotationQuaternion(rot) * Matrix.Translation(point);
            man.Reset();
            man.SetPosition(point);
            man.YawRight(time.Object, yaw);
            ValidateRotate(point, rot, trn, man);

            rot = Quaternion.RotationYawPitchRoll(0, -pitch * timeSeconds, 0);
            trn = Matrix.RotationQuaternion(rot) * Matrix.Translation(point);
            man.Reset();
            man.SetPosition(point);
            man.PitchDown(time.Object, pitch);
            ValidateRotate(point, rot, trn, man);

            rot = Quaternion.RotationYawPitchRoll(0, pitch * timeSeconds, 0);
            trn = Matrix.RotationQuaternion(rot) * Matrix.Translation(point);
            man.Reset();
            man.SetPosition(point);
            man.PitchUp(time.Object, pitch);
            ValidateRotate(point, rot, trn, man);

            rot = Quaternion.RotationYawPitchRoll(0, 0, -roll * timeSeconds);
            trn = Matrix.RotationQuaternion(rot) * Matrix.Translation(point);
            man.Reset();
            man.SetPosition(point);
            man.RollLeft(time.Object, roll);
            ValidateRotate(point, rot, trn, man);

            rot = Quaternion.RotationYawPitchRoll(0, 0, roll * timeSeconds);
            trn = Matrix.RotationQuaternion(rot) * Matrix.Translation(point);
            man.Reset();
            man.SetPosition(point);
            man.RollRight(time.Object, roll);
            ValidateRotate(point, rot, trn, man);

            rot = Quaternion.Identity;
            trn = Matrix.RotationQuaternion(rot) * Matrix.Translation(point);
            man.Reset();
            man.SetPosition(point);
            man.YawLeft(time.Object, yaw);
            man.YawRight(time.Object, yaw);
            man.PitchDown(time.Object, pitch);
            man.PitchUp(time.Object, pitch);
            man.RollLeft(time.Object, roll);
            man.RollRight(time.Object, roll);
            ValidateRotate(point, rot, trn, man);
        }
        private static void ValidateRotate(Vector3 point, Quaternion rot, Matrix trn, Manipulator3D man)
        {
            Assert.AreEqual(-trn.Forward, man.Forward, "Forward direction vector error");
            Assert.AreEqual(-trn.Backward, man.Backward, "Backward direction vector error");
            Assert.AreEqual(trn.Left, man.Left, "Left direction vector error");
            Assert.AreEqual(trn.Right, man.Right, "Right direction vector error");
            Assert.AreEqual(trn.Up, man.Up, "Up direction vector error");
            Assert.AreEqual(trn.Down, man.Down, "Down direction vector error");

            Assert.AreEqual(point, man.Position, "Position point error");
            Assert.AreEqual(rot, man.Rotation, "Rotation quaternion error");
            Assert.AreEqual(Vector3.One, man.Scaling, "Scaling error");

            Assert.AreEqual(trn, man.LocalTransform, "LocalTransform error");
            Assert.AreEqual(trn, man.GlobalTransform, "GlobalTransform error");
        }

        [TestMethod()]
        public void ScaleTest()
        {
            float timeSeconds = 1f / 60f;

            Manipulator3D man = new();

            //Time == zero
            Mock<IGameTime> time = new();
            time.SetupAllProperties();
            man.Reset();
            man.Scale(time.Object, 2);
            ValidateScale(Vector3.Zero, Vector3.One, Matrix.Identity, man);

            //Scaling == zero
            time = new();
            time.SetupAllProperties();
            time.Setup(x => x.ElapsedSeconds).Returns(() => timeSeconds);
            man.Reset();
            man.Scale(time.Object, 0);
            ValidateScale(Vector3.Zero, Vector3.One, Matrix.Identity, man);

            //Scaling == zero
            time = new();
            time.SetupAllProperties();
            time.Setup(x => x.ElapsedSeconds).Returns(() => timeSeconds);
            man.Reset();
            man.Scale(time.Object, Vector3.Zero);
            ValidateScale(Vector3.Zero, Vector3.One, Matrix.Identity, man);

            time = new();
            time.SetupAllProperties();
            time.Setup(x => x.ElapsedSeconds).Returns(() => timeSeconds);
            var point = new Vector3(10, 0, 10);
            float scale = 2;
            Vector3 scaling = new(scale);

            var trn = Matrix.Scaling(Vector3.One + scaling * timeSeconds) * Matrix.Translation(point);
            man.Reset();
            man.SetPosition(point);
            man.Scale(time.Object, scaling);
            ValidateScale(point, Vector3.One + scaling * timeSeconds, trn, man);

            trn = Matrix.Scaling(Vector3.One + scaling * timeSeconds) * Matrix.Translation(point);
            man.Reset();
            man.SetPosition(point);
            man.Scale(time.Object, scale);
            ValidateScale(point, Vector3.One + scaling * timeSeconds, trn, man);

            trn = Matrix.Translation(point);
            man.Reset();
            man.SetPosition(point);
            man.Scale(time.Object, 2f);
            man.Scale(time.Object, -2f);
            ValidateScale(point, Vector3.One, trn, man);
        }
        private static void ValidateScale(Vector3 point, Vector3 scaling, Matrix trn, Manipulator3D man)
        {
            Assert.AreEqual(Vector3.Normalize(-trn.Forward), man.Forward, "Forward direction vector error");
            Assert.AreEqual(Vector3.Normalize(-trn.Backward), man.Backward, "Backward direction vector error");
            Assert.AreEqual(Vector3.Normalize(trn.Left), man.Left, "Left direction vector error");
            Assert.AreEqual(Vector3.Normalize(trn.Right), man.Right, "Right direction vector error");
            Assert.AreEqual(Vector3.Normalize(trn.Up), man.Up, "Up direction vector error");
            Assert.AreEqual(Vector3.Normalize(trn.Down), man.Down, "Down direction vector error");

            Assert.AreEqual(point, man.Position, "Position point error");
            Assert.AreEqual(Quaternion.Identity, man.Rotation, "Rotation quaternion error");
            Assert.AreEqual(scaling, man.Scaling, "Scaling error");

            Assert.AreEqual(trn, man.LocalTransform, "LocalTransform error");
            Assert.AreEqual(trn, man.GlobalTransform, "GlobalTransform error");
        }
    }
}

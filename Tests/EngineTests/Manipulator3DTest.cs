using Engine;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace EngineTests
{
    [ExcludeFromCodeCoverage]
    [TestClass()]
    [OSCondition(OperatingSystems.Windows)]
    public class Manipulator3DTest
    {
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void SetupTest()
        {
            Console.WriteLine($"TestContext.TestName='{TestContext.TestName}'");
        }

        [TestMethod()]
        public void ConstructorTest()
        {
            Manipulator3D man = new();

            Assert.AreEqual(Vector3Extensions.ForwardLH, man.Forward, "Forward direction vector error");
            Assert.AreEqual(Vector3Extensions.BackwardLH, man.Backward, "Backward direction vector error");
            Assert.AreEqual(Vector3Extensions.Left, man.Left, "Left direction vector error");
            Assert.AreEqual(Vector3Extensions.Right, man.Right, "Right direction vector error");
            Assert.AreEqual(Vector3Extensions.Up, man.Up, "Up direction vector error");
            Assert.AreEqual(Vector3Extensions.Down, man.Down, "Down direction vector error");

            Assert.AreEqual(Vector3.Zero, man.Position, "Position point error");
            Assert.AreEqual(Quaternion.Identity, man.Rotation, "Rotation quaternion error");
            Assert.AreEqual(Vector3.One, man.Scaling, "Scaling error");

            Assert.AreEqual(Matrix4x4.Identity, man.LocalTransform, "LocalTransform error");
            Assert.AreEqual(Matrix4x4.Identity, man.GlobalTransform, "GlobalTransform error");
        }

        [TestMethod()]
        public void SetPositionTest()
        {
            Manipulator3D man = new();
            var point = Vector3.One;
            var trn = Matrix4x4.CreateTranslation(point);

            man.Reset();
            man.SetPosition(point.X, point.Y, point.Z);
            ValidateSetPosition(point, trn, man);

            man.Reset();
            man.SetPosition(point);
            ValidateSetPosition(point, trn, man);
        }
        private static void ValidateSetPosition(Vector3 point, Matrix4x4 trn, Manipulator3D man)
        {
            Assert.AreEqual(Vector3Extensions.ForwardLH, man.Forward, "Forward direction vector error");
            Assert.AreEqual(Vector3Extensions.BackwardLH, man.Backward, "Backward direction vector error");
            Assert.AreEqual(Vector3Extensions.Left, man.Left, "Left direction vector error");
            Assert.AreEqual(Vector3Extensions.Right, man.Right, "Right direction vector error");
            Assert.AreEqual(Vector3Extensions.Up, man.Up, "Up direction vector error");
            Assert.AreEqual(Vector3Extensions.Down, man.Down, "Down direction vector error");

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
            var angle = Helper.PiOver2;
            var rot = Quaternion.CreateFromYawPitchRoll(angle, 0, 0);
            var trn = Matrix4x4.CreateFromQuaternion(rot);

            man.Reset();
            man.SetRotation(Vector3Extensions.Up, angle);
            ValidateSetRotation(rot, trn, man);

            man.Reset();
            man.SetRotation(angle, 0, 0);
            ValidateSetRotation(rot, trn, man);

            man.Reset();
            man.SetRotation(rot);
            ValidateSetRotation(rot, trn, man);
        }
        private static void ValidateSetRotation(Quaternion rot, Matrix4x4 trn, Manipulator3D man)
        {
            Assert.AreEqual(-trn.GetForward(), man.Forward, "Forward direction vector error");
            Assert.AreEqual(-trn.GetBackward(), man.Backward, "Backward direction vector error");
            Assert.AreEqual(trn.GetLeft(), man.Left, "Left direction vector error");
            Assert.AreEqual(trn.GetRight(), man.Right, "Right direction vector error");
            Assert.AreEqual(trn.GetUp(), man.Up, "Up direction vector error");
            Assert.AreEqual(trn.GetDown(), man.Down, "Down direction vector error");

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
            var trn = Matrix4x4.CreateScale(scaling);

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
        private static void ValidateSetScaling(Vector3 scaling, Matrix4x4 trn, Manipulator3D man)
        {
            Assert.AreEqual(Vector3Extensions.ForwardLH, man.Forward, "Forward direction vector error");
            Assert.AreEqual(Vector3Extensions.BackwardLH, man.Backward, "Backward direction vector error");
            Assert.AreEqual(Vector3Extensions.Left, man.Left, "Left direction vector error");
            Assert.AreEqual(Vector3Extensions.Right, man.Right, "Right direction vector error");
            Assert.AreEqual(Vector3Extensions.Up, man.Up, "Up direction vector error");
            Assert.AreEqual(Vector3Extensions.Down, man.Down, "Down direction vector error");

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
            var angle = Helper.PiOver2;
            float scale = 2f;
            var scaling = new Vector3(scale);
            var rot = Quaternion.CreateFromYawPitchRoll(angle, 0, 0);
            var rotTrn = Matrix4x4.CreateFromQuaternion(rot);
            var trn = Matrix4x4.CreateScale(scaling) * rotTrn * Matrix4x4.CreateTranslation(point);

            man.Reset();
            man.SetTransform(point, Vector3Extensions.Up, angle, scale);
            ValidateSetTransform(point, rot, scaling, trn, man);

            man.Reset();
            man.SetTransform(point, Vector3Extensions.Up, angle, scaling);
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
        private static void ValidateSetTransform(Vector3 point, Quaternion rot, Vector3 scaling, Matrix4x4 trn, Manipulator3D man)
        {
            var rotTrn = Matrix4x4.CreateFromQuaternion(rot);

            Assert.AreEqual(-rotTrn.GetForward(), man.Forward, "Forward direction vector error");
            Assert.AreEqual(-rotTrn.GetBackward(), man.Backward, "Backward direction vector error");
            Assert.AreEqual(rotTrn.GetLeft(), man.Left, "Left direction vector error");
            Assert.AreEqual(rotTrn.GetRight(), man.Right, "Right direction vector error");
            Assert.AreEqual(rotTrn.GetUp(), man.Up, "Up direction vector error");
            Assert.AreEqual(rotTrn.GetDown(), man.Down, "Down direction vector error");

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
            man.LookAt(target, Vector3Extensions.Up);
            ValidateLookAt(Vector3.Zero, dir, man);

            var point = Vector3.One;
            dir = Vector3.Normalize(point - target);

            man.Reset();
            man.SetPosition(point);
            man.LookAt(target);
            ValidateLookAt(point, dir, man);

            man.Reset();
            man.SetPosition(point);
            man.LookAt(target, Vector3Extensions.Up);
            ValidateLookAt(point, dir, man);

            point = Vector3.One * 2f;
            dir = Vector3Extensions.ForwardLH;

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
            dir = Vector3Extensions.ForwardLH;
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
            dir = Vector3Extensions.ForwardLH;
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
            var normal = Vector3Extensions.Up;
            man.Reset();
            man.SetNormal(normal);
            ValidateSetNormal(point, normal, man);

            normal = Vector3.Normalize(Vector3.One);
            man.Reset();
            man.SetNormal(normal);
            ValidateSetNormal(point, normal, man);

            normal = Vector3.Normalize(Vector3Extensions.Left);
            man.Reset();
            man.SetNormal(normal);
            ValidateSetNormal(point, normal, man);

            normal = Vector3.Normalize(Vector3Extensions.Down);
            man.Reset();
            man.SetNormal(normal);
            ValidateSetNormal(point, normal, man);

            point = Vector3.One;
            normal = Vector3Extensions.Up;
            man.Reset();
            man.SetPosition(point);
            man.SetNormal(normal);
            ValidateSetNormal(point, normal, man);

            normal = Vector3.Normalize(Vector3.One);
            man.Reset();
            man.SetPosition(point);
            man.SetNormal(normal);
            ValidateSetNormal(point, normal, man);

            normal = Vector3.Normalize(Vector3Extensions.Left);
            man.Reset();
            man.SetPosition(point);
            man.SetNormal(normal);
            ValidateSetNormal(point, normal, man);

            normal = Vector3.Normalize(Vector3Extensions.Down);
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
            ValidateMove(Vector3.Zero, Matrix4x4.Identity, man);

            //Direction == zero
            time = new();
            time.SetupAllProperties();
            time.Setup(x => x.ElapsedSeconds).Returns(() => timeSeconds);
            man.Reset();
            man.Move(time.Object, Vector3.Zero, 1);
            ValidateMove(Vector3.Zero, Matrix4x4.Identity, man);

            //Velocity == zero
            time = new();
            time.SetupAllProperties();
            time.Setup(x => x.ElapsedSeconds).Returns(() => timeSeconds);
            man.Reset();
            man.Move(time.Object, Vector3.One, 0);
            ValidateMove(Vector3.Zero, Matrix4x4.Identity, man);

            time = new();
            time.SetupAllProperties();
            time.Setup(x => x.ElapsedSeconds).Returns(() => timeSeconds);
            var point = new Vector3(10, 0, 10);
            float velocity = 2f;

            var direction = Vector3.One;
            var translation = Vector3.Normalize(direction) * velocity * timeSeconds;
            var trn = Matrix4x4.CreateTranslation(point + translation);
            man.Reset();
            man.SetPosition(point);
            man.Move(time.Object, direction, velocity);
            ValidateMove(point + translation, trn, man);

            direction = Matrix4x4.Identity.GetForward();
            translation = Vector3.Normalize(direction) * velocity * timeSeconds;
            trn = Matrix4x4.CreateTranslation(point + translation);
            man.Reset();
            man.SetPosition(point);
            man.MoveForward(time.Object, velocity);
            ValidateMove(point + translation, trn, man);

            direction = Matrix4x4.Identity.GetBackward();
            translation = Vector3.Normalize(direction) * velocity * timeSeconds;
            trn = Matrix4x4.CreateTranslation(point + translation);
            man.Reset();
            man.SetPosition(point);
            man.MoveBackward(time.Object, velocity);
            ValidateMove(point + translation, trn, man);

            direction = Matrix4x4.Identity.GetLeft();
            translation = Vector3.Normalize(direction) * velocity * timeSeconds;
            trn = Matrix4x4.CreateTranslation(point + translation);
            man.Reset();
            man.SetPosition(point);
            man.MoveLeft(time.Object, -velocity);
            ValidateMove(point + translation, trn, man);

            direction = Matrix4x4.Identity.GetRight();
            translation = Vector3.Normalize(direction) * velocity * timeSeconds;
            trn = Matrix4x4.CreateTranslation(point + translation);
            man.Reset();
            man.SetPosition(point);
            man.MoveRight(time.Object, -velocity);
            ValidateMove(point + translation, trn, man);

            direction = Matrix4x4.Identity.GetUp();
            translation = Vector3.Normalize(direction) * velocity * timeSeconds;
            trn = Matrix4x4.CreateTranslation(point + translation);
            man.Reset();
            man.SetPosition(point);
            man.MoveUp(time.Object, velocity);
            ValidateMove(point + translation, trn, man);

            direction = Matrix4x4.Identity.GetDown();
            translation = Vector3.Normalize(direction) * velocity * timeSeconds;
            trn = Matrix4x4.CreateTranslation(point + translation);
            man.Reset();
            man.SetPosition(point);
            man.MoveDown(time.Object, velocity);
            ValidateMove(point + translation, trn, man);

            trn = Matrix4x4.CreateTranslation(point);
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
        private static void ValidateMove(Vector3 point, Matrix4x4 trn, Manipulator3D man)
        {
            Assert.AreEqual(Vector3Extensions.ForwardLH, man.Forward, "Forward direction vector error");
            Assert.AreEqual(Vector3Extensions.BackwardLH, man.Backward, "Backward direction vector error");
            Assert.AreEqual(Vector3Extensions.Left, man.Left, "Left direction vector error");
            Assert.AreEqual(Vector3Extensions.Right, man.Right, "Right direction vector error");
            Assert.AreEqual(Vector3Extensions.Up, man.Up, "Up direction vector error");
            Assert.AreEqual(Vector3Extensions.Down, man.Down, "Down direction vector error");

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
            man.Rotate(time.Object, Helper.PiOver2, 0, 0);
            ValidateRotate(Vector3.Zero, Quaternion.Identity, Matrix4x4.Identity, man);

            //Rotation == zero
            time = new();
            time.SetupAllProperties();
            time.Setup(x => x.ElapsedSeconds).Returns(() => timeSeconds);
            man.Reset();
            man.Rotate(time.Object, 0, 0, 0);
            ValidateRotate(Vector3.Zero, Quaternion.Identity, Matrix4x4.Identity, man);

            time = new();
            time.SetupAllProperties();
            time.Setup(x => x.ElapsedSeconds).Returns(() => timeSeconds);
            var point = new Vector3(10, 0, 10);
            float yaw = Helper.PiOver2;
            float pitch = Helper.PiOver2;
            float roll = Helper.PiOver2;

            var rot = Quaternion.CreateFromYawPitchRoll(yaw * timeSeconds, pitch * timeSeconds, roll * timeSeconds);
            var trn = Matrix4x4.CreateFromQuaternion(rot) * Matrix4x4.CreateTranslation(point);
            man.Reset();
            man.SetPosition(point);
            man.Rotate(time.Object, yaw, pitch, roll);
            ValidateRotate(point, rot, trn, man);

            rot = Quaternion.CreateFromYawPitchRoll(-yaw * timeSeconds, 0, 0);
            trn = Matrix4x4.CreateFromQuaternion(rot) * Matrix4x4.CreateTranslation(point);
            man.Reset();
            man.SetPosition(point);
            man.YawLeft(time.Object, yaw);
            ValidateRotate(point, rot, trn, man);

            rot = Quaternion.CreateFromYawPitchRoll(yaw * timeSeconds, 0, 0);
            trn = Matrix4x4.CreateFromQuaternion(rot) * Matrix4x4.CreateTranslation(point);
            man.Reset();
            man.SetPosition(point);
            man.YawRight(time.Object, yaw);
            ValidateRotate(point, rot, trn, man);

            rot = Quaternion.CreateFromYawPitchRoll(0, -pitch * timeSeconds, 0);
            trn = Matrix4x4.CreateFromQuaternion(rot) * Matrix4x4.CreateTranslation(point);
            man.Reset();
            man.SetPosition(point);
            man.PitchDown(time.Object, pitch);
            ValidateRotate(point, rot, trn, man);

            rot = Quaternion.CreateFromYawPitchRoll(0, pitch * timeSeconds, 0);
            trn = Matrix4x4.CreateFromQuaternion(rot) * Matrix4x4.CreateTranslation(point);
            man.Reset();
            man.SetPosition(point);
            man.PitchUp(time.Object, pitch);
            ValidateRotate(point, rot, trn, man);

            rot = Quaternion.CreateFromYawPitchRoll(0, 0, -roll * timeSeconds);
            trn = Matrix4x4.CreateFromQuaternion(rot) * Matrix4x4.CreateTranslation(point);
            man.Reset();
            man.SetPosition(point);
            man.RollLeft(time.Object, roll);
            ValidateRotate(point, rot, trn, man);

            rot = Quaternion.CreateFromYawPitchRoll(0, 0, roll * timeSeconds);
            trn = Matrix4x4.CreateFromQuaternion(rot) * Matrix4x4.CreateTranslation(point);
            man.Reset();
            man.SetPosition(point);
            man.RollRight(time.Object, roll);
            ValidateRotate(point, rot, trn, man);

            rot = Quaternion.Identity;
            trn = Matrix4x4.CreateFromQuaternion(rot) * Matrix4x4.CreateTranslation(point);
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
        private static void ValidateRotate(Vector3 point, Quaternion rot, Matrix4x4 trn, Manipulator3D man)
        {
            Assert.AreEqual(-trn.GetForward(), man.Forward, "Forward direction vector error");
            Assert.AreEqual(-trn.GetBackward(), man.Backward, "Backward direction vector error");
            Assert.AreEqual(trn.GetLeft(), man.Left, "Left direction vector error");
            Assert.AreEqual(trn.GetRight(), man.Right, "Right direction vector error");
            Assert.AreEqual(trn.GetUp(), man.Up, "Up direction vector error");
            Assert.AreEqual(trn.GetDown(), man.Down, "Down direction vector error");

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
            ValidateScale(Vector3.Zero, Vector3.One, Matrix4x4.Identity, man);

            //Scaling == zero
            time = new();
            time.SetupAllProperties();
            time.Setup(x => x.ElapsedSeconds).Returns(() => timeSeconds);
            man.Reset();
            man.Scale(time.Object, 0);
            ValidateScale(Vector3.Zero, Vector3.One, Matrix4x4.Identity, man);

            //Scaling == zero
            time = new();
            time.SetupAllProperties();
            time.Setup(x => x.ElapsedSeconds).Returns(() => timeSeconds);
            man.Reset();
            man.Scale(time.Object, Vector3.Zero);
            ValidateScale(Vector3.Zero, Vector3.One, Matrix4x4.Identity, man);

            time = new();
            time.SetupAllProperties();
            time.Setup(x => x.ElapsedSeconds).Returns(() => timeSeconds);
            var point = new Vector3(10, 0, 10);
            float scale = 2;
            Vector3 scaling = new(scale);

            var trn = Matrix4x4.CreateScale(Vector3.One + scaling * timeSeconds) * Matrix4x4.CreateTranslation(point);
            man.Reset();
            man.SetPosition(point);
            man.Scale(time.Object, scaling);
            ValidateScale(point, Vector3.One + scaling * timeSeconds, trn, man);

            trn = Matrix4x4.CreateScale(Vector3.One + scaling * timeSeconds) * Matrix4x4.CreateTranslation(point);
            man.Reset();
            man.SetPosition(point);
            man.Scale(time.Object, scale);
            ValidateScale(point, Vector3.One + scaling * timeSeconds, trn, man);

            trn = Matrix4x4.CreateTranslation(point);
            man.Reset();
            man.SetPosition(point);
            man.Scale(time.Object, 2f);
            man.Scale(time.Object, -2f);
            ValidateScale(point, Vector3.One, trn, man);
        }
        private static void ValidateScale(Vector3 point, Vector3 scaling, Matrix4x4 trn, Manipulator3D man)
        {
            Assert.AreEqual(Vector3.Normalize(-trn.GetForward()), man.Forward, "Forward direction vector error");
            Assert.AreEqual(Vector3.Normalize(-trn.GetBackward()), man.Backward, "Backward direction vector error");
            Assert.AreEqual(Vector3.Normalize(trn.GetLeft()), man.Left, "Left direction vector error");
            Assert.AreEqual(Vector3.Normalize(trn.GetRight()), man.Right, "Right direction vector error");
            Assert.AreEqual(Vector3.Normalize(trn.GetUp()), man.Up, "Up direction vector error");
            Assert.AreEqual(Vector3.Normalize(trn.GetDown()), man.Down, "Down direction vector error");

            Assert.AreEqual(point, man.Position, "Position point error");
            Assert.AreEqual(Quaternion.Identity, man.Rotation, "Rotation quaternion error");
            Assert.AreEqual(scaling, man.Scaling, "Scaling error");

            Assert.AreEqual(trn, man.LocalTransform, "LocalTransform error");
            Assert.AreEqual(trn, man.GlobalTransform, "GlobalTransform error");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;

namespace Common.Utils
{
    public class Controller
    {
        private Matrix localTransform = Matrix.Identity;
        private Quaternion rotation { get; set; }
        private Vector3 scaling { get; set; }
        private Vector3 position { get; set; }

        public Vector3 Position
        {
            get
            {
                return this.position;
            }
        }
        public Vector3 Forward
        {
            get
            {
                return this.localTransform.Forward;
            }
        }
        public Vector3 Backward
        {
            get
            {
                return this.localTransform.Backward;
            }
        }
        public Vector3 Left
        {
            get
            {
                return this.localTransform.Left;
            }
        }
        public Vector3 Right
        {
            get
            {
                return this.localTransform.Right;
            }
        }
        public Vector3 Up
        {
            get
            {
                return this.localTransform.Up;
            }
        }
        public Vector3 Down
        {
            get
            {
                return this.localTransform.Down;
            }
        }
        public Vector3 Scaling
        {
            get
            {
                return this.scaling;
            }
        }
        public Matrix LocalTransform
        {
            get
            {
                return this.localTransform;
            }
        }
        public Controller Following { get; set; }
        public Matrix FollowingRelative { get; set; }

        public Controller()
        {
            this.position = Vector3.Zero;
            this.rotation = Quaternion.Identity;
            this.scaling = new Vector3(1);
        }

        public void Update()
        {
            if (this.Following != null)
            {
                this.localTransform =
                    Matrix.Scaling(this.scaling) *
                    this.Following.localTransform *
                    this.FollowingRelative;
            }
            else
            {
                this.localTransform =
                    Matrix.Scaling(this.scaling) *
                    Matrix.RotationQuaternion(this.rotation) *
                    Matrix.Translation(this.position);
            }
        }

        public void Move(Vector3 d)
        {
            this.position += d;
        }
        public void MoveForward(float d)
        {
            this.position += this.Forward * -d;
        }
        public void MoveBackward(float d)
        {
            this.position += this.Backward * -d;
        }
        public void MoveLeft(float d)
        {
            this.position += this.Left * d;
        }
        public void MoveRight(float d)
        {
            this.position += this.Right * d;
        }
        public void MoveUp(float d)
        {
            this.position += this.Up * d;
        }
        public void MoveDown(float d)
        {
            this.position += this.Down * d;
        }
        public void Rotate(float yaw, float pitch, float roll)
        {
            this.rotation *= Quaternion.RotationYawPitchRoll(yaw, pitch, roll);
        }
        public void YawLeft(float a)
        {
            this.Rotate(-a, 0, 0);
        }
        public void YawRight(float a)
        {
            this.Rotate(a, 0, 0);
        }
        public void PitchUp(float a)
        {
            this.Rotate(0, -a, 0);
        }
        public void PitchDown(float a)
        {
            this.Rotate(0, a, 0);
        }
        public void RollLeft(float a)
        {
            this.Rotate(0, 0, a);
        }
        public void RollRight(float a)
        {
            this.Rotate(0, 0, -a);
        }
        public void Scale(float scale, Vector3? minSize = null, Vector3? maxSize = null)
        {
            this.Scale(new Vector3(scale), minSize, maxSize);
        }
        public void Scale(float scaleX, float scaleY, float scaleZ, Vector3? minSize = null, Vector3? maxSize = null)
        {
            this.Scale(new Vector3(scaleX, scaleY, scaleZ), minSize, maxSize);
        }
        public void Scale(Vector3 scale, Vector3? minSize = null, Vector3? maxSize = null)
        {
            this.SetScale(this.scaling + scale, minSize, maxSize);
        }

        public void SetPosition(float x, float y, float z)
        {
            this.SetPosition(new Vector3(x, y, z));
        }
        public void SetPosition(Vector3 position)
        {
            this.position = position;
        }
        public void SetRotation(float yaw, float pitch, float roll)
        {
            this.SetRotation(Quaternion.RotationYawPitchRoll(yaw, pitch, roll));
        }
        public void SetRotation(Quaternion rotation)
        {
            this.rotation = rotation;
        }
        public void SetScale(float scale, Vector3? minSize = null, Vector3? maxSize = null)
        {
            this.SetScale(new Vector3(scale), minSize, maxSize);
        }
        public void SetScale(float scaleX, float scaleY, float scaleZ, Vector3? minSize = null, Vector3? maxSize = null)
        {
            this.SetScale(new Vector3(scaleX, scaleY, scaleZ), minSize, maxSize);
        }
        public void SetScale(Vector3 scale, Vector3? minSize = null, Vector3? maxSize = null)
        {
            this.scaling = scale;

            if (maxSize.HasValue)
            {
                if (this.scaling.LengthSquared() > maxSize.Value.LengthSquared()) this.scaling = maxSize.Value;
            }

            if (minSize.HasValue)
            {
                if (this.scaling.LengthSquared() < minSize.Value.LengthSquared()) this.scaling = minSize.Value;
            }
        }
    }
}

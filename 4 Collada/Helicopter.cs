using Engine;
using SharpDX;

namespace Collada
{
    public class Helicopter
    {
        private Vector3 pilotPosition = new Vector3(-0.15f, 1.15f, -1.7f);
        private Vector3 copilotPosition = new Vector3(0.15f, 1.15f, -1.7f);
        private Vector3 leftMachineGunPosition = new Vector3(-0.15f, 1f, 2f);
        private Vector3 rightMachineGunPosition = new Vector3(0.15f, 1f, 2f);

        private Vector3 pilotView = new Vector3(0, 0, -1);
        private Vector3 copilotView = new Vector3(0, 0, -1);
        private Vector3 leftMachineGunView = Vector3.Normalize(new Vector3(-10, -2, -10));
        private Vector3 rightMachineGunView = Vector3.Normalize(new Vector3(10, -2, -10));

        public Vector3 Offset = Vector3.Zero;
        public Vector3 View = Vector3.ForwardLH;

        public Manipulator3D Manipulator = null;

        public Helicopter(Manipulator3D manipulator)
        {
            this.Manipulator = manipulator;

            this.SetPilot();
        }

        public void SetPilot()
        {
            this.Offset = this.pilotPosition;
            this.View = this.pilotView;
        }

        public void SetCopilot()
        {
            this.Offset = this.copilotPosition;
            this.View = this.copilotView;
        }

        public void SetLeftMachineGun()
        {
            this.Offset = this.leftMachineGunPosition;
            this.View = this.leftMachineGunView;
        }

        public void SetRightMachineGun()
        {
            this.Offset = this.rightMachineGunPosition;
            this.View = this.rightMachineGunView;
        }
    }
}

using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace BlenderBTech
{
    public partial class CameraPan
    {
        private readonly PerspectiveCamera Camera;
        private readonly Border ViewportHitBG;
        
        private Point3D OriginalCamPosition;
        private readonly AxisAngleRotation3D MainCamAngle;
        private readonly RotateTransform3D camRotateTransform;
        public Point3D CameraCenter = new Point3D(0, 0, 0);

        public CameraPan(PerspectiveCamera camera, Border border)
        {
            Camera = camera;
            ViewportHitBG = border;

            OriginalCamPosition = Camera.Position;
            camRotateTransform = new RotateTransform3D()
            {
                CenterX = CameraCenter.X,
                CenterY = CameraCenter.Y,
                CenterZ = CameraCenter.Z,
            };
            MainCamAngle = new AxisAngleRotation3D()
            {
                Axis = new Vector3D(1, 0, 0),
                Angle = 0
            };

            camRotateTransform.Rotation = MainCamAngle;
            Camera.Transform = camRotateTransform;

            ViewportHitBG.MouseMove += PanLookAroundViewport_MouseMove;
            ViewportHitBG.MouseDown += MiddleMouseButton_MouseDown;
            ViewportHitBG.MouseWheel += ZoomInOutViewport_MouseScroll;
        }

        Point TemporaryMousePosition;
        Point3D PreviousCameraPosition;

        Quaternion QuatX;
        Quaternion PreviousQuatX;
        Quaternion QuatY;
        Quaternion PreviousQuatY;

        private readonly float PanSpeed = 4f;
        private readonly float LookSensitivity = 100f;
        private readonly float ZoomInOutDistance = 1f;

        public Vector3D LookDirection(PerspectiveCamera camera, Point3D pointToLookAt) // Calculates vector direction between two points (LookAt() method)
        {
            Point3D CameraPosition = camera.Position;

            Vector3D VectorDirection = new Vector3D
                (pointToLookAt.X - CameraPosition.X,
                pointToLookAt.Y - CameraPosition.Y,
                pointToLookAt.Z - CameraPosition.Z);

            return VectorDirection;
        }

        public void PanLookAroundViewport_MouseMove(object sender, MouseEventArgs e) // Panning the viewport using the camera
        {
            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                Point mousePos = e.GetPosition(sender as Border); // Gets the current mouse pos
                Point3D newCamPos = new Point3D(
                    ((-mousePos.X + TemporaryMousePosition.X) / ViewportHitBG.Width * PanSpeed) + PreviousCameraPosition.X,
                    ((mousePos.Y - TemporaryMousePosition.Y) / ViewportHitBG.Height * PanSpeed) + PreviousCameraPosition.Y,
                    Camera.Position.Z); // Calculates the proportional distance to move the camera, 
                                                       // can be increased by changing the variable 'PanSpeed'

                if (Keyboard.IsKeyDown(Key.LeftCtrl)) // Pan viewport
                {
                    Camera.Position = newCamPos;
                }
                else // Look around viewport
                {
                    double RotY = (e.GetPosition(sender as Label).X - TemporaryMousePosition.X) / ViewportHitBG.Width * LookSensitivity; // MousePosX is the Y axis of a rotation
                    double RotX = (e.GetPosition(sender as Label).Y - TemporaryMousePosition.Y) / ViewportHitBG.Height * LookSensitivity; // MousePosY is the X axis of a rotation

                    QuatX = Quaternion.Multiply(new Quaternion(new Vector3D(1, 0, 0), -RotX), PreviousQuatX);
                    QuatY = Quaternion.Multiply(new Quaternion(new Vector3D(0, 1, 0), -RotY), PreviousQuatY);
                    Quaternion QuaternionRotation = Quaternion.Multiply(QuatY, QuatX); // Composite Quaternion between the x rotation and the y rotation
                    camRotateTransform.Rotation = new QuaternionRotation3D(QuaternionRotation); // MainCamera.Transform = RotateTransform3D 'camRotateTransform'
                }
            }
        }

        public void MiddleMouseButton_MouseDown(object sender, MouseEventArgs e) // Declares some constants when mouse button 3 is first held down
        {
            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                TemporaryMousePosition = e.GetPosition(sender as Label);
                PreviousCameraPosition = Camera.Position;
                PreviousQuatX = QuatX;
                PreviousQuatY = QuatY;

                CameraCenter = new Point3D(
                    CameraCenter.X + Camera.Position.X - OriginalCamPosition.X,
                    CameraCenter.Y + Camera.Position.Y - OriginalCamPosition.Y,
                    CameraCenter.Z + Camera.Position.Z - OriginalCamPosition.Z);
            }
        }

        public void ZoomInOutViewport_MouseScroll(object sender, MouseWheelEventArgs e)
        {
            var cam = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault().MainCamera;

            if (e.Delta > 0) // Wheel scrolled forwards - Zoom In
            {
                cam.Position = new Point3D(cam.Position.X, cam.Position.Y, cam.Position.Z - ZoomInOutDistance);
            }
            else // Wheel scrolled forwards - Zoom Out
            {
                cam.Position = new Point3D(cam.Position.X, cam.Position.Y, cam.Position.Z + ZoomInOutDistance);
            }
        }
    }
}
using System.Numerics;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Vulkan;

/* Zoltani Szabolcs
 * 524/2
 * zsim2317
 * */

namespace UNI_AIM
{
    internal class CameraDescriptor
    {
        //Setup the camera's location, directions, and movement speed
        private static Vector3D<float> CameraPosition = new Vector3D<float>(0.0f, 0.0f, 20.0f);
        private static Vector3D<float> CameraFront = new Vector3D<float>(0.0f, 0.0f, -1.0f);
        private static Vector3D<float> CameraUp = Vector3D<float>.UnitY;
        private static Vector3D<float> CameraRight = Vector3D<float>.UnitX;
        private static Vector3D<float> CameraDirection = Vector3D<float>.Zero;
        private static float CameraYaw = -90f;
        private static float CameraPitch = 0f;
        private static float CameraZoom = 45f;
        private static float CameraMoveSpeed = 50.5f;
        //Used to track change in mouse movement to allow for moving of the Camera
        private static Vector2 LastMousePosition;
        private static float CameraSlowerMoveSpeed = 50.5f;
        private static float CameraFasterMoveSpeed = 100.5f;
        
        private static List<int> firingAnimationi;
        private static List<float> bumpUp;
        private static double Time;
        private static double TimeToChange;
        private static bool isThirdPerson;
        private static int fov;
        private static bool showGui;

        public CameraDescriptor()
        {
            isThirdPerson = false;
            showGui = true;
            fov = 80;
            int count = 25;
            firingAnimationi = new List<int>();
            bumpUp = new List<float>();
            for (int i = 0; i < count; i++)
            {
                bumpUp.Add(0.70f * (float)Math.Sin(1.3 * Math.PI * ((float)i / (float)count)));
            }
            Time = 0;
            TimeToChange = 0.0001;
        }

        public void Update(double deltaTime)
        {
            if(firingAnimationi.Count == 0) { return; }
            Time += deltaTime;
            if(Time > TimeToChange)
            {
                for(int i = 0; i < firingAnimationi.Count; i++)
                {
                    CameraPitch += bumpUp[firingAnimationi[i]++];
                    if (firingAnimationi[i] > bumpUp.Count - 1)
                    {
                        firingAnimationi.RemoveAt(i);
                        i--;
                    }
                }
                Time = 0;
            }
            CalculateCameraAngles();
        }

        public bool isShowGUI() {  return showGui; }
        public void setShowGui() { showGui = !showGui; }

        public int GetFieldOfViewValue() { return fov; }

        public Matrix4X4<float> GetFieldOfView()
        {
            return Matrix4X4.CreatePerspectiveFieldOfView<float>(((float)Math.PI / 180) * fov, 1024f / 768f, 0.1f, 5000);
        }

        public void SetDefaultAngle()
        {
            CameraYaw = -90f;
            CameraPitch = 0f;
            CameraZoom = 45f;
            CameraPosition = new Vector3D<float>(0.0f, 0.0f, 20.0f);
            CameraFront = new Vector3D<float>(0.0f, 0.0f, -1.0f);
            CameraUp = Vector3D<float>.UnitY;
            CameraRight = Vector3D<float>.UnitX;
            CameraDirection = Vector3D<float>.Zero;
        }

        public void Bump()
        {
            firingAnimationi.Add(0);
        }

        public void MoreFov()
        {
            fov++;
        }

        public void LessFov()
        {
            fov--;
        }

        private static void CalculateCameraAngles()
        {
            //We don't want to be able to look behind us by going over our head or under our feet so make sure it stays within these bounds
            CameraPitch = Math.Clamp(CameraPitch, -89.0f, 89.0f);

            CameraDirection.X = MathF.Cos(DegreesToRadians(CameraYaw)) * MathF.Cos(DegreesToRadians(CameraPitch));
            CameraDirection.Y = MathF.Sin(DegreesToRadians(CameraPitch));
            CameraDirection.Z = MathF.Sin(DegreesToRadians(CameraYaw)) * MathF.Cos(DegreesToRadians(CameraPitch));
            CameraFront = Vector3D.Normalize(CameraDirection);
            CameraRight = Vector3D.Normalize(Vector3D.Cross(CameraFront, CameraUp));
        }

        private static float DegreesToRadians(float degrees)
        {
            return MathF.PI / 180f * degrees;
        }
        public unsafe void LookAtMouse(IMouse mouse, Vector2 position)
        {
            var lookSensitivity = 0.1f;
            if (LastMousePosition == default) { LastMousePosition = position; }
            else
            {
                var xOffset = (position.X - LastMousePosition.X) * lookSensitivity;
                var yOffset = (position.Y - LastMousePosition.Y) * lookSensitivity;
                LastMousePosition = position;

                CameraYaw += xOffset;
                CameraPitch -= yOffset;
                //Console.WriteLine(CameraYaw + " " + CameraPitch);
                CalculateCameraAngles();
            }
        }
        public unsafe void ZoomMouseWheel(IMouse mouse, ScrollWheel scrollWheel)
        {
            //We don't want to be able to zoom in too close or too far away so clamp to these values
            CameraZoom = Math.Clamp(CameraZoom - scrollWheel.Y, 1.0f, 45f);
        }
        public Matrix4X4<float> getView()
        {
            if(isThirdPerson == true)
            {
                Vector3D<float> pos = new Vector3D<float>();
                pos.X = CameraPosition.X - 2.5f;
                pos.Y = CameraPosition.Y + 1.5f;
                pos.Z = CameraPosition.Z + 2f;
                Vector3D<float> front = CameraFront;
                Vector3D<float> up = CameraUp;
                return Matrix4X4.CreateLookAt<float>(pos, CameraPosition, CameraUp);
            }
            return Matrix4X4.CreateLookAt<float>(CameraPosition, CameraPosition + CameraFront, CameraUp);
        }
        public Matrix4X4<float> getProjection(Vector2D<int> size)
        {
            return Matrix4X4.CreatePerspectiveFieldOfView(DegreesToRadians(CameraZoom), (float)size.X / size.Y, 0.1f, 100.0f);
        }

        public Vector3D<float> Position { get { return CameraPosition; } }
        public Vector3D<float> Front { get { return CameraFront; } }
        public Vector3D<float> Up { get { return CameraUp; } }
        public Vector3D<float> Right { get { return CameraRight; } }

        public void ThirdPerson()
        {
            isThirdPerson = !isThirdPerson;
        }

        public float getCameraYaw()
        {
            return CameraYaw;
        }
        public float getCameraPitch()
        {
            return CameraPitch;
        }

        public void setCameraPitch(float newCameraPitch)
        {
            CameraPitch = newCameraPitch;
        }
        public void MoveUp(float moveSpeed)
        {
            CameraPosition += CameraMoveSpeed * moveSpeed * CameraUp;
        }

        public void MoveDown(float moveSpeed)
        {
            CameraPosition -= CameraMoveSpeed * moveSpeed * CameraUp;
        }

        public void MoveRight(float moveSpeed)
        {
            CameraPosition += CameraMoveSpeed * Vector3D.Normalize(Vector3D.Cross(CameraFront, CameraUp)) * moveSpeed;
        }

        public void MoveLeft(float moveSpeed)
        {
            CameraPosition -= CameraMoveSpeed * Vector3D.Normalize(Vector3D.Cross(CameraFront, CameraUp)) * moveSpeed;
        }

        public void MoveForward(float moveSpeed)
        {
            CameraPosition += CameraMoveSpeed * moveSpeed * CameraFront;
        }

        public void MoveBackward(float moveSpeed)
        {
            CameraPosition -= CameraMoveSpeed * moveSpeed * CameraFront;
        }

        public void MoveFaster()
        {
            CameraMoveSpeed = CameraFasterMoveSpeed;
        }

        public void MoveSlower()
        {
            CameraMoveSpeed = CameraSlowerMoveSpeed;
        }
    }
}

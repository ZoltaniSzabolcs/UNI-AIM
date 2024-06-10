using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.SDL;
using Silk.NET.Vulkan;

namespace UNI_AIM
{
    internal class GlObjectTarget : GlObject
    {
        private readonly Vector3D<float> PlayerPosition;
        private Vector3D<float> Position;
        private Vector3D<float> Velocity;
        private int iMovement;
        private readonly List<Vector3D<float>> Movement;
        private double deltaTime;
        private readonly double timeToChange;
        public GlObjectTarget(uint vao, uint vertices, uint colors, uint indeces, uint indexArrayLength,
            GL gl, Vector3D<float> Position, List<Vector3D<float>> Movement, Matrix4X4<float> Scale,
            double deltaTime, double timeToChange, uint texture = 0)
            : base(vao, vertices, colors, indeces, indexArrayLength, gl, texture)
        {
            this.Position = Position;
            this.Scale = Scale;
            this.Movement = Movement;
            this.deltaTime = deltaTime;
            this.timeToChange = timeToChange;
            this.iMovement = 0;
            this.Velocity = Movement[iMovement];
        }

        public GlObjectTarget(GlObject glObject, GL gl, Vector3D<float> Position, List<Vector3D<float>> Movement, Matrix4X4<float> Scale,
             double timeToChange, uint texture = 0)
            : base(glObject.Vao, glObject.Vertices, glObject.Colors, glObject.Indices, glObject.IndexArrayLength, gl, texture)
        {
            this.Position = Position;
            this.Scale = Scale;
            this.Movement = Movement;
            this.timeToChange = timeToChange;
            this.deltaTime = 0;
            this.iMovement = 0;
            this.Velocity = Movement[iMovement];
        }

        public bool Update(double deltaTime)
        {
            //this.deltaTime += deltaTime;
            //Console.Write(this.deltaTime);
            //if(this.deltaTime > timeToChange)
            //{
            //    Velocity = Movement[iMovement];
            //    iMovement = iMovement + 1;
            //    this.deltaTime = 0;
            //}
            //iMovement = iMovement % Movement.Count;
            Position = Position + Velocity;
            Translation = Matrix4X4.CreateTranslation(Position);
            UpdateModelMatrix();
            //if (Vector3D.Distance(PlayerPosition, Position) > 100f)
            //{
            //    Console.WriteLine("Bullet destroyed");
            //    this.ReleaseGlObject();
            //    return true;
            //}
            return false;
        }

        private void UpdateModelMatrix()
        {
            this.ModelMatrix = Scale * Translation;
        }
        private static float DegreesToRadians(float degrees)
        {
            return MathF.PI / 180f * degrees;
        }
        public void FollowCamera(Vector3D<float> targetPosition, Vector3D<float> targetFront, Vector3D<float> targetUp, Vector3D<float> targetRight, float distance = 0.1f)
        {
            float offsetRight = 0.1f;
            float offsetDown = -0.08f;
            Vector3D<float> offsetPosition;
            offsetPosition = targetPosition + targetFront * distance;
            offsetPosition += targetRight * offsetRight;
            offsetPosition += targetUp * offsetDown;

            targetFront = offsetPosition + targetFront * distance;
            this.Translation = Matrix4X4.CreateTranslation(offsetPosition);
            Matrix4X4<float> lookAt;
            Matrix4X4.Invert(Matrix4X4.CreateLookAt(offsetPosition, targetFront, targetUp), out lookAt);


            this.ModelMatrix = RotationMatrix * lookAt * this.Scale * this.Translation;
        }

        public void CrosshairPlacement(Vector3D<float> targetPosition, Vector3D<float> targetFront, Vector3D<float> targetUp, float distance = 20f)
        {
            targetPosition = targetPosition + targetFront * distance;
            this.Translation = Matrix4X4.CreateTranslation(targetPosition);
            
            Matrix4X4<float> lookAt;
            Matrix4X4.Invert(Matrix4X4.CreateLookAt(targetPosition, targetPosition + targetFront, targetUp), out lookAt);

            this.ModelMatrix = lookAt * this.Scale * this.Translation;
        }
    }
}
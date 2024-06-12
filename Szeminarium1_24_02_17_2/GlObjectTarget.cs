using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace UNI_AIM
{
    internal class GlObjectTarget : GlObject
    {
        private Vector3D<float> Position;
        private Vector3D<float> Velocity;
        private int iMovement;
        private readonly List<Vector3D<float>> Movement;
        private double Time;
        private readonly double timeToChange;
        private float hitboxRadius;
        private bool shot;
        private bool dead;
        public GlObjectTarget(uint vao, uint vertices, uint colors, uint indeces, uint indexArrayLength,
            GL gl, Vector3D<float> Position, List<Vector3D<float>> Movement, Matrix4X4<float> Scale,
            double timeToChange, int iMovement, bool dead, float hitboxRadius, uint texture = 0)
            : base(vao, vertices, colors, indeces, indexArrayLength, gl, texture)
        {
            this.Position = Position;
            this.Scale = Scale;
            this.Movement = Movement;
            this.Time = 0;
            this.timeToChange = timeToChange;
            this.iMovement = iMovement;
            this.Velocity = Movement[iMovement];
            this.shot = false;
            this.dead = dead;
            this.hitboxRadius = hitboxRadius;
            this.Translation = Matrix4X4.CreateTranslation(Position);
            this.ModelMatrix = Scale * Translation;
        }

        
        public GlObjectTarget(GlObject glObject, GL gl, Vector3D<float> Position, List<Vector3D<float>> Movement, Matrix4X4<float> Scale,
             double timeToChange, int iMovement, bool dead, float hitboxRadius, uint texture = 0)
            : base(glObject.Vao, glObject.Vertices, glObject.Colors, glObject.Indices, glObject.IndexArrayLength, gl, texture)
        {
            this.Position = Position;
            this.Scale = Scale;
            this.Movement = Movement;
            this.timeToChange = timeToChange;
            this.Time = 0;
            this.iMovement = iMovement;
            this.Velocity = Movement[iMovement];
            this.shot = false;
            this.dead = dead;
            this.hitboxRadius = hitboxRadius;
            this.Translation = Matrix4X4.CreateTranslation(Position);
            this.ModelMatrix = Scale * Translation;
        }

        public GlObjectTarget(GlObject glObject, GL gl, Vector3D<float> Position, Matrix4X4<float> Scale,
            float hitboxRadius, uint texture = 0)
            : base(glObject.Vao, glObject.Vertices, glObject.Colors, glObject.Indices, glObject.IndexArrayLength, gl, texture)
        {
            this.Position = Position;
            this.Scale = Scale;
            this.Movement = new List<Vector3D<float>>();
            this.timeToChange = 0d;
            this.Time = 0;
            this.iMovement = 0;
            this.Velocity = new Vector3D<float>(0f, 0f, 0f);
            this.hitboxRadius = hitboxRadius;
            this.Translation = Matrix4X4.CreateTranslation(Position);
            this.ModelMatrix = Scale * Translation;
        }


        public List<Vector3D<float>> getMovement() { return this.Movement; }

        public bool Update(double deltaTime)
        {
            this.Time += deltaTime;
            if (this.shot) { return false; }
            if (this.dead && this.Time > timeToChange) { return true; }
            if (this.dead) {  return false; }
            if (this.Time > timeToChange)
            {
                //Console.WriteLine(this.Time);
                iMovement = (iMovement + 1) % Movement.Count;
                Velocity = Movement[iMovement];
                this.Time = 0;
            }
            this.Position = this.Position + this.Velocity;
            this.Translation = Matrix4X4.CreateTranslation(Position);
            UpdateModelMatrix();
            return false;
        }

        public void Shot()
        {
            this.shot = true;
        }

        public float getHitboxRadius() {  return this.hitboxRadius; }
        public Vector3D<float> getPosition() { return this.Position; }
        public void setPosition(Vector3D<float> Position) { this.Position = Position; }

        public bool isShot() {  return this.shot; }
        //public bool isDead() { return this.dead; }
        public void UpdateModelMatrix()
        {
            this.ModelMatrix = this.Scale * this.Translation;
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
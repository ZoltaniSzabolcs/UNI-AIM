using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.SDL;
using Silk.NET.Vulkan;

namespace UNI_AIM
{
    internal class GlObjectProjectile : GlObject
    {
        private Vector3D<float> PlayerPosition;
        private Vector3D<float> Position;
        private Vector3D<float> Velocity;
        private bool hit;
        public GlObjectProjectile(uint vao, uint vertices, uint colors, uint indeces, uint indexArrayLength, GL gl, Vector3D<float> Position, Vector3D<float> Velocity, Matrix4X4<float> Scale, uint texture = 0)
            : base(vao, vertices, colors, indeces, indexArrayLength, gl, texture)
        {
            this.Position = Position;
            this.Velocity = Velocity;
            this.Scale = Scale;
            this.hit = false;
        }
        public GlObjectProjectile(GlObject glObject, GL gl, Vector3D<float> Position, Vector3D<float> Velocity, Matrix4X4<float> Scale, uint texture = 0)
            : base(glObject.Vao, glObject.Vertices, glObject.Colors, glObject.Indices, glObject.IndexArrayLength, gl, texture)
        {
            this.Position = Position;
            this.Scale = Scale;
            this.Velocity = Velocity;
            this.hit= false;
        }

        public bool Update()
        {
            Position = Position + Velocity;
            Translation = Matrix4X4.CreateTranslation(Position);
            UpdateModelMatrix();
            if (Vector3D.Distance(PlayerPosition, Position) > 250f)
            {
                this.ReleaseGlObject();
                return true;
            }
            return false;
        }

        private void UpdateModelMatrix()
        {
            this.ModelMatrix = Scale * Translation;
        }

        public bool isHit() { return this.hit; }
        public bool CheckTargetCollision(GlObjectTarget target)
        {
            //Vector3D<float> targetPosition = target.getPosition();
            //targetPosition.X = -targetPosition.X;
            //if(Vector3D.Distance(target.getPosition(), new Vector3D<float>(Position.X, Position.Y, Position.Z)) < target.getHitboxRadius())
            if(Vector3D.Distance(target.getPosition(), Position) < target.getHitboxRadius())
            {
                this.hit = true;
                //Console.WriteLine(targetPosition + " " + Vector3D.Distance(target.getPosition(), this.Position) + " " + this.Position);
                return true;
            }
            //Console.WriteLine(targetPosition + " " + Vector3D.Distance(target.getPosition(), this.Position) + " " + this.Position);

            return false;
        }

        public bool CheckButtonCollision(GlObjectButton button)
        {
            if (Vector3D.Distance(button.getPosition(), Position) < button.getHitboxRadius())
            {
                this.hit = true;
                //Console.WriteLine(targetPosition + " " + Vector3D.Distance(target.getPosition(), this.Position) + " " + this.Position);
                return true;
            }
            //Console.WriteLine(targetPosition + " " + Vector3D.Distance(target.getPosition(), this.Position) + " " + this.Position);

            return false;
        }
    }
}
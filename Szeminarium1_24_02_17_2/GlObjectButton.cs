using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace UNI_AIM
{
    internal class GlObjectButton : GlObject
    {
        private Vector3D<float> Position;
        private float hitboxRadius;
        private string Name;
        public GlObjectButton(uint vao, uint vertices, uint colors, uint indeces, uint indexArrayLength,
            GL gl, Vector3D<float> Position, Matrix4X4<float> Scale, float hitboxRadius, string Name, uint texture = 0)
            : base(vao, vertices, colors, indeces, indexArrayLength, gl, texture)
        {
            this.Position = Position;
            this.Scale = Scale;
            this.hitboxRadius = hitboxRadius;
            this.Translation = Matrix4X4.CreateTranslation(Position);
            this.ModelMatrix = Scale * Translation;
            this.Name = Name;
        }

        public GlObjectButton(GlObject glObject, GL gl, Vector3D<float> Position, Matrix4X4<float> Scale,
             float hitboxRadius, string Name, uint texture = 0)
            : base(glObject.Vao, glObject.Vertices, glObject.Colors, glObject.Indices, glObject.IndexArrayLength, gl, texture)
        {
            this.Position = Position;
            this.Scale = Scale;
            this.hitboxRadius = hitboxRadius;
            this.Translation = Matrix4X4.CreateTranslation(Position);
            this.ModelMatrix = Scale * Translation;
            this.Name = Name;
        }

        public string getName() { return this.Name; }

        public void Update(double deltaTime)
        {
        }
        public float getHitboxRadius() {  return this.hitboxRadius; }
        public Vector3D<float> getPosition() { return this.Position; }
        private void UpdateModelMatrix()
        {
            this.ModelMatrix = this.Scale * this.Translation;
        }
    }
}
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace UNI_AIM
{
    internal class GlObjectWeapon : GlObject
    {
        private static Vector3D<float> WeaponFront;
        public GlObjectWeapon(uint vao, uint vertices, uint colors, uint indeces, uint indexArrayLength, GL gl, uint texture = 0)
            : base(vao, vertices, colors, indeces, indexArrayLength, gl, texture)
        {
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
    }
}
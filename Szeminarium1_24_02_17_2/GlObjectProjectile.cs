﻿using Silk.NET.Maths;
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
        public GlObjectProjectile(uint vao, uint vertices, uint colors, uint indeces, uint indexArrayLength, GL gl, Vector3D<float> Position, Vector3D<float> Velocity, Matrix4X4<float> Scale, uint texture = 0)
            : base(vao, vertices, colors, indeces, indexArrayLength, gl, texture)
        {
            this.Position = Position;
            this.Velocity = Velocity;
            this.Scale = Scale;
        }
        public GlObjectProjectile(GlObject glObject, GL gl, Vector3D<float> Position, Vector3D<float> Velocity, Matrix4X4<float> Scale, uint texture = 0)
            : base(glObject.Vao, glObject.Vertices, glObject.Colors, glObject.Indices, glObject.IndexArrayLength, gl, texture)
        {
            this.Position = Position;
            this.Scale = Scale;
            this.Velocity = Velocity;
        }

        public bool Update()
        {
            Position = Position + Velocity;
            Translation = Matrix4X4.CreateTranslation(Position);
            UpdateModelMatrix();
            if (Vector3D.Distance(PlayerPosition, Position) > 100f)
            {
                Console.WriteLine("Bullet destroyed");
                this.ReleaseGlObject();
                return true;
            }
            return false;
        }

        private void UpdateModelMatrix()
        {
            this.ModelMatrix = Scale * Translation;
        }
    }
}
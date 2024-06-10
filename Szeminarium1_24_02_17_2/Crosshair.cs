using Silk.NET.OpenGL;
using System;

namespace UNI_AIM
{
    public class Crosshair
    {
        private readonly GL gl;
        private readonly uint vao;
        private readonly uint vbo;
        private readonly uint shaderProgram;
        private float width;
        private float thickness;

        public Crosshair(GL gl, uint shaderProgram, float width = 0.05f, float thickness = 0.01f)
        {
            this.gl = gl;
            this.shaderProgram = shaderProgram;
            this.width = width;
            this.thickness = thickness;

            vao = gl.GenVertexArray();
            CheckError();
            vbo = gl.GenBuffer();
            CheckError();
            InitializeVertices();
            CheckError();
        }

        private unsafe void InitializeVertices()
        {
            gl.BindVertexArray(vao);
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
            UpdateVertices();

            // Position attribute
            gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), (void*)0);
            gl.EnableVertexAttribArray(0);
            CheckError();

            // Color attribute
            gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 6 * sizeof(float), (void*)(3 * sizeof(float)));
            gl.EnableVertexAttribArray(1);
            CheckError();

            gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
            gl.BindVertexArray(0);
        }

        public void SetSize(float width, float thickness)
        {
            this.width = width;
            this.thickness = this.thickness;
            UpdateVertices();
        }

        private void UpdateVertices()
        {
            float[] vertices = new float[]
            {
                // Horizontal line
                -width / 2, 0.0f, 0.0f,  1.0f, 1.0f, 1.0f, 1.0f,
                 width / 2, 0.0f, 0.0f,  1.0f, 1.0f, 1.0f, 1.0f,

                // Vertical line
                 0.0f, -thickness / 2, 0.0f,  1.0f, 1.0f, 1.0f, 1.0f,
                 0.0f,  thickness / 2, 0.0f,  1.0f, 1.0f, 1.0f, 1.0f,
            };

            gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
            gl.BufferData(BufferTargetARB.ArrayBuffer, (ReadOnlySpan<float>)vertices.AsSpan(), BufferUsageARB.DynamicDraw);
            CheckError();
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        }

        public void Render()
        {
            gl.UseProgram(shaderProgram);
            gl.BindVertexArray(vao);
            gl.DrawArrays(PrimitiveType.Lines, 0, 4);
            gl.BindVertexArray(0);
            gl.UseProgram(0);
        }

        public void Dispose()
        {
            gl.DeleteVertexArray(vao);
            gl.DeleteBuffer(vbo);
        }

        public void CheckError()
        {
            var error = (ErrorCode)gl.GetError();
            if (error != ErrorCode.NoError)
                throw new Exception("GL.GetError() returned " + error.ToString());
        }
    }
}



//using Silk.NET.OpenGL;
//using System;

//namespace UNI_AIM
//{
//    public class Crosshair
//    {
//        private readonly GL gl;
//        private readonly uint vao;
//        private readonly uint vbo;
//        private readonly uint shaderProgram;
//        private float width;
//        private float thickness;

//        private readonly float[] vertices;

//        public Crosshair(GL gl, uint shaderProgram, uint vao, uint vbo, float width = 0.05f, float thickness = 0.01f)
//        {
//            this.gl = gl;
//            this.shaderProgram = shaderProgram;
//            this.width = width;
//            this.thickness = thickness;
//        }

//        private unsafe void loadVertices()
//        {
//            gl.UseProgram(shaderProgram);
//            uint vao = gl.GenVertexArray();
//            uint vbo = gl.GenBuffer();

//            float[] vertices = new float[]
//            {
//                // Horizontal line
//                -width / 2, 0.0f, 0.0f,  1.0f, 1.0f, 1.0f, 1.0f,
//                 width / 2, 0.0f, 0.0f,  1.0f, 1.0f, 1.0f, 1.0f,

//                // Vertical line
//                 0.0f, -thickness / 2, 0.0f,  1.0f, 1.0f, 1.0f, 1.0f,
//                 0.0f,  thickness / 2, 0.0f,  1.0f, 1.0f, 1.0f, 1.0f,
//            };

//            gl.BindVertexArray(vao);
//            gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
//            //this.gl.BufferData(BufferTargetARB.ArrayBuffer, (System.nuint)(vertices.Length * sizeof(float)), vertices, BufferUsageARB.StaticDraw);
//            gl.BufferData(BufferTargetARB.ArrayBuffer, (ReadOnlySpan<float>)vertices.ToArray().AsSpan(), BufferUsageARB.StaticDraw);

//            // Position attribute
//            gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 7 * sizeof(float), 0);
//            gl.EnableVertexAttribArray(0);

//            // Color attribute
//            gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 7 * sizeof(float), (void*)(3 * sizeof(float)));
//            gl.EnableVertexAttribArray(1);

//            gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
//            gl.BindVertexArray(0);
//        }

//        public static unsafe Crosshair CreateCrosshair(GL gl, uint shaderProgram,  float width = 0.05f, float thickness = 0.01f)
//        {
//            gl.UseProgram(shaderProgram);
//            uint vao = gl.GenVertexArray();
//            uint vbo = gl.GenBuffer();

//            float[] vertices = new float[]
//            {
//                // Horizontal line
//                -width / 2, 0.0f, 0.0f,  1.0f, 1.0f, 1.0f, 1.0f,
//                 width / 2, 0.0f, 0.0f,  1.0f, 1.0f, 1.0f, 1.0f,

//                // Vertical line
//                 0.0f, -thickness / 2, 0.0f,  1.0f, 1.0f, 1.0f, 1.0f,
//                 0.0f,  thickness / 2, 0.0f,  1.0f, 1.0f, 1.0f, 1.0f,
//            };

//            gl.BindVertexArray(vao);
//            gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
//            //this.gl.BufferData(BufferTargetARB.ArrayBuffer, (System.nuint)(vertices.Length * sizeof(float)), vertices, BufferUsageARB.StaticDraw);
//            gl.BufferData(BufferTargetARB.ArrayBuffer, (ReadOnlySpan<float>)vertices.ToArray().AsSpan(), BufferUsageARB.StaticDraw);

//            // Position attribute
//            gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 7 * sizeof(float), 0);
//            gl.EnableVertexAttribArray(0);

//            // Color attribute
//            gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 7 * sizeof(float), (void*)(3 * sizeof(float)));
//            gl.EnableVertexAttribArray(1);

//            gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
//            gl.BindVertexArray(0);
//            return new Crosshair(gl, shaderProgram,  vao, vbo, width, thickness);
//        }

//        public void SetSize(float width, float thickness)
//        {
//            this.width = width;
//            this.thickness = thickness;
//            UpdateVertices();
//        }

//        private void UpdateVertices()
//        {
//            float[] updatedVertices = new float[]
//            {
//                // Horizontal line
//                -width / 2, 0.0f, 0.0f,  1.0f, 1.0f, 1.0f, 1.0f,
//                 width / 2, 0.0f, 0.0f,  1.0f, 1.0f, 1.0f, 1.0f,

//                // Vertical line
//                 0.0f, -thickness / 2, 0.0f,  1.0f, 1.0f, 1.0f, 1.0f,
//                 0.0f,  thickness / 2, 0.0f,  1.0f, 1.0f, 1.0f, 1.0f,
//            };

//            gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
//            //gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(updatedVertices.Length * sizeof(float)), updatedVertices, BufferUsageARB.StaticDraw);
//            gl.BufferData(BufferTargetARB.ArrayBuffer, (ReadOnlySpan<float>)vertices.ToArray().AsSpan(), BufferUsageARB.StaticDraw);
//            gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
//        }

//        public void Render()
//        {
//            gl.UseProgram(shaderProgram);
//            loadVertices();
//            CheckError();
//            //gl.BindVertexArray(vao);
//            gl.DrawArrays(PrimitiveType.Lines, 0, 4);
//            CheckError();
//            gl.BindVertexArray(0);
//            CheckError();
//            gl.UseProgram(0);
//        }

//        public void ReleaseCrosshair()
//        {
//            gl.DeleteVertexArray(vao);
//            gl.DeleteBuffer(vbo);
//        }

//        public void CheckError()
//        {
//            var error = (ErrorCode)gl.GetError();
//            if (error != ErrorCode.NoError)
//                throw new Exception("GL.GetError() returned " + error.ToString());
//        }
//    }
//}

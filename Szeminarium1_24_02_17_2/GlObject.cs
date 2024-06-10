using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.SDL;
using System;

namespace UNI_AIM
{
    internal class GlObject
    {
        public uint? Texture { get; private set; }
        public uint Vao { get; }
        public uint Vertices { get; }
        public uint Colors { get; }
        public uint Indices { get; }
        public uint IndexArrayLength { get; }
        public Matrix4X4<float> Scale;
        public Matrix4X4<float> Translation;
        public Matrix4X4<float> ModelMatrix;
        public Matrix4X4<float> RotationMatrix;
        private uint shaderProgram;

        private GL Gl;
        private float[] vertexData;
        private uint[] indexData;

        public GlObject(uint shaderProgram, uint vao, uint vertices, uint colors, uint indeces, uint indexArrayLength, GL gl, uint texture = 0)
        {
            this.shaderProgram = shaderProgram;
            this.Vao = vao;
            this.Vertices = vertices;
            this.Colors = colors;
            this.Indices = indeces;
            this.IndexArrayLength = indexArrayLength;
            this.Gl = gl;
            this.Texture = texture;
            this.Scale = Matrix4X4.CreateScale(10f);
            this.Translation = Matrix4X4.CreateTranslation(0f, 0f, 0f);
            this.ModelMatrix = Matrix4X4.CreateScale(1f);
        }

        public unsafe void Render(string textureUniformVariableName, string ModelMatrixVariableName, string NormalMatrixVariableName)
        {
            Gl.UseProgram(shaderProgram);
            SetModelMatrix(ModelMatrix, ModelMatrixVariableName, NormalMatrixVariableName);
            Gl.BindVertexArray(Vao);

            // Load vertices before rendering
            LoadVertices();

            if (Texture != 0)
            {
                int textureLocation = Gl.GetUniformLocation(shaderProgram, textureUniformVariableName);
                if (textureLocation == -1)
                {
                    throw new Exception($"{textureUniformVariableName} uniform not found on shader.");
                }

                // Set texture unit 0
                Gl.ActiveTexture(TextureUnit.Texture0);
                Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)GLEnum.Linear);
                Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)GLEnum.Linear);
                Gl.BindTexture(TextureTarget.Texture2D, Texture.Value);
            }

            Gl.DrawElements(PrimitiveType.Triangles, IndexArrayLength, DrawElementsType.UnsignedInt, null);
            Gl.BindVertexArray(0);

            if (Texture != 0)
            {
                Gl.BindTexture(TextureTarget.Texture2D, 0);
            }
        }

        private unsafe void LoadVertices()
        {
            Gl.BindBuffer(BufferTargetARB.ArrayBuffer, Vertices);
            //Gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertexData.Length * sizeof(float)), vertexData, BufferUsageARB.DynamicDraw);
            Gl.BufferData(BufferTargetARB.ArrayBuffer, (ReadOnlySpan<float>)vertexData.ToArray().AsSpan(), BufferUsageARB.StaticDraw);

            Gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, Indices);
            Gl.BufferData(BufferTargetARB.ElementArrayBuffer, (ReadOnlySpan<float>)vertexData.ToArray().AsSpan(), BufferUsageARB.StaticDraw);
            int size = indexData.Length * sizeof(uint);
        }

        private unsafe void SetModelMatrix(Matrix4X4<float> modelMatrix, string ModelMatrixVariableName, string NormalMatrixVariableName)
        {
            Gl.UseProgram(shaderProgram);
            int location = Gl.GetUniformLocation(shaderProgram, ModelMatrixVariableName);
            if (location == -1)
            {
                throw new Exception($"{ModelMatrixVariableName} uniform not found on shader.");
            }

            Gl.UniformMatrix4(location, 1, false, (float*)&modelMatrix);
            CheckError();

            var modelMatrixWithoutTranslation = new Matrix4X4<float>(modelMatrix.Row1, modelMatrix.Row2, modelMatrix.Row3, modelMatrix.Row4);
            modelMatrixWithoutTranslation.M41 = 0;
            modelMatrixWithoutTranslation.M42 = 0;
            modelMatrixWithoutTranslation.M43 = 0;
            modelMatrixWithoutTranslation.M44 = 1;

            Matrix4X4<float> modelInvers;
            Matrix4X4.Invert<float>(modelMatrixWithoutTranslation, out modelInvers);
            Matrix3X3<float> normalMatrix = new Matrix3X3<float>(Matrix4X4.Transpose(modelInvers));
            location = Gl.GetUniformLocation(shaderProgram, NormalMatrixVariableName);
            if (location == -1)
            {
                throw new Exception($"{NormalMatrixVariableName} uniform not found on shader.");
            }
            Gl.UniformMatrix3(location, 1, false, (float*)&normalMatrix);
            CheckError();
        }

        internal void ReleaseGlObject()
        {
            Gl.UseProgram(shaderProgram);
            // Always unbound the vertex buffer first, so no halfway results are displayed by accident
            Gl.DeleteBuffer(Vertices);
            Gl.DeleteBuffer(Colors);
            Gl.DeleteBuffer(Indices);
            Gl.DeleteVertexArray(Vao);
        }

        public void CheckError()
        {
            Gl.UseProgram(shaderProgram);
            var error = (ErrorCode)Gl.GetError();
            if (error != ErrorCode.NoError)
                throw new Exception("GL.GetError() returned " + error.ToString());
        }
    }
}


//using Silk.NET.Maths;
//using Silk.NET.OpenGL;
//using Silk.NET.SDL;

//namespace UNI_AIM
//{
//    internal class GlObject
//    {

//        public uint? Texture { get; private set; }
//        public uint Vao { get; }
//        public uint Vertices { get; }
//        public uint Colors { get; }
//        public uint Indices { get; }
//        public uint IndexArrayLength { get; }
//        public Matrix4X4<float> Scale;
//        public Matrix4X4<float> Translation;
//        public Matrix4X4<float> ModelMatrix;
//        public Matrix4X4<float> RotationMatrix;
//        private uint shaderProgram;

//        private GL Gl;

//        public GlObject(uint shaderProgram, uint vao, uint vertices, uint colors, uint indeces, uint indexArrayLength, GL gl, uint texture = 0)
//        {
//            this.shaderProgram = shaderProgram;
//            this.Vao = vao;
//            this.Vertices = vertices;
//            this.Colors = colors;
//            this.Indices = indeces;
//            this.IndexArrayLength = indexArrayLength;
//            this.Gl = gl;
//            this.Texture = texture;
//            this.Scale = Matrix4X4.CreateScale(10f);
//            this.Translation = Matrix4X4.CreateTranslation(0f, 0f, 0f);
//            this.ModelMatrix = Matrix4X4.CreateScale(1f);
//        }
//        public unsafe void Render(string textureUniformVariableName, string ModelMatrixVariableName, string NormalMatrixVariableName)
//        {
//            Gl.UseProgram(shaderProgram);
//            SetModelMatrix(ModelMatrix, ModelMatrixVariableName, NormalMatrixVariableName);
//            Gl.BindVertexArray(Vao);

//            if (Texture != 0)
//            {
//                int textureLocation = Gl.GetUniformLocation(shaderProgram, textureUniformVariableName);
//                if (textureLocation == -1)
//                {
//                    throw new Exception($"{textureUniformVariableName} uniform not found on shader.");
//                }

//                // Set texture unit 0
//                //Gl.Uniform1(textureLocation, 0);
//                Gl.ActiveTexture(TextureUnit.Texture0);
//                Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)GLEnum.Linear);
//                Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)GLEnum.Linear);
//                Gl.BindTexture(TextureTarget.Texture2D, Texture.Value);
//            }

//            Gl.DrawElements(PrimitiveType.Triangles, IndexArrayLength, DrawElementsType.UnsignedInt, null);
//            Gl.BindVertexArray(0);

//            if (Texture != 0)
//            {
//                Gl.BindTexture(TextureTarget.Texture2D, 0);
//            }
//        }

//        private unsafe void SetModelMatrix(Matrix4X4<float> modelMatrix, string ModelMatrixVariableName, string NormalMatrixVariableName)
//        {
//            Gl.UseProgram(shaderProgram);
//            int location = Gl.GetUniformLocation(shaderProgram, ModelMatrixVariableName);
//            if (location == -1)
//            {
//                throw new Exception($"{ModelMatrixVariableName} uniform not found on shader.");
//            }

//            Gl.UniformMatrix4(location, 1, false, (float*)&modelMatrix);
//            CheckError();

//            var modelMatrixWithoutTranslation = new Matrix4X4<float>(modelMatrix.Row1, modelMatrix.Row2, modelMatrix.Row3, modelMatrix.Row4);
//            modelMatrixWithoutTranslation.M41 = 0;
//            modelMatrixWithoutTranslation.M42 = 0;
//            modelMatrixWithoutTranslation.M43 = 0;
//            modelMatrixWithoutTranslation.M44 = 1;

//            Matrix4X4<float> modelInvers;
//            Matrix4X4.Invert<float>(modelMatrixWithoutTranslation, out modelInvers);
//            Matrix3X3<float> normalMatrix = new Matrix3X3<float>(Matrix4X4.Transpose(modelInvers));
//            location = Gl.GetUniformLocation(shaderProgram, NormalMatrixVariableName);
//            if (location == -1)
//            {
//                throw new Exception($"{NormalMatrixVariableName} uniform not found on shader.");
//            }
//            Gl.UniformMatrix3(location, 1, false, (float*)&normalMatrix);
//            CheckError();
//        }

//        internal void ReleaseGlObject()
//        {
//            Gl.UseProgram(shaderProgram);
//            // always unbound the vertex buffer first, so no halfway results are displayed by accident
//            Gl.DeleteBuffer(Vertices);
//            Gl.DeleteBuffer(Colors);
//            Gl.DeleteBuffer(Indices);
//            Gl.DeleteVertexArray(Vao);
//        }

//        public void CheckError()
//        {
//            Gl.UseProgram(shaderProgram);
//            var error = (ErrorCode)Gl.GetError();
//            if (error != ErrorCode.NoError)
//                throw new Exception("GL.GetError() returned " + error.ToString());
//        }
//    }
//}

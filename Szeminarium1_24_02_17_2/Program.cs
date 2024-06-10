using System.Numerics;
using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;

/* Zoltani Szabolcs
 * 524/2
 * zsim2317
 * */

namespace UNI_AIM
{
    internal static class Program
    {
        //private static CameraDescriptor cameraDescriptor = new();
        private static CameraDescriptor cameraDescriptor = new CameraDescriptor(new Vector3D<float>(0f, 0f, 20f));

        private static CubeArrangementModel cubeArrangementModel = new();

        private static IWindow window;
        private static IInputContext input;
        private static IKeyboard primaryKeyboard;


        private static GL Gl;

        private static uint program;

        private static GlCube skyBox;

        private static GlObject ak47;

        // imgui controller
        private static ImGuiController controller;
        private static IInputContext inputContext;
        private static string inputTextX = "", inputTextY = "", inputTextZ = "";

        // --------------------------------- fenybeallitashoz valtozok --------------------------------------------
        private static Vector3D<float> lightPosition = new Vector3D<float>(3f, 8f, 6f);
        private static float Shininess = 10;
        private static Vector3D<float> AmbientStrength = new Vector3D<float>(0.3f, 0.3f, 0.3f);
        private static Vector3D<float> SpecularStrength = new Vector3D<float>(0.2f, 0.2f, 0.2f);
        private static Vector3D<float> DiffuseStrength = new Vector3D<float>(0.4f, 0.4f, 0.4f);
        private static float ambientRed = 1f;
        private static float ambientGreen = 1f;
        private static float ambientBlue = 1f;

        private const string ModelMatrixVariableName = "uModel";
        private const string NormalMatrixVariableName = "uNormal";
        private const string ViewMatrixVariableName = "uView";
        private const string ProjectionMatrixVariableName = "uProjection";

        private const string TextureUniformVariableName = "uTexture";

        private const string LightColorVariableName = "lightColor";
        private const string LightPositionVariableName = "lightPos";
        private const string ViewPosVariableName = "viewPos";
        private const string ShininessVariableName = "shininess";

        private const string AmbientStrengthVariableName = "ambientStrength";
        private const string SpecularStrengthVariableName = "specularStrength";
        private const string DiffuseStrengthVariableName = "diffuseStrength";

        static void Main(string[] args)
        {
            WindowOptions windowOptions = WindowOptions.Default;
            windowOptions.Title = "UNI-AIM";
            windowOptions.Size = new Vector2D<int>(1200, 1200);

            // on some systems there is no depth buffer by default, so we need to make sure one is created
            windowOptions.PreferredDepthBufferBits = 24;

            window = Window.Create(windowOptions);

            window.Load += Window_Load;
            window.Update += Window_Update;
            window.Render += Window_Render;
            window.Closing += Window_Closing;

            window.Run();

            window.Dispose();
        }

        private static void Window_Load()
        {
            //Console.WriteLine("Load");

            // set up input handling
            inputContext = window.CreateInput();
            foreach (var keyboard in inputContext.Keyboards)
            {
                keyboard.KeyDown += Keyboard_KeyDown;
                primaryKeyboard = keyboard;
            }
            Console.WriteLine($"Keyboars count: {inputContext.Keyboards.Count} \nIf the keyboard is not working try unplugging some");
            if (primaryKeyboard != null)
            {
                primaryKeyboard.KeyDown += Keyboard_KeyDown;
            }

            //for (int i = 0; i < inputContext.Mice.Count; i++)
            //{
            //    inputContext.Mice[i].Cursor.CursorMode = CursorMode.Raw;
            //    inputContext.Mice[i].MouseMove += OnMouseMove;
            //    inputContext.Mice[i].Scroll += OnMouseWheel;
            //}

            Gl = window.CreateOpenGL();
            Gl.ClearColor(System.Drawing.Color.DimGray);

            controller = new ImGuiController(Gl, window, inputContext);

            SetUpObjects();

            LinkProgram();

            //Gl.Enable(EnableCap.CullFace);

            Gl.Enable(EnableCap.DepthTest);
            Gl.DepthFunc(DepthFunction.Lequal);
        }

        private static void LinkProgram()
        {
            uint vshader = Gl.CreateShader(ShaderType.VertexShader);
            uint fshader = Gl.CreateShader(ShaderType.FragmentShader);

            Gl.ShaderSource(vshader, ReadShader("VertexShader.vert"));
            Gl.CompileShader(vshader);
            Gl.GetShader(vshader, ShaderParameterName.CompileStatus, out int vStatus);
            if (vStatus != (int)GLEnum.True)
                throw new Exception("Vertex shader failed to compile: " + Gl.GetShaderInfoLog(vshader));

            Gl.ShaderSource(fshader, ReadShader("FragmentShader.frag"));
            Gl.CompileShader(fshader);

            program = Gl.CreateProgram();
            Gl.AttachShader(program, vshader);
            Gl.AttachShader(program, fshader);
            Gl.LinkProgram(program);
            Gl.GetProgram(program, GLEnum.LinkStatus, out var status);
            if (status == 0)
            {
                Console.WriteLine($"Error linking shader {Gl.GetProgramInfoLog(program)}");
            }
            Gl.DetachShader(program, vshader);
            Gl.DetachShader(program, fshader);
            Gl.DeleteShader(vshader);
            Gl.DeleteShader(fshader);
        }
        private static string ReadShader(string shaderFileName)
        {
            using (Stream shaderStream = typeof(Program).Assembly.GetManifestResourceStream("Szeminarium1_24_02_17_2.Shaders." + shaderFileName))
            using (StreamReader shaderReader = new StreamReader(shaderStream))
                return shaderReader.ReadToEnd();
        }
        private static void Keyboard_KeyDown(IKeyboard keyboard, Key key, int arg3)
        {
            //Console.WriteLine("Key pressed");
            //switch (key)
            //{
            //    case Key.Q:
            //        RotateSide('q');
            //        break;
            //}
        }

        //private static unsafe void OnMouseMove(IMouse mouse, Vector2 position)
        //{
        //    //if (mouse.IsButtonPressed(MouseButton.Right))
        //    //{
        //    //    cameraDescriptor.LookAtMouse(mouse, position);
        //    //}
        //    cameraDescriptor.LookAtMouse(mouse, position);
        //}

        //private static unsafe void OnMouseWheel(IMouse mouse, ScrollWheel scrollWheel)
        //{
        //    cameraDescriptor.ZoomMouseWheel(mouse, scrollWheel);
        //}

        private static void Window_Update(double deltaTime)
        {
            //Console.WriteLine($"Update after {deltaTime} [s].");
            // multithreaded
            // make sure it is threadsafe
            // NO GL calls

            cubeArrangementModel.AdvanceTime(deltaTime);

            //var moveSpeed = 2.5f * (float)deltaTime;

            //if (primaryKeyboard.IsKeyPressed(Key.Keypad5))
            //{
            //    //Move forwards
            //    cameraDescriptor.MoveForward(moveSpeed);

            //}
            //if (primaryKeyboard.IsKeyPressed(Key.Keypad2))
            //{
            //    //Move backwards
            //    cameraDescriptor.MoveBackward(moveSpeed);
            //}
            //if (primaryKeyboard.IsKeyPressed(Key.Keypad1))
            //{
            //    //Move left
            //    cameraDescriptor.MoveLeft(moveSpeed);
            //}
            //if (primaryKeyboard.IsKeyPressed(Key.Keypad3))
            //{
            //    //Move right
            //    cameraDescriptor.MoveRight(moveSpeed);
            //}
            //if (primaryKeyboard.IsKeyPressed(Key.Keypad7))
            //{
            //    //Move up
            //    cameraDescriptor.MoveUp(moveSpeed);
            //}
            //if (primaryKeyboard.IsKeyPressed(Key.Keypad4))
            //{
            //    //Move down
            //    cameraDescriptor.MoveDown(moveSpeed);
            //}
            var keyboard = inputContext.Keyboards[0];

            if (keyboard.IsKeyPressed(Key.Left))
            {
                cameraDescriptor.MoveLeft();
            }
            if (keyboard.IsKeyPressed(Key.Right))
            {
                cameraDescriptor.MoveRight();
            }
            if (keyboard.IsKeyPressed(Key.Down))
            {
                cameraDescriptor.MoveBack();
            }
            if (keyboard.IsKeyPressed(Key.Up))
            {
                cameraDescriptor.MoveFront();
            }
            if (keyboard.IsKeyPressed(Key.Q))
            {
                cameraDescriptor.GoUp();
            }
            if (keyboard.IsKeyPressed(Key.E))
            {
                cameraDescriptor.GoDown();
            }
            if (keyboard.IsKeyPressed(Key.A))
            {
                cameraDescriptor.Yaw -= 1.0f;
            }
            if (keyboard.IsKeyPressed(Key.D))
            {
                cameraDescriptor.Yaw += 1.0f;
            }
            if (keyboard.IsKeyPressed(Key.W))
            {
                cameraDescriptor.Pitch += 0.8f;
                cameraDescriptor.Pitch = Math.Min(cameraDescriptor.Pitch, 89.9f); // korlat hogy ne forduljon at
            }
            if (keyboard.IsKeyPressed(Key.S))
            {
                cameraDescriptor.Pitch -= 0.8f;
                cameraDescriptor.Pitch = Math.Max(cameraDescriptor.Pitch, -89.9f);
            }


            controller.Update((float)deltaTime);
        }

        private static unsafe void Window_Render(double deltaTime)
        {
            //Console.WriteLine($"Render after {deltaTime} [s].");

            // GL here
            Gl.Clear(ClearBufferMask.ColorBufferBit);
            Gl.Clear(ClearBufferMask.DepthBufferBit);

            Gl.UseProgram(program);

            SetViewMatrix();
            SetProjectionMatrix();

            SetLightColor();
            SetLightPosition();
            SetViewerPosition();
            SetShininess();

            //SetAmbientStrength();
            //SetSpecularStrength();
            //SetDiffuseStrength();

            DrawSkyBox();

            DrawGLObjects();

            //ImguiSettings();
            //controller.Render();
        }

        private static unsafe void ImguiSettings()
        {
            // fenybeallitasok vezerlo hozzaadasa beallitasa
            // beallitja hogy egybol latszodjanak ne egy lenyilo ablakocska legyen
            ImGuiNET.ImGui.Begin("Lighting properties", ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar);

            ImGuiNET.ImGui.SliderFloat("Shininess", ref Shininess, 1, 80);
            ImGui.Text("Ambient Light Color");
            ImGuiNET.ImGui.SliderFloat("Red", ref ambientRed, 0, 1);
            ImGuiNET.ImGui.SliderFloat("Green", ref ambientGreen, 0, 1);
            ImGuiNET.ImGui.SliderFloat("Blue", ref ambientBlue, 0, 1);
            // mutassa a kikevert szint es az aranyt hogy mibol mennyi van
            Vector3 color = new Vector3(ambientRed, ambientGreen, ambientBlue);
            ImGui.ColorEdit3("color", ref color);

            float dobozSzelessege = ImGui.CalcTextSize("00000").X; // kiszamitjuk 5db 0 mennyi helyet foglal

            ImGui.Text("Set Light Position");

            // dobozok a feny poziciojanak a beallitasahoz
            ImGui.SetNextItemWidth(dobozSzelessege);
            if (ImGui.InputText("X coord", ref inputTextX, 5))
            { // 100 a bemeneti szoveg meretet jelenti
                lightPosition.X = (inputTextX == "" || inputTextX == "-") ? 0f : float.Parse(inputTextX);  // ha nem ir be semmit a felhasznalo akkor alapertelmezetten legyen 0
                SetLightPosition();                     // inputText == "-" azert kell hogy ha beirok egy "-" jelet es meg nem irtam szamot ne akadjon ki a parsefloat
            }

            ImGui.SameLine(); // ugyanazon a vonalon tartjuk a widgetet
            ImGui.SetNextItemWidth(dobozSzelessege);
            if (ImGui.InputText("Y coord", ref inputTextY, 5))
            { // 100 a bemeneti szoveg meretet jelenti
                lightPosition.Y = (inputTextY == "" || inputTextY == "-") ? 0f : float.Parse(inputTextY);
                SetLightPosition();
            }

            ImGui.SameLine();
            ImGui.SetNextItemWidth(dobozSzelessege);
            if (ImGui.InputText("Z coord", ref inputTextZ, 5))
            { // 100 a bemeneti szoveg meretet jelenti
                lightPosition.Z = (inputTextZ == "" || inputTextZ == "-") ? 0f : float.Parse(inputTextZ);
                SetLightPosition();
            }

            ImGuiNET.ImGui.End();
        }
        private static unsafe void SetLightColor()
        {
            int location = Gl.GetUniformLocation(program, LightColorVariableName);

            if (location == -1)
            {
                throw new Exception($"{LightColorVariableName} uniform not found on shader.");
            }

            Gl.Uniform3(location, ambientRed, ambientGreen, ambientBlue);
            CheckError();
        }

        private static unsafe void SetLightPosition()
        {
            int location = Gl.GetUniformLocation(program, LightPositionVariableName);

            if (location == -1)
            {
                throw new Exception($"{LightPositionVariableName} uniform not found on shader.");
            }

            Gl.Uniform3(location, lightPosition.X, lightPosition.Y, lightPosition.Z);
            CheckError();
        }
        private static unsafe void SetViewerPosition()
        {
            int location = Gl.GetUniformLocation(program, ViewPosVariableName);

            if (location == -1)
            {
                throw new Exception($"{ViewPosVariableName} uniform not found on shader.");
            }

            Gl.Uniform3(location, cameraDescriptor.Position.X, cameraDescriptor.Position.Y, cameraDescriptor.Position.Z);
            CheckError();
        }
        private static unsafe void SetShininess()
        {
            int location = Gl.GetUniformLocation(program, ShininessVariableName);

            if (location == -1)
            {
                throw new Exception($"{ShininessVariableName} uniform not found on shader.");
            }

            Gl.Uniform1(location, Shininess);
            CheckError();
        }

        // ------------------------------------------- Light settings ---------------------------------------------------------
        //private static unsafe void SetAmbientStrength()
        //{
        //    int location = Gl.GetUniformLocation(program, AmbientStrengthVariableName);
        //    if (location == -1)
        //    {
        //        throw new Exception($"{AmbientStrengthVariableName} uniform not found on shader.");
        //    }
        //    Gl.Uniform3(location, AmbientStrength.X, AmbientStrength.Y, AmbientStrength.Z);
        //    CheckError();
        //}

        //private static unsafe void SetSpecularStrength()
        //{
        //    int location = Gl.GetUniformLocation(program, SpecularStrengthVariableName);
        //    if (location == -1)
        //    {
        //        throw new Exception($"{SpecularStrengthVariableName} uniform not found on shader.");
        //    }
        //    Gl.Uniform3(location, SpecularStrength.X, SpecularStrength.Y, SpecularStrength.Z);
        //    CheckError();
        //}

        //private static unsafe void SetDiffuseStrength()
        //{
        //    int location = Gl.GetUniformLocation(program, DiffuseStrengthVariableName);
        //    if (location == -1)
        //    {
        //        throw new Exception($"{DiffuseStrength} uniform not found on shader.");
        //    }
        //    Gl.Uniform3(location, DiffuseStrength.X, DiffuseStrength.Y, DiffuseStrength.Z);
        //    CheckError();
        //}

        private static unsafe void DrawSkyBox()
        {
            Matrix4X4<float> modelMatrix = Matrix4X4.CreateScale(3500f);
            SetModelMatrix(modelMatrix);
            Gl.BindVertexArray(skyBox.Vao);

            int textureLocation = Gl.GetUniformLocation(program, TextureUniformVariableName);
            if (textureLocation == -1)
            {
                throw new Exception($"{TextureUniformVariableName} uniform not found on shader.");
            }
            // set texture 0
            Gl.Uniform1(textureLocation, 0);

            Gl.ActiveTexture(TextureUnit.Texture0);
            Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)GLEnum.Linear);
            Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)GLEnum.Linear);
            Gl.BindTexture(TextureTarget.Texture2D, skyBox.Texture.Value);

            Gl.DrawElements(GLEnum.Triangles, skyBox.IndexArrayLength, GLEnum.UnsignedInt, null);
            Gl.BindVertexArray(0);

            CheckError();
            Gl.BindTexture(TextureTarget.Texture2D, 0);
            CheckError();
        }

        private static unsafe void DrawGLObjects()
        {
            ak47.Render(program, TextureUniformVariableName, ModelMatrixVariableName, NormalMatrixVariableName);
            //pigeon2.Render(program, TextureUniformVariableName, ModelMatrixVariableName, NormalMatrixVariableName);
        }

        private static unsafe void SetModelMatrix(Matrix4X4<float> modelMatrix)
        {
            int location = Gl.GetUniformLocation(program, ModelMatrixVariableName);
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
            location = Gl.GetUniformLocation(program, NormalMatrixVariableName);
            if (location == -1)
            {
                throw new Exception($"{NormalMatrixVariableName} uniform not found on shader.");
            }
            Gl.UniformMatrix3(location, 1, false, (float*)&normalMatrix);
            CheckError();
        }

        private static unsafe void SetUpObjects()
        {
            float[] face1Color = [1f, 0f, 0f, 1.0f];
            float[] face2Color = [0.0f, 1.0f, 0.0f, 1.0f];
            float[] face3Color = [0.0f, 0.0f, 1.0f, 1.0f];
            float[] face4Color = [1.0f, 0.0f, 1.0f, 1.0f];
            float[] face5Color = [0.0f, 1.0f, 1.0f, 1.0f];
            float[] face6Color = [1.0f, 1.0f, 0.0f, 1.0f];

            skyBox = GlCube.CreateInteriorCube(Gl, "");
            //pigeon = ObjectResourceReader.CreateObjectWithTextureFromResource(Gl, "12249_Bird_v1_L2.obj", "12249_Bird_v1_diff.jpg");
            ak47 = ObjectResourceReader.CreateObjectWithTextureFromResource(Gl, "Çè-47.obj", "123456_wire_115115115_color.png");
            ak47.ModelMatrix = Matrix4X4.CreateScale(0.75f);
        }



        private static void Window_Closing()
        {
            // RELEASING EVERYTING
            //foreach (GlCube glCube in glCubes)
            //{
            //    glCube.ReleaseGlCube();
            //}
            skyBox.ReleaseGlObject();
            ak47.ReleaseGlObject();
            //pigeon2.ReleaseGlObject();
        }
        private static unsafe void SetProjectionMatrix()
        {
            var projectionMatrix = Matrix4X4.CreatePerspectiveFieldOfView<float>((float)Math.PI / 2f, 1024f / 768f, 0.1f, 5000);
            int location = Gl.GetUniformLocation(program, ProjectionMatrixVariableName);

            if (location == -1)
            {
                throw new Exception($"{ViewMatrixVariableName} uniform not found on shader.");
            }

            Gl.UniformMatrix4(location, 1, false, (float*)&projectionMatrix);
            CheckError();
        }

        private static unsafe void SetViewMatrix()
        {
            Matrix4X4<float> viewMatrix;
            viewMatrix = Matrix4X4.CreateLookAt(cameraDescriptor.PositionInWorld, cameraDescriptor.TargetInWorld, cameraDescriptor.UpVector);
            int location = Gl.GetUniformLocation(program, ViewMatrixVariableName);

            if (location == -1)
            {
                throw new Exception($"{ViewMatrixVariableName} uniform not found on shader.");
            }

            Gl.UniformMatrix4(location, 1, false, (float*)&viewMatrix);
            CheckError();
        }

        public static void CheckError()
        {
            var error = (ErrorCode)Gl.GetError();
            if (error != ErrorCode.NoError)
                throw new Exception("GL.GetError() returned " + error.ToString());
        }
    }
}
using System.Numerics;
using System.Reflection.PortableExecutable;
using System.Reflection;
using ImGuiNET;
using NAudio.Wave;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;
using Silk.NET.Vulkan;

/* Zoltani Szabolcs
 * 524/2
 * zsim2317
 * */

namespace UNI_AIM
{
    internal static class Program
    {
        private static CameraDescriptor cameraDescriptor = new CameraDescriptor();
        //private static CameraDescriptor cameraDescriptor = new CameraDescriptor(new Vector3D<float>(0f, 0f, 20f));

        private static IWindow window;
        private static IInputContext input;
        private static IKeyboard primaryKeyboard;

        private static WaveOutEvent waveOut;
        private static Mp3FileReader mp3FileReader;
        private static Stream audioStream;

        private static Random random;
        private static GL Gl;

        private static uint program;

        private static GlCube skyBox;

        private static PlayerStatistics playerStatistics;
        private static GlObjectWeapon ak47;
        private static GlObjectWeapon crosshair;
        private static bool weaponHolstered;
        private static bool silentMode;
        private static bool endless;
        private static bool started;
        private static bool ducks;

        private static List<GlObjectButton> buttons;
        private static GlObjectButton resetTargetButton;
        private static GlObjectButton deleteTargetButton;
        private static GlObjectButton endlessTargetButton;
        private static GlObjectButton plusTargetButton;
        private static GlObjectButton minusTargetButton;
        private static GlObjectButton plusFovButton;
        private static GlObjectButton minusFovButton;
        //private static GlObjectButton ducksButton;
        private static int targetCount = 5;

        private static List<GlObjectProjectile> projectiles;

        private static List<GlObjectTarget> targets;
        private static List<Vector3D<float>> targetMoveSet;
        private static List<Vector3D<float>> targetPositionSet;
        private static Vector3D<float> targetSpacing = new Vector3D<float>(75, 75, -50);

        private static float[] RedColor = { 1.0f, 0.0f, 0.0f, 1.0f };
        private static float[] RedColor75 = { 1.0f, 0.23f, 0.23f, 1.0f };
        private static float[] GreenColor = { 0.0f, 1.0f, 0.0f, 1.0f };
        private static float[] GreenColor50 = { 0.46f, 0.94f, 0.46f, 1.0f };
        private static float[] BlueColor = { 0.0f, 0.0f, 1.0f, 1.0f };
        private static float[] BlueColor50 = { 0.23f, 0.65f, 1.0f, 1.0f };
        private static float[] WhiteColor = { 1.0f, 1.0f, 1.0f, 1.0f };
        private static float[] YellowColor = { 1.0f, 1.0f, 0.0f, 1.0f };

        // imgui controller
        private static ImGuiController controller;
        private static IInputContext inputContext;
        private static string inputTextX = "", inputTextY = "", inputTextZ = "";

        // --------------------------------- Light settings --------------------------------------------
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
            windowOptions.Size = new Vector2D<int>(1500, 1500);

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
            playerStatistics = new PlayerStatistics();
            targetPositionSet = new List<Vector3D<float>>();
            //targetMoveSet = new List<Vector3D<float>>();
            endless = false;
            started = false;
            weaponHolstered = false;
            ducks = false;
            random = new Random();
            //Console.WriteLine("Load");

            // set up input handling
            inputContext = window.CreateInput();
            foreach (var keyboard in inputContext.Keyboards)
            {
                //keyboard.KeyDown += Keyboard_KeyDown;
                primaryKeyboard = keyboard;
            }
            Console.WriteLine($"Keyboard count: {inputContext.Keyboards.Count} \nIf the keyboard is not working try unplugging some");
            if (primaryKeyboard != null)
            {
                primaryKeyboard.KeyDown += Keyboard_KeyDown;
                primaryKeyboard.KeyUp += Keyboard_KeyUp;
            }
            Console.WriteLine($"Mice count: {inputContext.Mice.Count} \nIf the Mice is not working try unplugging some");
            for (int i = 0; i < inputContext.Mice.Count; i++)
            {
                inputContext.Mice[i].Cursor.CursorMode = CursorMode.Raw;
                inputContext.Mice[i].MouseMove += OnMouseMove;
                inputContext.Mice[i].MouseDown += OnMouseDown;
                inputContext.Mice[i].Scroll += OnMouseWheel;
            }

            Gl = window.CreateOpenGL();
            Gl.ClearColor(System.Drawing.Color.DimGray);

            controller = new ImGuiController(Gl, window, inputContext);

            SetUpObjects();

            LinkProgram();

            //Gl.Enable(EnableCap.CullFace);

            projectiles = new List<GlObjectProjectile>() { };
            targets = new List<GlObjectTarget>() { };

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
        private static void InitTargetsEndless()
        {
            endless = true;
            targetPositionSet.Clear();
            targetPositionSet = new List<Vector3D<float>>();
            for (int i = 0; i < targetCount; i++)
            {
                targetPositionSet.Add(new Vector3D<float>(
                    (float)random.NextDouble() * targetSpacing.X - targetSpacing.X / 2.0f,
                    (float)random.NextDouble() * targetSpacing.Y - targetSpacing.Y / 2.0f,
                    targetSpacing.Z));
            }

            float hitboxRadius = 6f;
            foreach (var pos in targetPositionSet)
            {
                if (ducks == true)
                {
                    Console.WriteLine("Adding duck");
                    GlObject glObject;
                    glObject = ObjectResourceReader.CreateObjectWithTextureFromResource(Gl, "12249_Bird_v1_L2.obj", "12249_Bird_v1_diff.jpg");
                    glObject.Translation = Matrix4X4.CreateRotationX(((float)Math.PI / 2.0f));
                    targets.Add(new GlObjectTarget(glObject, Gl, pos, Matrix4X4.CreateScale(1.0f), hitboxRadius));
                }
                else
                {
                    //Console.WriteLine("Adding sphere");
                    GlObject glObject;
                    glObject = ObjectResourceReader.CreateObjectFromResource(Gl, "sphere.obj", WhiteColor);
                    targets.Add(new GlObjectTarget(glObject, Gl, pos, Matrix4X4.CreateScale(10f), hitboxRadius));
                }
            }
        }
        private static void InitTargetsRandom()
        {
            targetPositionSet.Clear();
            float speed = 0.1f;
            Vector3D<float> spacing = new Vector3D<float>(75, 75, -50);
            int moveCount = 5;
            float[] moveBitsPlus = new float[moveCount];
            float[] moveBitsMinus = new float[moveCount];
            for (int i = 0; i < moveCount; i++)
            {
                moveBitsPlus[i] = (float)random.NextDouble() * speed + speed;
                moveBitsMinus[i] = -moveBitsPlus[i];
            }
            targetMoveSet = new List<Vector3D<float>>();
            for (int i = 0; i < moveCount; i++)
            {

                targetMoveSet.Add(new Vector3D<float>(
                    moveBitsPlus[i],
                    0f,
                    0f));
                targetMoveSet.Add(new Vector3D<float>(
                    moveBitsMinus[i],
                    0f,
                    0f));
                targetMoveSet.Add(new Vector3D<float>(
                    0f,
                    moveBitsPlus[i],
                    0f));
                targetMoveSet.Add(new Vector3D<float>(
                    0f,
                    moveBitsMinus[i],
                    0f));
            }
            targetMoveSet = targetMoveSet.OrderBy(x => Random.Shared.Next()).ToList();
            targetPositionSet.Clear();
            for (int i = 0; i < targetCount; i++)
            {
                targetPositionSet.Add(new Vector3D<float>(
                    (float)random.NextDouble() * spacing.X - spacing.X / 2.0f,
                    (float)random.NextDouble() * spacing.Y - spacing.Y / 2.0f,
                    spacing.Z));
            }
            float hitboxRadius = 6f;
            foreach (var pos in targetPositionSet)
            {
                if (ducks == true)
                {
                    GlObject glObject;
                    glObject = ObjectResourceReader.CreateObjectWithTextureFromResource(Gl, "12249_Bird_v1_L2.obj", "12249_Bird_v1_diff.jpg");
                    glObject.Translation = Matrix4X4.CreateRotationX(((float)Math.PI / 2.0f));
                    //targets.Add(new GlObjectTarget(glObject, Gl, pos, Matrix4X4.CreateScale(0.01f), hitboxRadius));
                    targets.Add(new GlObjectTarget(glObject, Gl, pos, targetMoveSet, Matrix4X4.CreateScale(0.01f),
                        random.NextDouble() * 2, (int)random.Next(targetMoveSet.Count), false, hitboxRadius));
                }
                else
                {
                    GlObject glObject;
                    glObject = ObjectResourceReader.CreateObjectFromResource(Gl, "sphere.obj", WhiteColor);
                    //targets.Add(new GlObjectTarget(glObject, Gl, pos, Matrix4X4.CreateScale(10f), hitboxRadius));
                    targets.Add(new GlObjectTarget(glObject, Gl, pos, targetMoveSet, Matrix4X4.CreateScale(10f),
                        random.NextDouble() * 2, (int)random.Next(targetMoveSet.Count), false, hitboxRadius));
                }
            }
        }
        private static string ReadShader(string shaderFileName)
        {
            using (Stream shaderStream = typeof(Program).Assembly.GetManifestResourceStream("Szeminarium1_24_02_17_2.Shaders." + shaderFileName))
            using (StreamReader shaderReader = new StreamReader(shaderStream))
                return shaderReader.ReadToEnd();
        }
        private static void Keyboard_KeyDown(IKeyboard keyboard, Key key, int arg3)
        {
            switch (key)
            {
                case Key.ShiftLeft:
                    cameraDescriptor.MoveFaster();
                    break;
                case Key.Q:
                    weaponHolstered = !weaponHolstered;
                    silentMode = weaponHolstered;
                    break;
                case Key.E:
                    silentMode = !silentMode;
                    break;
                case Key.N:
                    cameraDescriptor.SetDefaultAngle();
                    break;
                case Key.T:
                    cameraDescriptor.ThirdPerson();
                    break;
                case Key.G:
                    if(cameraDescriptor.isHelpShown() == false)
                    {
                        cameraDescriptor.setShowGui();
                    }
                    break;
                case Key.H:
                    if(cameraDescriptor.isShowGUI() == false)
                    {
                        cameraDescriptor.setHelpShown();
                    }
                    break;
                case Key.I:
                    cameraDescriptor.MoreFov();
                    break;
                case Key.K:
                    cameraDescriptor.LessFov();
                    break;
                case Key.U:
                    if (started == false)
                    {
                        targetCount++;
                    }
                    break;
                case Key.J:
                    if (started == false && targetCount > 1)
                    {
                        targetCount--;
                    }
                    break;
            }
        }

        private static void Keyboard_KeyUp(IKeyboard keyboard, Key key, int arg3)
        {
            switch (key)
            {
                case Key.ShiftLeft:
                    cameraDescriptor.MoveSlower();
                    break;
            }
        }

        private static unsafe void OnMouseMove(IMouse mouse, Vector2 position)
        {
            //if (mouse.IsButtonPressed(MouseButton.Right))
            //{
            //    cameraDescriptor.LookAtMouse(mouse, position);
            //}
            cameraDescriptor.LookAtMouse(mouse, position);
        }

        private static void OnMouseDown(IMouse mouse, MouseButton button)
        {
            if(weaponHolstered == false)
            {
                cameraDescriptor.Bump();
            }
            if(silentMode == false)
            {
                Thread weaponSoundThread = new Thread(new ParameterizedThreadStart(PlayMp3File));
                weaponSoundThread.Start("Szeminarium1_24_02_17_2.Resources.Sound.ak-47_shot.mp3");
            }
            if (projectiles.Count < 5)
            {
                Vector3D<float> velocity = cameraDescriptor.Front * 5f;
                GlObject glObject = ObjectResourceReader.CreateObjectFromResource(Gl, "sphere.obj", YellowColor);
                projectiles.Add(new GlObjectProjectile(
                    glObject.Vao,
                    glObject.Vertices,
                    glObject.Colors,
                    glObject.Indices,
                    glObject.IndexArrayLength,
                    Gl,
                    cameraDescriptor.Position,
                    velocity,
                    Matrix4X4.CreateScale(0.1f)));
            }
            else
            {
                Console.WriteLine("No more projectile");
            }
        }

        private static unsafe void OnMouseWheel(IMouse mouse, ScrollWheel scrollWheel)
        {
            cameraDescriptor.ZoomMouseWheel(mouse, scrollWheel);
        }

        private static void Window_Update(double deltaTime)
        {
            //Console.WriteLine($"Update after {deltaTime} [s].");
            // multithreaded
            // make sure it is threadsafe
            // NO GL calls

            cameraDescriptor.Update(deltaTime);

            float moveSpeed = (float)deltaTime;

            if (primaryKeyboard.IsKeyPressed(Key.W))
            {
                //Move forwards
                cameraDescriptor.MoveForward(moveSpeed);

            }
            if (primaryKeyboard.IsKeyPressed(Key.S))
            {
                //Move backwards
                cameraDescriptor.MoveBackward(moveSpeed);
            }
            if (primaryKeyboard.IsKeyPressed(Key.A))
            {
                //Move left
                cameraDescriptor.MoveLeft(moveSpeed);
            }
            if (primaryKeyboard.IsKeyPressed(Key.D))
            {
                //Move right
                cameraDescriptor.MoveRight(moveSpeed);
            }
            if (primaryKeyboard.IsKeyPressed(Key.Space))
            {
                //Move up
                cameraDescriptor.MoveUp(moveSpeed);
            }
            if (primaryKeyboard.IsKeyPressed(Key.ControlLeft))
            {
                //Move down
                cameraDescriptor.MoveDown(moveSpeed);
            }
            ak47.FollowCamera(cameraDescriptor.Position, cameraDescriptor.Front, cameraDescriptor.Up, cameraDescriptor.Right);
            crosshair.CrosshairPlacement(cameraDescriptor.Position, cameraDescriptor.Front, cameraDescriptor.Up);

            List<GlObjectProjectile> toRemoveProjectile = new List<GlObjectProjectile>();
            foreach (var projectile in projectiles)
            {
                if (projectile.Update() == true)
                {
                    toRemoveProjectile.Add(projectile);
                }
                foreach (var target in targets)
                {
                    if (projectile.CheckTargetCollision(target) == true)
                    {
                        if(endless == true)
                        {
                            target.setPosition(new Vector3D<float>(
                                (float)random.NextDouble() * targetSpacing.X - targetSpacing.X / 2.0f,
                                (float)random.NextDouble() * targetSpacing.Y - targetSpacing.Y / 2.0f,
                                targetSpacing.Z));
                            target.Translation = Matrix4X4.CreateTranslation(target.getPosition());
                            target.UpdateModelMatrix();
                        }
                        else
                        {
                            target.Shot();
                        }
                    }
                }
                foreach (var button in buttons)
                {
                    if (projectile.CheckButtonCollision(button) == true)
                    {
                        HandleButtonPressed(button.getName());
                    }
                }
            }
            foreach (var projectile in toRemoveProjectile)
            {
                if (projectile.isHit() == true && started == true)
                {
                    playerStatistics.Hit();
                }
                else
                {
                    if(started == true)
                    {
                        playerStatistics.Miss();
                    }
                }
                projectile.ReleaseGlObject();
                projectiles.Remove(projectile);
            }
            toRemoveProjectile.Clear();


            List<GlObjectTarget> toRemove = new List<GlObjectTarget>();
            foreach (var target in targets)
            {
                if (endless == false)
                {
                    if (target.Update(deltaTime) == true)
                    {
                        toRemove.Add(target);
                    }
                }
            }

            foreach (var targetToRemove in toRemove)
            {
                targetToRemove.ReleaseGlObject();
                targets.Remove(targetToRemove);
            }
            toRemove.Clear();

            foreach (var target in targets)
            {
                if (target.isShot() == true)
                {
                    toRemove.Add(target);
                    if (targets.Count == 1)
                    {
                        started = false;
                    }
                }
            }
            foreach (var target in toRemove)
            {

                target.ReleaseGlObject();
                GlObject glObject = ObjectResourceReader.CreateObjectFromResource(Gl, "sphere.obj", RedColor);
                targets.Add(new GlObjectTarget(
                    glObject,
                    Gl,
                    target.getPosition(),
                    target.getMovement(),
                    target.Scale,
                    2,
                    0,
                    true,
                    target.getHitboxRadius()));
                targets.Remove(target);
            }

            controller.Update((float)deltaTime);
        }

        private static void HandleButtonPressed(string buttonName)
        {
            switch (buttonName)
            {
                case "resetButton":
                    if (targets.Count == 0)
                    {
                        playerStatistics.ResetScore();
                        started = true;
                        endless = false;
                        InitTargetsRandom();
                    }
                    break;
                case "deleteButton":
                    started = false;
                    foreach (var target in targets)
                    {
                        target.ReleaseGlObject();
                    }
                    targets.Clear();
                    break;
                case "endlessButton":
                    if(targets.Count == 0)
                    {
                        playerStatistics.ResetScore();
                        started = true;
                        InitTargetsEndless();
                    }
                    break;
                case "plusTargetButton":
                    //Console.WriteLine("Plus target pressed");
                    if (started == false)
                    {
                        //Console.WriteLine("Target added");
                        targetCount++;
                    }
                    break;
                case "minusTargetButton":
                    //Console.WriteLine("Minus target pressed");
                    if (started == false && targetCount > 1)
                    {
                        //Console.WriteLine("Target removed");
                        targetCount--;
                    }
                    break;
                case "plusFovButton":
                    if(started == false)
                    {
                        cameraDescriptor.MoreFov();
                    }
                    break;
                case "minusFovButton":
                    if(started == false)
                    {
                        cameraDescriptor.LessFov();
                    }
                    break;
                case "ducksButton":
                    Console.WriteLine("Ducks pressed");
                    if (started == false)
                    {
                        Console.WriteLine("Ducks changed: " + ducks);
                        ducks = !ducks;
                    }
                    break;
            }
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

            if(cameraDescriptor.isShowGUI() == true)
            {
                ImguiStatistics();
                controller.Render();
            }
            if(cameraDescriptor.isHelpShown() == true)
            {
                ImguiHelp();
                controller.Render();
            }
        }

        private static unsafe void ImguiHelp()
        {
            ImGuiNET.ImGui.Begin("Help", ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar);
            ImGui.Text("On the right there are 7 buttons you can shoot");
            ImGui.Text("The first up down pair changes the number of targets");
            ImGui.Text("The second to pair change the angle of field of view");
            ImGui.Text("The upper blue starts endless aim training");
            ImGui.Text("The bottom blue starts moving target training");
            ImGui.Text("Press WASD to move");
            ImGui.Text("Use your primary mouse to aim");
            ImGui.Text("\tH - toggle help gui");
            ImGui.Text("\tQ - holster weapon");
            ImGui.Text("\tE - make silent weapon");
            ImGui.Text("\tT - third person view");
            ImGui.Text("\tI - increase field of view angle by 1");
            ImGui.Text("\tK - decrease field of view angle by 1");
            ImGui.Text("\tU - increase target count");
            ImGui.Text("\tJ - decrease target count");
            ImGui.Text("\tN - reset player position");

            ImGuiNET.ImGui.End();
        }

        private static unsafe void ImguiStatistics()
        {
            ImGuiNET.ImGui.Begin("Statistics", ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar);
            ImGui.Text("Target count: " + targetCount);
            ImGui.Text("Field of view: " + cameraDescriptor.GetFieldOfViewValue());
            ImGui.Text("Targets alive: " + targets.Count);
            ImGui.Text("Total accuracy: " + playerStatistics.GetTotalAccuracy());
            ImGui.Text("Current accuracy: " + playerStatistics.GetCurrentAccuracy());

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
            Matrix4X4<float> modelMatrix = Matrix4X4.CreateScale(4000f);
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
            if(weaponHolstered == false)
            {
                ak47.Render(program, TextureUniformVariableName, ModelMatrixVariableName, NormalMatrixVariableName);
            }
            crosshair.Render(program, TextureUniformVariableName, ModelMatrixVariableName, NormalMatrixVariableName);
            foreach (var projectile in projectiles)
            {
                projectile.Render(program, TextureUniformVariableName, ModelMatrixVariableName, NormalMatrixVariableName);
            }
            foreach (var target in targets)
            {
                target.Render(program, TextureUniformVariableName, ModelMatrixVariableName, NormalMatrixVariableName);
            }
            foreach (var button in buttons)
            {
                button.Render(program, TextureUniformVariableName, ModelMatrixVariableName, NormalMatrixVariableName);
            }
            CheckError();
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
            skyBox = GlCube.CreateInteriorCube(Gl, "");
            GlObject glObject = ObjectResourceReader.CreateObjectWithTextureFromResource(Gl, "Çè-47.obj", "123456_wire_115115115_color.png");
            
            ak47 = new GlObjectWeapon(glObject.Vao, glObject.Vertices, glObject.Colors, glObject.Indices, glObject.IndexArrayLength, Gl, glObject.Texture.Value);
            ak47.Scale = Matrix4X4.CreateScale(0.001f);
            ak47.RotationMatrix = (Matrix4X4<float>)Matrix4X4.CreateRotationY((float)Math.PI);
            
            glObject = ObjectResourceReader.CreateObjectFromResource(Gl, "golfball_lowpoly.obj", RedColor);
            crosshair = new GlObjectWeapon(glObject.Vao, glObject.Vertices, glObject.Colors, glObject.Indices, glObject.IndexArrayLength, Gl, glObject.Texture.Value);
            crosshair.Scale = Matrix4X4.CreateScale(0.001f);
            crosshair.RotationMatrix = Matrix4X4.CreateRotationZ((float)Math.PI);
            
            glObject = ObjectResourceReader.CreateObjectFromResource(Gl, "sphere.obj", BlueColor50);
            resetTargetButton = new GlObjectButton(glObject, Gl, new Vector3D<float>(5.0f, 0.0f, 20.0f), (Matrix4X4<float>)Matrix4X4.CreateScale(0.8), 0.45f, "resetButton");
            
            glObject = ObjectResourceReader.CreateObjectFromResource(Gl, "sphere.obj", RedColor);
            deleteTargetButton = new GlObjectButton(glObject, Gl, new Vector3D<float>(5.0f, 0.0f, 21.0f), (Matrix4X4<float>)Matrix4X4.CreateScale(0.8), 0.45f, "deleteButton");

            glObject = ObjectResourceReader.CreateObjectFromResource(Gl, "sphere.obj", BlueColor);
            endlessTargetButton = new GlObjectButton(glObject, Gl, new Vector3D<float>(5.0f, 1.0f, 20.0f), (Matrix4X4<float>)Matrix4X4.CreateScale(0.8), 0.45f, "endlessButton"); 
            
            glObject = ObjectResourceReader.CreateObjectFromResource(Gl, "sphere.obj", GreenColor50);
            plusTargetButton = new GlObjectButton(glObject, Gl, new Vector3D<float>(5.0f, 1.0f, 18.0f), (Matrix4X4<float>)Matrix4X4.CreateScale(0.8), 0.7f, "plusTargetButton");
            glObject = ObjectResourceReader.CreateObjectFromResource(Gl, "sphere.obj", RedColor75);
            minusTargetButton = new GlObjectButton(glObject, Gl, new Vector3D<float>(5.0f, 0.0f, 18.0f), (Matrix4X4<float>)Matrix4X4.CreateScale(0.8), 0.45f, "minusTargetButton");

            glObject = ObjectResourceReader.CreateObjectFromResource(Gl, "sphere.obj", GreenColor50);
            plusFovButton = new GlObjectButton(glObject, Gl, new Vector3D<float>(5.0f, 1.0f, 19.0f), (Matrix4X4<float>)Matrix4X4.CreateScale(0.8), 0.45f, "plusFovButton");
            glObject = ObjectResourceReader.CreateObjectFromResource(Gl, "sphere.obj", RedColor75);
            minusFovButton = new GlObjectButton(glObject, Gl, new Vector3D<float>(5.0f, 0.0f, 19.0f), (Matrix4X4<float>)Matrix4X4.CreateScale(0.8), 0.45f, "minusFovButton");

            //glObject = ObjectResourceReader.CreateObjectFromResource(Gl, "sphere.obj", YellowColor);
            //ducksButton = new GlObjectButton(glObject, Gl, new Vector3D<float>(5.0f, -1f, 19.0f), (Matrix4X4<float>)Matrix4X4.CreateScale(0.8), 0.45f, "ducksButton");

            buttons = new List<GlObjectButton> { 
                resetTargetButton,
                deleteTargetButton,
                endlessTargetButton,
                plusTargetButton,
                minusTargetButton,
                plusFovButton,
                minusFovButton,
                //ducksButton,
            };
            CheckError();
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
            crosshair.ReleaseGlObject();
            foreach (var obj in projectiles)
            {
                obj.ReleaseGlObject();
            }
            foreach (var obj in targets)
            {
                obj.ReleaseGlObject();
            }
            foreach (var obj in buttons)
            {
                obj.ReleaseGlObject();
            }
            AudioDispose();
            //pigeon2.ReleaseGlObject();
        }
        private static unsafe void SetProjectionMatrix()
        {
            var projectionMatrix = cameraDescriptor.GetFieldOfView();
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
            Matrix4X4<float> viewMatrix = cameraDescriptor.getView();
            //viewMatrix = Matrix4X4.CreateLookAt(cameraDescriptor.PositionInWorld, cameraDescriptor.TargetInWorld, cameraDescriptor.UpVector);
            int location = Gl.GetUniformLocation(program, ViewMatrixVariableName);

            if (location == -1)
            {
                throw new Exception($"{ViewMatrixVariableName} uniform not found on shader.");
            }

            Gl.UniformMatrix4(location, 1, false, (float*)&viewMatrix);
            CheckError();
        }

        private static void PlayMp3File(object FilePath)
        {
            string resourceName = (string)FilePath;
            Assembly currentAssembly = Assembly.GetExecutingAssembly();

            using (Stream resourceStream = currentAssembly.GetManifestResourceStream(resourceName))
            {
                if (resourceStream == null)
                {
                    Console.WriteLine("Resource not found: " + resourceName);
                    return;
                }

                // Itt használhatod az NAudio könyvtárat a stream lejátszására
                using (var mp3Reader = new Mp3FileReader(resourceStream))
                using (var waveOut = new WaveOutEvent())
                {
                    waveOut.Init(mp3Reader);
                    waveOut.Play();
                    while (waveOut.PlaybackState == PlaybackState.Playing)
                    {
                        System.Threading.Thread.Sleep(1000);
                    }
                }
            }
        }

        private static void AudioManager(string resourceName)
        {
            Assembly currentAssembly = Assembly.GetExecutingAssembly();
            audioStream = currentAssembly.GetManifestResourceStream(resourceName);

            if (audioStream == null)
            {
                Console.WriteLine("Resource not found: " + resourceName);
                return;
            }
            InitializeSoundPlayer();
        }

        private static void InitializeSoundPlayer()
        {
            if (mp3FileReader != null) mp3FileReader.Dispose();
            if (waveOut != null) waveOut.Dispose();

            audioStream.Position = 0; // Reset the stream position to the beginning
            mp3FileReader = new Mp3FileReader(audioStream);
            waveOut = new WaveOutEvent();
            waveOut.Init(mp3FileReader);
        }

        private static void Play()
        {
            if (waveOut != null && waveOut.PlaybackState != PlaybackState.Playing)
            {
                waveOut.Play();
                //Console.WriteLine("Playback started...");
            }
        }

        private static void Stop()
        {
            if (waveOut != null && waveOut.PlaybackState != PlaybackState.Stopped)
            {
                waveOut.Stop();
                //Console.WriteLine("Playback stopped.");
            }
        }

        private static void AudioDispose()
        {
            mp3FileReader?.Dispose();
            waveOut?.Dispose();
            audioStream?.Dispose();
        }

        public static void CheckError()
        {
            var error = (ErrorCode)Gl.GetError();
            if (error != ErrorCode.NoError)
                throw new Exception("GL.GetError() returned " + error.ToString());
        }
    }
}
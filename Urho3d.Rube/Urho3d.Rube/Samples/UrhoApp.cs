using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Urho;
using Urho.Urho2D;

namespace Urho3d.Rube.Samples
{
    public class UrhoApp : Application
    {

        private Scene _scene;
        private Camera _camera;


        [Preserve]
        public UrhoApp(ApplicationOptions options = null) : base(options)
        {
            // mouseDownEventToken = new Action<MouseButtonDownEventArgs>(HandleMouseButtonDown);
            mouseButtonUpToken = new Action<MouseButtonUpEventArgs>(HandleMouseButtonUp);
            mouseMoveEventToken = new Action<MouseMovedEventArgs>(HandleMouseMove);
        }

        static UrhoApp()
        {
            UnhandledException += (s, e) =>
            {
                if (Debugger.IsAttached) Debugger.Break();
                e.Handled = true;
            };                       
        }

        protected override void Start()
        {
            Log.LogMessage += e => Debug.WriteLine($"[{e.Level}] {e.Message}");
            base.Start();

            //if (Platform == Platforms.Android || Platform == Platforms.iOS || Options.TouchEmulation)
            //{
            //    InitTouchInput();
            //}

            Input.Enabled = true;
            Input.SetMouseVisible(true, false);            

#if DEBUG            
            MonoDebugHud = new MonoDebugHud(this);
            MonoDebugHud.Show();
#endif            

            _createScene();            
            _setupViewport();

            
            SubscribeToEvents();
        }


        protected MonoDebugHud MonoDebugHud { get; set; }


        protected override void OnUpdate(float timeStep)
        {
            if (Input.GetMouseButtonDown(MouseButton.Left))
            {
                var a = 5;
            }
        }




        private void _createScene()
        {
            // create scene
            this._scene = new Scene();
            this._scene.CreateComponent<Octree>();
            this._scene.CreateComponent<DebugRenderer>();
            this._createCamera();

            Rube rube = new Rube();
            rube.LoadWorld(this._scene);
        }


        private void _createCamera()
        {
            // Create camera
            Node CameraNode = _scene.CreateChild("MainCamera");
            CameraNode.Position = (new Vector3(0.0f, 0.0f, -0.10f)); 
            this._camera = CameraNode.CreateComponent<Camera>();
            this._camera.Orthographic = true;
            
            var graphics = Graphics;
            // x = Screen Width (px)
            // y = Screen Height(px)
            // s = Desired Height of Photoshop Square(px)
            // Camera Size = x / ((( x / y ) * 2 ) * s ) = 10 sprites de 's'
            // this._camera.OrthoSize = graphics.Width / (((graphics.Width / graphics.Height) * 2) * 32);

            this._camera.OrthoSize = (float)graphics.Height * PixelSize;
            // set zoom with design resolution for view all sample (zomm (1.0) for view in 3:2 1080X720)
            this._camera.Zoom = 1.0f * Math.Min((float)graphics.Width / 1080.0f, (float)graphics.Height / 720.0f);
        }




        private void _setupViewport()
        {
            Viewport viewport = new Viewport(Context, this._scene, this._camera, null);
            var renderer = Renderer;

            renderer.SetViewport(0, viewport);

            // Zone zone = renderer.DefaultZone;
            // zone.FogColor = (new Color(1f, 0.1f, 0.1f)); // Set background color for the scene
        }




        RigidBody2D dummyBody;
        Node pickedNode;
        Action<MouseButtonDownEventArgs> mouseDownEventToken;
        Action<MouseButtonUpEventArgs> mouseButtonUpToken;
        Action<MouseMovedEventArgs> mouseMoveEventToken;
        Subscription mouseDownEventToken2;

        void SubscribeToEvents()
        {
            Engine.PostRenderUpdate += (PostRenderUpdateEventArgs obj) =>
            {
                // If draw debug mode is enabled, draw viewport debug geometry, which will show eg. drawable bounding boxes and skeleton
                // bones. Note that debug geometry has to be separately requested each frame. Disable depth test so that we can see the
                // bones properly
                // if (drawDebug) scene.GetComponent<PhysicsWorld2D>().DrawDebugGeometry();
                this._scene.GetComponent<PhysicsWorld2D>().DrawDebugGeometry();
            };

            // Input.MouseButtonDown += this.mouseDownEventToken;

            // Input.MouseButtonDown += Input_MouseButtonDown;
            this.mouseDownEventToken2 = Input.SubscribeToMouseButtonDown(HandleMouseButtonDown2);
            
            //if (TouchEnabled)
            //{
            //    touchBeginEventToken = Input.SubscribeToTouchBegin(HandleTouchBegin3);
            //}
        }



        void HandleMouseButtonDown2(MouseButtonDownEventArgs args)
        {
            var a = 5;
        }

        private void Input_MouseButtonDown(MouseButtonDownEventArgs obj)
        {
            var a = 5;
        }

        void HandleMouseButtonDown(MouseButtonDownEventArgs args)
        {
            Input input = Input;
            PhysicsWorld2D physicsWorld = this._scene.GetComponent<PhysicsWorld2D>();
            RigidBody2D rigidBody = physicsWorld.GetRigidBody(input.MousePosition.X, input.MousePosition.Y, uint.MaxValue); // Raycast for RigidBody2Ds to pick
            if (rigidBody != null)
            {
                pickedNode = rigidBody.Node;
                //log.Info(pickedNode.name);
                StaticSprite2D staticSprite = pickedNode.GetComponent<StaticSprite2D>();
                staticSprite.Color = (new Color(1.0f, 0.0f, 0.0f, 1.0f)); // Temporary modify color of the picked sprite

                // Create a ConstraintMouse2D - Temporary apply this constraint to the pickedNode to allow grasping and moving with the mouse
                ConstraintMouse2D constraintMouse = pickedNode.CreateComponent<ConstraintMouse2D>();
                constraintMouse.Target = GetMousePositionXY();
                constraintMouse.MaxForce = 1000 * rigidBody.Mass;
                constraintMouse.CollideConnected = true;
                constraintMouse.OtherBody = dummyBody;  // Use dummy body instead of rigidBody. It's better to create a dummy body automatically in ConstraintMouse2D
                constraintMouse.DampingRatio = 0.0f;
            }

            Input.MouseMoved += this.mouseMoveEventToken;
            Input.MouseButtonUp += this.mouseButtonUpToken;

        }


        Vector2 GetMousePositionXY()
        {
            Input input = Input;
            var graphics = Graphics;
            Vector3 screenPoint = new Vector3((float)input.MousePosition.X / graphics.Width, (float)input.MousePosition.Y / graphics.Height, 0.0f);
            Vector3 worldPoint = this._camera.ScreenToWorldPoint(screenPoint);
            return new Vector2(worldPoint.X, worldPoint.Y);
        }

        void HandleMouseMove(MouseMovedEventArgs args)
        {
            if (pickedNode != null)
            {
                ConstraintMouse2D constraintMouse = pickedNode.GetComponent<ConstraintMouse2D>();
                constraintMouse.Target = GetMousePositionXY();
            }

        }

        void HandleMouseButtonUp(MouseButtonUpEventArgs args)
        {
            if (pickedNode != null)
            {
                StaticSprite2D staticSprite = pickedNode.GetComponent<StaticSprite2D>();
                staticSprite.Color = (new Color(1.0f, 1.0f, 1.0f, 1.0f)); // Restore picked sprite color
                pickedNode.RemoveComponent<ConstraintMouse2D>();
                pickedNode = null;
            }

            if (null != this.mouseMoveEventToken) Input.MouseMoved -= mouseMoveEventToken;
            if (null != this.mouseButtonUpToken) Input.MouseButtonUp -= mouseButtonUpToken;
        }

    }
}

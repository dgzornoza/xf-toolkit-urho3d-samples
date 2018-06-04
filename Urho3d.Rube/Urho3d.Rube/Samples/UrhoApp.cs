﻿using System;
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

        protected bool TouchEnabled { get; set; }
        private RigidBody2D _dummyBody;


        [Preserve]
        public UrhoApp(ApplicationOptions options = null) : base(options)
        {
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

            if (Platform == Platforms.Android || Platform == Platforms.iOS || Platform == Platforms.UWP || Options.TouchEmulation)
            {
                TouchEnabled = true;
            }

            Input.Enabled = true;            

#if DEBUG
            MonoDebugHud = new MonoDebugHud(this);
            MonoDebugHud.Show();
#endif            

            this._createScene();
            this._createCamera();
            this._setupViewport();


            SubscribeToEvents();


            Rube rube = new Rube();
            rube.LoadWorld(this._scene);
        }


        protected MonoDebugHud MonoDebugHud { get; set; }


        protected override void OnUpdate(float timeStep)
        {
        }




        private void _createScene()
        {
            // create scene
            this._scene = new Scene();
            this._scene.CreateComponent<Octree>();
            this._scene.CreateComponent<DebugRenderer>();
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






        Node pickedNode;
        bool drawDebug = true;

        void SubscribeToEvents()
        {
            Engine.PostRenderUpdate += (PostRenderUpdateEventArgs obj) =>
            {
                // If draw debug mode is enabled, draw viewport debug geometry, which will show eg. drawable bounding boxes and skeleton
                // bones. Note that debug geometry has to be separately requested each frame. Disable depth test so that we can see the
                // bones properly
                if (drawDebug)
                {
                    this._scene.GetComponent<PhysicsWorld2D>().DrawDebugGeometry();
                    Renderer.DrawDebugGeometry(false);
                }

            };

            if (TouchEnabled)
            {
                Input.TouchBegin += HandleTouchBegin3;

                // dumybody
                Node dummyNode = this._scene.CreateChild("DummyNode");
                this._dummyBody = dummyNode.CreateComponent<RigidBody2D>();
                this._scene.GetComponent<PhysicsWorld2D>().DrawJoint = true;
            }
        }



        void HandleTouchBegin3(TouchBeginEventArgs args)
        {
            var graphics = Graphics;
            PhysicsWorld2D physicsWorld = this._scene.GetComponent<PhysicsWorld2D>();
            RigidBody2D rigidBody = physicsWorld.GetRigidBody(args.X, args.Y, uint.MaxValue); // Raycast for RigidBody2Ds to pick
            if (rigidBody != null)
            {
                pickedNode = rigidBody.Node;
                // StaticSprite2D staticSprite = pickedNode.GetComponent<StaticSprite2D>();
                // staticSprite.Color = (new Color(1.0f, 0.0f, 0.0f, 1.0f)); // Temporary modify color of the picked sprite
                // rigidBody = pickedNode.GetComponent<RigidBody2D>();

                // Create a ConstraintMouse2D - Temporary apply this constraint to the pickedNode to allow grasping and moving with touch
                ConstraintMouse2D constraintMouse = pickedNode.CreateComponent<ConstraintMouse2D>();
                Vector3 pos = this._camera.ScreenToWorldPoint(new Vector3((float)args.X / graphics.Width, (float)args.Y / graphics.Height, 0.0f));
                constraintMouse.Target = new Vector2(pos.X, pos.Y);
                constraintMouse.MaxForce = 1000 * rigidBody.Mass;
                constraintMouse.CollideConnected = true;
                constraintMouse.OtherBody = _dummyBody;  // Use dummy body instead of rigidBody. It's better to create a dummy body automatically in ConstraintMouse2D
                constraintMouse.DampingRatio = 0;
            }

            Input.TouchMove += HandleTouchMove3;
            Input.TouchEnd += HandleTouchEnd3;
        }

        void HandleTouchEnd3(TouchEndEventArgs args)
        {
            if (pickedNode != null)
            {
                // StaticSprite2D staticSprite = pickedNode.GetComponent<StaticSprite2D>();
                // staticSprite.Color = (new Color(1.0f, 1.0f, 1.0f, 1.0f)); // Restore picked sprite color
                pickedNode.RemoveComponent<ConstraintMouse2D>(); // Remove temporary constraint
                pickedNode = null;
            }


            Input.TouchMove -= HandleTouchMove3;
            Input.TouchEnd -= HandleTouchEnd3;
        }

        void HandleTouchMove3(TouchMoveEventArgs args)
        {
            if (pickedNode != null)
            {
                var graphics = Graphics;
                ConstraintMouse2D constraintMouse = pickedNode.GetComponent<ConstraintMouse2D>();
                Vector3 pos = this._camera.ScreenToWorldPoint(new Vector3((float)args.X / graphics.Width, (float)args.Y / graphics.Height, 0.0f));
                constraintMouse.Target = new Vector2(pos.X, pos.Y);
            }
        }


    }
}

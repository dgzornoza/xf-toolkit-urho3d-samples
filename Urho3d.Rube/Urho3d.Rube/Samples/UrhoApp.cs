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
        public UrhoApp(ApplicationOptions options = null) : base(options) { }

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
            base.Start();
            CreateScene2();
            // _createScene();
            _setupViewport();
        }



        protected override void OnUpdate(float timeStep)
        {
            this._scene.GetComponent<PhysicsWorld2D>().DrawDebugGeometry();
        }




        private void _createScene()
        {
            // create scene
            this._scene = new Scene();
            this._scene.CreateComponent<Octree>();
            this._createCamera();

            // Rube rube = new Rube();
            // rube.LoadWorld(this._scene);



            // Create a polygon
            Node polygon = _scene.CreateChild("Polygon");
            polygon.Position = (new Vector3(1.6f, -2.0f, 0.0f));
            polygon.SetScale(0.7f);
            //StaticSprite2D polygonSprite = polygon.CreateComponent<StaticSprite2D>();
            //polygonSprite.Sprite = cache.GetSprite2D("Urho2D/Aster.png");
            RigidBody2D polygonBody = polygon.CreateComponent<RigidBody2D>();
            polygonBody.BodyType = BodyType2D.Dynamic;
            CollisionPolygon2D polygonShape = polygon.CreateComponent<CollisionPolygon2D>();
            polygonShape.VertexCount = 6; // Set number of vertices (mandatory when using SetVertex())
            polygonShape.SetVertex(0, new Vector2(-0.8f, -0.3f));
            polygonShape.SetVertex(1, new Vector2(0.5f, -0.8f));
            polygonShape.SetVertex(2, new Vector2(0.8f, -0.3f));
            polygonShape.SetVertex(3, new Vector2(0.8f, 0.5f));
            polygonShape.SetVertex(4, new Vector2(0.5f, 0.9f));
            polygonShape.SetVertex(5, new Vector2(-0.5f, 0.7f));
            polygonShape.Density = 1.0f; // Set shape density (kilograms per meter squared)
            polygonShape.Friction = 0.3f; // Set friction
            polygonShape.Restitution = 0.0f; // Set restitution (no bounce)

            // Create 4x3 grid
            for (uint i = 0; i < 5; ++i)
            {
                Node edgeNode = _scene.CreateChild("VerticalEdge");
                RigidBody2D edgeBody = edgeNode.CreateComponent<RigidBody2D>();
                CollisionEdge2D edgeShape = edgeNode.CreateComponent<CollisionEdge2D>();
                edgeShape.SetVertices(new Vector2(i * 2.5f - 5.0f, -3.0f), new Vector2(i * 2.5f - 5.0f, 3.0f));
                edgeShape.Friction = 0.5f; // Set friction
            }
        }


        private void _createCamera()
        {
            // Create camera
            Node CameraNode = _scene.CreateChild("MainCamera");
            CameraNode.Position = (new Vector3(0.0f, 0.0f, -10.0f)); 
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
        }




        void CreateScene2()
        {
            _scene = new Scene();
            _scene.CreateComponent<Octree>();
            _scene.CreateComponent<DebugRenderer>();
            // Create camera node
            Node CameraNode = _scene.CreateChild("Camera");
            // Set camera's position
            CameraNode.Position = (new Vector3(0.0f, 0.0f, -10.0f));

            Camera camera = CameraNode.CreateComponent<Camera>();
            camera.Orthographic = true;

            var graphics = Graphics;
            camera.OrthoSize = (float)graphics.Height * PixelSize;
            camera.Zoom = 1.2f * Math.Min((float)graphics.Width / 1280.0f, (float)graphics.Height / 800.0f);
            // Set zoom according to user's resolution to ensure full visibility (initial zoom (1.2) is set for full visibility at 1280x800 resolution)

            // Create 2D physics world component
            PhysicsWorld2D physicsWorld = _scene.CreateComponent<PhysicsWorld2D>();

            var cache = ResourceCache;
            // Sprite2D boxSprite = cache.GetSprite2D("Urho2D/Box.png");
            // Sprite2D ballSprite = cache.GetSprite2D("Urho2D/Ball.png");

            // Create ground.
            Node groundNode = _scene.CreateChild("Ground");
            groundNode.Position = (new Vector3(0.0f, -3.0f, 0.0f));
            groundNode.Scale = new Vector3(200.0f, 1.0f, 0.0f);

            // Create 2D rigid body for gound
            /*RigidBody2D groundBody = */
            groundNode.CreateComponent<RigidBody2D>();

            //StaticSprite2D groundSprite = groundNode.CreateComponent<StaticSprite2D>();
            //groundSprite.Sprite = boxSprite;

            // Create box collider for ground
            CollisionBox2D groundShape = groundNode.CreateComponent<CollisionBox2D>();
            // Set box size
            groundShape.Size = new Vector2(0.32f, 0.32f);
            // Set friction
            groundShape.Friction = 0.5f;

            for (uint i = 0; i < 5; ++i)
            {
                Node node = _scene.CreateChild("RigidBody");
                node.Position = (new Vector3(NextRandom(-0.1f, 0.1f), 5.0f + i * 0.4f, 0.0f));

                // Create rigid body
                RigidBody2D body = node.CreateComponent<RigidBody2D>();
                body.BodyType = BodyType2D.Dynamic;
                //StaticSprite2D staticSprite = node.CreateComponent<StaticSprite2D>();

                if (i % 2 == 0)
                {
                    //staticSprite.Sprite = boxSprite;

                    // Create box
                    CollisionBox2D box = node.CreateComponent<CollisionBox2D>();
                    // Set size
                    box.Size = new Vector2(0.32f, 0.32f);
                    // Set density
                    box.Density = 1.0f;
                    // Set friction
                    box.Friction = 0.5f;
                    // Set restitution
                    box.Restitution = 0.1f;
                }
                else
                {
                    //StaticSprite2D staticSprite = node.CreateComponent<StaticSprite2D>();
                    //staticSprite.Sprite = ballSprite;

                    // Create circle
                    CollisionCircle2D circle = node.CreateComponent<CollisionCircle2D>();
                    // Set radius
                    circle.Radius = 0.16f;
                    // Set density
                    circle.Density = 1.0f;
                    // Set friction.
                    circle.Friction = 0.5f;
                    // Set restitution
                    circle.Restitution = 0.1f;
                }
            }
        }

        public static float NextRandom(float min, float max) { return (float)((new Random().NextDouble() * (max - min)) + min); }

    }
}

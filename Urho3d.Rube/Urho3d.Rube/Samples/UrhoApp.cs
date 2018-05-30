using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Urho;

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
            _createScene();
            _setupViewport();
        }





        private void _createScene()
        {
            // create scene
            this._scene = new Scene();
            this._scene.CreateComponent<Octree>();
            this._createCamera();
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
    }
}

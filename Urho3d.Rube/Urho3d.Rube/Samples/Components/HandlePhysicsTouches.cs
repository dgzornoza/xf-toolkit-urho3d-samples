using System;
using System.Collections.Generic;
using System.Text;
using Urho;
using Urho.Urho2D;
using Urho3d.Rube.Samples;

namespace Urho3d.Rube.Components
{
    /// <summary>
    /// Component for handle touches in physics 'RigidBody2D' adding contraint for move bodies.
    /// </summary>
    public class HandlePhysicsTouches : Component
    {
        private const string DUMMY_NODE_NAME = "HandlePhysicsTouches_DummyNode";

        private Scene _scene;
        private Camera _lazyCamera;
        private Node _pickedNode;
        private RigidBody2D _dummyBody;        
        
        
        public HandlePhysicsTouches() { }


        public override void OnSceneSet(Scene scene)
        {
            base.OnSceneSet(scene);                        

            // create
            if (null != scene)
            {                
                // dumybody
                Node dummyNode = scene.CreateChild(DUMMY_NODE_NAME);
                this._dummyBody = dummyNode.CreateComponent<RigidBody2D>();                

                // add touch event
                var input = Application.Input;
                input.TouchBegin += _handleTouchBegin;
            }
            // destroy
            else
            {
                // dumybody                
                this._scene.RemoveChild(_dummyBody.Node);

                // remove touch event
                var input = Application.Input;
                input.TouchBegin -= _handleTouchBegin;
            }

            this._scene = scene;
        }




        private void _handleTouchBegin(TouchBeginEventArgs args)
        {
            
            var graphics = Application.Graphics;
            var input = Application.Input;

            PhysicsWorld2D physicsWorld = this._scene.GetComponent<PhysicsWorld2D>();
            // Raycast for RigidBody2Ds to pick
            RigidBody2D rigidBody = physicsWorld.GetRigidBody(args.X, args.Y, uint.MaxValue); 
            if (rigidBody != null)
            {
                _pickedNode = rigidBody.Node;

                // Create a ConstraintMouse2D - Temporary apply this constraint to the pickedNode to allow grasping and moving with touch
                ConstraintMouse2D constraintMouse = _pickedNode.CreateComponent<ConstraintMouse2D>();
                Vector3 pos = this._camera.ScreenToWorldPoint(new Vector3((float)args.X / graphics.Width, (float)args.Y / graphics.Height, 0.0f));
                constraintMouse.Target = new Vector2(pos.X, pos.Y);
                constraintMouse.MaxForce = 1000 * rigidBody.Mass;
                constraintMouse.CollideConnected = true;
                // Use dummy body instead of rigidBody. It's better to create a dummy body automatically in ConstraintMouse2D
                constraintMouse.OtherBody = _dummyBody;  
                constraintMouse.DampingRatio = 0;
            }

            input.TouchMove += _handleTouchMove;
            input.TouchEnd += _handleTouchEnd;
        }

        private void _handleTouchEnd(TouchEndEventArgs args)
        {
            var input = Application.Input;

            if (_pickedNode != null)
            {
                // Remove temporary constraint
                _pickedNode.RemoveComponent<ConstraintMouse2D>(); 
                _pickedNode = null;
            }


            input.TouchMove -= _handleTouchMove;
            input.TouchEnd -= _handleTouchEnd;
        }

        private void _handleTouchMove(TouchMoveEventArgs args)
        {
            if (_pickedNode != null)
            {
                var graphics = Application.Graphics;
                ConstraintMouse2D constraintMouse = _pickedNode.GetComponent<ConstraintMouse2D>();
                Vector3 pos = this._camera.ScreenToWorldPoint(new Vector3((float)args.X / graphics.Width, (float)args.Y / graphics.Height, 0.0f));
                constraintMouse.Target = new Vector2(pos.X, pos.Y);
            }
        }

        private Camera _camera => this._lazyCamera ?? (this._lazyCamera = this._scene.GetChild(SamplesConfig.mainCameraNodeName).GetComponent<Camera>());
            
    }
}

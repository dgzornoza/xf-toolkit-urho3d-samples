using System;
using System.Collections.Generic;
using System.Text;
using Urho;
using Urho.Urho2D;
using Urho3d.Rube.Samples;

namespace Urho3d.Rube.Core
{
    public class HandlePhysicsTouches
    {
        private readonly Scene _scene;
        private readonly Camera _camera;
        private Node pickedNode;
        private RigidBody2D _dummyBody;
        private bool _touchEnabled;
        

        public HandlePhysicsTouches(Scene scene)
        {
            this._scene = scene;
            this._camera = this._scene.GetChild(SamplesConfig.mainCameraNodeName).GetComponent<Camera>();
            
            // dumybody
            Node dummyNode = this._scene.CreateChild("DummyNode");
            this._dummyBody = dummyNode.CreateComponent<RigidBody2D>();
            this._scene.GetComponent<PhysicsWorld2D>().DrawJoint = true;

            var input = Urho.Application.Current.Input;
            input.TouchBegin += HandleTouchBegin;
        }


        private void HandleTouchBegin(TouchBeginEventArgs args)
        {
            var graphics = Urho.Application.Current.Graphics;
            var input = Urho.Application.Current.Input;

            PhysicsWorld2D physicsWorld = this._scene.GetComponent<PhysicsWorld2D>();
            RigidBody2D rigidBody = physicsWorld.GetRigidBody(args.X, args.Y, uint.MaxValue); // Raycast for RigidBody2Ds to pick
            if (rigidBody != null)
            {
                pickedNode = rigidBody.Node;

                // Create a ConstraintMouse2D - Temporary apply this constraint to the pickedNode to allow grasping and moving with touch
                ConstraintMouse2D constraintMouse = pickedNode.CreateComponent<ConstraintMouse2D>();
                Vector3 pos = this._camera.ScreenToWorldPoint(new Vector3((float)args.X / graphics.Width, (float)args.Y / graphics.Height, 0.0f));
                constraintMouse.Target = new Vector2(pos.X, pos.Y);
                constraintMouse.MaxForce = 1000 * rigidBody.Mass;
                constraintMouse.CollideConnected = true;
                // Use dummy body instead of rigidBody. It's better to create a dummy body automatically in ConstraintMouse2D
                constraintMouse.OtherBody = _dummyBody;  
                constraintMouse.DampingRatio = 0;
            }

            input.TouchMove += HandleTouchMove;
            input.TouchEnd += HandleTouchEnd;
        }

        private void HandleTouchEnd(TouchEndEventArgs args)
        {
            var input = Urho.Application.Current.Input;

            if (pickedNode != null)
            {
                // Remove temporary constraint
                pickedNode.RemoveComponent<ConstraintMouse2D>(); 
                pickedNode = null;
            }


            input.TouchMove -= HandleTouchMove;
            input.TouchEnd -= HandleTouchEnd;
        }

        private void HandleTouchMove(TouchMoveEventArgs args)
        {
            if (pickedNode != null)
            {
                var graphics = Urho.Application.Current.Graphics;
                ConstraintMouse2D constraintMouse = pickedNode.GetComponent<ConstraintMouse2D>();
                Vector3 pos = this._camera.ScreenToWorldPoint(new Vector3((float)args.X / graphics.Width, (float)args.Y / graphics.Height, 0.0f));
                constraintMouse.Target = new Vector2(pos.X, pos.Y);
            }
        }
    }
}

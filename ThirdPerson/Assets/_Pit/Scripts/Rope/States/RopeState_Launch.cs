using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Pit.Rope
{
    // - launch: Se lanza el destino de la cuerda,
    // añadiendo joints poco a poco conforme se aleja del origen.
    // No se aplican fuerzas spring sobre nada
    // (a no ser que se alcanza el número máximo de joints).
    public class RopeState_Launch : RopeStateBase
    {
        private bool _appliedStop = false;
        private bool _applyingStop = false;

        protected override void OnEnter()
        {
            _appliedStop = false;
            //context.OriginJoint.affectedBySpring = false;
            //context.DestinyJoint.affectedBySpring = false;
            context.OriginJoint.isKinematic = true;
            context.DestinyJoint.isKinematic = false;
            context.cancelHookInput.action.performed += OnCancelInput;
            //context.DestinyJoint.isKinematic = false;
            //
            //for(int i = 1; i < context.joints.Count; i++)
            //{
            //    context.joints[i].drag = 0f;
            //    context.joints[i].isKinematic = false;
            //}
            //LaunchJoint(context.DestinyJoint);
            LaunchJoint(context.DestinyJoint);
        }

        protected override void OnFixedUpdate()
        {
            context.ComputeRopeLength();
            if (CheckHooked()) return;

            if (context.MiddleJointCount >= context.maxMiddleJoints)
            {
                if (!_appliedStop)
                {
                    ApplyJakobsenConstraints(fixedOrigin: true, applyCompensationForces: true);
                }
                else
                {
                    ApplyJakobsenConstraints(fixedOrigin:false, applyCompensationForces:true);
                }
            }
            //ApplyGravityForces();
            ApplyRandomForces();
            CheckJointAddition();
        }

        //private async 

        private bool CheckHooked()
        {
            if (context.hookChecker.IsColliding)
            {
                parent.SetState<RopeState_Hooked>();
                return true;
            }
            else return false;
        }

        private void CheckJointAddition()
        {
            bool mustAddJoint = context.MiddleJointCount < context.maxMiddleJoints && context.ropeLength / (context.MiddleJointCount + 1) > context.maxEdgeLength;
            if (mustAddJoint)
            {
                var joint = context.AddJoint();
                //joint.applyVerletSimulation = true;
                joint.isKinematic = false;
                //joint.isKinematic = false;
                LaunchJoint(joint);
            }
        }

        private void LaunchJoint(RopeJoint joint)
        {
            joint.drag = 0f;
            joint.transform.position = context.OriginJoint.transform.position;
            joint.velocity = context.OriginJoint.targetVelocity + context.transform.forward * context.launchVelocity;
        }

        //private void LaunchJointVerlet(RopeJoint joint)
        //{
        //    joint.transform.position = context.OriginJoint.transform.position;
        //    joint.previousPosition = joint.transform.position - context.transform.forward * context.launchVelocity * Time.fixedDeltaTime;
        //}

        protected override void OnExit()
        {
            context.cancelHookInput.action.performed -= OnCancelInput;
        }

        private void OnCancelInput(InputAction.CallbackContext context)
        {
            if (context.performed) parent.SetState<RopeState_Idle>();
        }
    }
}

using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Pit.Rope
{
    // - hooked: El destino ha alcanzado una superficie a la que se engancha,
    // se aplica fuerzas sobre todo menos destino y origen.
    public class RopeState_Hooked : RopeStateBase
    {
        private bool _usingRapel = false;
        protected override void OnEnter()
        {
            //context.OriginJoint.affectedBySpring = true;
            //context.DestinyJoint.affectedBySpring = false;
            //context.DestinyJoint.isKinematic = true;
            context.OriginJoint.isKinematic = true;
            context.DestinyJoint.isKinematic = true;
            context.DestinyJoint.transform.SetParent(context.hookChecker.lastObj.transform);
            context.DestinyJoint.SetTargetRigidbody(context.hookChecker.lastObj.GetComponent<Rigidbody>());
            //context.ropeLength = Vector3.Distance(context.DestinyJoint.position, context.OriginJoint.position);
            context.ComputeRopeLength();
            context.launchInput.action.started += OnHookInput;
            context.launchInput.action.canceled += OnHookInput;
            context.cancelHookInput.action.performed += OnCancelInput;
            _usingRapel = context.launchInput.action.IsPressed();
            ApplyStopForces();
        }

        protected override void OnFixedUpdate()
        {
            bool canUseRapel = _usingRapel && context.MiddleJointCount > 0;
            //context.OriginJoint.affectedBySpring = !canUseRapel;
            //ApplySpringForces();
            //ApplyVerletSimulation();
            ApplyJakobsenConstraints();
            UseRapel();
            
        }

        private void UseRapel()
        {
            //Debug.Log(context.Tension);
            bool canUseRapel = _usingRapel && context.MiddleJointCount > 0 && context.Tension <= context.maxTensionToRapel;
            if (canUseRapel)
            {
                float lengthRemoved = context.rapelSpeed * Time.fixedDeltaTime;
                context.rapelJointPct += lengthRemoved / context.edgeLength;
                //Vector3 dir = context.joints[1].position - context.joints[0].position;
                //context.OriginJoint.AddForce(-Physics.gravity + dir.normalized * context.rapelForce * context.rapelSpeed);
                //context.OriginJoint.Move(dir.normalized * lengthRemoved - Physics.gravity*Time.fixedDeltaTime);
                if (context.rapelJointPct > 1)
                {
                    context.ropeLength -= context.edgeLength;
                    context.rapelJointPct -= 1f;
                    context.RemoveFirstJoint();
                    if (context.MiddleJointCount == 0)
                    {
                        context.ropeLength = context.edgeLength;
                        context.rapelJointPct = 0f;
                    }
                }
            }
        }

        protected override void OnExit()
        {
            //context.DestinyJoint.isKinematic = false;
            context.DestinyJoint.transform.SetParent(null);
            context.DestinyJoint.SetTargetRigidbody(null);
            context.cancelHookInput.action.performed -= OnCancelInput;
            context.launchInput.action.started -= OnHookInput;
            context.launchInput.action.canceled -= OnHookInput;
        }

        private void OnHookInput(InputAction.CallbackContext context)
        {
            if (context.started) _usingRapel = true;
            if (context.canceled) _usingRapel = false;
        }

        private void OnCancelInput(InputAction.CallbackContext context)
        {
            if (context.performed) parent.SetState<RopeState_Idle>();
        }
    }
}

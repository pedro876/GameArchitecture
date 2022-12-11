using UnityEngine;
using UnityEngine.InputSystem;

namespace Pit.Rope
{
    // - idle: Se mantiene la cuerda como un nodo único que sigue al padre
    public class RopeState_Idle : RopeStateBase
    {
        protected override void OnEnter()
        {
            base.OnEnter();
            context.RemoveAllJoints();
            //context.OriginJoint.affectedBySpring = false;
            //context.DestinyJoint.affectedBySpring = false;
            //context.DestinyJoint.isKinematic = true;
            context.OriginJoint.isKinematic = true;
            context.DestinyJoint.isKinematic = true;
            context.DestinyJoint.gameObject.SetActive(false);
            context.joints[0].edge.gameObject.SetActive(false);
            context.OriginJoint.transform.localPosition = Vector3.zero;
            context.DestinyJoint.transform.localPosition = Vector3.zero;

            context.launchInput.action.performed += OnLaunchInput;
            context.rapelJointPct = 0f;
        }

        private void OnLaunchInput(InputAction.CallbackContext ctx)
        {
            if (ctx.performed)
            {
                parent.SetState<RopeState_Launch>();
            }
        }

        protected override void OnExit()
        {
            context.DestinyJoint.gameObject.SetActive(true);
            context.joints[0].edge.gameObject.SetActive(true);
            context.launchInput.action.performed -= OnLaunchInput;
        }
    }
}

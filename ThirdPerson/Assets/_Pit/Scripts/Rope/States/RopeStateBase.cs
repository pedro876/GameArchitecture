using Architecture.HFSM;
using System.ComponentModel.Design.Serialization;
using UnityEngine;

namespace Pit.Rope
{
    //Sandbox
    public abstract class RopeStateBase : HState<RopeContext>
    {
        protected override void OnLateUpdate()
        {
            //Put edges (cylinders that represent the rope)
            for (int i = 0; i < context.joints.Count - 1; i++)
            {
                Vector3 middle = (context.joints[i].transform.position + context.joints[i + 1].transform.position) * 0.5f;
                Vector3 dir = context.joints[i + 1].transform.position - context.joints[i].transform.position;
                float mag = dir.magnitude;
                if (mag > 0f)
                {
                    Quaternion rot = Quaternion.LookRotation(dir) * Quaternion.Euler(new Vector3(90f, 0f, 0f));
                    context.joints[i].edge.rotation = rot;
                }
                float scale = mag * 0.5f;
                context.joints[i].edge.position = middle;
                context.joints[i].edge.localScale = new Vector3(context.joints[i].edge.localScale.x, scale, context.joints[i].edge.localScale.z);
            }
        }

        #region Spring forces
        //protected void ApplySpringForces()
        //{
        //    context.ComputeSpringLength();
        //    for (int i = 0; i < context.joints.Count; i++)
        //    {
        //        if (context.joints[i].affectedBySpring)
        //        {
        //            float springLength = context.springLength;
        //            if (i == 0)
        //            {
        //                springLength *= (1f - context.rapelJointPct);
        //                //context.joints[i].AddForce(GetSpringForce(i, springLength) / (1f - context.rapelJointPct));
        //            }
        //            else
        //            {
        //                
        //            }
        //            context.joints[i].AddForce(GetSpringForce(i, springLength));
        //        }
        //    }
        //}
        //private Vector3 GetSpringForce(int index, float springLength)
        //{
        //    Vector3 finalForce = Vector3.zero;
        //    Vector3 currentPos = context.joints[index].position;
        //
        //    if (index > 0)
        //    {
        //        Vector3 prevPos = context.joints[index - 1].position;
        //        Vector3 toPrev = prevPos - currentPos;
        //        float toPrevDisplacement = toPrev.magnitude - springLength;
        //        if (toPrevDisplacement < 0f) toPrevDisplacement *= (1f - context.ignoreCompressed);
        //        Vector3 toPrevForce = toPrev.normalized * toPrevDisplacement * context.springForce;
        //        finalForce += toPrevForce;
        //    }
        //    if (index < context.joints.Count - 1)
        //    {
        //        Vector3 nextPos = context.joints[index + 1].position;
        //        Vector3 toNext = nextPos - currentPos;
        //        float toNextDisplacement = toNext.magnitude - springLength;
        //        if (toNextDisplacement < 0f) toNextDisplacement *= (1f - context.ignoreCompressed);
        //        Vector3 toNextForce = toNext.normalized * toNextDisplacement * context.springForce;
        //        finalForce += toNextForce;
        //    }
        //
        //    return finalForce;
        //}
        #endregion

        #region Verlet & Jakobsen Rope

        //protected void ApplyVerletSimulation()
        //{
        //    float squaredDeltaTime = Time.fixedDeltaTime * Time.fixedDeltaTime;
        //    for(int i = 1; i < context.joints.Count; i++)
        //    {
        //        RopeJoint joint = context.joints[i];
        //        if (joint.applyVerletSimulation)
        //        {
        //            var newPos = 2 * joint.position - joint.previousPosition + squaredDeltaTime * context.acceleration;
        //            joint.previousPosition = joint.position;
        //            joint.SetPosition(newPos);
        //        }
        //    }
        //}

        protected void ApplyJakobsenConstraints(bool fixedOrigin = false, bool applyCompensationForces = true)
        {
            //We initialize the array of jakobsen positions
            for (int i = 0; i < context.joints.Count; i++)
            {
                context.joints[i].jakobsenPosition = context.joints[i].position;
            }

            //Then, we apply jakobsen relaxation constraints over a number of passes defined in the context
            //We will treat kinematic joints as fixed particles and compensate based on that
            context.ComputeEdgeLength();
            for (int pass = 0; pass < context.jakobsenPasses; pass++)
            {
                for(int i = 0; i < context.joints.Count-1; i++) //For each pair of particles
                {
                    var joint_0 = context.joints[i];
                    var joint_1 = context.joints[i + 1];
                    Vector3 dir = joint_1.jakobsenPosition - joint_0.jakobsenPosition;
                    float dist = dir.magnitude;
                    dir.Normalize();

                    float edgeLength = context.edgeLength;
                    if (i == 0) edgeLength *= (1f - context.rapelJointPct);
                    float correction = dist - edgeLength;

                    if(correction < 0f)
                    {
                        correction *= (1f-context.ignoreCompressed);
                    }

                    bool joint_0_fixed = joint_0.isTargetKinematic || (i == 0 && fixedOrigin);
                    bool joint_1_fixed = joint_1.isTargetKinematic;

                    if (joint_0_fixed && !joint_1_fixed) //Correct only right
                    {
                        joint_1.jakobsenPosition -= dir * correction;
                    }
                    else if (!joint_0_fixed && joint_1_fixed) //Correct only left
                    {
                        joint_0.jakobsenPosition += dir * correction;
                    }
                    else if(!joint_0_fixed && !joint_1_fixed) //Correct both
                    {
                        float massSum = joint_0.mass + joint_1.mass;
                        float leftCorrection = correction * (1f-joint_0.mass / massSum);
                        float rightCorrection = correction * (1f-joint_1.mass / massSum);
                        //float leftCorrection = correction * 0.5f;
                        //float rightCorrection = correction * 0.5f;
                        joint_0.jakobsenPosition += dir * leftCorrection;
                        joint_1.jakobsenPosition -= dir * rightCorrection;
                    }
                }
            }

            //Once the passes have been computed, we must apply the jakobsen positions
            for (int i = 0; i < context.joints.Count; i++)
            {
                var joint = context.joints[i];
                bool joint_fixed = joint.isTargetKinematic || (i == 0 && fixedOrigin);
                if (!joint_fixed)
                {
                    Vector3 dir = joint.jakobsenPosition - joint.position;
                    //joint.AddForce(dir*context.springConstantK, ForceMode.Force);
                    //joint.targetVelocity += dir*joint.mass;
                    if (applyCompensationForces)
                    {
                        joint.AddForce(dir/**joint.mass*/ * context.springForce, ForceMode.VelocityChange);
                    }
                    joint.Move(dir);
                }
            }
        }

        #endregion

        protected void ApplyRandomForces()
        {
            if (context.randomness <= 0f) return;
            for (int i = 0; i < context.joints.Count-1; i++)
            {
                if (!context.joints[i].isKinematic)
                {
                    Vector3 force = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * context.randomness;
                    context.joints[i].AddForce(force);
                }
            }
        }

        protected void ApplyStopForces(bool affectOrigin = true)
        {
            foreach (var joint in context.joints)
            {
                if (!affectOrigin && joint == context.OriginJoint) continue;
                joint.velocity = context.velocityPctWhenStopeed * joint.velocity;
            }
        }

        //protected void ApplyGravityForces()
        //{
        //    for (int i = 0; i < context.joints.Count; i++)
        //    {
        //        context.joints[i].AddForce(Vector3.down * context.extraGravity, ForceMode.Acceleration);
        //    }
        //}
    }
}

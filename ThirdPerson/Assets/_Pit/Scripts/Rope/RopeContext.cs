using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Pit.Rope
{
    [System.Serializable]
    public class RopeContext
    {
        [Header("References")]
        [SerializeField] Rigidbody characterRb;

        [Header("Input")]
        public InputActionReference launchInput;
        public InputActionReference cancelHookInput;

        [Header("Prototypes")]
        [SerializeField] GameObject jointPrototype;
        [SerializeField] GameObject edgePrototype;

        [Header("Launch & Hook")]
        public float launchVelocity = 10f;
        [Range(-1, 1)] public float velocityPctWhenStopeed = 0f;
        [SerializeField] LayerMask hookMask;

        [Header("Rapel")]
        public float rapelSpeed = 1f;
        //public float rapelForce = 1f;
        public float maxTensionToRapel = 1.1f;
        //public float extraGravity = 0f;
        //[HideInInspector] public Vector3 acceleration;

        [Header("Joints")]
        public float springForce = 10f;
        public float maxEdgeLength = 5f;
        //public float drag = 2f;
        public float randomness = 1f;
        public int maxMiddleJoints = 10;
        public int jakobsenPasses = 10;
        [Range(0, 1)] public float ignoreCompressed = 1f;
        //[Range(0, 1)] public float springLengthMultiplier = 1f;

        //HIDDEN
        [HideInInspector] public Transform transform;
        [HideInInspector] public List<RopeJoint> joints;
        //[HideInInspector] public List<Transform> edges;
        [HideInInspector] public TriggerChecker hookChecker;
        [HideInInspector] public float ropeLength = 0f;
        [HideInInspector] public float edgeLength = 0f;
        [HideInInspector] public float rapelJointPct = 0f;

        public RopeJoint OriginJoint => joints[0];
        public RopeJoint DestinyJoint => joints[joints.Count - 1];
        public int MiddleJointCount => joints.Count - 2;
        public Vector3 RopeDir => joints[1].position - joints[0].position;
        public float Tension => GetCurrentRopeLength() / (ropeLength-edgeLength*rapelJointPct);

        public void Initialize(Transform transform)
        {
            this.transform = transform;
            //this.acceleration = Physics.gravity.normalized * (Physics.gravity.magnitude + extraGravity);
            int count = transform.childCount;
            for (int i = 0; i < count; i++) GameObject.DestroyImmediate(transform.GetChild(0).gameObject);

            //Origin
            joints = new List<RopeJoint>()
            {
                InstantiateJoint(),
                InstantiateJoint(),
            };

            OriginJoint.transform.SetParent(transform);
            OriginJoint.gameObject.name = transform.gameObject.name+"_Origin";
            DestinyJoint.gameObject.name = transform.gameObject.name + "_Destiny";
            //OriginJoint.isKinematic = true;

            OriginJoint.SetTargetRigidbody(characterRb);


            hookChecker = DestinyJoint.gameObject.AddComponent<TriggerChecker>();
            hookChecker.layerMask = hookMask;

            OriginJoint.SetEdge(InstantiateEdge());
            OriginJoint.edge.gameObject.name = transform.gameObject.name + "_Edge_0";

            launchInput.action.Enable();
            cancelHookInput.action.Enable();
        }

        public void ComputeEdgeLength()
        {
            edgeLength = ropeLength / (MiddleJointCount + 1)/* * springLengthMultiplier*/;
        }

        public void ComputeRopeLength()
        {
            ropeLength = GetCurrentRopeLength();
        }

        public float GetCurrentRopeLength()
        {
            float totalLength = 0f;
            for (int i = 0; i < joints.Count - 1; i++)
            {
                float distance = Vector3.Distance(joints[i].position, joints[i + 1].position);
                //if (i == 0) distance *= (1f - rapelJointPct);
                totalLength += distance;

            }
            return totalLength;
        }

        public RopeJoint AddJoint()
        {
            var joint = InstantiateJoint();
            joint.SetEdge(InstantiateEdge());
            joints.Insert(1, joint);

            joint.edge.gameObject.name = transform.gameObject.name + "_Edge_" + (joints.Count - 2);
            joint.gameObject.name = transform.gameObject.name + "_Joint_" + (joints.Count - 2);
            return joint;
        }

        private RopeJoint InstantiateJoint()
        {
            var jointObj = GameObject.Instantiate(jointPrototype);
            return new RopeJoint(jointObj.transform);
        }

        private Transform InstantiateEdge()
        {
            var edge = GameObject.Instantiate(edgePrototype);
            return edge.transform;
        }

        public void RemoveFirstJoint()
        {
            GameObject.DestroyImmediate(joints[1].gameObject);
            GameObject.DestroyImmediate(joints[1].edge.gameObject);
            joints.RemoveAt(1);
        }

        public void RemoveAllJoints()
        {
            for (int i = 1; i < joints.Count - 1; i++)
            {
                GameObject.DestroyImmediate(joints[i].gameObject);
                if (joints[i].edge != null)
                    GameObject.DestroyImmediate(joints[i].edge.gameObject);
            }
            var first = OriginJoint;
            var last = DestinyJoint;
            joints.Clear();
            joints.Add(first);
            joints.Add(last);
        }
    }
}



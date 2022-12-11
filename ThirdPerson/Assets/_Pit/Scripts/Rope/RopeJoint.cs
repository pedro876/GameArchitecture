using UnityEngine;

namespace Pit.Rope
{
    public class RopeJoint
    {
        //public bool affectedBySpring;
        //public bool applyVerletSimulation;
        public bool applyJakobsenConstraints;
        public GameObject gameObject;
        public Transform transform;
        public Vector3 jakobsenPosition;
        private Rigidbody rb;
        private Rigidbody parentRb;
        private Rigidbody targetRb => parentRb != null ? parentRb : rb;
        public Transform edge { get; private set; }
        public SphereCollider collider { get; private set; }

        public RopeJoint(Transform transform)
        {
            //this.affectedBySpring = true;
            this.applyJakobsenConstraints = false;
            //this.applyVerletSimulation = false;
            this.transform = transform;
            this.gameObject = transform.gameObject;
            this.rb = transform.GetComponent<Rigidbody>();
            this.collider = transform.GetComponent<SphereCollider>();
            this.collider.enabled = true;
        }

        public void SetTargetRigidbody(Rigidbody rb)
        {
            this.parentRb = rb;
            collider.enabled = rb == null;
        }

        public void SetEdge(Transform edge)
        {
            this.edge = edge;
            //edge.SetParent(transform);
        }

        public Vector3 position => rb.position;
        public Vector3 targetPosition => targetRb.position;
        public float mass => targetRb.mass;
        public Vector3 velocity { get => rb.velocity; set => rb.velocity = value; }
        public Vector3 targetVelocity { get => targetRb.velocity; set => targetRb.velocity = value; }
        public bool isKinematic { get => rb.isKinematic; set => rb.isKinematic = value; }
        public bool isTargetKinematic { get => targetRb.isKinematic; set => targetRb.isKinematic = value; }
        public float drag { get => rb.drag; set => rb.drag = value; }
        public void AddForce(Vector3 force, ForceMode forceMode = ForceMode.Force)
        {
            targetRb.AddForce(force, forceMode);
        }

        public void Move(Vector3 displacement)
        {
            targetRb.MovePosition(targetRb.position + displacement);
        }

        public void SetPosition(Vector3 position)
        {
            targetRb.MovePosition(position);
        }
    }
}

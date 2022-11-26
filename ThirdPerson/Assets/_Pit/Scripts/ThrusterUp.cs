using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrusterUp : MonoBehaviour
{
    private Rigidbody rb;
    [SerializeField] float force = 1f;
    [SerializeField] float maxDistance = 1f;
    [SerializeField] float minForcePct = 0.8f;
    [SerializeField] float maxForcePct = 1f;
    [SerializeField] float exponent = 2f;
    [SerializeField] LayerMask layerMask;
    [SerializeField] ForceMode forceMode;

    private float distance;
    private bool inRange;

    float time = 0f;

    private void Awake()
    {
        rb = GetComponentInParent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        time += Time.fixedDeltaTime;
        time %= 1f;
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitInfo, maxDistance, layerMask))
        {
            inRange = true;
            distance = hitInfo.distance;
            float finalForce = Mathf.Pow((1f - distance / maxDistance), exponent) * force*1000f;
            finalForce = finalForce * Mathf.SmoothStep(minForcePct, maxForcePct, 0.5f+0.5f*Mathf.Cos((time * Mathf.PI*2f)));
            rb.AddForce(Vector3.up * finalForce * Time.fixedDeltaTime, forceMode);
        }
        else
            inRange = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = inRange ? Color.cyan : Color.white;
        Gizmos.DrawSphere(transform.position, 0.05f);
        if (inRange)
        {
            Gizmos.DrawLine(transform.position, transform.position + Vector3.down * distance);
        }
    }
}

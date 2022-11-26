using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
public class CameraController : MonoBehaviour, Controls.ICameraMapActions
{
    private const int TIME_TO_UNLOCK_CAMERA = 100;

    [Tooltip("This is the target the camera will follow")] public Transform target;

    [Header("Angle")]
    [SerializeField][Tooltip("Maximum height of the camera relative to its target")] float topHeight = 1.4f;
    [SerializeField][Tooltip("Minimum height of the camera relative to its target")] float bottomHeight = -0.75f;
    [SerializeField][Tooltip("Minimum angle from up to look direction")] int minAngle = 30;
    [SerializeField][Tooltip("Maximum angle from up to look direction")] int maxAngle = 120;

    [Header("Distance")]
    [SerializeField][Tooltip("The camera will try to recover distance when not colliding")] float preferredDistance = 4f;
    [SerializeField] float recoverDistanceMaxTime = 1f;
    [SerializeField] float recoverDistanceThreshold = 0.03f;
    private float _recoverDistanceTime = 0f;
    private float _originalDistance = 0f;
    private bool _recoveringDistance = false;


    [Header("Raycasting")]
    [SerializeField][Range(0f,1f)] float surfaceNormalWeight = 0.5f;
    [SerializeField] float minDistToTarget = 0.75f;
    [SerializeField] LayerMask layerMask;

    private Vector2 _inputAxis = Vector2.zero;
    private Rigidbody _rb;
    private SphereCollider _col;
    private bool _isColliding = false;
    private bool _isOccluded = false;

    public void OnAxis(InputAction.CallbackContext context) => _inputAxis = context.ReadValue<Vector2>();

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        //_rb.interpolation = RigidbodyInterpolation.Extrapolate;
        _rb.constraints = RigidbodyConstraints.FreezeRotation;
        _rb.drag = 0f;
        _rb.angularDrag = 0f;
        _rb.mass = 1f;
        _rb.useGravity = false;
        //_rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        _col = GetComponent<SphereCollider>();
    }

    private void Start()
    {
        InputManager.controls.CameraMap.SetCallbacks(this);
        UnlockCamera();

        async void UnlockCamera()
        {
            await Task.Delay(TIME_TO_UNLOCK_CAMERA);
            InputManager.controls.CameraMap.Enable();
        }
    }

    private Vector3 _lastRayOrigin;
    private Vector3 _lastRayDestiny;

    private void LateUpdate()
    {
        //X ROTATION
        var rotX = Quaternion.AngleAxis(_inputAxis.x * Time.deltaTime, Vector3.up);

        //Y ROTATION
        float angleY = _inputAxis.y * Time.deltaTime;
        float currentAngle = Vector3.Angle(Vector3.up, -transform.forward);
        float newAngle = currentAngle - angleY;
        if (newAngle > maxAngle)
            angleY = -maxAngle + currentAngle;
        else if (newAngle < minAngle)
            angleY = -minAngle + currentAngle;
        var rotY = Quaternion.AngleAxis(angleY, transform.right);

        //OCCLUSION CHECKING
        Vector3 relativePosition = transform.position - target.position;
        Vector3 newRelativePosition = rotX * rotY * relativePosition;

        Vector3 origin = target.position + newRelativePosition.normalized * minDistToTarget;
        Vector3 destiny = target.position + newRelativePosition.normalized * (newRelativePosition.magnitude - _col.radius);
        Vector3 dir = destiny - origin;
        _lastRayDestiny = destiny;
        _lastRayOrigin = origin;
        if (Physics.Raycast(origin, dir, out RaycastHit hit, dir.magnitude, layerMask))
            _isOccluded = hit.transform != transform;
        else
            _isOccluded = false;

        if (_isOccluded)
        {
            //Debug.Log(hit.distance);
            Vector3 outOfSurfaceDir = Vector3.Lerp(-dir.normalized, hit.normal, surfaceNormalWeight) * _col.radius;
            newRelativePosition = (hit.point + outOfSurfaceDir) - target.position;

            //newRelativePosition = newRelativePosition.normalized * (hit.distance+minDistToTarget-_col.radius);//(hit.distance-_col.radius);
        }

        //CLAMP CAMERA HEIGHT
        bool isClamped = false;
        isClamped = newRelativePosition.y + recoverDistanceThreshold >= topHeight || newRelativePosition.y-recoverDistanceThreshold <= bottomHeight;
        if (newRelativePosition.y >= topHeight)
        {
            float op = newRelativePosition.y - topHeight;
            float alpha = Vector3.Angle(Vector3.down, -newRelativePosition);
            float beta = 180f - alpha - 90f;
            float hyp = op / Mathf.Sin(Mathf.Deg2Rad * beta);
            float newMagnitude = newRelativePosition.magnitude - hyp;
            newRelativePosition = newRelativePosition.normalized * newMagnitude;
            isClamped = true;
        }

        if (newRelativePosition.y <= bottomHeight)
        {
            float op = newRelativePosition.y - bottomHeight;
            float alpha = Vector3.Angle(Vector3.down, -newRelativePosition);
            float beta = 180f - alpha - 90f;
            float hyp = op / Mathf.Sin(Mathf.Deg2Rad * beta);
            float newMagnitude = newRelativePosition.magnitude - hyp;
            newRelativePosition = newRelativePosition.normalized * newMagnitude;
            isClamped = true;
        }

        

        //DISTANCE RECOVERING
        float distance = newRelativePosition.magnitude;
        if (!isClamped && !_isColliding && !_isOccluded && distance < preferredDistance-recoverDistanceThreshold) //must recover distance
        {
            if (!_recoveringDistance)
            {
                _originalDistance = newRelativePosition.magnitude;
                _recoverDistanceTime = Time.deltaTime;
                _recoveringDistance = true;
            }
            else
            {
                _recoverDistanceTime += Time.deltaTime;
                if(_recoverDistanceTime > recoverDistanceMaxTime)
                {
                    _recoveringDistance = false;
                }
            }

            float targetDistance = Mathf.SmoothStep(_originalDistance, preferredDistance, _recoverDistanceTime / recoverDistanceMaxTime);
            newRelativePosition = newRelativePosition.normalized * targetDistance;
        }
        else
        {
            _recoveringDistance = false;
        }

        

        Vector3 newWorldPosition = newRelativePosition + target.position;

        //Vector3 step = (newPosition - transform.position) / iterations;
        //Vector3 originalPosition = transform.position;
        //Vector3 step = (newPosition - transform.position) / iterations;
        //Vector3 originalPos = transform.position;
        //for (int i = 1; i <= iterations; i++)
        //{
        //    //Vector3 step = (newPosition - _rb.position) / (iterations-i);
        //    _rb.MovePosition(originalPos + step*i);
        //}

        _rb.MovePosition(newWorldPosition);

        

        //_rb.velocity = Vector3.zero;
        //_rb.AddForce(newPosition - transform.position, mode);
        //_rb.velocity = (newPosition - transform.position)/Time.fixedDeltaTime;
        transform.LookAt(target.position);
        //var lookRot = Quaternion.LookRotation((target.position - newPosition).normalized);
        //transform.rotation = lookRot;
        //_rb.MoveRotation(lookRot);
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        Gizmos.color = _isOccluded ? Color.red : Color.green;
        Gizmos.DrawLine(_lastRayOrigin, _lastRayDestiny);
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Gizmos.DrawWireSphere(Vector3.zero, _col.radius);
        Gizmos.DrawWireSphere(transform.InverseTransformPoint(target.position), minDistToTarget);
    }

    private void OnCollisionEnter(Collision collision)
    {
        _isColliding = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        _isColliding = false;
    }
}
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
public class CameraController : MonoBehaviour, Controls.ICameraMapActions
{
    private const int TIME_TO_UNLOCK_CAMERA = 500;

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
    [SerializeField] private bool _recoveringDistance = false;


    [Header("Raycasting")]
    [SerializeField][Range(0f,1f)] float surfaceNormalWeight = 0.25f;
    [SerializeField] float minDistToTarget = 0.75f;
    [SerializeField] LayerMask occlusionMask;

    private Vector2 _inputAxis = Vector2.zero;
    private Rigidbody _rb;
    private SphereCollider _col;
    private bool _isColliding = false;
    private int _collisionCount = 0;
    private bool _isOccluded = false;

    private Vector3 _lastTargetPosition;

    private Vector3 _lastRayOrigin;
    private Vector3 _lastRayDestiny;

    public void OnAxis(InputAction.CallbackContext context) => _inputAxis = context.ReadValue<Vector2>();

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.constraints = RigidbodyConstraints.FreezeRotation;
        _rb.drag = 0f;
        _rb.angularDrag = 0f;
        _rb.mass = 1f;
        _rb.useGravity = false;
        _col = GetComponent<SphereCollider>();
        _collisionCount = 0;
        if(target != null)
            _lastTargetPosition = target.position;
    }

    private void Start()
    {
        transform.position = target.position - target.forward * preferredDistance;

        InputManager.controls.CameraMap.SetCallbacks(this);
        UnlockCamera();

        async void UnlockCamera()
        {
            await Task.Delay(TIME_TO_UNLOCK_CAMERA);
            InputManager.controls.CameraMap.Enable();
        }
    }

    private void FixedUpdate()
    {
        float deltaTime = Time.fixedDeltaTime;
        if (target == null) return;
        //ROTATION
        Vector3 forward = _rb.rotation * Vector3.forward;
        Vector3 right = _rb.rotation * Vector3.right;
        var rotX = Quaternion.AngleAxis(_inputAxis.x * deltaTime, Vector3.up);
        float angleY = _inputAxis.y * deltaTime;
        float currentAngle = Vector3.Angle(Vector3.up, forward);
        float newAngle = currentAngle + angleY;
        if (newAngle > maxAngle)
            angleY = maxAngle - currentAngle;
        else if (newAngle < minAngle)
            angleY = minAngle - currentAngle;
        var rotY = Quaternion.AngleAxis(angleY, right);

        //OCCLUSION CHECKING
        Vector3 relativePosition = _rb.position - _lastTargetPosition;
        Vector3 newRelativePosition = rotX * rotY * relativePosition;

        Vector3 origin = _lastTargetPosition + newRelativePosition.normalized * minDistToTarget;
        Vector3 destiny = _lastTargetPosition + newRelativePosition.normalized * newRelativePosition.magnitude;
        Vector3 dir = destiny - origin;
        _lastRayDestiny = destiny;
        _lastRayOrigin = origin;
        if (Physics.Raycast(origin, dir, out RaycastHit hit, dir.magnitude, occlusionMask))
            _isOccluded = hit.transform != transform;
        else
            _isOccluded = false;
        
        if (_isOccluded)
        {
            Vector3 outOfSurfaceDir = Vector3.Lerp(-dir.normalized, hit.normal, surfaceNormalWeight) * _col.radius;
            newRelativePosition = (hit.point + outOfSurfaceDir) - _lastTargetPosition;
        }

        //CLAMP CAMERA HEIGHT
        bool isClamped = newRelativePosition.y + recoverDistanceThreshold >= topHeight || newRelativePosition.y-recoverDistanceThreshold <= bottomHeight;
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
        bool wrongDistance = _recoveringDistance || distance < preferredDistance - recoverDistanceThreshold || distance > preferredDistance + recoverDistanceThreshold;
        if (!isClamped && !_isColliding && !_isOccluded && wrongDistance) //must recover distance
        {
            if (!_recoveringDistance)
            {
                _originalDistance = newRelativePosition.magnitude;
                _recoverDistanceTime = deltaTime;
                _recoveringDistance = true;
            }
            else
            {
                _recoverDistanceTime += deltaTime;
                if(_recoverDistanceTime > recoverDistanceMaxTime)
                {
                    _recoveringDistance = false;
                }
            }

            float lerp = Mathf.SmoothStep(0f, 1f, _recoverDistanceTime / recoverDistanceMaxTime);

            float targetDistance = Mathf.Lerp(_originalDistance, preferredDistance, lerp);
            newRelativePosition = newRelativePosition.normalized * targetDistance;
        }
        else
        {
            _recoveringDistance = false;
        }

        Vector3 newWorldPosition = newRelativePosition + target.position;

        _rb.MovePosition(newWorldPosition);
        //_rb.velocity = (newWorldPosition - _rb.position)/Time.fixedDeltaTime;
        _rb.MoveRotation(Quaternion.LookRotation(-newRelativePosition));
        //transform.LookAt(target.position);
        _lastTargetPosition = target.position;
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
        _collisionCount++;
        _isColliding = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        _collisionCount--;
        if(_collisionCount <= 0)
        {
            _collisionCount = 0;
            _isColliding = false;
        }
    }
}

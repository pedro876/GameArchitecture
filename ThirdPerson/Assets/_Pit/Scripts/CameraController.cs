using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour, Controls.ICameraMapActions
{
    private const int TIME_TO_UNLOCK_CAMERA = 500;

    [Tooltip("This is the target the camera will follow")] public Transform target;
    [SerializeField] bool recenterOnStart = true;

    [Header("Interpolation")]
    [SerializeField] bool interpolate = true;
    [SerializeField] float interpolationSpeed = 8f;

    [Header("Inclination")]
    [SerializeField][Tooltip("Minimum angle from up to look direction with camera looking upwards")] int minAngle = 60;
    [SerializeField][Tooltip("Maximum angle from up to look direction with camera looking downwards")] int maxAngle = 120;

    [Header("Clamping")]
    [SerializeField] bool clampCameraHeight = true;
    [SerializeField][Tooltip("Maximum height of the camera relative to its target")] float topHeight = 1.4f;
    [SerializeField][Tooltip("Minimum height of the camera relative to its target")] float bottomHeight = -0.75f;

    [Header("Distance")]
    [SerializeField][Tooltip("The camera will try to recover distance when not colliding")] float preferredDistance = 4f;

    [Header("Occlusion")]
    [SerializeField] bool checkOcclussion = true;
    [SerializeField] float radius = 0.3f;
    [SerializeField][Range(0f,1f)] float surfacePenetration = 0.7f;
    [SerializeField][Range(0f, 1f)] float surfaceNormalWeight = 0.25f;
    [SerializeField] float minDistToTarget = 0.3f;
    [SerializeField] LayerMask occlusionMask;

    private bool _isOverlapping = false;
    private Collider[] _overlappers;

    private Vector2 _inputAxis = Vector2.zero;
    private bool _isOccluded = false;

    private Vector3 _lastTargetPosition;
    private Vector3 _lastRayOrigin;
    private Vector3 _lastRayDestiny;
    private Vector3 _oldSphereWorldPos;
    private Quaternion _oldSphereWorldRot;

    public void OnAxis(InputAction.CallbackContext context) => _inputAxis = context.ReadValue<Vector2>();

    private void Awake()
    {
        _overlappers = new Collider[2];
        
    }

    private void Start()
    {
        if(recenterOnStart) transform.position = target.position - target.forward * preferredDistance;
        if (target != null)
        {
            _lastTargetPosition = target.position;
            _oldSphereWorldPos = transform.position;
            _oldSphereWorldRot = transform.rotation;
        }

        InputManager.controls.CameraMap.SetCallbacks(this);
        UnlockCamera();
        async void UnlockCamera()
        {
            await Task.Delay(TIME_TO_UNLOCK_CAMERA);
            InputManager.controls.CameraMap.Enable();
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;
        MoveCamera();
    }

    private void MoveCamera()
    {
        ComputeTargetCameraTransform(out var targetPos, out var targetRot);
        if (interpolate)
        {
            targetPos = Vector3.Lerp(transform.position, targetPos, interpolationSpeed * Time.deltaTime);
            targetRot = Quaternion.Lerp(transform.rotation, targetRot, interpolationSpeed * Time.deltaTime);
        }
        transform.SetPositionAndRotation(targetPos, targetRot);
    }

    private void ComputeTargetCameraTransform(out Vector3 cameraTargetPos, out Quaternion cameraTargetRot)
    {
        Vector3 newSphereRelativePos = RotateCameraSphere(_oldSphereWorldPos - _lastTargetPosition);
        newSphereRelativePos = newSphereRelativePos.normalized * preferredDistance;

        newSphereRelativePos = ClampCameraHeight(newSphereRelativePos, out bool neededClamping);
        newSphereRelativePos = CheckOcclusion(newSphereRelativePos, out Vector3 cameraLocalPos, out bool neededRelocation);

        Vector3 newSphereWorldPos = newSphereRelativePos + target.position;
        _oldSphereWorldPos = newSphereWorldPos;
        _oldSphereWorldRot = Quaternion.LookRotation(target.position - newSphereWorldPos);
        Vector3 newCameraWorldPos = cameraLocalPos + newSphereWorldPos;
        cameraTargetPos = newCameraWorldPos;
        cameraTargetRot = Quaternion.LookRotation(target.position - newCameraWorldPos);
        _lastTargetPosition = target.position;
    }

    private Vector3 RotateCameraSphere(Vector3 sphereRelativePos)
    {
        Vector3 forward = _oldSphereWorldRot * Vector3.forward;
        Vector3 right = _oldSphereWorldRot * Vector3.right;
        var rotX = Quaternion.AngleAxis(_inputAxis.x * Time.deltaTime, Vector3.up);
        float angleY = _inputAxis.y * Time.deltaTime;
        float currentAngle = Vector3.Angle(Vector3.up, forward);
        float newAngle = currentAngle + angleY;
        if (newAngle > maxAngle)
            angleY = maxAngle - currentAngle;
        else if (newAngle < minAngle)
            angleY = minAngle - currentAngle;
        var rotY = Quaternion.AngleAxis(angleY, right);
        sphereRelativePos = rotX * rotY * sphereRelativePos;
        return sphereRelativePos;
    }

    /// <summary>
    /// Clamps the camera so that it does not separate too much from the player in the y axis.
    /// </summary>
    /// <param name="sphereRelativePos"></param>
    /// <param name="needsClamping"></param>
    /// <returns>The relative position of the sphere clamped between bottomHeight and topHeight.</returns>
    private Vector3 ClampCameraHeight(Vector3 sphereRelativePos, out bool needsClamping)
    {
        if (!clampCameraHeight)
        {
            needsClamping = false;
            return sphereRelativePos;
        }
        needsClamping = sphereRelativePos.y >= topHeight || sphereRelativePos.y <= bottomHeight;
        if (needsClamping)
        {
            float op = sphereRelativePos.y - (sphereRelativePos.y >= topHeight ? topHeight : bottomHeight);
            float alpha = Vector3.Angle(Vector3.down, -sphereRelativePos);
            float beta = 180f - alpha - 90f;
            float hyp = op / Mathf.Sin(Mathf.Deg2Rad * beta);
            float newMagnitude = sphereRelativePos.magnitude - hyp;
            sphereRelativePos = sphereRelativePos.normalized * newMagnitude;
            needsClamping = true;
        }
        return sphereRelativePos;
    }

    /// <summary>
    /// Relocates the camera according to occlusion checking towards its target. If occlusion checking is disabled, it will return the input unmodified.
    /// </summary>
    /// <param name="sphereRelativePos"></param>
    /// <param name="cameraLocalPos"></param>
    /// <param name="needsRelocation"></param>
    /// <returns>Returns a relocated sphere position taking into account occlusion via raycast and sphere overlapping.
    /// Additionally, returns a relocated camera to prevent it from penetrating walls and a boolean indicating
    /// if the camera had to be relocated.</returns>
    private Vector3 CheckOcclusion(Vector3 sphereRelativePos, out Vector3 cameraLocalPos, out bool needsRelocation)
    {
        if (!checkOcclussion)
        {
            needsRelocation = false;
            cameraLocalPos = Vector3.zero;
            return sphereRelativePos;
        }
        Vector3 origin = _lastTargetPosition + sphereRelativePos.normalized * minDistToTarget;
        Vector3 destiny = _lastTargetPosition + sphereRelativePos.normalized * (sphereRelativePos.magnitude + radius);
        Vector3 raycastDir = destiny - origin;
        _lastRayDestiny = destiny;
        _lastRayOrigin = origin;
        if (Physics.Raycast(origin, raycastDir, out RaycastHit hit, raycastDir.magnitude, occlusionMask))
            _isOccluded = hit.transform != transform;
        else
            _isOccluded = false;

        float finalPreferredDistance = preferredDistance;
        if (_isOccluded)
        {
            finalPreferredDistance = hit.distance + minDistToTarget - surfacePenetration * radius;
            _isOverlapping = Physics.OverlapSphereNonAlloc(_oldSphereWorldPos, radius, _overlappers, occlusionMask, QueryTriggerInteraction.Ignore) > 1;
            if (_isOverlapping)
                cameraLocalPos = -raycastDir.normalized * (surfacePenetration * radius);
            else
            {
                Vector3 outOfSurfaceDir = Vector3.Lerp(-raycastDir.normalized, hit.normal, surfaceNormalWeight).normalized * (surfacePenetration * radius);
                cameraLocalPos = outOfSurfaceDir;
            }
        }
        else
        {
            _isOverlapping = false;
            cameraLocalPos = Vector3.zero;
        }

        float distance = sphereRelativePos.magnitude;
        if (distance > finalPreferredDistance)
        {
            sphereRelativePos = sphereRelativePos.normalized * finalPreferredDistance;
            needsRelocation = true;
        }
        else
            needsRelocation = false;
        return sphereRelativePos;
    }

    #region GIZMOS
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = (checkOcclussion && _isOccluded) ? Color.red : Color.blue;
        if (Application.isPlaying && checkOcclussion)
        {
            Gizmos.DrawLine(_lastRayOrigin, _lastRayDestiny);
        }
        else if(target != null)
        {
            transform.LookAt(target);
            Gizmos.DrawLine(transform.position, target.position);
        }

        var pos = Application.isPlaying ? _oldSphereWorldPos : transform.position;
        var rot = Application.isPlaying ? _oldSphereWorldRot : transform.rotation;

        Gizmos.matrix = Matrix4x4.TRS(pos, rot, Vector3.one);
        Gizmos.DrawWireSphere(Vector3.zero, radius*surfacePenetration);
        Gizmos.color = _isOverlapping ? Color.red : Color.green;
        Gizmos.DrawWireSphere(Vector3.zero, radius);
        if(target != null)
        {
            Gizmos.matrix = Matrix4x4.TRS(target.position, rot, Vector3.one);
            Gizmos.DrawWireSphere(Vector3.zero, minDistToTarget);
        }
            
    }
#endif
    #endregion
}

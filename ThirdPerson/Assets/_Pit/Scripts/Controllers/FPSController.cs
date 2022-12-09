using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;

public class FPSController : MonoBehaviour
{
    [Header("Motion")]
    [SerializeField] float motionSpeed = 1f;
    [SerializeField] float motionLerp = 1f;

    [Header("Camera")]
    [SerializeField] float camLerp = 1f;
    [SerializeField] Vector2 cameraSensibility = Vector2.one;
    [SerializeField] float minCameraAngle = 10f;
    [SerializeField] float maxCameraAngle = 170f;

    [Header("Jumping")]
    [SerializeField] float jumpForce = 200f;
    [SerializeField] float jumpForceOnHook = 200f;
    [SerializeField] float jumpCooldown = 0.25f;

    [Header("References")]
    [SerializeField] TriggerChecker groundChecker;
    private CinemachineVirtualCamera _cam;
    private Controls _controls;
    private Rigidbody _rb;
    private Hook _hook;

    private Vector2 _rawMotionInput;
    private Vector2 _rawCamInput;
    private Vector2 _motionInput;
    private Vector2 _camInput;

    public bool IsGrounded => groundChecker.IsColliding;

    private void Awake()
    {
        _controls = InputManager.controls;
        _cam = GetComponentInChildren<CinemachineVirtualCamera>();
        _rb = GetComponent<Rigidbody>();
        _hook = GetComponentInChildren<Hook>();
    }

    #region InputBinding

    private void OnEnable()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        _controls.MovementMap.Enable();
        _controls.CameraMap.Enable();
        _controls.MovementMap.Motion.performed += OnMotion;
        _controls.CameraMap.Axis.performed += OnAxis;
        _controls.MovementMap.Jump.performed += OnJump;
        _controls.MovementMap.Jump.canceled += OnJump;
    }

    private void OnDisable()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        _controls.MovementMap.Disable();
        _controls.CameraMap.Disable();
        _controls.MovementMap.Motion.performed -= OnMotion;
        _controls.CameraMap.Axis.performed -= OnAxis;
        _controls.MovementMap.Jump.performed -= OnJump;
        _controls.MovementMap.Jump.canceled -= OnJump;
    }

    private void OnMotion(InputAction.CallbackContext context) => _rawMotionInput = context.ReadValue<Vector2>();
    private void OnAxis(InputAction.CallbackContext context) => _rawCamInput = context.ReadValue<Vector2>();
    private void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (IsGrounded) Jump(jumpForce);
            else _hook.SetState(Hook.State.LAUNCHED);
        }
        if (context.canceled)
        {
            if(!IsGrounded && _hook.state == Hook.State.HOOKED)
            {
                Jump(jumpForceOnHook);
            }
            _hook.SetState(Hook.State.RAPEL);
        }
    }

    #endregion

    #region Movement

    private void FixedUpdate()
    {
        ProcessInput();
        MoveCharacter();
        MoveCamera();
    }

    private void ProcessInput()
    {
        _motionInput = Vector2.Lerp(_motionInput, _rawMotionInput, motionLerp * Time.fixedDeltaTime);
        _camInput = Vector2.Lerp(_camInput, _rawCamInput, camLerp * Time.fixedDeltaTime);
    }

    private void MoveCharacter()
    {
        Vector3 motionDir = MotionToCameraSpace(out float magnitude);
        if(magnitude > 0f)
            _rb.MovePosition(_rb.position + motionDir * motionSpeed * Time.fixedDeltaTime);
    }

    private void MoveCamera()
    {
        float angleX = _camInput.x * cameraSensibility.x * Time.fixedDeltaTime ;
        transform.rotation = Quaternion.AngleAxis(angleX, transform.up) * transform.rotation;
        float angleY = -_camInput.y * cameraSensibility.y * Time.fixedDeltaTime;
        angleY = ClampCamAngleY(angleY);
        _cam.transform.localRotation = Quaternion.AngleAxis(angleY, Vector3.right) * _cam.transform.localRotation;
    }

    private Vector3 MotionToCameraSpace(out float magnitude)
    {
        Vector3 motionInput = new Vector3(_motionInput.x, 0f, _motionInput.y);
        magnitude = motionInput.magnitude;
        if (magnitude > 1f)
        {
            motionInput /= magnitude;
            magnitude = 1f;
        }
        Vector3 localDir = _cam.transform.TransformDirection(motionInput);
        Vector3 projectedDir = Vector3.ProjectOnPlane(localDir, Vector3.up).normalized * magnitude;
        return projectedDir;
    }

    private float ClampCamAngleY(float angleY)
    {
        float currentAngle = Vector3.Angle(transform.up, _cam.transform.forward);
        float newAngle = currentAngle + angleY;
        if(newAngle > maxCameraAngle)
        {
            angleY = maxCameraAngle - currentAngle;
        }
        if(newAngle < minCameraAngle)
        {
            angleY = minCameraAngle - currentAngle;
        }
        return angleY;
    }

    private void Jump(float force)
    {
        groundChecker.DisableForSeconds(jumpCooldown);
        _rb.AddForce(transform.up * force, ForceMode.Acceleration);
    }

    #endregion
}

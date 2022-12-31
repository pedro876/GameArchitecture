using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class TPSController2 : MonoBehaviour
{
    public event Action<float> OnGrounded;

    [Header("Ground")]
    [SerializeField] LayerMask groundLayerMask = int.MaxValue;
    [SerializeField] float groundCheckOffset = 0f;
    [SerializeField] float toGroundForce = 3f;
    [SerializeField] float goingUpThreshold = 1f; //If the character y velocity surpasses this (before ground force), then it's going up.

    [Header("Motion")]
    [SerializeField] float acceleration = 1f;
    [SerializeField] float maxVelocity = 7f;
    [SerializeField] float motionInputLerp = 10f;

    [Header("Turning")]
    [SerializeField] float viewInputLerp = 10f;
    [SerializeField] float turningSpeed = 2f;
    [SerializeField] float maxViewInput = 100f;

    private Controls _controls;
    private Transform _cam;
    private CharacterController _characterController;
    private Rigidbody _rb;
    private Vector2 _rawMotionInput;
    private Vector2 _motionInput;
    private Vector2 _rawViewInput;
    private Vector2 _viewInput;
    private bool _grounded = true;
    private bool _goingUp = false;

    private Vector3 _motionVelocity;

    private void Awake()
    {
        _controls = InputManager.controls;
        _characterController = GetComponent<CharacterController>();
        _cam = Camera.main.transform;
        _rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        _grounded = CheckGroundedSphere();
        _rb.isKinematic = _grounded;
        _characterController.enabled = _grounded;
    }

    void OnMotionInput(InputAction.CallbackContext ctx) => _rawMotionInput = ctx.ReadValue<Vector2>();
    void OnViewInput(InputAction.CallbackContext ctx) => _rawViewInput = ctx.ReadValue<Vector2>();

    private void OnEnable()
    {
        _controls.MovementMap.Enable();
        _controls.CameraMap.Enable();
        _controls.MovementMap.Motion.performed += OnMotionInput;
        _controls.CameraMap.Axis.performed += OnViewInput;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnDisable()
    {
        _controls.MovementMap.Disable();
        _controls.CameraMap.Disable();
        _controls.MovementMap.Motion.performed -= OnMotionInput;
        _controls.CameraMap.Axis.performed -= OnViewInput;
        _rawMotionInput = Vector2.zero;
        _motionInput = Vector2.zero;
        _rawViewInput = Vector2.zero;
        _viewInput = Vector2.zero;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    void Update()
    {
        ProcessInput();
        ApplyTurningRotation();
        ApplyMovement();
        CheckGroundedAndFalling();
    }

    private void ProcessInput()
    {
        _motionInput = Vector2.Lerp(_motionInput, _rawMotionInput, motionInputLerp * Time.deltaTime);
        _viewInput = Vector2.Lerp(_viewInput, _rawViewInput, viewInputLerp * Time.deltaTime);
        _viewInput = Vector2.ClampMagnitude(_viewInput, maxViewInput);
    }

    private void ApplyTurningRotation()
    {
        transform.Rotate(Vector3.up, turningSpeed * _viewInput.x * Time.deltaTime);
    }
    Vector3 finalDir;
    private void ApplyMovement()
    {
        if (_grounded)
        {
            float finalMaxVelocity = maxVelocity * _motionInput.magnitude;
            Vector3 targetDir = new Vector3(_motionInput.x, 0f, _motionInput.y);
            float dirLength = targetDir.magnitude;
            if(dirLength > 0f)
            {
                targetDir = Vector3.ProjectOnPlane(_cam.TransformDirection(targetDir), Vector3.up).normalized;
            }

            Vector3 currentDir = _motionVelocity / maxVelocity;
            finalDir = targetDir - currentDir;
            if (finalDir.magnitude < 1f) finalDir.Normalize();

            Debug.Log(finalDir);

            _motionVelocity += finalDir * acceleration * Time.deltaTime;

            if (_motionVelocity.magnitude > finalMaxVelocity) _motionVelocity = _motionVelocity.normalized * finalMaxVelocity;

            Vector3 finalMovement = _motionVelocity*Time.deltaTime;
            finalMovement.y -= 0.03f;

            _characterController.Move(finalMovement);
            _motionVelocity = _characterController.velocity;

            _goingUp = _motionVelocity.y > goingUpThreshold;
            if (!_goingUp)
            {
                Vector3 toGroundMovement = new Vector3(0f, -toGroundForce * Time.deltaTime, 0f);
                _characterController.Move(toGroundMovement);
            }
        }
    }

    private void CheckGroundedAndFalling()
    {
        bool wasGrounded = _grounded;

        _grounded = CheckGroundedSphere();

        float fallVelocity = _rb.velocity.y;
        _rb.isKinematic = _grounded;
        _characterController.enabled = _grounded;

        bool startedFalling = wasGrounded && !_grounded;
        if (startedFalling)
        {
            _rb.velocity = Vector3.ProjectOnPlane(_motionVelocity, Vector3.up);
        }
        bool justGrounded = !wasGrounded && _grounded;
        if (justGrounded)
        {
            OnGrounded?.Invoke(fallVelocity);
        }
    }

    private bool CheckGroundedSphere()
    {
        return Physics.CheckSphere(
            transform.position + transform.up * groundCheckOffset,
            _characterController.radius, groundLayerMask, QueryTriggerInteraction.Ignore);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = _grounded ? Color.red : Color.green;
        Gizmos.DrawLine(transform.position, transform.position + finalDir);
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Gizmos.DrawWireSphere(transform.up * groundCheckOffset, GetComponent<CharacterController>().radius);
    }
}

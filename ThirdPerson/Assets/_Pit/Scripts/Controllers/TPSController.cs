using UnityEngine;
using UnityEngine.InputSystem;

public class TPSController : MonoBehaviour
{
    [Header("Ground")]
    [SerializeField] LayerMask groundLayerMask = int.MaxValue;
    [SerializeField] float groundCheckOffset = 0f;
    [SerializeField] float toGroundForce = 3f;
    [SerializeField] float goingUpThreshold = 1f; //If the character y velocity surpasses this (before ground force), then it's going up.

    [Header("Motion")]
    [SerializeField] float motionInputLerp = 10f;
    [SerializeField] float movingThreshold = 0.2f;

    [Header("Turning")]
    [SerializeField] float viewInputLerp = 10f;
    [SerializeField] float turningSpeed = 2f;
    [SerializeField] float maxViewInput = 100f;

    private Controls _controls;
    private Animator _anim;
    private CharacterController _characterController;
    private Rigidbody _rb;
    private Vector2 _rawMotionInput;
    private Vector2 _motionInput;
    private Vector2 _rawViewInput;
    private Vector2 _viewInput;
    private Vector3 _velocity;
    private bool _grounded = true;
    private bool _goingUp = false;

    private void Awake()
    {
        _controls = InputManager.controls;
        _anim = GetComponent<Animator>();
        _characterController = GetComponent<CharacterController>();
        _rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        _grounded = true;
        _rb.isKinematic = true;
        _characterController.enabled = true;
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
        SetAnimatorParameters();
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

    private void SetAnimatorParameters()
    {
        _anim.SetFloat("MotionX", _motionInput.x);
        _anim.SetFloat("MotionY", _motionInput.y);
        _anim.SetFloat("ViewX", _viewInput.x / maxViewInput);
        _anim.SetBool("Moving", _motionInput.magnitude > movingThreshold || _rawMotionInput.magnitude > movingThreshold);
    }

    private void CheckGroundedAndFalling()
    {
        bool wasGrounded = _grounded;

        _grounded = Physics.CheckSphere(
            transform.position + transform.up * groundCheckOffset, 
            _characterController.radius, groundLayerMask, QueryTriggerInteraction.Ignore);

        _rb.isKinematic = _grounded;
        _characterController.enabled = _grounded;

        bool startedFalling = wasGrounded && !_grounded;
        if (startedFalling)
        {
            _rb.velocity = Vector3.ProjectOnPlane(_velocity, Vector3.up);
        }
    }

    private void OnAnimatorMove()
    {
        if (_grounded)
        {
            Vector3 animationMovement = _anim.deltaPosition;
            animationMovement.y -= 0.03f; //Makes slope limit reliable

            _characterController.Move(animationMovement);
            _goingUp = _characterController.velocity.y > goingUpThreshold;
            _velocity = animationMovement / Time.deltaTime;

            if (!_goingUp)
            {
                Vector3 toGroundMovement = new Vector3(0f, -toGroundForce * Time.deltaTime, 0f);
                _characterController.Move(toGroundMovement);
            }
            
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = _grounded ? Color.red : Color.green;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Gizmos.DrawWireSphere(transform.up * groundCheckOffset, GetComponent<CharacterController>().radius);
    }
}

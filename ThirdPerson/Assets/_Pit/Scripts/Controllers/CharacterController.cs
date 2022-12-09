using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterController : MonoBehaviour, Controls.IMovementMapActions
{
    [SerializeField] ForceMode forceMode;
    [SerializeField] float speedMultiplier = 1f;
    [SerializeField] float characterRotationSpeed = 1f;
    [SerializeField][Range(0,1)] float characterDirWeight = 0.5f;

    private Rigidbody _rb;
    private Transform _cam;
    private Vector2 _inputMotion = Vector2.zero;

    public void OnJump(InputAction.CallbackContext context)
    {
        throw new System.NotImplementedException();
    }

    public void OnMotion(InputAction.CallbackContext context) => _inputMotion = context.ReadValue<Vector2>();
    private void Awake()
    {
        InputManager.controls.MovementMap.SetCallbacks(this);
        InputManager.controls.MovementMap.Enable();
        _rb = GetComponent<Rigidbody>();
        _cam = Camera.main.transform;
    }

    private void FixedUpdate()
    {
        float magnitude = Mathf.Clamp01(_inputMotion.magnitude);
        if (magnitude == 0f) return;
        Vector3 rawInput = new Vector3(_inputMotion.x, 0f, _inputMotion.y);
        Vector3 inputViewDir = _cam.TransformDirection(rawInput);
        inputViewDir = Vector3.ProjectOnPlane(inputViewDir, Vector3.up).normalized;
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(inputViewDir), characterRotationSpeed * Time.fixedDeltaTime);

        Vector3 inputCharacterDir = Vector3.Lerp(inputViewDir, transform.forward, characterDirWeight) * magnitude;
        //inputCharacterDir = Vector3.ProjectOnPlane(inputCharacterDir, Vector3.up).normalized * magnitude;
        //Debug.Log(inputCharacterDir);
        //inputViewDir *= magnitude;

        _rb.AddForce(inputCharacterDir * speedMultiplier * Time.fixedDeltaTime, forceMode);
        //transform.rotation = Quaternion.LookRotation(inputViewDir);
    }
}

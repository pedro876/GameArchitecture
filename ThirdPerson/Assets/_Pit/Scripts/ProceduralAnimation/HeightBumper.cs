using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeightBumper : MonoBehaviour
{
    [SerializeField] FlyweightCurve flyweightCurve;
    [SerializeField] float downForce = 0.03f;
    [SerializeField] float timeStretch = 1f;

    private Transform _rig;
    private CharacterController _characterController;
    private TPSController2 _tpsController;

    private float _originalHeight;
    private float _currentHeight;
    private Vector3 _originalCenter;
    private float _maxTime;
    private float _originalRigScaleY;

    private bool _runningAnim = false;
    private float _runningTime = 0f;
    private float _minHeight;

    private Vector3 _pointBottom;
    private Vector3 _pointTop;

    private void Awake()
    {
        _rig = transform;
        _characterController = GetComponentInParent<CharacterController>();
        _tpsController = GetComponentInParent<TPSController2>();
        _tpsController.OnGrounded += ApplyDownForce;
    }

    void Start()
    {
        _originalHeight = _characterController.height;
        _originalCenter = _characterController.center;
        _maxTime = flyweightCurve.curve.keys[flyweightCurve.curve.length - 1].time * timeStretch;
        _pointBottom = transform.position + _originalCenter - transform.forward * _originalHeight * 0.5f;
        _pointTop = transform.position + _originalCenter + transform.forward * _originalHeight * 0.5f;
        _originalRigScaleY = _rig != null ? _rig.localScale.z : 1f;
    }

    private void ApplyDownForce(float fallVelocity)
    {
        if (!isActiveAndEnabled) return;
        fallVelocity *= downForce;
        //Debug.Log("Applying down force: " + fallVelocity);
        _minHeight = _originalHeight + fallVelocity;
        _runningAnim = true;
        _runningTime = 0f;
    }
    

    private void LateUpdate()
    {
        Simulate();
        ApplyOnRig();
    }

    private void Simulate()
    {
        _pointBottom = transform.position + _originalCenter - transform.forward * _originalHeight * 0.5f;
        if (_runningAnim)
        {
            _runningTime += Time.deltaTime;
            if (_runningTime > _maxTime)
            {
                _runningTime = _maxTime;
                _runningAnim = false;
            }

            _currentHeight = Mathf.LerpUnclamped(_minHeight, _originalHeight, flyweightCurve.curve.Evaluate(_runningTime / timeStretch));
        }
        else _currentHeight = _originalHeight;
        Vector3 center = _originalCenter;
        center.y = center.y - (_originalHeight - _currentHeight) * 0.5f;
        _pointTop = transform.position + center + transform.forward * _currentHeight * 0.5f;
    }

    private void ApplyOnRig()
    {
        if (_rig == null) return;
        float newScaleY = (_currentHeight / _originalHeight)*_originalRigScaleY;
        Vector3 currentScale = _rig.localScale;
        if (!currentScale.z.Equals(newScaleY))
        {
            currentScale.z = newScaleY;
            _rig.localScale = currentScale;
        }
    }

    private void OnDrawGizmos()
    {
        if (!isActiveAndEnabled) return;
        if (!Application.isPlaying)
        {
            CharacterController characterController = GetComponentInParent<CharacterController>();
            _pointBottom = transform.position + characterController.center - transform.forward * characterController.height * 0.5f;
            _pointTop = transform.position + characterController.center + transform.forward * characterController.height * 0.5f;
        }
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(_pointBottom, 0.1f);
        Gizmos.DrawSphere(_pointTop, 0.1f);
    }
}

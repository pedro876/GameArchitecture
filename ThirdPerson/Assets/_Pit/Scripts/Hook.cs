using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hook : MonoBehaviour
{
    public enum State
    {
        READY,
        LAUNCHED,
        HOOKED,
        RAPEL,
    }
    public State state;

    [SerializeField] float rapelImpulseForce = 100f;
    [SerializeField] float rapelForce = 10f;
    [SerializeField] float launchForce = 10f;

    [Header("References")]
    [SerializeField] Rigidbody playerRb;
    [SerializeField] Transform target;
    private Rigidbody _targetRb;
    private TriggerChecker _triggerChecker;

    private Vector3 _originalTargetPos;
    private Quaternion _originalTargetRot;


    private void Awake()
    {
        state = State.READY;
        _targetRb = target.GetComponent<Rigidbody>();
        _triggerChecker = target.GetComponent<TriggerChecker>();
        _originalTargetPos = target.localPosition;
        _originalTargetRot = target.localRotation;
    }

    public void SetState(State newState)
    {
        //Exceptions
        if (newState == State.RAPEL && state == State.READY) return; 

        //Apply
        switch (newState)
        {
            case State.READY:
                break;
            case State.LAUNCHED:
                Restore();
                target.SetParent(null, true);
                _targetRb.isKinematic = false;
                _targetRb.velocity = target.forward * launchForce;
                state = State.LAUNCHED;
                break;
            case State.HOOKED:
                _targetRb.isKinematic = true;
                Vector3 rapelDir = (target.position - transform.position).normalized;
                playerRb.AddForce(rapelDir * rapelImpulseForce, ForceMode.Impulse);
                break;
            case State.RAPEL:
                Restore();
                break;
        }
        state = newState;
    }

    private void Restore()
    {
        _targetRb.isKinematic = true;
        target.SetParent(transform);
        target.localPosition = _originalTargetPos;
        target.localRotation = _originalTargetRot;
    }

    private void FixedUpdate()
    {
        switch (state)
        {
            case State.LAUNCHED:
                if (_triggerChecker.IsColliding)
                {
                    SetState(State.HOOKED);
                }
                break;
            case State.HOOKED:
                Vector3 rapelDir = (target.position - transform.position).normalized;
                playerRb.AddForce(rapelDir * rapelForce * Time.fixedDeltaTime * 1000f);
                break;
        }
    }
}

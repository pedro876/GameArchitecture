using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AccelerationTilter : MonoBehaviour
{
    private TPSController2 _tpsController;
    private Transform _rig;

    [SerializeField] float lerp = 8f;
    [SerializeField] float maxTilt = 20f;
    Quaternion originalRot;

    Vector3 velocity;

    private void Awake()
    {
        _rig = transform;
        _tpsController = GetComponentInParent<TPSController2>();
    }

    private void Start()
    {
        originalRot = _rig.localRotation;
    }

    private void LateUpdate()
    {
        velocity = Vector3.Lerp(velocity, _tpsController.MotionVelocity, lerp * Time.deltaTime);

        float tiltAngle = (velocity.magnitude / _tpsController.MaxVelocity)*maxTilt;

        //y back
        //x right

        float weightX = Vector3.Dot(_rig.right, velocity.normalized);
        float weightY = Vector3.Dot(-_rig.up, velocity.normalized);

        Quaternion tiltRot = Quaternion.Euler(tiltAngle*weightY, tiltAngle*weightX, 0f);

        _rig.localRotation =  originalRot* tiltRot;
    }
}

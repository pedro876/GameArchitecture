using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RopeTest : MonoBehaviour
{
    //Debemos lanzar una bola desde el origen que sea independiente de la cuerda
    //Conforme la bola se aleja, generamos una esfera de cuerda por cada 5 (distance) unidades
    [SerializeField] float timeScale = 1f;
    [SerializeField] float extraGravity = 0f;
    [SerializeField] float randomness = 1f;
    [SerializeField, Range(0,1)] float ignoreCompressed = 1f;
    [SerializeField] bool correctAdvancedSpheres = true;
    [SerializeField] int maxJoints = 10;
    [SerializeField] float launchVelocity = 10f;
    [SerializeField] float drag = 2f;
    [SerializeField] float springConstantK = 10f;
    [SerializeField] float maxSpringLength = 5f;
    [SerializeField, Range(-1, 1)] float velocityPctWhenStopeed = 0f;
    [SerializeField, Range(0, 1)] float springLengthMultiplier = 1f;

    private List<Rigidbody> _spheres;
    private List<Transform> _cylinders;
    private TriggerChecker _collisionChecker;
    private bool _stopped = false;
    private float _ropeLength = 0f;

    private Rigidbody OriginSphere => _spheres[0];
    private Rigidbody DestinySphere => _spheres[_spheres.Count - 1];
    private int JointCount => _spheres.Count - 2;

    private void Awake()
    {
        //Origin
        _spheres = new List<Rigidbody>();
        _spheres.Add(transform.GetChild(0).GetComponent<Rigidbody>());
        OriginSphere.isKinematic = true;

        //Launch
        _spheres.Add(transform.GetChild(1).GetComponent<Rigidbody>());
        DestinySphere.isKinematic = false;
        _collisionChecker = DestinySphere.GetComponent<TriggerChecker>();
        LaunchSphere(DestinySphere);
        foreach (var sphere in _spheres) sphere.drag = 0f;

        //Cylinder
        _cylinders = new List<Transform>();
        _cylinders.Add(transform.GetChild(2));
    }

    private void FixedUpdate()
    {
        Time.timeScale = timeScale;
        if (_collisionChecker.IsColliding)
        {
            _stopped = true;
            _spheres[_spheres.Count - 1].isKinematic = true;
            foreach (var sphere in _spheres)
            {
                sphere.drag = drag;
                sphere.velocity = velocityPctWhenStopeed * sphere.velocity;
            }
            _ropeLength = ComputeLength();
        }

        if (JointCount > 0 && (JointCount >= maxJoints || _stopped))
        {
            float springLength = CalculateSpringLength();
            for (int i = 1; i < _spheres.Count-1; i++)
            {
                _spheres[i].AddForce(GetSpringForce(i, springLength));
            }
        }

        for (int i = 1; i < _spheres.Count; i++)
        {
            _spheres[i].AddForce(Vector3.down * extraGravity);
        }

        if (!_stopped)
        {
            _ropeLength = ComputeLength();
            //Check if we must add a joint
            bool mustAddJoint = JointCount < maxJoints && _ropeLength / (JointCount+1) > maxSpringLength;
            if (mustAddJoint)
            {
                var joint = CreateJoint();
                //Should we add a launch force for the new generated joint?
                LaunchSphere(joint);
            }
        }

        //Put cylinders
        for(int i = 0; i < _spheres.Count-1; i++)
        {
            Vector3 middle = (_spheres[i].transform.position + _spheres[i + 1].transform.position) *0.5f;
            Vector3 dir = _spheres[i + 1].transform.position - _spheres[i].transform.position;
            float mag = dir.magnitude;
            if(mag > 0f)
            {
                Quaternion rot = Quaternion.LookRotation(dir) * Quaternion.Euler(new Vector3(90f, 0f, 0f));
                _cylinders[i].transform.rotation = rot;
            }
            float scale = mag * 0.5f;
            _cylinders[i].transform.position = middle;
            _cylinders[i].transform.localScale = new Vector3(_cylinders[i].transform.localScale.x, scale, _cylinders[i].transform.localScale.z);
        }
    }

    private float CalculateSpringLength()
    {
        return _ropeLength / (JointCount + 1) * springLengthMultiplier;
    }

    private Vector3 GetSpringForce(int index, float springLength)
    {
        Vector3 prevPos = _spheres[index - 1].position;
        Vector3 nextPos = _spheres[index + 1].position;
        Vector3 currentPos = _spheres[index].position;


        Vector3 toPrev = prevPos - currentPos;
        Vector3 toNext = nextPos - currentPos;
        if (correctAdvancedSpheres)
        {
            bool hasPassedNext = Vector3.Dot(nextPos - OriginSphere.position, toNext) < -0.1f; //Si la dirección desde prev a next es contraria a la dirección desde current a next
            if (hasPassedNext)
            {
                //Debug.Log("Has passed: " + index);
                toNext = (OriginSphere.position - currentPos).normalized* springLength*1.1f;
            }
        }
        float toPrevDisplacement = toPrev.magnitude - springLength;
        float toNextDisplacement = toNext.magnitude - springLength;
        if (toPrevDisplacement < 0f) toPrevDisplacement *= (1f-ignoreCompressed);
        if (toNextDisplacement < 0f) toNextDisplacement *= (1f-ignoreCompressed);

        Vector3 toPrevForce = toPrev.normalized * toPrevDisplacement * springConstantK;
        Vector3 toNextForce = toNext.normalized * toNextDisplacement * springConstantK;


        Vector3 finalForce = toPrevForce + toNextForce;

        if (randomness > 0f)
        {
            finalForce += new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * randomness;
        }

        return finalForce;
    }

    private Rigidbody CreateJoint()
    {
        var jointObj = Instantiate(OriginSphere, transform);
        var rb = jointObj.GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.drag = 0f;

        var cylinder = Instantiate(_cylinders[0], transform);
        _cylinders.Add(cylinder);

        //Where do we add the joint?
        //- Before the launch sphere?
        //- After the origin sphere?
        //- In the middle of the longest segment?
        //I think it should be after the origin sphere, to simulate that it is continously launching rope
        _spheres.Insert(1, rb);
        return rb;
    }

    private void LaunchSphere(Rigidbody sphere)
    {
        sphere.transform.position = OriginSphere.transform.position;
        sphere.velocity = transform.forward * launchVelocity;
    }

    private float ComputeLength()
    {
        float totalLength = 0f;
        for(int i = 0; i < _spheres.Count-1; i++)
        {
            totalLength += Vector3.Distance(_spheres[i].position, _spheres[i + 1].position);
        }
        return totalLength;
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        if (JointCount > 0)
        { 
            float springLength = CalculateSpringLength() / springLengthMultiplier;
            for (int i = 0; i < _spheres.Count - 1; i++)
            {
                Vector3 toNext = _spheres[i + 1].position - _spheres[i].position;
                float tension = toNext.magnitude - springLength;
                Gizmos.color = Color.Lerp(Color.blue, Color.red, tension/3f);
                Gizmos.DrawLine(_spheres[i].position, _spheres[i + 1].position);
            }
        }
    }
}   

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architecture.Services;
using UnityEngine.InputSystem;
using Architecture.ObserverGroup;

public class CubeMove : MonoBehaviour
{
    [SerializeField] InputActionMap map;

    void Start()
    {
        map["up"].performed += (evt) =>
        {
            Debug.Log("up");
            Move(Vector3.forward);
        };
        map["down"].performed += (evt) =>
        {
            Debug.Log("down");
            Move(-Vector3.forward);
        };
        map["left"].performed += (evt) =>
        {
            Debug.Log("left");
            Move(-Vector3.right);
        };
        map["right"].performed += (evt) =>
        {
            Debug.Log("right");
            Move(Vector3.right);
        };
        map.Enable();
    }

    private void Move(Vector3 displacement)
    {
        transform.position += displacement;
    }
}

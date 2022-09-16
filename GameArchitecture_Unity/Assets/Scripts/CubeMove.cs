using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architecture.ServiceLocator;
using UnityEngine.InputSystem;
using Architecture.ObserverGroup;

public enum CubeDirections
{
    up, down, left, right,
}

public class CubeMove : MonoBehaviour
{
    [SerializeField] InputActionMap map;

    private void Awake()
    {
        //EventGroup<CubeDirections>.Subscribe(this, CubeDirections.up);
        //EventGroup<CubeDirections>.enableLog = true;
    }

    void Start()
    {
        /*string[] names = System.Enum.GetNames(typeof(CubeDirections));
        for(int i = 0; i < names.Length; i++)
        {
            map[names[i]].performed += (evt) => 
        }*/
        map["up"].performed += (evt) =>
        {
            ObserverGroup<CubeDirections>.InvokeEvent(CubeDirections.up, "arriba");
            Move(Vector3.forward);
        };
        map["down"].performed += (evt) =>
        {
            ObserverGroup<CubeDirections>.InvokeEvent(CubeDirections.down, "abajo");
            Move(-Vector3.forward);
        };
        map["left"].performed += (evt) =>
        {
            ObserverGroup<CubeDirections>.InvokeEvent(CubeDirections.left, "izquierda");
            Move(-Vector3.right);
        };
        map["right"].performed += (evt) =>
        {
            ObserverGroup<CubeDirections>.InvokeEvent(CubeDirections.right, "derecha");
            Move(Vector3.right);
        };
        map.Enable();
    }

    private void Move(Vector3 displacement)
    {
        transform.position += displacement;
    }
}

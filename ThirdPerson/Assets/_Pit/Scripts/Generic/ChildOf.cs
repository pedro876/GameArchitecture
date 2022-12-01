using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildOf : MonoBehaviour
{
    [SerializeField] Transform parent;
    [SerializeField] bool worldPositionStays = true;

    private void Awake()
    {
        transform.SetParent(parent, worldPositionStays);
        DestroyImmediate(this);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeScale : MonoBehaviour
{
    [SerializeField] float scale = 1f;

    // Update is called once per frame
    void Update()
    {
        Time.timeScale = scale;
    }
}

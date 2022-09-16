using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Architecture.ObserverGroup;

public class EvtLog : Observer<CubeDirections>
{
    TextMeshProUGUI text;
    //[SerializeField] string otherProperties = "HelloWorld";

    public override void Notify(CubeDirections cd, object evtInfo)
    {
        text.text += $"\nevt: {cd}, evtInfo: {(string)evtInfo}";
        Debug.Log(cd);
    }

    protected override void DoAwake()
    {
        text = GetComponent<TextMeshProUGUI>();
        text.text = "None";
    }
}

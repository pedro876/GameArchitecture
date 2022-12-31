using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Flyweight Curve", menuName = "Pit/New Flyweight Curve")]
public class FlyweightCurve : ScriptableObject
{
    [SerializeField] public AnimationCurve curve;
}

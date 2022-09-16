using System.Collections;
using UnityEngine;
using System;

namespace Architecture.ObserverGroup
{
    public interface IObserver<TEnum> where TEnum : Enum
    {
        void Notify(TEnum evt, object evtInfo);
        string GetName();
    }
}
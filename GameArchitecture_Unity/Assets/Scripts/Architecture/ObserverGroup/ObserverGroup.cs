using System;
using System.Collections.Generic;
using UnityEngine;
namespace Architecture.ObserverGroup
{
    public static class ObserverGroup<TEnum> where TEnum : Enum
    {

        public static bool enableLog = false;

        private static Dictionary<TEnum, HashSet<IObserver<TEnum>>> _observers = new Dictionary<TEnum, HashSet<IObserver<TEnum>>>();

        private static readonly string HEADER = $"[EVENT GROUP<{typeof(TEnum)}>] ";

        static ObserverGroup()
        {
            Type enumType = typeof(TEnum);
            string[] names = Enum.GetNames(enumType);
            for(int i = 0; i < names.Length; i++)
            {
                TEnum enumValue = (TEnum)Enum.Parse(enumType, names[i]);
                _observers[enumValue] = new HashSet<IObserver<TEnum>>();
            }
        }

        public static bool InvokeEvent(TEnum evt, object evtInfo = null)
        {
            int observerCount = _observers[evt].Count;
            if(observerCount > 0)
            {
                foreach (var observer in _observers[evt])
                {
                    observer.Notify(evt, evtInfo);
                }
            }

            if (enableLog)
                Debug.Log($"{HEADER}Event {evt} invoked, {observerCount} observers listened");

            return observerCount > 0;
        }

        public static void Subscribe(IObserver<TEnum> observer, TEnum evt)
        {
            if (enableLog && _observers[evt].Contains(observer)) //Already subscribed
            {
                Debug.LogWarning($"{HEADER}Observer {observer.GetName()} was already subscribed to event {evt}");
            }
            
            _observers[evt].Add(observer);
        }

        public static void Subscribe(IObserver<TEnum> observer, List<TEnum> evtList)
        {
            foreach (var evt in evtList)
                Subscribe(observer, evt);
        }

        public static void SubscribeAll(IObserver<TEnum> observer)
        {
            foreach(var evt in _observers)
            {
                evt.Value.Add(observer);
            }
        }

        public static void Unsubscribe(IObserver<TEnum> observer, TEnum evt)
        {
            if (_observers[evt].Contains(observer))
            {
                _observers[evt].Remove(observer);
                return;
            }

            if(enableLog)
                Debug.LogWarning($"{HEADER}Observer {observer.GetName()} was not subscribed to {evt}");
        }

        public static void Unsubscribe(IObserver<TEnum> observer, List<TEnum> evtList)
        {
            foreach(var evt in evtList)
                Unsubscribe(observer, evt);
        }

        public static void UnsubscribeAll(IObserver<TEnum> observer)
        {
            foreach(var evt in _observers)
            {
                evt.Value.Remove(observer);
            }
        }
    }
}

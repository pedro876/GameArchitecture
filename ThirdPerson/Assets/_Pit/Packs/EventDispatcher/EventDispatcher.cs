using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pit.EventDispatching
{
    public static class EventDispatcher
    {
        private static Dictionary<Enum, ListenerGroup> _groups = new Dictionary<Enum, ListenerGroup>();
        private static Dictionary<Type, Enum[]> _enumsPerType = new Dictionary<Type, Enum[]>();

        #region ObserverGroup

        private class ListenerGroup
        {
            private Enum _evt;
            private HashSet<IEventListener> _listeners;
            private HashSet<IEventListener> _listenersToAdd;
            private HashSet<IEventListener> _listenersToRemove;
            private bool _isNotifying;

            public ListenerGroup(Enum evt)
            {
                _evt = evt;
                _isNotifying = false;
                _listeners = new HashSet<IEventListener>();
                _listenersToAdd = new HashSet<IEventListener>();
                _listenersToRemove = new HashSet<IEventListener>();
            }

            public void Invoke(object evtInfo)
            {
                _isNotifying = true;
                foreach (var observer in _listeners)
                {
                    observer.Notify(_evt, evtInfo);
                }
                _isNotifying = false;

                foreach(var observer in _listenersToAdd)
                    _listeners.Add(observer);
                _listenersToAdd.Clear();
                foreach (var observer in _listenersToRemove)
                    _listeners.Remove(observer);
                _listenersToRemove.Clear();
            }

            public int Count => _listeners.Count;

            public void AddListener(IEventListener listener)
            {
                if (_isNotifying)
                    _listenersToAdd.Add(listener);
                else
                    _listeners.Add(listener);
            }

            public void RemoveListener(IEventListener listener)
            {
                if (_isNotifying)
                    _listenersToRemove.Add(listener);
                else
                    _listeners.Remove(listener);
            }
        }

        #endregion

        #region Invocation

        public static bool Invoke(Enum evt, object evtInfo = null)
        {
            Type type = evt.GetType();
            int listenerCount = 0;
            if(_groups.TryGetValue(evt, out ListenerGroup group))
            {
                group.Invoke(evtInfo);
                listenerCount = group.Count;
            }
            return listenerCount > 0;
        }

        #endregion

        #region Subscription

        public static void Subscribe(IEventListener listener, Enum evt)
        {
            GetGroup(evt).AddListener(listener);
        }

        public static void Subscribe(IEventListener listener, List<Enum> evts)
        {
            foreach(var evt in evts)
                GetGroup(evt).AddListener(listener);
        }

        public static void SubscribeAll<TEnum>(IEventListener listener) where TEnum : Enum
        {
            SubscribeAll(typeof(TEnum), listener);
        }

        public static void SubscribeAll(Type type, IEventListener listener)
        {
            Enum[] values = GetEnumsForType(type);
            foreach (var value in values)
                Subscribe(listener, value);
        }

        #endregion

        #region Unsubscription

        public static void Unsubscribe(IEventListener listener, Enum evt)
        {
            GetGroup(evt).RemoveListener(listener);
        }

        public static void Unsubscribe(IEventListener listener, List<Enum> evts)
        {
            foreach (var evt in evts)
                GetGroup(evt).RemoveListener(listener);
        }

        public static void UnsubscribeAll<TEnum>(IEventListener listener) where TEnum : Enum
        {
            UnsubscribeAll(typeof(TEnum), listener);
        }

        public static void UnsubscribeAll(Type type, IEventListener listener)
        {
            Enum[] values = GetEnumsForType(type);
            foreach (var value in values)
                Unsubscribe(listener, value);
        }

        #endregion

        #region SafeGetters

        private static ListenerGroup GetGroup(Enum evt)
        {
            if (!_groups.ContainsKey(evt))
                _groups.Add(evt, new ListenerGroup(evt));
            return _groups[evt];
        }

        private static Enum[] GetEnumsForType(Type type)
        {
            Enum[] values;
            if(_enumsPerType.TryGetValue(type, out values))
            {
                return values;
            }
            else
            {
                string[] names = Enum.GetNames(type);
                values = new Enum[names.Length];
                for (int i = 0; i < names.Length; i++)
                {
                    values[i] = (Enum)Enum.Parse(type, names[i]);
                }
                _enumsPerType[type] = values;
                return values;
            }
        }
        #endregion
    }
}

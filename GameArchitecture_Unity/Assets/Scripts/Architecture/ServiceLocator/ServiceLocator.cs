using System;
using System.Collections.Generic;
using UnityEngine;

namespace Architecture.ServiceLocator
{
    public static class ServiceLocator
    {
        private const string HEADER = "[SERVICES] ";

        private static Dictionary<Type, object> _services = new Dictionary<Type, object>();
        public static bool enableLog = false;

        static ServiceLocator()
        {
            Debug.Log($"{HEADER}Instantiated ServiceLocator");
        }

        public static T Get<T>()
        {
            Type type = typeof(T);
            try
            {
                object value = _services[type];
                return (T)value;
            }
            catch (KeyNotFoundException)
            {
                if(enableLog) Debug.LogError($"{HEADER}Service {type} not found");
                return default(T);
            }
        }

        public static T Set<T>(T overrideService)
        {
            Type type = typeof(T);
            if (_services.ContainsKey(type))
            {
                if (enableLog) Debug.LogWarning($"{HEADER}Service {type} overridden");
            }
            _services[type] = overrideService;
            return overrideService;
        }

        public static void Clean()
        {
            if (enableLog) Debug.LogWarning($"{HEADER}Cleaning {_services.Count} services");
            _services.Clear();
        }
    }
}
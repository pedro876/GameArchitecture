using System;
using System.Collections.Generic;
using UnityEngine;

namespace Architecture.Services
{
    public static class ServiceLocator
    {
        public static bool enableLog = true;

        private static Dictionary<Type, object> _services = new Dictionary<Type, object>();

        private const string HEADER = "[SERVICES] ";

        static ServiceLocator()
        {
            Debug.Log($"{HEADER}Instantiated ServiceLocator".AddSuccessPrefix());
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
            if (enableLog)
            {
                if (_services.ContainsKey(type))
                {
                    Debug.LogWarning($"{HEADER}Service {type} overridden");
                }
                else
                    Debug.Log($"{HEADER}Service {type} registered".AddSuccessPrefix());
            }
            
            _services[type] = overrideService;
            return overrideService;
        }

        public static bool Has<T>()
        {
            return _services.ContainsKey(typeof(T));
        }

        public static void Clean()
        {
            if (enableLog) Debug.LogWarning($"{HEADER}Cleaning {_services.Count} services");
            _services.Clear();
        }
    }
}
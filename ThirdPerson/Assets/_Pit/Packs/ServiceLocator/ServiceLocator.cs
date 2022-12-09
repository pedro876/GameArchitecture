using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Pit.Services
{
    public static partial class ServiceLocator
    {
        public static bool enableLog = true;
        private const string HEADER = "[SERVICES] ";

        private static Dictionary<Type, object> _services = new Dictionary<Type, object>();
        private static Dictionary<Type, int> _requesters = new Dictionary<Type, int>();
        private static Dictionary<Type, SemaphoreSlim> _semaphores = new Dictionary<Type, SemaphoreSlim>();

        static ServiceLocator()
        {
            Debug.Log($"{HEADER}Instantiated ServiceLocator");
        }

        public static bool Has<T>() => _services.ContainsKey(typeof(T));

        public static T Get<T>() where T : class
        {
            Type type = typeof(T);
            try
            {
                object value = _services[type];
                return (T)value;
            }
            catch (KeyNotFoundException)
            {
                if (enableLog)
                    Debug.LogError($"{HEADER}Service {type} not found");
                return null;
            }
        }

        public static async void GetAsync<T>(Action<T> callback) where T : class
        {
            if (!Has<T>())
            {
                Type type = typeof(T);
                if (!_semaphores.ContainsKey(type))
                {
                    _semaphores[type] = new SemaphoreSlim(0);
                    _requesters.Add(type, 0);
                }
                _requesters[type] = _requesters[type] + 1;
                await _semaphores[type].WaitAsync();
            }
            callback(Get<T>());
        }

        public static T Set<T>(T service, bool overrideService = true) where T : class
        {
            Type type = typeof(T);
            if (_services.ContainsKey(type))
            {
                if (overrideService)
                {
                    _services[type] = service;
                    RunHooks<T>();
                    Debug.LogWarning($"{HEADER}Service {type} overridden");
                }
            }
            else
            {
                _services[type] = service;
                if (_requesters.ContainsKey(type) && _requesters[type] > 0)
                {
                    // Requesters were waiting asynchronously for this
                    _semaphores[type].Release(_requesters[type] - _semaphores[type].CurrentCount);
                    _requesters[type] = 0;
                } 
                    

                RunHooks<T>();
                Debug.Log($"{HEADER}Service {type} registered");
            }

            return service;
        }

        public static void Clean()
        {
            if (enableLog) Debug.LogWarning($"{HEADER}Cleaning {_services.Count} services");
            _services.Clear();
            _requesters.Clear();
            _hooks.Clear();
            _hooksToAdd.Clear();
            _hooksToRemove.Clear();
            _semaphores.Clear();
            _runningHooks.Clear();
        }
    }
}
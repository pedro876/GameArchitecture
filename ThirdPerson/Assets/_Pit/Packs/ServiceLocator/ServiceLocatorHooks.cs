using System;
using System.Collections.Generic;

namespace Pit.Services
{
    public static partial class ServiceLocator
    {
        private static Dictionary<Type, object> _hookedServices = new Dictionary<Type, object>();
        private static Dictionary<Type, HashSet<Action>> _hooks = new Dictionary<Type, HashSet<Action>>();
        private static Dictionary<Type, bool> _runningHooks = new Dictionary<Type, bool>();
        private static Dictionary<Type, HashSet<Action>> _hooksToRemove = new Dictionary<Type, HashSet<Action>>();
        private static Dictionary<Type, HashSet<Action>> _hooksToAdd = new Dictionary<Type, HashSet<Action>>();

        public static HookService<T> GetHook<T>() where T : class
        {
            Type type = typeof(T);
            if (!_hookedServices.ContainsKey(type))
            {
                _hookedServices.Add(type, new InstantiableHookService<T>());
            }
            return (_hookedServices[type] as HookService<T>);
        }

        private class InstantiableHookService<TService> : HookService<TService> where TService : class
        {
            public InstantiableHookService()
            {
                Hook<TService>(() => {
                    service = Get<TService>();
                    if(service != null)
                        OnModified();
                });
            }
        }

        public static void Hook<T>(Action action) where T : class
        {
            Type type = typeof(T);
            if (_runningHooks.ContainsKey(type) && _runningHooks[type]) // Collection of hooks is being iterated right now, postponing addition
            {
                if (!_hooksToAdd.ContainsKey(type))
                    _hooksToAdd[type] = new HashSet<Action>() { action };
                else
                    _hooksToAdd[type].Add(action);
            }
            else // Collection is not being iterated, adding directly
            {
                if (!_hooks.ContainsKey(type))
                    _hooks.Add(type, new HashSet<Action>() { action });
                else
                    _hooks[type].Add(action);

                if (Has<T>())
                {
                    action();
                }
            }
        }

        public static void UnHook<T>(Action action)
        {
            Type type = typeof(T);
            if (_runningHooks.ContainsKey(type) && _runningHooks[type]) // Collection of hooks is being iterated right now, postponing removal
            {
                if (!_hooksToRemove.ContainsKey(type))
                    _hooksToRemove[type] = new HashSet<Action>() { action };
                else
                    _hooksToRemove[type].Add(action);
            }
            else // Collection is not being iterated, removing directly
            {
                if (_hooks.ContainsKey(type))
                    _hooks[type].Remove(action);
            }
        }

        private static void RunHooks<T>() where T : class
        {
            Type type = typeof(T);
            if (!_hooks.ContainsKey(type)) return;

            _runningHooks[type] = true;

            // Run hooks
            foreach (var hook in _hooks[type])
            {
                hook.Invoke();
            }

            // Remove hooks
            if (_hooksToRemove.ContainsKey(type))
            {
                foreach (var action in _hooksToRemove[type])
                {
                    _hooks[type].Remove(action);
                }
                _hooksToRemove[type].Clear();
            }

            // Add hooks
            if (_hooksToAdd.ContainsKey(type))
            {
                foreach (var action in _hooksToAdd[type])
                {
                    _hooks[type].Add(action);

                    // If a service of this type is registered already, run the action
                    if (_services.ContainsKey(type))
                    {
                        action();
                    }
                }
                _hooksToAdd[type].Clear();
            }

            _runningHooks[type] = false;
        }
    }
}
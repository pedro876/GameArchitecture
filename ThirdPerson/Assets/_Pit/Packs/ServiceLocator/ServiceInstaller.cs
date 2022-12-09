using UnityEngine;
using System;

namespace Pit.Services
{
    internal static class ServiceInstaller
    {
        [RuntimeInitializeOnLoadMethod]
        private static void InstallServices()
        {
            //ServiceLocator.Clean();
            //Log("Installing...");
            //ServiceLocator.Set<IInputManager>(CreateClassService<InputManager>(), false);
            //ServiceLocator.Set<ISceneHandler>(CreateClassService<SceneHandler>(), false);
            //ServiceLocator.Set<ISceneLoader>(CreateClassService<SceneLoader>(), false);
            //ServiceLocator.Set<IPlayerFactory>(CreateClassService<PlayerFactory>(), false);
            //ServiceLocator.Set<IGameManager>(CreateComponentService<GameManager>(), false);
            //Log("Finished");
        }

        private static TService CreateClassService<TService>() where TService : class, new()
        {
            return new TService();
        }

        private static TComponent CreateComponentService<TComponent>() where TComponent : Component
        {
            Type type = typeof(TComponent);
            var objs = UnityEngine.Object.FindObjectsOfType(type);
            foreach (var obj in objs)
            {
                UnityEngine.Object.DestroyImmediate(obj);
            }
            var gameObj = new GameObject(type.Name, type);
            var comp = gameObj.GetComponent<TComponent>();
            UnityEngine.Object.DontDestroyOnLoad(gameObj);
            return comp;
        }

//        private static void Log(string txt)
//        {
//#if UNITY_EDITOR
//            Debug.Log($"[SERVICE INSTALLER] {txt}");
//#endif
//        }
    }

}


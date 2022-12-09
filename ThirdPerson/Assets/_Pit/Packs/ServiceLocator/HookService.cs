using System;

namespace Pit.Services
{
    /// <summary>
    /// The implementation of this class is located inside the ServiceLocator. This way, only the ServiceLocator can manage the instances of this class.
    /// If it wasn't handled this way, the user would have to manually dispose of the instances in order to unhook the variable service when</summary>
    /// a service user is destroyed or it just doesn't need to use the service anymore.
    /// <typeparam name="TService"></typeparam>
    public abstract class HookService<TService> where TService : class
    {
        public event Action Modified;
        public TService service { get; protected set; }
        public static bool operator true(HookService<TService> hookedService) => hookedService.service != null;
        public static bool operator false(HookService<TService> hookedService) => hookedService.service == null;
        protected void OnModified()
        {
            Modified?.Invoke();
        }
    }

}

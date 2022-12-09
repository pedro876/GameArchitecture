using System;

namespace Pit.EventDispatching
{
    public interface IEventListener
    {
        void Notify(Enum evt, object evtInfo);
    }
}

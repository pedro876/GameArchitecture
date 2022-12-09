using System;
using System.Collections.Generic;

namespace Architecture.HFSM
{
    public sealed class HFSM<TContext>
    {
        public TContext context;
        private HState<TContext> _root;

        public HFSM(TContext context, params HState<TContext>[] states)
        {
            _root = new HState<TContext>(states);
            _root.SetContext(context);
            _root.Initialize();
            this.context = context;
        }

        public void ChangeContext(TContext newContext)
        {
            context = newContext;
            _root.SetContext(newContext);
        }
        public void SetState<T>() => _root.SetState<T>();
        public void Enter() => _root.Enter();
        public void Resume() => _root.Resume();
        public void Update() => _root.Update();
        public void LateUpdate() => _root.LateUpdate();
        public void FixedUpdate() => _root.FixedUpdate();
        public void Exit() => _root.Exit();
        public HState<TContext> GetCurrentState() => _root.GetCurrentSubState();
        public HState<TContext> GetLeafState() => _root.GetLeafState();
        public bool IsCurrentState<TState>() where TState : HState<TContext> => _root.IsCurrentState<TState>();
    }
}

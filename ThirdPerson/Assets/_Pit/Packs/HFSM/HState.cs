using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Architecture.HFSM
{
    public class HState<TContext>
    {
        public HState<TContext> parent;
        protected TContext context;

        private HState<TContext> _previousSubState;
        private HState<TContext> _currentSubState;
        private HState<TContext> _defaultSubState;
        private Dictionary<Type, HState<TContext>> _subStates;

        #region Instantiation

        public HState()
        {
            _subStates = new Dictionary<Type, HState<TContext>>();
        }

        public HState(params HState<TContext>[] subStates)
        {
            _subStates = new Dictionary<Type, HState<TContext>>();
            SetSubStates(subStates);
        }

        public HState<TContext> GetCurrentSubState()
        {
            if(_currentSubState == null)
            {
                return this;
            }
            else
            {
                return _currentSubState.GetCurrentSubState();
            }
        }

        public HState<TContext> GetLeafState()
        {
            if(_currentSubState == null)
            {
                return this;
            }
            else
            {
                return _currentSubState.GetLeafState();
            }
        }

        public bool IsCurrentState<TState>() where TState : HState<TContext>
        {
            if(this is TState)
            {
                return true;
            }
            else if(_currentSubState != null)
            {
                return _currentSubState.IsCurrentState<TState>();
            }
            return false;
        }

        public HState<TContext> SetSubStates(params HState<TContext>[] newSubStates)
        {
            _subStates.Clear();
            _currentSubState = null;
            _defaultSubState = null;
            if (newSubStates != null && newSubStates.Length > 0)
            {
                _defaultSubState = newSubStates[0];
                foreach (var child in newSubStates)
                {
                    child.SetParent(this);
                }
            }
            return this;
        }

        internal void SetContext(TContext context)
        {
            this.context = context;
            if (_subStates != null)
            {
                foreach (var subState in _subStates.Values)
                {
                    subState.SetContext(context);
                }
            }
        }

        internal void Initialize()
        {
            OnInitialized();
            foreach (var subState in _subStates.Values)
            {
                subState.Initialize();
            }
        }

        private void SetParent(HState<TContext> parent)
        {
            this.parent = parent;
            parent._subStates[GetType()] = this;
        }

        #endregion

        #region State
        public void SetState<TState>()
        {
            SetStateByType(typeof(TState));
        }

        public void SetPreviousState()
        {
            if (_previousSubState != null)
                SetStateByType(_previousSubState.GetType());
        }

        public void SetStateByType(Type type)
        {
            if (_subStates == null) return;
            if (_subStates.TryGetValue(type, out var newState))
            {
                _currentSubState?.Exit();
                _previousSubState = _currentSubState;
                _currentSubState = newState;
                newState?.Enter();
            }
            else
            {
                UnityEngine.Debug.LogWarning($"[HFSM] Intended state not found: {type.Name}");
            }
        }
        #endregion

        #region Execution
        internal void Enter()
        {
            _currentSubState = _defaultSubState;
            OnEnter();
            _currentSubState?.Enter();
        }

        internal void Resume()
        {
            OnResume();
            _currentSubState?.Enter();
        }

        internal void Update()
        {
            OnUpdate();
            _currentSubState?.Update();
        }

        internal void LateUpdate()
        {
            OnLateUpdate();
            _currentSubState?.LateUpdate();
        }

        internal void FixedUpdate()
        {
            OnFixedUpdate();
            _currentSubState?.FixedUpdate();
        }

        internal void Exit()
        {
            OnExit();
            _currentSubState?.Exit();
        }
        #endregion

        #region Events
        protected virtual void OnInitialized() { }
        protected virtual void OnEnter() { }
        protected virtual void OnResume() { }
        protected virtual void OnUpdate() { }
        protected virtual void OnLateUpdate() { }
        protected virtual void OnFixedUpdate() { }
        protected virtual void OnExit() { }
        #endregion
    }
}

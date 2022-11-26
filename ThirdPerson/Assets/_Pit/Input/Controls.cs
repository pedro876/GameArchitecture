//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.4.4
//     from Assets/_Pit/Input/Controls.inputactions
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public partial class @Controls : IInputActionCollection2, IDisposable
{
    public InputActionAsset asset { get; }
    public @Controls()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""Controls"",
    ""maps"": [
        {
            ""name"": ""CameraMap"",
            ""id"": ""2e81927d-937e-4217-9a09-b0cc27a02631"",
            ""actions"": [
                {
                    ""name"": ""Axis"",
                    ""type"": ""PassThrough"",
                    ""id"": ""36f263cb-9504-482e-973c-1c596a0d5dd2"",
                    ""expectedControlType"": ""Delta"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""a002a70a-f9c3-445d-964f-c94924ce957c"",
                    ""path"": ""<Mouse>/delta"",
                    ""interactions"": """",
                    ""processors"": ""ScaleVector2(x=100,y=100)"",
                    ""groups"": """",
                    ""action"": ""Axis"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // CameraMap
        m_CameraMap = asset.FindActionMap("CameraMap", throwIfNotFound: true);
        m_CameraMap_Axis = m_CameraMap.FindAction("Axis", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }
    public IEnumerable<InputBinding> bindings => asset.bindings;

    public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
    {
        return asset.FindAction(actionNameOrId, throwIfNotFound);
    }
    public int FindBinding(InputBinding bindingMask, out InputAction action)
    {
        return asset.FindBinding(bindingMask, out action);
    }

    // CameraMap
    private readonly InputActionMap m_CameraMap;
    private ICameraMapActions m_CameraMapActionsCallbackInterface;
    private readonly InputAction m_CameraMap_Axis;
    public struct CameraMapActions
    {
        private @Controls m_Wrapper;
        public CameraMapActions(@Controls wrapper) { m_Wrapper = wrapper; }
        public InputAction @Axis => m_Wrapper.m_CameraMap_Axis;
        public InputActionMap Get() { return m_Wrapper.m_CameraMap; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(CameraMapActions set) { return set.Get(); }
        public void SetCallbacks(ICameraMapActions instance)
        {
            if (m_Wrapper.m_CameraMapActionsCallbackInterface != null)
            {
                @Axis.started -= m_Wrapper.m_CameraMapActionsCallbackInterface.OnAxis;
                @Axis.performed -= m_Wrapper.m_CameraMapActionsCallbackInterface.OnAxis;
                @Axis.canceled -= m_Wrapper.m_CameraMapActionsCallbackInterface.OnAxis;
            }
            m_Wrapper.m_CameraMapActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Axis.started += instance.OnAxis;
                @Axis.performed += instance.OnAxis;
                @Axis.canceled += instance.OnAxis;
            }
        }
    }
    public CameraMapActions @CameraMap => new CameraMapActions(this);
    public interface ICameraMapActions
    {
        void OnAxis(InputAction.CallbackContext context);
    }
}
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
                    ""expectedControlType"": ""Vector2"",
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
                    ""processors"": ""ScaleVector2(x=20,y=20)"",
                    ""groups"": """",
                    ""action"": ""Axis"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""d3a43f51-2c48-40c6-9ee0-1c80c88dcf8b"",
                    ""path"": ""<Gamepad>/rightStick"",
                    ""interactions"": """",
                    ""processors"": ""ScaleVector2(x=100,y=-100)"",
                    ""groups"": """",
                    ""action"": ""Axis"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""MovementMap"",
            ""id"": ""78486ec0-fbcd-45d3-bc5c-4de166b2a2c6"",
            ""actions"": [
                {
                    ""name"": ""Motion"",
                    ""type"": ""PassThrough"",
                    ""id"": ""61c399e4-55df-4fd7-8c7a-14f125f6134c"",
                    ""expectedControlType"": ""Stick"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Jump"",
                    ""type"": ""Button"",
                    ""id"": ""6c5fb12f-6148-4aa5-ba6f-9c2c303127bd"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""2D Vector"",
                    ""id"": ""155219c8-6c37-49af-9466-8b4a7dfd6e5b"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Motion"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""a23bcf60-cadc-46eb-a782-bc802cf0bf27"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Motion"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""c333bbfe-3d19-4370-91be-702233b9a98c"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Motion"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""34553df4-2a5d-4bd1-b088-c49ac2bf9afc"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Motion"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""e0dbbfcc-276f-4f17-8f28-e252254f90d2"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Motion"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""db7e0fb3-b41d-4c27-9076-4a0cf788c49b"",
                    ""path"": ""<Gamepad>/leftStick"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Motion"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""2177cdf1-6619-44a8-a16d-86f5bc7b6826"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Jump"",
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
        // MovementMap
        m_MovementMap = asset.FindActionMap("MovementMap", throwIfNotFound: true);
        m_MovementMap_Motion = m_MovementMap.FindAction("Motion", throwIfNotFound: true);
        m_MovementMap_Jump = m_MovementMap.FindAction("Jump", throwIfNotFound: true);
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

    // MovementMap
    private readonly InputActionMap m_MovementMap;
    private IMovementMapActions m_MovementMapActionsCallbackInterface;
    private readonly InputAction m_MovementMap_Motion;
    private readonly InputAction m_MovementMap_Jump;
    public struct MovementMapActions
    {
        private @Controls m_Wrapper;
        public MovementMapActions(@Controls wrapper) { m_Wrapper = wrapper; }
        public InputAction @Motion => m_Wrapper.m_MovementMap_Motion;
        public InputAction @Jump => m_Wrapper.m_MovementMap_Jump;
        public InputActionMap Get() { return m_Wrapper.m_MovementMap; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(MovementMapActions set) { return set.Get(); }
        public void SetCallbacks(IMovementMapActions instance)
        {
            if (m_Wrapper.m_MovementMapActionsCallbackInterface != null)
            {
                @Motion.started -= m_Wrapper.m_MovementMapActionsCallbackInterface.OnMotion;
                @Motion.performed -= m_Wrapper.m_MovementMapActionsCallbackInterface.OnMotion;
                @Motion.canceled -= m_Wrapper.m_MovementMapActionsCallbackInterface.OnMotion;
                @Jump.started -= m_Wrapper.m_MovementMapActionsCallbackInterface.OnJump;
                @Jump.performed -= m_Wrapper.m_MovementMapActionsCallbackInterface.OnJump;
                @Jump.canceled -= m_Wrapper.m_MovementMapActionsCallbackInterface.OnJump;
            }
            m_Wrapper.m_MovementMapActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Motion.started += instance.OnMotion;
                @Motion.performed += instance.OnMotion;
                @Motion.canceled += instance.OnMotion;
                @Jump.started += instance.OnJump;
                @Jump.performed += instance.OnJump;
                @Jump.canceled += instance.OnJump;
            }
        }
    }
    public MovementMapActions @MovementMap => new MovementMapActions(this);
    public interface ICameraMapActions
    {
        void OnAxis(InputAction.CallbackContext context);
    }
    public interface IMovementMapActions
    {
        void OnMotion(InputAction.CallbackContext context);
        void OnJump(InputAction.CallbackContext context);
    }
}

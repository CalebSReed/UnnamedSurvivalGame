// GENERATED AUTOMATICALLY FROM 'Assets/PlayerInput/PlayerInputActions.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @PlayerInputActions : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @PlayerInputActions()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""PlayerInputActions"",
    ""maps"": [
        {
            ""name"": ""PlayerDefault"",
            ""id"": ""73ab4f9a-2d1f-4cab-a16a-b98906a424b0"",
            ""actions"": [
                {
                    ""name"": ""Movement"",
                    ""type"": ""Value"",
                    ""id"": ""909b9af2-2577-4039-a9df-11a9592b79d7"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""ZoomCam"",
                    ""type"": ""Value"",
                    ""id"": ""6381e1b6-f1e1-4db1-9d2f-3f7f3ae99abe"",
                    ""expectedControlType"": ""Axis"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""MousePosition"",
                    ""type"": ""Value"",
                    ""id"": ""ab7280be-9501-48cc-b06f-b29ebbc644dc"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""RotateCamLeft"",
                    ""type"": ""Button"",
                    ""id"": ""69a6e653-bb66-49df-b73d-57f9371d2038"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""RotateCamRight"",
                    ""type"": ""Button"",
                    ""id"": ""26cf4c37-d855-4f31-bfea-a6699d2a0dd7"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""InteractButton"",
                    ""type"": ""Button"",
                    ""id"": ""25429cfa-7bf4-4986-bf9d-34afdabd86c8"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""CancelButton"",
                    ""type"": ""Button"",
                    ""id"": ""3a09766d-c227-4f8a-a1a9-d19f3f6e7bc9"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""2D Vector"",
                    ""id"": ""6798e6df-27d4-4347-ad44-1a8d387f20bd"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Movement"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""b1bbb655-3c6d-4fb7-80a0-7c99f8ba13b8"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""149e49ba-c064-4c85-bfff-82dfb9d8bbd4"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""4f27d676-08f7-4825-ab23-2be7b2250f4e"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""bda6ec03-f54e-49b1-91f8-7fb1bdab7414"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""47ed3d73-1dec-40b5-b4f9-e29ebec5da95"",
                    ""path"": ""<Mouse>/scroll/y"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""ZoomCam"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""a408e2d6-a0ab-4eca-aae1-57705e583911"",
                    ""path"": ""<Mouse>/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""MousePosition"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""731fc408-61ec-4ae2-8282-d3a786728b05"",
                    ""path"": ""<Keyboard>/q"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""RotateCamLeft"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""99fd2559-fe7b-49bf-b464-49b8cd9111b9"",
                    ""path"": ""<Keyboard>/e"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""RotateCamRight"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""da66dd23-c7ea-47da-bab2-723ddcf25254"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""InteractButton"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""ec3c6d7c-619a-4371-b3a6-cf5671d34a4a"",
                    ""path"": ""<Mouse>/rightButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""CancelButton"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": [
        {
            ""name"": ""Keyboard"",
            ""bindingGroup"": ""Keyboard"",
            ""devices"": [
                {
                    ""devicePath"": ""<Keyboard>"",
                    ""isOptional"": false,
                    ""isOR"": false
                },
                {
                    ""devicePath"": ""<Mouse>"",
                    ""isOptional"": false,
                    ""isOR"": false
                }
            ]
        }
    ]
}");
        // PlayerDefault
        m_PlayerDefault = asset.FindActionMap("PlayerDefault", throwIfNotFound: true);
        m_PlayerDefault_Movement = m_PlayerDefault.FindAction("Movement", throwIfNotFound: true);
        m_PlayerDefault_ZoomCam = m_PlayerDefault.FindAction("ZoomCam", throwIfNotFound: true);
        m_PlayerDefault_MousePosition = m_PlayerDefault.FindAction("MousePosition", throwIfNotFound: true);
        m_PlayerDefault_RotateCamLeft = m_PlayerDefault.FindAction("RotateCamLeft", throwIfNotFound: true);
        m_PlayerDefault_RotateCamRight = m_PlayerDefault.FindAction("RotateCamRight", throwIfNotFound: true);
        m_PlayerDefault_InteractButton = m_PlayerDefault.FindAction("InteractButton", throwIfNotFound: true);
        m_PlayerDefault_CancelButton = m_PlayerDefault.FindAction("CancelButton", throwIfNotFound: true);
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

    // PlayerDefault
    private readonly InputActionMap m_PlayerDefault;
    private IPlayerDefaultActions m_PlayerDefaultActionsCallbackInterface;
    private readonly InputAction m_PlayerDefault_Movement;
    private readonly InputAction m_PlayerDefault_ZoomCam;
    private readonly InputAction m_PlayerDefault_MousePosition;
    private readonly InputAction m_PlayerDefault_RotateCamLeft;
    private readonly InputAction m_PlayerDefault_RotateCamRight;
    private readonly InputAction m_PlayerDefault_InteractButton;
    private readonly InputAction m_PlayerDefault_CancelButton;
    public struct PlayerDefaultActions
    {
        private @PlayerInputActions m_Wrapper;
        public PlayerDefaultActions(@PlayerInputActions wrapper) { m_Wrapper = wrapper; }
        public InputAction @Movement => m_Wrapper.m_PlayerDefault_Movement;
        public InputAction @ZoomCam => m_Wrapper.m_PlayerDefault_ZoomCam;
        public InputAction @MousePosition => m_Wrapper.m_PlayerDefault_MousePosition;
        public InputAction @RotateCamLeft => m_Wrapper.m_PlayerDefault_RotateCamLeft;
        public InputAction @RotateCamRight => m_Wrapper.m_PlayerDefault_RotateCamRight;
        public InputAction @InteractButton => m_Wrapper.m_PlayerDefault_InteractButton;
        public InputAction @CancelButton => m_Wrapper.m_PlayerDefault_CancelButton;
        public InputActionMap Get() { return m_Wrapper.m_PlayerDefault; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(PlayerDefaultActions set) { return set.Get(); }
        public void SetCallbacks(IPlayerDefaultActions instance)
        {
            if (m_Wrapper.m_PlayerDefaultActionsCallbackInterface != null)
            {
                @Movement.started -= m_Wrapper.m_PlayerDefaultActionsCallbackInterface.OnMovement;
                @Movement.performed -= m_Wrapper.m_PlayerDefaultActionsCallbackInterface.OnMovement;
                @Movement.canceled -= m_Wrapper.m_PlayerDefaultActionsCallbackInterface.OnMovement;
                @ZoomCam.started -= m_Wrapper.m_PlayerDefaultActionsCallbackInterface.OnZoomCam;
                @ZoomCam.performed -= m_Wrapper.m_PlayerDefaultActionsCallbackInterface.OnZoomCam;
                @ZoomCam.canceled -= m_Wrapper.m_PlayerDefaultActionsCallbackInterface.OnZoomCam;
                @MousePosition.started -= m_Wrapper.m_PlayerDefaultActionsCallbackInterface.OnMousePosition;
                @MousePosition.performed -= m_Wrapper.m_PlayerDefaultActionsCallbackInterface.OnMousePosition;
                @MousePosition.canceled -= m_Wrapper.m_PlayerDefaultActionsCallbackInterface.OnMousePosition;
                @RotateCamLeft.started -= m_Wrapper.m_PlayerDefaultActionsCallbackInterface.OnRotateCamLeft;
                @RotateCamLeft.performed -= m_Wrapper.m_PlayerDefaultActionsCallbackInterface.OnRotateCamLeft;
                @RotateCamLeft.canceled -= m_Wrapper.m_PlayerDefaultActionsCallbackInterface.OnRotateCamLeft;
                @RotateCamRight.started -= m_Wrapper.m_PlayerDefaultActionsCallbackInterface.OnRotateCamRight;
                @RotateCamRight.performed -= m_Wrapper.m_PlayerDefaultActionsCallbackInterface.OnRotateCamRight;
                @RotateCamRight.canceled -= m_Wrapper.m_PlayerDefaultActionsCallbackInterface.OnRotateCamRight;
                @InteractButton.started -= m_Wrapper.m_PlayerDefaultActionsCallbackInterface.OnInteractButton;
                @InteractButton.performed -= m_Wrapper.m_PlayerDefaultActionsCallbackInterface.OnInteractButton;
                @InteractButton.canceled -= m_Wrapper.m_PlayerDefaultActionsCallbackInterface.OnInteractButton;
                @CancelButton.started -= m_Wrapper.m_PlayerDefaultActionsCallbackInterface.OnCancelButton;
                @CancelButton.performed -= m_Wrapper.m_PlayerDefaultActionsCallbackInterface.OnCancelButton;
                @CancelButton.canceled -= m_Wrapper.m_PlayerDefaultActionsCallbackInterface.OnCancelButton;
            }
            m_Wrapper.m_PlayerDefaultActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Movement.started += instance.OnMovement;
                @Movement.performed += instance.OnMovement;
                @Movement.canceled += instance.OnMovement;
                @ZoomCam.started += instance.OnZoomCam;
                @ZoomCam.performed += instance.OnZoomCam;
                @ZoomCam.canceled += instance.OnZoomCam;
                @MousePosition.started += instance.OnMousePosition;
                @MousePosition.performed += instance.OnMousePosition;
                @MousePosition.canceled += instance.OnMousePosition;
                @RotateCamLeft.started += instance.OnRotateCamLeft;
                @RotateCamLeft.performed += instance.OnRotateCamLeft;
                @RotateCamLeft.canceled += instance.OnRotateCamLeft;
                @RotateCamRight.started += instance.OnRotateCamRight;
                @RotateCamRight.performed += instance.OnRotateCamRight;
                @RotateCamRight.canceled += instance.OnRotateCamRight;
                @InteractButton.started += instance.OnInteractButton;
                @InteractButton.performed += instance.OnInteractButton;
                @InteractButton.canceled += instance.OnInteractButton;
                @CancelButton.started += instance.OnCancelButton;
                @CancelButton.performed += instance.OnCancelButton;
                @CancelButton.canceled += instance.OnCancelButton;
            }
        }
    }
    public PlayerDefaultActions @PlayerDefault => new PlayerDefaultActions(this);
    private int m_KeyboardSchemeIndex = -1;
    public InputControlScheme KeyboardScheme
    {
        get
        {
            if (m_KeyboardSchemeIndex == -1) m_KeyboardSchemeIndex = asset.FindControlSchemeIndex("Keyboard");
            return asset.controlSchemes[m_KeyboardSchemeIndex];
        }
    }
    public interface IPlayerDefaultActions
    {
        void OnMovement(InputAction.CallbackContext context);
        void OnZoomCam(InputAction.CallbackContext context);
        void OnMousePosition(InputAction.CallbackContext context);
        void OnRotateCamLeft(InputAction.CallbackContext context);
        void OnRotateCamRight(InputAction.CallbackContext context);
        void OnInteractButton(InputAction.CallbackContext context);
        void OnCancelButton(InputAction.CallbackContext context);
    }
}

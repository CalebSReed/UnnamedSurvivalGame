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
                },
                {
                    ""name"": ""SpecialModifier"",
                    ""type"": ""Button"",
                    ""id"": ""42764821-d08a-4c6d-9486-7840145c8f8d"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""UseButton"",
                    ""type"": ""Button"",
                    ""id"": ""fccc6b96-f206-4758-9928-5d75b3132fcd"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""OpenCraftingTab"",
                    ""type"": ""Button"",
                    ""id"": ""bdf7fefc-41fa-46d2-bf49-f58c16df64a6"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Pause"",
                    ""type"": ""Button"",
                    ""id"": ""bf074d76-830c-46e4-b6b8-40d135d01853"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""OpenJournal"",
                    ""type"": ""Button"",
                    ""id"": ""557c2404-5dd7-47b4-bded-acfec27e5662"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""FreeCrafting"",
                    ""type"": ""Button"",
                    ""id"": ""739fd95a-0e80-448e-81e9-ac6bc042d448"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""DeployModifier"",
                    ""type"": ""Button"",
                    ""id"": ""e536f10b-1a15-427a-8c90-d40a0f025843"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""GodMode"",
                    ""type"": ""Button"",
                    ""id"": ""a15028f7-51a8-482f-84ef-3c403094af86"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""FreeStuff"",
                    ""type"": ""Button"",
                    ""id"": ""b710cf1c-1c91-4a6e-bd3f-599d636fe244"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Speed"",
                    ""type"": ""Button"",
                    ""id"": ""082ea5c2-c441-48b2-a687-642b702edfd3"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""SpecialInteract"",
                    ""type"": ""Button"",
                    ""id"": ""ed7f38fa-e72c-4f97-9ecc-237ca0a7c28a"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Reset"",
                    ""type"": ""Button"",
                    ""id"": ""b484fd1b-b9ae-4c14-92b5-9ae842d53326"",
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
                },
                {
                    ""name"": """",
                    ""id"": ""9659470d-9dc1-48de-aff6-cc800321083e"",
                    ""path"": ""<Keyboard>/leftShift"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""SpecialModifier"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""d4f53077-33ea-459d-bc23-bff369d4edb5"",
                    ""path"": ""<Mouse>/rightButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""UseButton"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""0dce45b8-7418-46ef-8373-bfa1d27ab326"",
                    ""path"": ""<Keyboard>/tab"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""OpenCraftingTab"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""19ad2b20-61b7-42a4-97e2-6eac7e26ad19"",
                    ""path"": ""<Keyboard>/c"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""OpenCraftingTab"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""ff5430be-7f69-4011-b88f-14ae61ed09fe"",
                    ""path"": ""<Keyboard>/escape"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""Pause"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""5cbef652-986d-4b85-aeaa-950ab5b59cfc"",
                    ""path"": ""<Keyboard>/j"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""OpenJournal"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""042012bc-06ed-43f2-90e0-a3241d941841"",
                    ""path"": ""<Keyboard>/f2"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""FreeCrafting"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""e58de59e-2590-4ebf-a80b-4b28d284f977"",
                    ""path"": ""<Keyboard>/leftCtrl"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""DeployModifier"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""49d32f63-36bf-433c-ae31-a17d94c58926"",
                    ""path"": ""<Keyboard>/f1"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""GodMode"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""ae5718b4-710a-4fb1-b95a-615c4aac8370"",
                    ""path"": ""<Keyboard>/f3"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""FreeStuff"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""47fd09cb-748b-4413-8062-8337de072669"",
                    ""path"": ""<Keyboard>/f12"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""Speed"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""a5c03dd3-7438-4d24-9c07-aef05b705887"",
                    ""path"": ""<Mouse>/rightButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""SpecialInteract"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""dabd4b71-f049-4915-99f8-defb87ad5131"",
                    ""path"": ""<Keyboard>/f9"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""Reset"",
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
        m_PlayerDefault_SpecialModifier = m_PlayerDefault.FindAction("SpecialModifier", throwIfNotFound: true);
        m_PlayerDefault_UseButton = m_PlayerDefault.FindAction("UseButton", throwIfNotFound: true);
        m_PlayerDefault_OpenCraftingTab = m_PlayerDefault.FindAction("OpenCraftingTab", throwIfNotFound: true);
        m_PlayerDefault_Pause = m_PlayerDefault.FindAction("Pause", throwIfNotFound: true);
        m_PlayerDefault_OpenJournal = m_PlayerDefault.FindAction("OpenJournal", throwIfNotFound: true);
        m_PlayerDefault_FreeCrafting = m_PlayerDefault.FindAction("FreeCrafting", throwIfNotFound: true);
        m_PlayerDefault_DeployModifier = m_PlayerDefault.FindAction("DeployModifier", throwIfNotFound: true);
        m_PlayerDefault_GodMode = m_PlayerDefault.FindAction("GodMode", throwIfNotFound: true);
        m_PlayerDefault_FreeStuff = m_PlayerDefault.FindAction("FreeStuff", throwIfNotFound: true);
        m_PlayerDefault_Speed = m_PlayerDefault.FindAction("Speed", throwIfNotFound: true);
        m_PlayerDefault_SpecialInteract = m_PlayerDefault.FindAction("SpecialInteract", throwIfNotFound: true);
        m_PlayerDefault_Reset = m_PlayerDefault.FindAction("Reset", throwIfNotFound: true);
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
    private readonly InputAction m_PlayerDefault_SpecialModifier;
    private readonly InputAction m_PlayerDefault_UseButton;
    private readonly InputAction m_PlayerDefault_OpenCraftingTab;
    private readonly InputAction m_PlayerDefault_Pause;
    private readonly InputAction m_PlayerDefault_OpenJournal;
    private readonly InputAction m_PlayerDefault_FreeCrafting;
    private readonly InputAction m_PlayerDefault_DeployModifier;
    private readonly InputAction m_PlayerDefault_GodMode;
    private readonly InputAction m_PlayerDefault_FreeStuff;
    private readonly InputAction m_PlayerDefault_Speed;
    private readonly InputAction m_PlayerDefault_SpecialInteract;
    private readonly InputAction m_PlayerDefault_Reset;
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
        public InputAction @SpecialModifier => m_Wrapper.m_PlayerDefault_SpecialModifier;
        public InputAction @UseButton => m_Wrapper.m_PlayerDefault_UseButton;
        public InputAction @OpenCraftingTab => m_Wrapper.m_PlayerDefault_OpenCraftingTab;
        public InputAction @Pause => m_Wrapper.m_PlayerDefault_Pause;
        public InputAction @OpenJournal => m_Wrapper.m_PlayerDefault_OpenJournal;
        public InputAction @FreeCrafting => m_Wrapper.m_PlayerDefault_FreeCrafting;
        public InputAction @DeployModifier => m_Wrapper.m_PlayerDefault_DeployModifier;
        public InputAction @GodMode => m_Wrapper.m_PlayerDefault_GodMode;
        public InputAction @FreeStuff => m_Wrapper.m_PlayerDefault_FreeStuff;
        public InputAction @Speed => m_Wrapper.m_PlayerDefault_Speed;
        public InputAction @SpecialInteract => m_Wrapper.m_PlayerDefault_SpecialInteract;
        public InputAction @Reset => m_Wrapper.m_PlayerDefault_Reset;
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
                @SpecialModifier.started -= m_Wrapper.m_PlayerDefaultActionsCallbackInterface.OnSpecialModifier;
                @SpecialModifier.performed -= m_Wrapper.m_PlayerDefaultActionsCallbackInterface.OnSpecialModifier;
                @SpecialModifier.canceled -= m_Wrapper.m_PlayerDefaultActionsCallbackInterface.OnSpecialModifier;
                @UseButton.started -= m_Wrapper.m_PlayerDefaultActionsCallbackInterface.OnUseButton;
                @UseButton.performed -= m_Wrapper.m_PlayerDefaultActionsCallbackInterface.OnUseButton;
                @UseButton.canceled -= m_Wrapper.m_PlayerDefaultActionsCallbackInterface.OnUseButton;
                @OpenCraftingTab.started -= m_Wrapper.m_PlayerDefaultActionsCallbackInterface.OnOpenCraftingTab;
                @OpenCraftingTab.performed -= m_Wrapper.m_PlayerDefaultActionsCallbackInterface.OnOpenCraftingTab;
                @OpenCraftingTab.canceled -= m_Wrapper.m_PlayerDefaultActionsCallbackInterface.OnOpenCraftingTab;
                @Pause.started -= m_Wrapper.m_PlayerDefaultActionsCallbackInterface.OnPause;
                @Pause.performed -= m_Wrapper.m_PlayerDefaultActionsCallbackInterface.OnPause;
                @Pause.canceled -= m_Wrapper.m_PlayerDefaultActionsCallbackInterface.OnPause;
                @OpenJournal.started -= m_Wrapper.m_PlayerDefaultActionsCallbackInterface.OnOpenJournal;
                @OpenJournal.performed -= m_Wrapper.m_PlayerDefaultActionsCallbackInterface.OnOpenJournal;
                @OpenJournal.canceled -= m_Wrapper.m_PlayerDefaultActionsCallbackInterface.OnOpenJournal;
                @FreeCrafting.started -= m_Wrapper.m_PlayerDefaultActionsCallbackInterface.OnFreeCrafting;
                @FreeCrafting.performed -= m_Wrapper.m_PlayerDefaultActionsCallbackInterface.OnFreeCrafting;
                @FreeCrafting.canceled -= m_Wrapper.m_PlayerDefaultActionsCallbackInterface.OnFreeCrafting;
                @DeployModifier.started -= m_Wrapper.m_PlayerDefaultActionsCallbackInterface.OnDeployModifier;
                @DeployModifier.performed -= m_Wrapper.m_PlayerDefaultActionsCallbackInterface.OnDeployModifier;
                @DeployModifier.canceled -= m_Wrapper.m_PlayerDefaultActionsCallbackInterface.OnDeployModifier;
                @GodMode.started -= m_Wrapper.m_PlayerDefaultActionsCallbackInterface.OnGodMode;
                @GodMode.performed -= m_Wrapper.m_PlayerDefaultActionsCallbackInterface.OnGodMode;
                @GodMode.canceled -= m_Wrapper.m_PlayerDefaultActionsCallbackInterface.OnGodMode;
                @FreeStuff.started -= m_Wrapper.m_PlayerDefaultActionsCallbackInterface.OnFreeStuff;
                @FreeStuff.performed -= m_Wrapper.m_PlayerDefaultActionsCallbackInterface.OnFreeStuff;
                @FreeStuff.canceled -= m_Wrapper.m_PlayerDefaultActionsCallbackInterface.OnFreeStuff;
                @Speed.started -= m_Wrapper.m_PlayerDefaultActionsCallbackInterface.OnSpeed;
                @Speed.performed -= m_Wrapper.m_PlayerDefaultActionsCallbackInterface.OnSpeed;
                @Speed.canceled -= m_Wrapper.m_PlayerDefaultActionsCallbackInterface.OnSpeed;
                @SpecialInteract.started -= m_Wrapper.m_PlayerDefaultActionsCallbackInterface.OnSpecialInteract;
                @SpecialInteract.performed -= m_Wrapper.m_PlayerDefaultActionsCallbackInterface.OnSpecialInteract;
                @SpecialInteract.canceled -= m_Wrapper.m_PlayerDefaultActionsCallbackInterface.OnSpecialInteract;
                @Reset.started -= m_Wrapper.m_PlayerDefaultActionsCallbackInterface.OnReset;
                @Reset.performed -= m_Wrapper.m_PlayerDefaultActionsCallbackInterface.OnReset;
                @Reset.canceled -= m_Wrapper.m_PlayerDefaultActionsCallbackInterface.OnReset;
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
                @SpecialModifier.started += instance.OnSpecialModifier;
                @SpecialModifier.performed += instance.OnSpecialModifier;
                @SpecialModifier.canceled += instance.OnSpecialModifier;
                @UseButton.started += instance.OnUseButton;
                @UseButton.performed += instance.OnUseButton;
                @UseButton.canceled += instance.OnUseButton;
                @OpenCraftingTab.started += instance.OnOpenCraftingTab;
                @OpenCraftingTab.performed += instance.OnOpenCraftingTab;
                @OpenCraftingTab.canceled += instance.OnOpenCraftingTab;
                @Pause.started += instance.OnPause;
                @Pause.performed += instance.OnPause;
                @Pause.canceled += instance.OnPause;
                @OpenJournal.started += instance.OnOpenJournal;
                @OpenJournal.performed += instance.OnOpenJournal;
                @OpenJournal.canceled += instance.OnOpenJournal;
                @FreeCrafting.started += instance.OnFreeCrafting;
                @FreeCrafting.performed += instance.OnFreeCrafting;
                @FreeCrafting.canceled += instance.OnFreeCrafting;
                @DeployModifier.started += instance.OnDeployModifier;
                @DeployModifier.performed += instance.OnDeployModifier;
                @DeployModifier.canceled += instance.OnDeployModifier;
                @GodMode.started += instance.OnGodMode;
                @GodMode.performed += instance.OnGodMode;
                @GodMode.canceled += instance.OnGodMode;
                @FreeStuff.started += instance.OnFreeStuff;
                @FreeStuff.performed += instance.OnFreeStuff;
                @FreeStuff.canceled += instance.OnFreeStuff;
                @Speed.started += instance.OnSpeed;
                @Speed.performed += instance.OnSpeed;
                @Speed.canceled += instance.OnSpeed;
                @SpecialInteract.started += instance.OnSpecialInteract;
                @SpecialInteract.performed += instance.OnSpecialInteract;
                @SpecialInteract.canceled += instance.OnSpecialInteract;
                @Reset.started += instance.OnReset;
                @Reset.performed += instance.OnReset;
                @Reset.canceled += instance.OnReset;
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
        void OnSpecialModifier(InputAction.CallbackContext context);
        void OnUseButton(InputAction.CallbackContext context);
        void OnOpenCraftingTab(InputAction.CallbackContext context);
        void OnPause(InputAction.CallbackContext context);
        void OnOpenJournal(InputAction.CallbackContext context);
        void OnFreeCrafting(InputAction.CallbackContext context);
        void OnDeployModifier(InputAction.CallbackContext context);
        void OnGodMode(InputAction.CallbackContext context);
        void OnFreeStuff(InputAction.CallbackContext context);
        void OnSpeed(InputAction.CallbackContext context);
        void OnSpecialInteract(InputAction.CallbackContext context);
        void OnReset(InputAction.CallbackContext context);
    }
}

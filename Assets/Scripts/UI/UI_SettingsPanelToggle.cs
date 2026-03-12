using UnityEngine;
using UnityEngine.InputSystem;

public class UI_SettingsPanelToggle : MonoBehaviour
{
    [SerializeField] private GameObject targetObject;
    [SerializeField] private bool keepTargetObjectActiveWhenHidden;
    [SerializeField] private InputActionReference showMenuAction;
    [SerializeField] private InputActionReference hideMenuAction;

    private void Start()
    {
        SetVisibility(false);
    }

    private void OnEnable()
    {
        EnableAction(showMenuAction, OnShowMenu);
        EnableAction(hideMenuAction, OnHideMenu);
    }

    private void OnDisable()
    {
        DisableAction(showMenuAction, OnShowMenu);
        DisableAction(hideMenuAction, OnHideMenu);
    }

    private void Update()
    {
        if (WasFallbackShowPressed())
        {
            ShowMenu();
        }

        if (WasFallbackHidePressed())
        {
            HideMenu();
        }
    }

    public void ToggleTargetVisibility()
    {
        if (targetObject == null)
        {
            return;
        }

        targetObject.SetActive(!targetObject.activeSelf);
    }

    public void ShowMenu()
    {
        SetVisibility(true);
    }

    public void HideMenu()
    {
        SetVisibility(false);
    }

    private void SetVisibility(bool visible)
    {
        if (targetObject == null)
        {
            return;
        }

        bool shouldTargetStayActive = keepTargetObjectActiveWhenHidden && !visible;
        targetObject.SetActive(visible || shouldTargetStayActive);
    }

    private static bool WasFallbackShowPressed()
    {
#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKeyDown(KeyCode.JoystickButton0) || Input.GetKeyDown(KeyCode.Return))
        {
            return true;
        }
#endif
        return (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame)
               || (Keyboard.current != null && Keyboard.current.enterKey.wasPressedThisFrame);
    }

    private static bool WasFallbackHidePressed()
    {
#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            return true;
        }
#endif
        return (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame)
               || (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
               || (Keyboard.current != null && Keyboard.current.backspaceKey.wasPressedThisFrame);
    }

    private void OnShowMenu(InputAction.CallbackContext _)
    {
        ShowMenu();
    }

    private void OnHideMenu(InputAction.CallbackContext _)
    {
        HideMenu();
    }

    private static void EnableAction(InputActionReference actionReference, System.Action<InputAction.CallbackContext> callback)
    {
        if (actionReference == null || actionReference.action == null)
        {
            return;
        }

        actionReference.action.performed += callback;
        actionReference.action.Enable();
    }

    private static void DisableAction(InputActionReference actionReference, System.Action<InputAction.CallbackContext> callback)
    {
        if (actionReference == null || actionReference.action == null)
        {
            return;
        }

        actionReference.action.performed -= callback;
        actionReference.action.Disable();
    }
}

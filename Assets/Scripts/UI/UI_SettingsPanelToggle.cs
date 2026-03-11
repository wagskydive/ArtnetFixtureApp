using UnityEngine;
using UnityEngine.InputSystem;

public class UI_SettingsPanelToggle : MonoBehaviour
{
    [SerializeField] private GameObject targetObject;
    [SerializeField] private InputActionReference showMenuAction;
    [SerializeField] private InputActionReference hideMenuAction;
    [SerializeField] private KeyCode fallbackShowKey = KeyCode.JoystickButton0;
    [SerializeField] private KeyCode fallbackHideKey = KeyCode.Escape;

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
        if (Input.GetKeyDown(fallbackShowKey))
        {
            ShowMenu();
        }

        if (Input.GetKeyDown(fallbackHideKey))
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

        targetObject.SetActive(visible);
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

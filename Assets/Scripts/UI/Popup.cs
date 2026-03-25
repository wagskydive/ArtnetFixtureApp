using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Popup : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Selectable defaultSelection;
    [SerializeField] private InputActionReference backAction;
    [SerializeField] private bool closeOnBackButton = true;
    [SerializeField] private bool keepNavigationInsidePopup = true;

    private GameObject _previousSelected;

    private void Awake()
    {
        if (panelRoot == null)
        {
            panelRoot = gameObject;
        }
    }

    private void OnEnable()
    {
        EnableAction(backAction, OnBackAction);
        FocusDefaultSelection();
    }

    private void OnDisable()
    {
        DisableAction(backAction, OnBackAction);
    }

    private void Update()
    {
        if (!IsOpen())
        {
            return;
        }

        if (closeOnBackButton && WasFallbackBackPressed())
        {
            Close();
            return;
        }

        if (keepNavigationInsidePopup)
        {
            KeepSelectionWithinPopup();
        }
    }

    public void Open()
    {
        _previousSelected = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;

        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
        }

        FocusDefaultSelection();
    }

    public void Close()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }

        if (_previousSelected != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(_previousSelected);
        }
    }

    private bool IsOpen()
    {
        return panelRoot != null && panelRoot.activeInHierarchy;
    }

    private void FocusDefaultSelection()
    {
        if (EventSystem.current == null)
        {
            return;
        }

        Selectable selectable = defaultSelection != null ? defaultSelection : GetComponentInChildren<Selectable>(false);
        if (selectable == null || !selectable.gameObject.activeInHierarchy || !selectable.IsInteractable())
        {
            return;
        }

        selectable.Select();
        EventSystem.current.SetSelectedGameObject(selectable.gameObject);
    }

    private void KeepSelectionWithinPopup()
    {
        if (EventSystem.current == null)
        {
            return;
        }

        GameObject selectedObject = EventSystem.current.currentSelectedGameObject;
        if (selectedObject == null)
        {
            FocusDefaultSelection();
            return;
        }

        if (!selectedObject.transform.IsChildOf(panelRoot.transform) && selectedObject != panelRoot)
        {
            FocusDefaultSelection();
        }
    }

    private void OnBackAction(InputAction.CallbackContext _)
    {
        if (closeOnBackButton)
        {
            Close();
        }
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

    private static bool WasFallbackBackPressed()
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
}

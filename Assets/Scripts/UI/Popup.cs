using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class Popup : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Selectable defaultSelection;
    [SerializeField] private InputActionReference backAction;
    [SerializeField] private bool closeOnBackButton = true;
    [SerializeField] private bool keepNavigationInsidePopup = true;

    private GameObject _previousSelected;
    private readonly List<UI_DpadNavigationController> _blockedNavigationControllers = new List<UI_DpadNavigationController>();
    private bool _navigationBlockApplied;

    public static event Action<Popup> OnPopupNavigationBlocked;

    public static event Action<Popup> OnPopupNavigationBlockReleased;

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
        if (IsOpen())
        {
            ApplyNavigationBlock();
            FocusDefaultSelection();
        }
    }

    private void OnDisable()
    {
        ReleaseNavigationBlock();
        DisableAction(backAction, OnBackAction);
    }

    private void Update()
    {
        if (!IsOpen())
        {
            if (_navigationBlockApplied)
            {
                RestorePreviousSelection();
                ReleaseNavigationBlock();
            }

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

        ApplyNavigationBlock();
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

        ReleaseNavigationBlock();
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

    private void ApplyNavigationBlock()
    {
        if (_navigationBlockApplied)
        {
            return;
        }

        _blockedNavigationControllers.Clear();

        if (!keepNavigationInsidePopup || panelRoot == null)
        {
            return;
        }

        UI_DpadNavigationController[] controllers = FindObjectsByType<UI_DpadNavigationController>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        for (int i = 0; i < controllers.Length; i++)
        {
            UI_DpadNavigationController controller = controllers[i];
            if (controller == null || !controller.isActiveAndEnabled)
            {
                continue;
            }

            bool isPopupController = controller.transform.IsChildOf(panelRoot.transform) || controller.gameObject == panelRoot;
            if (isPopupController)
            {
                continue;
            }

            controller.enabled = false;
            _blockedNavigationControllers.Add(controller);
        }

        _navigationBlockApplied = true;
        OnPopupNavigationBlocked?.Invoke(this);
    }

    private void ReleaseNavigationBlock()
    {
        for (int i = 0; i < _blockedNavigationControllers.Count; i++)
        {
            if (_blockedNavigationControllers[i] != null)
            {
                _blockedNavigationControllers[i].enabled = true;
            }
        }

        _blockedNavigationControllers.Clear();
        _navigationBlockApplied = false;
        OnPopupNavigationBlockReleased?.Invoke(this);
    }

    private void RestorePreviousSelection()
    {
        if (_previousSelected == null || EventSystem.current == null)
        {
            return;
        }

        EventSystem.current.SetSelectedGameObject(_previousSelected);
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

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UI_DpadNavigationController : MonoBehaviour
{
    [SerializeField] private Selectable[] orderedSelectables;
    [SerializeField] private bool horizontalWrap = false;
    [SerializeField] private InputActionReference navigateAction;
    [SerializeField] private InputActionReference submitAction;

    private int _currentIndex;

    private void OnEnable()
    {
        ConfigureNavigation();
        SelectFirstValid();
        EnableAction(navigateAction, OnNavigate);
        EnableAction(submitAction, OnSubmit);
    }

    private void OnDisable()
    {
        DisableAction(navigateAction, OnNavigate);
        DisableAction(submitAction, OnSubmit);
    }

    private void Update()
    {
        if (orderedSelectables == null || orderedSelectables.Length == 0)
        {
            return;
        }

        if (WasFallbackSubmitPressed())
        {
            SubmitCurrentSelection();
        }

        Vector2 fallbackNavigation = ReadFallbackNavigation();
        if (fallbackNavigation != Vector2.zero)
        {
            HandleNavigationInput(fallbackNavigation);
        }
    }

    public void HandleNavigationInput(Vector2 navigationInput)
    {
        if (navigationInput == Vector2.zero)
        {
            return;
        }

        float horizontalMagnitude = Mathf.Abs(navigationInput.x);
        float verticalMagnitude = Mathf.Abs(navigationInput.y);

        if (verticalMagnitude >= horizontalMagnitude)
        {
            Move(navigationInput.y > 0f ? -1 : 1);
            return;
        }

        if (!horizontalWrap)
        {
            return;
        }

        Move(navigationInput.x > 0f ? 1 : -1);
    }

    public void Move(int delta)
    {
        if (!HasSelectables())
        {
            return;
        }

        int count = orderedSelectables.Length;
        for (int attempts = 0; attempts < count; attempts++)
        {
            _currentIndex = (_currentIndex + delta + count) % count;
            if (IsSelectable(_currentIndex))
            {
                SelectIndex(_currentIndex);
                return;
            }
        }
    }

    public void SubmitCurrentSelection()
    {
        if (!HasSelectables() || !IsSelectable(_currentIndex) || EventSystem.current == null)
        {
            return;
        }

        Selectable selected = orderedSelectables[_currentIndex];
        var submitData = new BaseEventData(EventSystem.current);
        ExecuteEvents.Execute<ISubmitHandler>(selected.gameObject, submitData, ExecuteEvents.submitHandler);
    }

    private void SelectIndex(int index)
    {
        if (!IsSelectable(index))
        {
            return;
        }

        _currentIndex = index;
        orderedSelectables[index].Select();
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(orderedSelectables[index].gameObject);
        }
    }

    private void ConfigureNavigation()
    {
        if (!HasSelectables())
        {
            return;
        }

        for (int i = 0; i < orderedSelectables.Length; i++)
        {
            Selectable current = orderedSelectables[i];
            if (current == null)
            {
                continue;
            }

            Navigation navigation = current.navigation;
            navigation.mode = Navigation.Mode.Explicit;
            navigation.selectOnUp = orderedSelectables[(i - 1 + orderedSelectables.Length) % orderedSelectables.Length];
            navigation.selectOnDown = orderedSelectables[(i + 1) % orderedSelectables.Length];
            if (horizontalWrap)
            {
                navigation.selectOnLeft = navigation.selectOnUp;
                navigation.selectOnRight = navigation.selectOnDown;
            }
            current.navigation = navigation;
        }
    }

    private void SelectFirstValid()
    {
        if (!HasSelectables())
        {
            return;
        }

        for (int i = 0; i < orderedSelectables.Length; i++)
        {
            if (!IsSelectable(i))
            {
                continue;
            }

            SelectIndex(i);
            return;
        }
    }

    private bool HasSelectables()
    {
        return orderedSelectables != null && orderedSelectables.Length > 0;
    }

    private bool IsSelectable(int index)
    {
        Selectable selectable = orderedSelectables[index];
        return selectable != null && selectable.IsInteractable() && selectable.gameObject.activeInHierarchy;
    }

    private void OnNavigate(InputAction.CallbackContext context)
    {
        HandleNavigationInput(context.ReadValue<Vector2>());
    }

    private void OnSubmit(InputAction.CallbackContext _)
    {
        SubmitCurrentSelection();
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

    private static Vector2 ReadFallbackNavigation()
    {
#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            return Vector2.down;
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            return Vector2.up;
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            return Vector2.right;
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            return Vector2.left;
        }
#endif
        if (Keyboard.current != null)
        {
            if (Keyboard.current.downArrowKey.wasPressedThisFrame)
            {
                return Vector2.down;
            }

            if (Keyboard.current.upArrowKey.wasPressedThisFrame)
            {
                return Vector2.up;
            }

            if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
            {
                return Vector2.right;
            }

            if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
            {
                return Vector2.left;
            }
        }

        if (Gamepad.current != null)
        {
            if (Gamepad.current.dpad.down.wasPressedThisFrame)
            {
                return Vector2.down;
            }

            if (Gamepad.current.dpad.up.wasPressedThisFrame)
            {
                return Vector2.up;
            }

            if (Gamepad.current.dpad.right.wasPressedThisFrame)
            {
                return Vector2.right;
            }

            if (Gamepad.current.dpad.left.wasPressedThisFrame)
            {
                return Vector2.left;
            }
        }

        return Vector2.zero;
    }

    private static bool WasFallbackSubmitPressed()
    {
#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.JoystickButton0))
        {
            return true;
        }
#endif
        return (Keyboard.current != null && (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame))
               || (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame);
    }
}

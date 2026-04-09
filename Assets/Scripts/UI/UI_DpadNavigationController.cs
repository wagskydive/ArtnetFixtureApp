using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Linq;

public class UI_DpadNavigationController : MonoBehaviour
{
    [SerializeField] private Selectable[] orderedSelectables;
    [SerializeField] private bool allowHorizontalNavigation = true;
    [SerializeField] private bool allowVerticalNavigation = true;
    [SerializeField] private bool horizontalWrap = false;
    [SerializeField] private bool verticalWrap = false;
    [SerializeField] private InputActionReference navigateAction;
    [SerializeField] private InputActionReference submitAction;
    [SerializeField] private InputActionReference cancelAction;
    [SerializeField] private UnityEvent onCancel;

    private readonly List<Selectable> _runtimeSelectables = new List<Selectable>();
    private static readonly Dictionary<InputAction, int> ActionUsageCounts = new Dictionary<InputAction, int>();
    private int _currentIndex;
    private int _lastSubmitFrame = -1;
    private int _lastCancelFrame = -1;

    bool cancelButtonBlocked;

    public void SetLastCancelFrame(int frame)
    {
        _lastCancelFrame = frame;
    }

    public void SetCancelButtonBlock(bool block)
    {
        cancelButtonBlocked = block;
    }

    private void OnEnable()
    {
        RebuildSelectables();
        SelectFirstValid();
        EnableAction(navigateAction, OnNavigate);
        EnableAction(submitAction, OnSubmit);
        EnableAction(cancelAction, OnCancel);
    }

    private void OnDisable()
    {
        DisableAction(navigateAction, OnNavigate);
        DisableAction(submitAction, OnSubmit);
        DisableAction(cancelAction, OnCancel);
    }


    private void Update()
    {
        if (WasFallbackCancelPressed())
        {
            HandleCancelInput();
        }
    }

    public void HandleNavigationInput(Vector2 navigationInput)
    {
        if (navigationInput == Vector2.zero)
        {
            return;
        }

        RebuildSelectables();
        if (!HasSelectables())
        {
            return;
        }

        int currentIndex = GetCurrentIndex();
        if (!IsSelectable(currentIndex))
        {
            SelectFirstValid();
            currentIndex = _currentIndex;
        }

        int targetIndex = FindNearestIndexInDirection(currentIndex, navigationInput);
        if (targetIndex >= 0)
        {
            SelectIndex(targetIndex);
        }
    }

    public void Move(int delta)
    {
        RebuildSelectables();
        if (!HasSelectables())
        {
            return;
        }

        int count = _runtimeSelectables.Count;
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



    public void HandleCancelInput()
    {
        if (_lastCancelFrame == Time.frameCount)
        {
            return;
        }
        _lastCancelFrame = Time.frameCount;
        if(!cancelButtonBlocked)
        {
            onCancel?.Invoke();
        }        
    }

    public void SubmitCurrentSelection()
    {
        if (_lastSubmitFrame == Time.frameCount)
        {
            return;
        }

        RebuildSelectables();
        int selectedIndex = GetCurrentIndex();
        if (!HasSelectables() || !IsSelectable(selectedIndex) || EventSystem.current == null)
        {
            return;
        }

        _lastSubmitFrame = Time.frameCount;
        Selectable selected = _runtimeSelectables[selectedIndex];
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
        _runtimeSelectables[index].Select();
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(_runtimeSelectables[index].gameObject);
        }
    }

    private void SelectFirstValid()
    {
        if (!HasSelectables())
        {
            return;
        }

        for (int i = 0; i < _runtimeSelectables.Count; i++)
        {
            if (!IsSelectable(i))
            {
                continue;
            }

            SelectIndex(i);
            return;
        }
    }

    private void RebuildSelectables()
    {
        _runtimeSelectables.Clear();

        if (orderedSelectables != null && orderedSelectables.Length > 0)
        {
            for (int i = 0; i < orderedSelectables.Length; i++)
            {
                if (orderedSelectables[i] != null)
                {
                    _runtimeSelectables.Add(orderedSelectables[i]);
                }
            }

            return;
        }

        Selectable[] discovered = GetComponentsInChildren<Selectable>(false);
        for (int i = 0; i < discovered.Length; i++)
        {
            if (discovered[i] != null)
            {
                _runtimeSelectables.Add(discovered[i]);
            }
        }

        _runtimeSelectables.Sort(CompareByScreenPosition);
    }

    private int CompareByScreenPosition(Selectable left, Selectable right)
    {
        Vector2 leftPosition = GetScreenPosition(left);
        Vector2 rightPosition = GetScreenPosition(right);

        const float epsilon = 1f;
        if (Mathf.Abs(leftPosition.y - rightPosition.y) > epsilon)
        {
            return rightPosition.y.CompareTo(leftPosition.y);
        }

        return leftPosition.x.CompareTo(rightPosition.x);
    }

    private int FindNearestIndexInDirection(int originIndex, Vector2 navigationInput)
    {
        if (!TryResolveNavigationAxis(navigationInput, out bool useVertical, out int axisSign))
        {
            return -1;
        }

        Vector2 origin = GetScreenPosition(_runtimeSelectables[originIndex]);
        int bestIndex = -1;
        float bestScore = float.MaxValue;

        for (int i = 0; i < _runtimeSelectables.Count; i++)
        {
            if (i == originIndex || !IsSelectable(i))
            {
                continue;
            }

            Vector2 candidatePosition = GetScreenPosition(_runtimeSelectables[i]);
            Vector2 delta = candidatePosition - origin;
            float axisDelta = useVertical ? delta.y : delta.x;
            if ((axisSign > 0 && axisDelta <= 0f) || (axisSign < 0 && axisDelta >= 0f))
            {
                continue;
            }

            float perpendicularDelta = useVertical ? Mathf.Abs(delta.x) : Mathf.Abs(delta.y);
            float score = Mathf.Abs(axisDelta) * 1000f + perpendicularDelta;
            if (score < bestScore)
            {
                bestScore = score;
                bestIndex = i;
            }
        }

        if (bestIndex >= 0)
        {
            return bestIndex;
        }

        if (!ShouldWrap(useVertical))
        {
            return -1;
        }

        // Wrap to the furthest selectable on the opposite side while keeping close perpendicular alignment.
        float wrapBestPerpendicular = float.MaxValue;
        float wrapBestAxisDistance = -1f;
        for (int i = 0; i < _runtimeSelectables.Count; i++)
        {
            if (i == originIndex || !IsSelectable(i))
            {
                continue;
            }

            Vector2 candidatePosition = GetScreenPosition(_runtimeSelectables[i]);
            Vector2 delta = candidatePosition - origin;
            float axisDelta = useVertical ? delta.y : delta.x;
            bool isOppositeDirection = axisSign > 0 ? axisDelta < 0f : axisDelta > 0f;
            if (!isOppositeDirection)
            {
                continue;
            }

            float perpendicularDelta = useVertical ? Mathf.Abs(delta.x) : Mathf.Abs(delta.y);
            float axisDistance = Mathf.Abs(axisDelta);
            if (perpendicularDelta < wrapBestPerpendicular ||
                (Mathf.Approximately(perpendicularDelta, wrapBestPerpendicular) && axisDistance > wrapBestAxisDistance))
            {
                wrapBestPerpendicular = perpendicularDelta;
                wrapBestAxisDistance = axisDistance;
                bestIndex = i;
            }
        }

        return bestIndex;
    }

    private bool TryResolveNavigationAxis(Vector2 navigationInput, out bool useVertical, out int axisSign)
    {
        float horizontalMagnitude = Mathf.Abs(navigationInput.x);
        float verticalMagnitude = Mathf.Abs(navigationInput.y);
        bool horizontalAvailable = allowHorizontalNavigation && horizontalMagnitude > 0f;
        bool verticalAvailable = allowVerticalNavigation && verticalMagnitude > 0f;

        if (!horizontalAvailable && !verticalAvailable)
        {
            useVertical = true;
            axisSign = 1;
            return false;
        }

        useVertical = verticalAvailable && (!horizontalAvailable || verticalMagnitude >= horizontalMagnitude);
        axisSign = useVertical
            ? (navigationInput.y >= 0f ? 1 : -1)
            : (navigationInput.x >= 0f ? 1 : -1);
        return true;
    }

    private bool ShouldWrap(bool useVertical)
    {
        return useVertical ? verticalWrap : horizontalWrap;
    }

    private int GetCurrentIndex()
    {
        if (EventSystem.current != null)
        {
            GameObject selectedObject = EventSystem.current.currentSelectedGameObject;
            if (selectedObject != null)
            {
                for (int i = 0; i < _runtimeSelectables.Count; i++)
                {
                    if (_runtimeSelectables[i] != null && _runtimeSelectables[i].gameObject == selectedObject)
                    {
                        _currentIndex = i;
                        return i;
                    }
                }
            }
        }

        return _currentIndex;
    }

    private bool HasSelectables()
    {
        return _runtimeSelectables.Count > 0;
    }

    private bool IsSelectable(int index)
    {
        if (index < 0 || index >= _runtimeSelectables.Count)
        {
            return false;
        }

        Selectable selectable = _runtimeSelectables[index];
        return selectable != null && selectable.IsInteractable() && selectable.gameObject.activeInHierarchy;
    }

    private static Vector2 GetScreenPosition(Selectable selectable)
    {
        if (selectable == null)
        {
            return Vector2.zero;
        }

        return RectTransformUtility.WorldToScreenPoint(null, selectable.transform.position);
    }

    private void OnNavigate(InputAction.CallbackContext context)
    {
        HandleNavigationInput(context.ReadValue<Vector2>());
    }

    private void OnSubmit(InputAction.CallbackContext _)
    {
        SubmitCurrentSelection();
    }

    private void OnCancel(InputAction.CallbackContext _)
    {
        HandleCancelInput();
    }

    private static bool WasFallbackCancelPressed()
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

    private static void EnableAction(InputActionReference actionReference, System.Action<InputAction.CallbackContext> callback)
    {
        if (actionReference == null || actionReference.action == null)
        {
            return;
        }

        InputAction action = actionReference.action;
        action.performed += callback;

        ActionUsageCounts.TryGetValue(action, out int usageCount);
        usageCount++;
        ActionUsageCounts[action] = usageCount;

        if (usageCount == 1)
        {
            action.Enable();
        }
    }

    private static void DisableAction(InputActionReference actionReference, System.Action<InputAction.CallbackContext> callback)
    {
        if (actionReference == null || actionReference.action == null)
        {
            return;
        }

        InputAction action = actionReference.action;
        action.performed -= callback;

        if (!ActionUsageCounts.TryGetValue(action, out int usageCount))
        {
            return;
        }

        usageCount = Mathf.Max(usageCount - 1, 0);
        if (usageCount == 0)
        {
            ActionUsageCounts.Remove(action);
            action.Disable();
            return;
        }

        ActionUsageCounts[action] = usageCount;
    }
}

using System.Collections.Generic;
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

    private readonly List<Selectable> _runtimeSelectables = new List<Selectable>();
    private int _currentIndex;
    private int _lastSubmitFrame = -1;
    private bool _submitHeld;
    private bool _selectablesDirty = true;

    private void OnEnable()
    {
        RebuildSelectables();
        SelectFirstValid();
        _submitHeld = false;
        EnableAction(navigateAction, OnNavigate);
        EnableAction(submitAction, OnSubmit);
        EnableCanceledAction(submitAction, OnSubmitCanceled);
    }

    private void OnDisable()
    {
        DisableAction(navigateAction, OnNavigate);
        DisableAction(submitAction, OnSubmit);
        DisableCanceledAction(submitAction, OnSubmitCanceled);
        _submitHeld = false;
    }

    private void LateUpdate()
    {
        EnsureSelectionIsValid();
    }

    private void OnTransformChildrenChanged()
    {
        _selectablesDirty = true;
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

        int targetIndex = FindSelectableNavigationTarget(currentIndex, navigationInput);
        if (targetIndex >= 0)
        {
            SelectIndex(targetIndex);
        }
    }

    private int FindSelectableNavigationTarget(int originIndex, Vector2 navigationInput)
    {
        int builtInTargetIndex = FindBuiltInSelectableTarget(originIndex, navigationInput);
        if (builtInTargetIndex >= 0)
        {
            return builtInTargetIndex;
        }

        return FindNearestIndexInDirection(originIndex, navigationInput);
    }

    private int FindBuiltInSelectableTarget(int originIndex, Vector2 navigationInput)
    {
        if (!IsSelectable(originIndex))
        {
            return -1;
        }

        Selectable origin = _runtimeSelectables[originIndex];
        if (origin == null)
        {
            return -1;
        }

        Selectable target = null;
        if (Mathf.Abs(navigationInput.y) >= Mathf.Abs(navigationInput.x))
        {
            target = navigationInput.y > 0f ? origin.FindSelectableOnUp() : origin.FindSelectableOnDown();
        }
        else if (horizontalWrap)
        {
            target = navigationInput.x > 0f ? origin.FindSelectableOnRight() : origin.FindSelectableOnLeft();
        }

        if (target == null)
        {
            return -1;
        }

        for (int i = 0; i < _runtimeSelectables.Count; i++)
        {
            if (_runtimeSelectables[i] == target && IsSelectable(i))
            {
                return i;
            }
        }

        return -1;
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
        if (!_selectablesDirty && _runtimeSelectables.Count > 0)
        {
            return;
        }

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

            _selectablesDirty = false;
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
        _selectablesDirty = false;
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
        float horizontalMagnitude = Mathf.Abs(navigationInput.x);
        float verticalMagnitude = Mathf.Abs(navigationInput.y);
        bool useVertical = verticalMagnitude >= horizontalMagnitude;
        Vector2 axisDirection = useVertical
            ? new Vector2(0f, navigationInput.y > 0f ? 1f : -1f)
            : new Vector2(navigationInput.x > 0f ? 1f : -1f, 0f);

        if (!useVertical && !horizontalWrap)
        {
            return -1;
        }

        Vector2 origin = GetScreenPosition(_runtimeSelectables[originIndex]);
        int bestIndex = -1;
        float bestAxisDistance = float.MaxValue;
        float bestCrossDistance = float.MaxValue;
        float bestSquaredDistance = float.MaxValue;

        for (int i = 0; i < _runtimeSelectables.Count; i++)
        {
            if (i == originIndex || !IsSelectable(i))
            {
                continue;
            }

            Vector2 candidatePosition = GetScreenPosition(_runtimeSelectables[i]);
            Vector2 delta = candidatePosition - origin;
            float axisDelta = useVertical ? delta.y : delta.x;
            if ((axisDirection.y > 0f && axisDelta <= 0f) ||
                (axisDirection.y < 0f && axisDelta >= 0f) ||
                (axisDirection.x > 0f && axisDelta <= 0f) ||
                (axisDirection.x < 0f && axisDelta >= 0f))
            {
                continue;
            }

            float axisDistance = Mathf.Abs(axisDelta);
            float crossDistance = Mathf.Abs(useVertical ? delta.x : delta.y);
            float squaredDistance = delta.sqrMagnitude;
            if (axisDistance < bestAxisDistance - 0.01f ||
                (Mathf.Abs(axisDistance - bestAxisDistance) <= 0.01f && crossDistance < bestCrossDistance - 0.01f) ||
                (Mathf.Abs(axisDistance - bestAxisDistance) <= 0.01f && Mathf.Abs(crossDistance - bestCrossDistance) <= 0.01f && squaredDistance < bestSquaredDistance))
            {
                bestAxisDistance = axisDistance;
                bestCrossDistance = crossDistance;
                bestSquaredDistance = squaredDistance;
                bestIndex = i;
            }
        }

        if (bestIndex >= 0)
        {
            return bestIndex;
        }

        if (!horizontalWrap || useVertical)
        {
            return -1;
        }

        // Wrap horizontally to the nearest item on the opposite side.
        float wrapBest = float.MaxValue;
        for (int i = 0; i < _runtimeSelectables.Count; i++)
        {
            if (i == originIndex || !IsSelectable(i))
            {
                continue;
            }

            Vector2 candidatePosition = GetScreenPosition(_runtimeSelectables[i]);
            float score = Mathf.Abs(candidatePosition.y - origin.y);
            if (score < wrapBest)
            {
                wrapBest = score;
                bestIndex = i;
            }
        }

        return bestIndex;
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

    private void OnSubmit(InputAction.CallbackContext context)
    {
        if (context.ReadValue<float>() <= 0.5f)
        {
            return;
        }

        if (_submitHeld)
        {
            return;
        }

        _submitHeld = true;

        SubmitCurrentSelection();
    }

    private void OnSubmitCanceled(InputAction.CallbackContext _)
    {
        _submitHeld = false;
    }

    private void EnsureSelectionIsValid()
    {
        RebuildSelectables();
        if (!HasSelectables())
        {
            return;
        }

        int currentIndex = GetCurrentIndex();
        if (!IsSelectable(currentIndex))
        {
            SelectFirstValid();
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

    private static void EnableCanceledAction(InputActionReference actionReference, System.Action<InputAction.CallbackContext> callback)
    {
        if (actionReference == null || actionReference.action == null)
        {
            return;
        }

        actionReference.action.canceled += callback;
    }

    private static void DisableCanceledAction(InputActionReference actionReference, System.Action<InputAction.CallbackContext> callback)
    {
        if (actionReference == null || actionReference.action == null)
        {
            return;
        }

        actionReference.action.canceled -= callback;
    }
}

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_DpadNavigationController : MonoBehaviour
{
    [SerializeField] private Selectable[] orderedSelectables;
    [SerializeField] private bool horizontalWrap = false;

    private int _currentIndex;

    private void OnEnable()
    {
        ConfigureNavigation();
        SelectIndex(0);
    }

    private void Update()
    {
        if (orderedSelectables == null || orderedSelectables.Length == 0)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            Move(1);
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            Move(-1);
        }
        else if (horizontalWrap && Input.GetKeyDown(KeyCode.RightArrow))
        {
            Move(1);
        }
        else if (horizontalWrap && Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Move(-1);
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.JoystickButton0))
        {
            SubmitCurrentSelection();
        }
    }

    public void Move(int delta)
    {
        int count = orderedSelectables.Length;
        _currentIndex = (_currentIndex + delta + count) % count;
        SelectIndex(_currentIndex);
    }

    public void SubmitCurrentSelection()
    {
        Selectable selected = orderedSelectables[_currentIndex];
        if (selected == null)
        {
            return;
        }

        var submitData = new BaseEventData(EventSystem.current);
        selected.OnSubmit(submitData);
    }

    private void SelectIndex(int index)
    {
        if (orderedSelectables[index] == null)
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
        if (orderedSelectables == null)
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
}

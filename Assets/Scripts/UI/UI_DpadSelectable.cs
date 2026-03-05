using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_DpadSelectable : Selectable, ISubmitHandler, IPointerClickHandler
{
    [SerializeField] private UnityEvent onSubmit;

    public void OnSubmit(BaseEventData eventData)
    {
        onSubmit?.Invoke();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!IsInteractable())
        {
            return;
        }

        OnSubmit(eventData);
    }
}

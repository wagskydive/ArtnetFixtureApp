using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UI_InfoPanelController : MonoBehaviour
{
    [TextArea] public string defaultText;
    [SerializeField] private Text infoText;


    private GameObject lastSelected;

    void Awake()
    {
        DmxModeManager.OnModeChanged += HandleModeChange;
    }

    void HandleModeChange(DmxModeManager.FixtureMode mode)
    {
        SetInfoText();
    }

    void Update()
    {
        GameObject selected = EventSystem.current.currentSelectedGameObject;

        if (selected == lastSelected)
        {
            return;
        }

        lastSelected = selected;

        if (selected == null)
        {
            infoText.text = defaultText;
            return;
        }
        SetInfoText();

    }

    public void SetInfoText()
    {
        GameObject selected = EventSystem.current.currentSelectedGameObject;
        IInfoText info = selected.GetComponent<IInfoText>();

        if (info != null)
            infoText.text = info.GetInfoText();
        else
            infoText.text = defaultText;
    }
}

public interface IInfoText
{
    string GetInfoText();
}
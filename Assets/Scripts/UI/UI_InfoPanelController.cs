using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class UI_InfoPanelController : MonoBehaviour
{
    [TextArea] public string defaultText;
    [SerializeField] private Text infoText;
    [SerializeField] private Text webUiText;
    [SerializeField] private GameObject webUiPanel;

    private GameObject lastSelected;

    void Awake()
    {
        DmxModeManager.OnModeChanged += HandleModeChange;
    }

    void Start()
    {
        SetWebUiText();
    }

    private void SetWebUiText()
    {
        IInfoText info = webUiPanel.GetComponent<IInfoText>();

        if (info != null)
            webUiText.text = info.GetInfoText();
        else
            webUiText.text = defaultText;
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
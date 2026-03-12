using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class webuiBindings : MonoBehaviour
{
    public Button settingsTab;
    public Button dmxTab;
    public Button loginBtn;
    public Button restartFixture;
    public TextMeshProUGUI settingsStatus;
    public TextMeshProUGUI Text_5;
    public TextMeshProUGUI Text_6;
    public Button sendDMX;

    // UnityEvents for external scripts
    public UnityEvent OnsettingsTabClicked;
    public UnityEvent OndmxTabClicked;
    public UnityEvent OnloginBtnClicked;
    public UnityEvent OnrestartFixtureClicked;
    public UnityEvent OnsendDMXClicked;

    private void Awake()
    {
        if (settingsTab != null) settingsTab.onClick.AddListener(() => OnsettingsTabClicked?.Invoke());
        if (dmxTab != null) dmxTab.onClick.AddListener(() => OndmxTabClicked?.Invoke());
        if (loginBtn != null) loginBtn.onClick.AddListener(() => OnloginBtnClicked?.Invoke());
        if (restartFixture != null) restartFixture.onClick.AddListener(() => OnrestartFixtureClicked?.Invoke());
        if (sendDMX != null) sendDMX.onClick.AddListener(() => OnsendDMXClicked?.Invoke());
    }
}

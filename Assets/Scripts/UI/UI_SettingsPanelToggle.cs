using UnityEngine;

public class UI_SettingsPanelToggle : MonoBehaviour
{
    [SerializeField] private GameObject targetObject;
    [SerializeField] private KeyCode settingsKey = KeyCode.Menu;

    private void Update()
    {
        if (Input.GetKeyDown(settingsKey))
        {
            ToggleTargetVisibility();
        }
    }

    public void ToggleTargetVisibility()
    {
        if (targetObject == null)
        {
            return;
        }

        targetObject.SetActive(!targetObject.activeSelf);
    }
}

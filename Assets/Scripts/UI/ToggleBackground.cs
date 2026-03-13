using UnityEngine;
using UnityEngine.UI;

public class ToggleBackground : MonoBehaviour
{
    public Toggle toggle;
    public Image backgroundOn;
    public Image backgroundOff;

    void Start()
    {
        toggle.onValueChanged.AddListener(OnToggleChanged);
        OnToggleChanged(toggle.isOn); // initialize
    }

    void OnToggleChanged(bool isOn)
    {
        if (isOn)
        {
            if (backgroundOn == null)
            {
                return;
            }
            toggle.targetGraphic = backgroundOn;
        }
        else
        {
            if (backgroundOff == null)
            {
                return;
            }
            toggle.targetGraphic = backgroundOff;
        }

        backgroundOn.gameObject.SetActive(isOn);
        backgroundOff.gameObject.SetActive(!isOn);
    }
}
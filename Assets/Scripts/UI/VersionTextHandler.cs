using MobileVersionCode;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class VersionTextHandler : MonoBehaviour
{

    void Start()
    {
        string versionString = "Version: " + Application.version;
#if UNITY_ANDROID && !UNITY_EDITOR

      versionString = versionString+$"  Version Code: {VersionCode.GetVersionCode()}";

#endif

        GetComponent<Text>().text = versionString;
        Debug.Log(versionString);
    }

}

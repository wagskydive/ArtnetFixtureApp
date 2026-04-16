using System.Collections;
using UnityEngine;

public class DmxBootstrap : MonoBehaviour
{
    IEnumerator Start()
    {
        yield return new WaitForSeconds(.2f);
        DmxSettingsService.Instance.Load();
    }
}
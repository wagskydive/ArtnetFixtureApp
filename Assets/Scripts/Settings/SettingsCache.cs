using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SettingsCache : MonoBehaviour
{
    Dictionary<string, object> cachedValues = new Dictionary<string, object>();

    public static SettingsCache Instance;

    public bool isDirty = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public object GetDynamicValue(string key, object defaultValue)
    {
        if (cachedValues.ContainsKey(key))
        {
            return cachedValues[key];
        }
        else
        {
            return defaultValue;
        }
    }

    public int GetIntValue(string key, int defaultValue)
    {
        if (isDirty)
        {
            cachedValues = new Dictionary<string, dynamic>();
            isDirty = false;
        }
        if (cachedValues.ContainsKey(key))
        {
            return (int)cachedValues[key];
        }
        else
        {
            int returnValue = PlayerPrefs.GetInt(key, defaultValue);
            cachedValues.Add(key, returnValue);
            return returnValue;
        }
    }

    public string GetStringValue(string key, string defaultValue)
    {
        if (isDirty)
        {
            cachedValues = new Dictionary<string, dynamic>();
            isDirty = false;
        }
        if (cachedValues.ContainsKey(key))
        {
            return (string)cachedValues[key];
        }
        else
        {
            string returnValue = PlayerPrefs.GetString(key, defaultValue);
            cachedValues.Add(key, returnValue);
            return returnValue;
        }
    }

    public float GetFloatValue(string key, float defaultValue)
    {
        if (isDirty)
        {
            cachedValues = new Dictionary<string, dynamic>();
            isDirty = false;
        }
        if (cachedValues.ContainsKey(key))
        {
            return (float)cachedValues[key];
        }
        else
        {
            float returnValue = PlayerPrefs.GetFloat(key, defaultValue);
            cachedValues.Add(key, returnValue);
            return returnValue;
        }
    }

    public void SetDirty()
    {
        isDirty = true;
    }

}

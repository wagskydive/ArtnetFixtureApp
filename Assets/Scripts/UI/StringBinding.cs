using System;
using System.Reflection;
using UnityEngine;

[Serializable]
public class StringBinding
{
    public Component target;
    public string fieldName;

    public string GetValue()
    {
        if (target == null || string.IsNullOrEmpty(fieldName))
            return null;

        var type = target.GetType();

        var field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);
        if (field != null && field.FieldType == typeof(string))
        {
            return (string)field.GetValue(target);
        }

        return null;
    }
}
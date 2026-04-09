using System;
using System.Reflection;
using UnityEngine;

[Serializable]
public class ValueBinding
{
    public GameObject targetObject;
    public Component targetComponent;
    public string memberName;

    [SerializeField] private ValueType valueType;

    private MemberInfo cachedMember;
    private Type cachedType;

    public enum ValueType
    {
        String,
        Int,
        Float,
        Bool
    }

    void Cache()
    {
        if (targetComponent == null || string.IsNullOrEmpty(memberName))
            return;

        var type = targetComponent.GetType();

        if (cachedMember != null && cachedType == type)
            return;

        cachedType = type;

        // Try field first
        var field = type.GetField(memberName,
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        if (field != null)
        {
            cachedMember = field;
            return;
        }

        // Then property
        var prop = type.GetProperty(memberName,
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        if (prop != null && prop.CanRead)
        {
            cachedMember = prop;
        }
    }

    public object GetValue()
    {
        if (targetComponent == null)
            return null;

        Cache();

        if (cachedMember is FieldInfo field)
            return field.GetValue(targetComponent);

        if (cachedMember is PropertyInfo prop)
            return prop.GetValue(targetComponent);

        return null;
    }

    // Typed helpers
    public string GetString() => GetValue() as string;
    public int GetInt() => GetValue() is int v ? v : 0;
    public float GetFloat() => GetValue() is float v ? v : 0f;
    public bool GetBool() => GetValue() is bool v && v;
}
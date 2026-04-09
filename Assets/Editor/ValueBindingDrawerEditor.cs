using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

[CustomPropertyDrawer(typeof(ValueBinding))]
public class ValueBindingDrawerEditor : PropertyDrawer
{
    static Dictionary<System.Type, MemberInfo[]> cache = new();

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var goProp = property.FindPropertyRelative("targetObject");
        var compProp = property.FindPropertyRelative("targetComponent");
        var memberProp = property.FindPropertyRelative("memberName");

        float h = EditorGUIUtility.singleLineHeight;
        float s = 2;

        Rect r1 = new(position.x, position.y, position.width, h);
        Rect r2 = new(position.x, position.y + h + s, position.width, h);
        Rect r3 = new(position.x, position.y + (h + s) * 2, position.width, h);

        // 1. GameObject
        EditorGUI.PropertyField(r1, goProp);

        GameObject go = goProp.objectReferenceValue as GameObject;

        if (go != null)
        {
            // 2. Components dropdown
            var components = go.GetComponents<MonoBehaviour>()
                               .Where(c => c != null)
                               .ToArray();

            string[] compNames = components
                .Select(c => ObjectNames.GetInspectorTitle(c))
                .ToArray();

            int compIndex = System.Array.IndexOf(
                components,
                compProp.objectReferenceValue
            );

            if (compIndex < 0) compIndex = 0;

            int newCompIndex = EditorGUI.Popup(r2, "Component", compIndex, compNames);

            if (components.Length > 0)
                compProp.objectReferenceValue = components[newCompIndex];

            var selectedComp = compProp.objectReferenceValue as Component;

            // 3. Member dropdown
            if (selectedComp != null)
            {
                var members = GetValidMembers(selectedComp.GetType());

                string[] memberNames = members
                    .Select(m => $"{m.Name} ({GetNiceTypeName(GetMemberType(m))})")
                    .ToArray();

                int memberIndex = members.FindIndex(m => m.Name == memberProp.stringValue);
                if (memberIndex < 0) memberIndex = 0;

                int newMemberIndex = EditorGUI.Popup(r3, "Value", memberIndex, memberNames);

                if (members.Count > 0)
                    memberProp.stringValue = members[newMemberIndex].Name;
            }
            else
            {
                EditorGUI.LabelField(r3, "Value", "Select a component");
            }
        }
        else
        {
            EditorGUI.LabelField(r2, "Component", "Select a GameObject");
            EditorGUI.LabelField(r3, "Value", "Select a GameObject");
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight * 3 + 6;
    }

    // 🔥 Cached reflection
    List<MemberInfo> GetValidMembers(System.Type type)
    {
        if (!cache.TryGetValue(type, out var members))
        {
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            var fields = type.GetFields(flags)
                .Where(f =>
                    IsSupportedType(f.FieldType) &&
                    (f.IsPublic || f.GetCustomAttribute<SerializeField>() != null));

            var props = type.GetProperties(flags)
                .Where(p =>
                    p.CanRead &&
                    IsSupportedType(p.PropertyType) &&
                    p.GetIndexParameters().Length == 0);

            members = fields.Cast<MemberInfo>()
                .Concat(props)
                .ToArray();

            cache[type] = members;
        }

        return members.ToList();
    }

    bool IsSupportedType(System.Type t)
    {
        return t == typeof(string) ||
               t == typeof(int) ||
               t == typeof(float) ||
               t == typeof(bool);
    }

    System.Type GetMemberType(MemberInfo m)
    {
        return m switch
        {
            FieldInfo f => f.FieldType,
            PropertyInfo p => p.PropertyType,
            _ => null
        };
    }

    string GetNiceTypeName(System.Type t)
    {
        if (t == typeof(string)) return "string";
        if (t == typeof(int)) return "int";
        if (t == typeof(float)) return "float";
        if (t == typeof(bool)) return "bool";
        return t.Name;
    }
}
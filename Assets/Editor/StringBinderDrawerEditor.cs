using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Reflection;

[CustomPropertyDrawer(typeof(StringBinding))]
public class StringBindingDrawerEditor : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var targetProp = property.FindPropertyRelative("target");
        var fieldNameProp = property.FindPropertyRelative("fieldName");

        Rect line1 = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        Rect line2 = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2, position.width, EditorGUIUtility.singleLineHeight);

        // Draw component field
        EditorGUI.PropertyField(line1, targetProp);

        Component target = targetProp.objectReferenceValue as Component;

        if (target != null)
        {
            var fields = target.GetType()
                .GetFields(BindingFlags.Public | BindingFlags.Instance)
                .Where(f => f.FieldType == typeof(string))
                .ToArray();

            string[] options = fields.Select(f => f.Name).ToArray();

            int currentIndex = Mathf.Max(0, System.Array.IndexOf(options, fieldNameProp.stringValue));

            int selectedIndex = EditorGUI.Popup(line2, "Field", currentIndex, options);

            if (options.Length > 0)
                fieldNameProp.stringValue = options[selectedIndex];
        }
        else
        {
            EditorGUI.LabelField(line2, "Field", "Select a component first");
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight * 2 + 4;
    }
}
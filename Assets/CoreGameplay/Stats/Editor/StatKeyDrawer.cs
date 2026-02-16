#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Linq;

[CustomPropertyDrawer(typeof(StatKeyAttribute))]
public sealed class StatKeyDrawer : PropertyDrawer
{
    public override void OnGUI(
        Rect position,
        SerializedProperty property,
        GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.String)
        {
            EditorGUI.LabelField(position, label.text, "Use on string only");
            return;
        }

        var keys = StatKeyRegistry.AllIds; // 👈 список ВСЕХ ключей
        int index = Mathf.Max(0, keys.IndexOf(property.stringValue));

        int newIndex = EditorGUI.Popup(
            position,
            label.text,
            index,
            keys.ToArray()
        );

        property.stringValue = keys[newIndex];
    }
}
#endif

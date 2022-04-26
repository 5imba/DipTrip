/*
using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;
using System;

public class NamedArrayAttribute : PropertyAttribute
{
    public readonly string[] names;
    public readonly string name;
    public readonly bool singleName;
    public NamedArrayAttribute(string[] names) { this.names = names; singleName = false; }
    public NamedArrayAttribute(string name) { this.name = name; singleName = true; }
}


[CustomPropertyDrawer(typeof(NamedArrayAttribute))]
public class NamedArrayDrawer : PropertyDrawer
{
#if UNITY_EDITOR
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        try
        {
            MatchCollection matches = Regex.Matches(property.propertyPath, @"(?<=Array.data\[).*(?=\])");
            int pos = int.Parse(matches[matches.Count-1].Value);
        
            string content = ((NamedArrayAttribute)attribute).singleName 
                ? string.Format("{0} {1}", ((NamedArrayAttribute)attribute).name, pos)
                : ((NamedArrayAttribute)attribute).names[pos];

            EditorGUI.ObjectField(position, property, new GUIContent(content));

            //EditorGUI.PropertyField(position, property, new GUIContent(content), true);
        }
        catch
        {
            EditorGUI.ObjectField(position, property, label);
        }
    }
#endif
}

*/

/*
[CustomPropertyDrawer(typeof(PatternSettings.PatternPoint))]
public class PatternPointDrawerUIE : PropertyDrawer
{
    // Draw the property inside the given rect
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Using BeginProperty / EndProperty on the parent property means that
        // prefab override logic works on the entire property.
        EditorGUI.BeginProperty(position, label, property);

        // Draw label
        try
        {
            int index = int.Parse(Regex.Match(property.propertyPath, @"(?<=points.Array.data\[).*(?=\])").Value);
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), new GUIContent("Point "+index));
        }
        catch(Exception e)
        {
            Debug.LogException(e);
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
        }

        // Don't make child fields be indented
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        // Calculate rects
        int propWidth = (int)position.width / 4;
        var side0 = new Rect(position.x, position.y, propWidth, position.height);
        var side1 = new Rect(position.x + propWidth, position.y, propWidth, position.height);
        var side2 = new Rect(position.x + 2*propWidth, position.y, propWidth, position.height);
        var side3 = new Rect(position.x + 3*propWidth, position.y, propWidth, position.height);

        // Draw fields - passs GUIContent.none to each so they are drawn without labels
        EditorGUI.PropertyField(side0, property.FindPropertyRelative("side0"), GUIContent.none);
        EditorGUI.PropertyField(side1, property.FindPropertyRelative("side1"), GUIContent.none);
        EditorGUI.PropertyField(side2, property.FindPropertyRelative("side2"), GUIContent.none);
        EditorGUI.PropertyField(side3, property.FindPropertyRelative("side3"), GUIContent.none);

        // Set indent back to what it was
        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();
    }
}
*/

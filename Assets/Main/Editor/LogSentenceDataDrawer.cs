using NarrativeGP.Logs;
using UnityEditor;
using UnityEngine;

namespace NarrativeGP.Editor
{
    [CustomPropertyDrawer(typeof(LogSentenceData))]
    public class LogSentenceDataDrawer : PropertyDrawer
    {
        private const float VerticalSpacing = 4f;
        private const float ButtonHeight = 22f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty rawTextProperty = property.FindPropertyRelative("rawText");
            SerializedProperty segmentsProperty = property.FindPropertyRelative("segments");

            float y = position.y;
            Rect foldoutRect = new(position.x, y, position.width, EditorGUIUtility.singleLineHeight);
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);
            y += EditorGUIUtility.singleLineHeight + VerticalSpacing;

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;

                float rawTextHeight = EditorGUI.GetPropertyHeight(rawTextProperty, true);
                Rect rawTextRect = new(position.x, y, position.width, rawTextHeight);
                EditorGUI.PropertyField(rawTextRect, rawTextProperty, new GUIContent("Raw Text"), true);
                y += rawTextHeight + VerticalSpacing;

                Rect buttonRect = new(position.x, y, 140f, ButtonHeight);
                if (GUI.Button(buttonRect, "Parse Raw Text"))
                {
                    ParseRawTextIntoSegments(rawTextProperty, segmentsProperty);
                    property.serializedObject.ApplyModifiedProperties();
                    GUI.changed = true;
                }

                y += ButtonHeight + VerticalSpacing;

                float segmentsHeight = EditorGUI.GetPropertyHeight(segmentsProperty, true);
                Rect segmentsRect = new(position.x, y, position.width, segmentsHeight);
                EditorGUI.PropertyField(segmentsRect, segmentsProperty, new GUIContent("Segments"), true);

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight + VerticalSpacing;

            if (!property.isExpanded)
            {
                return height;
            }

            SerializedProperty rawTextProperty = property.FindPropertyRelative("rawText");
            SerializedProperty segmentsProperty = property.FindPropertyRelative("segments");

            height += EditorGUI.GetPropertyHeight(rawTextProperty, true) + VerticalSpacing;
            height += ButtonHeight + VerticalSpacing;
            height += EditorGUI.GetPropertyHeight(segmentsProperty, true);
            return height;
        }

        private static void ParseRawTextIntoSegments(SerializedProperty rawTextProperty, SerializedProperty segmentsProperty)
        {
            LogSentenceData parsedSentence = LogSentenceParser.Parse(rawTextProperty.stringValue);
            segmentsProperty.ClearArray();

            if (parsedSentence?.segments == null)
            {
                return;
            }

            segmentsProperty.arraySize = parsedSentence.segments.Count;

            for (int i = 0; i < parsedSentence.segments.Count; i++)
            {
                LogSegmentData sourceSegment = parsedSentence.segments[i];
                SerializedProperty targetSegment = segmentsProperty.GetArrayElementAtIndex(i);

                SerializedProperty segmentTypeProperty = targetSegment.FindPropertyRelative("segmentType");
                SerializedProperty textProperty = targetSegment.FindPropertyRelative("text");
                SerializedProperty optionsProperty = targetSegment.FindPropertyRelative("options");
                SerializedProperty correctIndexProperty = targetSegment.FindPropertyRelative("correctIndex");

                segmentTypeProperty.enumValueIndex = (int)sourceSegment.segmentType;
                textProperty.stringValue = sourceSegment.text ?? string.Empty;
                correctIndexProperty.intValue = sourceSegment.correctIndex;

                optionsProperty.ClearArray();
                if (sourceSegment.options != null)
                {
                    optionsProperty.arraySize = sourceSegment.options.Count;
                    for (int optionIndex = 0; optionIndex < sourceSegment.options.Count; optionIndex++)
                    {
                        optionsProperty.GetArrayElementAtIndex(optionIndex).stringValue = sourceSegment.options[optionIndex];
                    }
                }
            }
        }
    }
}

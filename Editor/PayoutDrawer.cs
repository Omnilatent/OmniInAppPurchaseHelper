using UnityEngine;
using UnityEditor;

namespace Omnilatent.InAppPurchase.EditorNS
{
    [CustomPropertyDrawer(typeof(Payout))]
    public class PayoutDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Draw the main label
            Rect labelRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(labelRect, label);

            // Calculate rects for the second line
            Rect contentRect = new Rect(position.x,
                position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing,
                position.width, EditorGUIUtility.singleLineHeight);

            float spacing = 10f;
            float labelWidth = 60f;
            float fieldWidth = (contentRect.width - 2 * spacing) / 3f;

            var typeRect = new Rect(contentRect.x, contentRect.y, fieldWidth, contentRect.height);
            var subtypeIdLabelRect = new Rect(typeRect.xMax + spacing, contentRect.y, labelWidth, contentRect.height);
            var subtypeIdFieldRect = new Rect(subtypeIdLabelRect.xMax, contentRect.y, fieldWidth - labelWidth, contentRect.height);
            var quantityLabelRect = new Rect(subtypeIdFieldRect.xMax + spacing, contentRect.y, labelWidth, contentRect.height);
            var quantityFieldRect = new Rect(quantityLabelRect.xMax, contentRect.y, fieldWidth - labelWidth, contentRect.height);

            float dividerWidth = 2f;
            var divider1Rect = new Rect(subtypeIdFieldRect.xMax + spacing / 2, contentRect.y, dividerWidth, contentRect.height);

            // Draw fields
            EditorGUI.PropertyField(typeRect, property.FindPropertyRelative("payoutType"), GUIContent.none);

            EditorGUI.LabelField(subtypeIdLabelRect, "Subtype:");
            EditorGUI.PropertyField(subtypeIdFieldRect, property.FindPropertyRelative("subtypeId"), GUIContent.none);

            EditorGUI.DrawRect(divider1Rect, Color.gray);
            EditorGUI.LabelField(quantityLabelRect, "Quantity:");
            EditorGUI.PropertyField(quantityFieldRect, property.FindPropertyRelative("quantity"), GUIContent.none);

            EditorGUI.EndProperty();
        }
    }
}
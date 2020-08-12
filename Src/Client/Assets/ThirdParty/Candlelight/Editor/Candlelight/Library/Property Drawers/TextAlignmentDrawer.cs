// 
// TextAlignmentDrawer.cs
// 
// Copyright (c) 2014, Candlelight Interactive, LLC
// All rights reserved.
// 
// This file is licensed according to the terms of the Unity Asset Store EULA:
// http://download.unity3d.com/assetstore/customer-eula.pdf
// 
// This file contains as custom PropertyDrawer for TextAlignment. It mirrors that
// found in UnityEditor.UI.FontDataDrawer.

using UnityEditor;
using UnityEngine;
using System.Reflection;

namespace Candlelight
{
	/// <summary>
	/// Text alignment drawer.
	/// </summary>
	[CustomPropertyDrawer(typeof(TextAnchor))]
	public class TextAlignmentDrawer : PropertyDrawer
	{
		/// <summary>
		/// The styles in <see cref="UnityEditor.UI.FontDataDrawer"/>.
		/// </summary>
		private static readonly System.Type s_Styles =
			typeof(UnityEditor.UI.FontDataDrawer).GetNestedType("Styles", BindingFlags.NonPublic);
		/// <summary>
		/// The text alignment hash.
		/// </summary>
		private static readonly int s_TextAlignmentHash = "DoTextAligmentControl".GetHashCode();

		/// <summary>
		/// Creates an editor toggle.
		/// </summary>
		/// <returns><see langword="true"/>, if toggle is on; otherwise, <see langword="false"/>.</returns>
		/// <param name="position">Position.</param>
		/// <param name="label">Label.</param>
		/// <param name="value">Current toggle value.</param>
		/// <param name="style">Style.</param>
		private static bool EditorToggle(Rect position, GUIContent label, bool value, GUIStyle style)
		{
			int hashCode = "AlignToggle".GetHashCode ();
			int controlID = GUIUtility.GetControlID (hashCode, FocusType.Passive, position);
			Event current = Event.current;
			if (
				GUIUtility.keyboardControl == controlID &&
				current.type == EventType.KeyDown && (
					current.keyCode == KeyCode.Space ||
					current.keyCode == KeyCode.KeypadEnter ||
					current.keyCode == KeyCode.Return
				)
			)
			{
				value = !value;
				current.Use();
				GUI.changed = true;
			}
			if (
				current.type == EventType.MouseDown &&
				Event.current.button == 0 &&
				position.Contains(Event.current.mousePosition)
			)
			{
				GUIUtility.keyboardControl = controlID;
				EditorGUIUtility.editingTextField = false;
				HandleUtility.Repaint ();
			}
			return GUI.Toggle(position, controlID, value, label, style);
		}
		
		/// <summary>
		/// Raises the GUI event.
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="property">Property.</param>
		/// <param name="label">Label.</param>
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			int controlID = GUIUtility.GetControlID(s_TextAlignmentHash, FocusType.Passive, position);
			EditorGUIUtility.SetIconSize(new Vector2(15f, 15f));
			EditorGUI.BeginProperty(position, label, property);
			Rect rect = EditorGUI.PrefixLabel(position, controlID, label);
			float num = 60f;
			float num2 = Mathf.Clamp(rect.width - num * 2f, 2f, 10f);
			Rect position2 = new Rect(rect.x, rect.y, num, rect.height);
			Rect position3 = new Rect(position2.xMax + num2, rect.y, num, rect.height);
			DisplayHorizontalAligmentControl(position2, property);
			DisplayVerticalAligmentControl(position3, property);
			EditorGUI.EndProperty();
			EditorGUIUtility.SetIconSize(Vector2.zero);
		}
		
		/// <summary>
		/// Sets the horizontal alignment.
		/// </summary>
		/// <param name="property">Property.</param>
		/// <param name="horizontalAlignment">Horizontal alignment.</param>
		private void SetHorizontalAlignment(SerializedProperty property, HorizontalTextAligment horizontalAlignment)
		{
			foreach (Object obj in property.serializedObject.targetObjects)
			{
				VerticalTextAligment verticalAlignment =
					((TextAnchor)property.enumValueIndex).GetVerticalAlignment();
				Undo.RecordObject(obj, "Horizontal Alignment");
				property.enumValueIndex = (int)TextAnchorX.GetAnchor(verticalAlignment, horizontalAlignment);
				EditorUtility.SetDirty(obj);
			}
		}

		/// <summary>
		/// Displays the horizontal aligment control.
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="property">Property.</param>
		private void DisplayHorizontalAligmentControl(Rect position, SerializedProperty property)
		{
			TextAnchor intValue = (TextAnchor)property.intValue;
			HorizontalTextAligment horizontalAlignment = intValue.GetHorizontalAlignment();
			bool flag = horizontalAlignment == HorizontalTextAligment.Left;
			bool flag2 = horizontalAlignment == HorizontalTextAligment.Center;
			bool flag3 = horizontalAlignment == HorizontalTextAligment.Right;
			if (property.hasMultipleDifferentValues)
			{
				Object[] targetObjects = property.serializedObject.targetObjects;
				for(int i = 0; i < targetObjects.Length; i++)
				{
					Object obj = targetObjects [i];
					UnityEngine.UI.Text text = obj as UnityEngine.UI.Text;
					horizontalAlignment = text.alignment.GetHorizontalAlignment();
					flag = flag || horizontalAlignment == HorizontalTextAligment.Left;
					flag2 = flag2 || horizontalAlignment == HorizontalTextAligment.Center;
					flag3 = flag3 || horizontalAlignment == HorizontalTextAligment.Right;
				}
			}
			position.width = 20f;
			EditorGUI.BeginChangeCheck();
			EditorToggle(
				position,
				!flag ?
					s_Styles.GetStaticFieldValue<GUIContent>("m_LeftAlignText") :
					s_Styles.GetStaticFieldValue<GUIContent>("m_LeftAlignTextActive"),
				flag,
				s_Styles.GetStaticFieldValue<GUIStyle>("alignmentButtonLeft")
			);
			if (EditorGUI.EndChangeCheck())
			{
				SetHorizontalAlignment(property, HorizontalTextAligment.Left);
			}
			position.x += position.width;
			EditorGUI.BeginChangeCheck();
			EditorToggle(
				position,
				!flag2 ?
					s_Styles.GetStaticFieldValue<GUIContent>("m_CenterAlignText") :
					s_Styles.GetStaticFieldValue<GUIContent>("m_CenterAlignTextActive"),
				flag2,
				s_Styles.GetStaticFieldValue<GUIStyle>("alignmentButtonMid")
			);
			if (EditorGUI.EndChangeCheck())
			{
				SetHorizontalAlignment(property, HorizontalTextAligment.Center);
			}
			position.x += position.width;
			EditorGUI.BeginChangeCheck();
			EditorToggle(
				position,
				!flag3 ?
					s_Styles.GetStaticFieldValue<GUIContent>("m_RightAlignText") :
					s_Styles.GetStaticFieldValue<GUIContent>("m_RightAlignTextActive"),
				flag3,
				s_Styles.GetStaticFieldValue<GUIStyle>("alignmentButtonRight")
			);
			if (EditorGUI.EndChangeCheck())
			{
				this.SetHorizontalAlignment(property, HorizontalTextAligment.Right);
			}
		}

		/// <summary>
		/// Displays the vertical aligment control.
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="property">Property.</param>
		private void DisplayVerticalAligmentControl(Rect position, SerializedProperty property)
		{
			TextAnchor intValue = (TextAnchor)property.intValue;
			VerticalTextAligment verticalAlignment = intValue.GetVerticalAlignment();
			bool flag = verticalAlignment == VerticalTextAligment.Top;
			bool flag2 = verticalAlignment == VerticalTextAligment.Middle;
			bool flag3 = verticalAlignment == VerticalTextAligment.Bottom;
			if (property.hasMultipleDifferentValues)
			{
				Object[] targetObjects = property.serializedObject.targetObjects;
				for(int i = 0; i < targetObjects.Length; i++)
				{
					Object obj = targetObjects [i];
					TextAnchor alignment2 =
						(TextAnchor)new SerializedObject(obj).FindProperty(property.propertyPath).enumValueIndex;
					verticalAlignment = alignment2.GetVerticalAlignment();
					flag = flag || verticalAlignment == VerticalTextAligment.Top;
					flag2 = flag2 || verticalAlignment == VerticalTextAligment.Middle;
					flag3 = flag3 || verticalAlignment == VerticalTextAligment.Bottom;
				}
			}
			position.width = 20f;
			EditorGUI.BeginChangeCheck();
			EditorToggle(
				position,
				!flag ?
					s_Styles.GetStaticFieldValue<GUIContent>("m_TopAlignText") :
					s_Styles.GetStaticFieldValue<GUIContent>("m_TopAlignTextActive"),
				flag,
				s_Styles.GetStaticFieldValue<GUIStyle>("alignmentButtonLeft")
			);
			if (EditorGUI.EndChangeCheck())
			{
				this.SetVerticalAlignment(property, VerticalTextAligment.Top);
			}
			position.x += position.width;
			EditorGUI.BeginChangeCheck();
			EditorToggle(
				position,
				!flag2 ?
					s_Styles.GetStaticFieldValue<GUIContent>("m_MiddleAlignText") :
					s_Styles.GetStaticFieldValue<GUIContent>("m_MiddleAlignTextActive"),
				flag2,
				s_Styles.GetStaticFieldValue<GUIStyle>("alignmentButtonMid")
			);
			if (EditorGUI.EndChangeCheck())
			{
				SetVerticalAlignment(property, VerticalTextAligment.Middle);
			}
			position.x += position.width;
			EditorGUI.BeginChangeCheck();
			EditorToggle(
				position,
				!flag3 ?
					s_Styles.GetStaticFieldValue<GUIContent>("m_BottomAlignText") :
					s_Styles.GetStaticFieldValue<GUIContent>("m_BottomAlignTextActive"),
				flag3,
				s_Styles.GetStaticFieldValue<GUIStyle>("alignmentButtonRight")
			);
			if (EditorGUI.EndChangeCheck())
			{
				SetVerticalAlignment(property, VerticalTextAligment.Bottom);
			}
		}

		/// <summary>
		/// Sets the vertical alignment.
		/// </summary>
		/// <param name="property">Property.</param>
		/// <param name="verticalAlignment">Vertical alignment.</param>
		private void SetVerticalAlignment(SerializedProperty property, VerticalTextAligment verticalAlignment)
		{
			foreach (Object obj in property.serializedObject.targetObjects)
			{
				HorizontalTextAligment horizontalAlignment =
					((TextAnchor)property.enumValueIndex).GetHorizontalAlignment();
				Undo.RecordObject(obj, "Vertical Alignment");
				property.enumValueIndex = (int)TextAnchorX.GetAnchor(verticalAlignment, horizontalAlignment);
				EditorUtility.SetDirty(obj);
			}
		}
	}
}
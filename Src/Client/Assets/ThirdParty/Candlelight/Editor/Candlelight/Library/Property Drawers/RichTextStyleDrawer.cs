// 
// RichTextStyleDrawer.cs
// 
// Copyright (c) 2014, Candlelight Interactive, LLC
// All rights reserved.
// 
// This file is licensed according to the terms of the Unity Asset Store EULA:
// http://download.unity3d.com/assetstore/customer-eula.pdf
// 
// This file contains a custom property drawer for RichTextStyle.

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace Candlelight
{
	/// <summary>
	/// Rich text style drawer.
	/// </summary>
	[CustomPropertyDrawer(typeof(RichTextStyle))]
	public class RichTextStyleDrawer : PropertyDrawer
	{
		/// <summary>
		/// The content of the colorization field.
		/// </summary>
		private static readonly GUIContent colorizationGUIContent =
			new GUIContent("Color Tags", "Enable if instances of this style should wrap text in <color> tags.");

		#region SerializedProperties
		private Dictionary<string, SerializedProperty> fontStyle = new Dictionary<string, SerializedProperty>();
		private Dictionary<string, SerializedProperty> replacementColor = new Dictionary<string, SerializedProperty>();
		private Dictionary<string, SerializedProperty> shouldReplaceColor =
			new Dictionary<string, SerializedProperty>();
		private Dictionary<string, SerializedProperty> sizeScalar = new Dictionary<string, SerializedProperty>();
		#endregion

		/// <summary>
		/// Gets the height of the property.
		/// </summary>
		/// <returns>The property height.</returns>
		/// <param name="property">Property.</param>
		/// <param name="label">Label.</param>
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUIUtility.singleLineHeight * 3f + EditorGUIX.controlMargin.vertical;
		}

		/// <summary>
		/// Initialize the specified property.
		/// </summary>
		/// <param name="property">Property.</param>
		private void Initialize(SerializedProperty property)
		{
			if (!fontStyle.ContainsKey(property.propertyPath))
			{
				fontStyle.Add(property.propertyPath, property.FindPropertyRelative("m_FontStyle"));
				replacementColor.Add(property.propertyPath, property.FindPropertyRelative("m_ReplacementColor"));
             	shouldReplaceColor.Add(property.propertyPath, property.FindPropertyRelative("m_ShouldReplaceColor"));
				sizeScalar.Add(property.propertyPath, property.FindPropertyRelative("m_SizeScalar"));
			}
		}

		/// <summary>
		/// Raises the GUI event.
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="property">Property.</param>
		/// <param name="label">Label.</param>
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			// TODO: handle multiple different values
			Initialize(property);
			float entireWidth = position.width;
			float entireX = position.x;
			position.height = EditorGUIUtility.singleLineHeight;
			EditorGUI.PropertyField(position, sizeScalar[property.propertyPath]);
			position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
			EditorGUI.PropertyField(position, fontStyle[property.propertyPath]);
			position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
			float checkboxWidth = 14f;
			position = EditorGUI.IndentedRect(position);
			position.width =
				EditorGUIUtility.labelWidth + checkboxWidth - EditorGUI.indentLevel * EditorGUIX.pixelsPerIndentLevel;
			int oldIndent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;
			float oldLabelWidth = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth -= oldIndent * EditorGUIX.pixelsPerIndentLevel;
			EditorGUI.PropertyField(position, shouldReplaceColor[property.propertyPath], colorizationGUIContent);
			EditorGUI.indentLevel = oldIndent;
			EditorGUIUtility.labelWidth = oldLabelWidth;
			EditorGUI.BeginDisabledGroup(!shouldReplaceColor[property.propertyPath].boolValue);
			{
				position.x = entireX + position.width + EditorGUIX.standardHorizontalSpacing;
				position.width = entireWidth - (position.x - entireX);
				EditorGUI.PropertyField(position, replacementColor[property.propertyPath], GUIContent.none);
			}
			EditorGUI.EndDisabledGroup();
		}
	}
}
// 
// HyperTextTextStyleDrawer.cs
// 
// Copyright (c) 2014, Candlelight Interactive, LLC
// All rights reserved.
// 
// This file is licensed according to the terms of the Unity Asset Store EULA:
// http://download.unity3d.com/assetstore/customer-eula.pdf
// 
// This file contains a custom property drawer for HyperTextStyles.Text.

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace Candlelight.UI
{
	/// <summary>
	/// Hyper text text style drawer.
	/// </summary>
	[CustomPropertyDrawer(typeof(HyperTextStyles.Text))]
	public class HyperTextTextStyleDrawer : HyperTextStyleDrawer
	{
		#region Labels
		private static readonly GUIContent colorizationGUIContent =
			new GUIContent("Color", "Enable if instances of this style should wrap text in <color> tags.");
		private static readonly GUIContent fontStyleGUIContent =
			new GUIContent("Style", "Style to apply to the font face.");
		private static readonly GUIContent tagGUIContent =
			new GUIContent("Tag", "Unique name in the collection of styles used to reference style.");
		#endregion
		/// <summary>
		/// The height of the property.
		/// </summary>
		public static readonly float propertyHeight =
			4f * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);

		#region Serialized Properties
		private Dictionary<string, SerializedProperty> fontStyle = new Dictionary<string, SerializedProperty>();
		private Dictionary<string, SerializedProperty> replacementColor = new Dictionary<string, SerializedProperty>();
		private Dictionary<string, SerializedProperty> shouldReplaceColor =
			new Dictionary<string, SerializedProperty>();
		private Dictionary<string, SerializedProperty> tag = new Dictionary<string, SerializedProperty>();
		#endregion

		/// <summary>
		/// Gets the height of the property.
		/// </summary>
		/// <value>The height of the property.</value>
		protected override float PropertyHeight { get { return propertyHeight; } }
		/// <summary>
		/// Gets the size property name prefix.
		/// </summary>
		/// <value>The size property name prefix.</value>
		protected override string SizePropertyNamePrefix { get { return "m_TextStyle."; } }

		/// <summary>
		/// Displays the custom fields.
		/// </summary>
		/// <returns>The number of lines drawn in the inspector.</returns>
		/// <param name="firstLinePosition">Position of the first line.</param>
		/// <param name="property">Property.</param>
		protected override int DisplayCustomFields(Rect firstLinePosition, SerializedProperty property)
		{
			float entireWidth = firstLinePosition.width;
			float entireX = firstLinePosition.x;
			EditorGUI.PropertyField(firstLinePosition, fontStyle[property.propertyPath], fontStyleGUIContent);
			firstLinePosition.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
			firstLinePosition.width = EditorGUIUtility.labelWidth + 14f;
			EditorGUI.PropertyField(
				firstLinePosition, shouldReplaceColor[property.propertyPath], colorizationGUIContent
			);
			firstLinePosition.x += EditorGUIUtility.labelWidth + EditorGUIX.standardHorizontalSpacing + 14f;
			firstLinePosition.width = entireWidth - (firstLinePosition.x - entireX);
			EditorGUI.BeginDisabledGroup(!shouldReplaceColor[property.propertyPath].boolValue);
			{
				EditorGUI.PropertyField(firstLinePosition, replacementColor[property.propertyPath], GUIContent.none);
			}
			EditorGUI.EndDisabledGroup();
			return 2;
		}

		/// <summary>
		/// Displays the identifier field for this style.
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="property">Property.</param>
		protected override void DisplayIdentifierField(Rect position, SerializedProperty property)
		{
			EditorGUI.PropertyField(position, tag[property.propertyPath], tagGUIContent);
		}

		/// <summary>
		/// Initialize this instance.
		/// </summary>
		/// <param name="property">Property.</param>
		protected override void Initialize(SerializedProperty property)
		{
			base.Initialize(property);
			if (!fontStyle.ContainsKey(property.propertyPath))
			{
				fontStyle.Add(
					property.propertyPath,
					property.FindPropertyRelative(string.Format("{0}m_FontStyle", SizePropertyNamePrefix))
				);
				replacementColor.Add(
					property.propertyPath, 
					property.FindPropertyRelative(string.Format("{0}m_ReplacementColor", SizePropertyNamePrefix))
				);
				shouldReplaceColor.Add(
					property.propertyPath, 
					property.FindPropertyRelative(string.Format("{0}m_ShouldReplaceColor", SizePropertyNamePrefix))
				);
				tag.Add(property.propertyPath, property.FindPropertyRelative("m_Tag"));
			}
		}
	}
}
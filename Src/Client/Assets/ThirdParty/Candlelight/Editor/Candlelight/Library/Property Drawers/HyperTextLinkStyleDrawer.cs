// 
// HyperTextLinkStyleDrawer.cs
// 
// Copyright (c) 2014, Candlelight Interactive, LLC
// All rights reserved.
// 
// This file is licensed according to the terms of the Unity Asset Store EULA:
// http://download.unity3d.com/assetstore/customer-eula.pdf
// 
// This file contains a custom property drawer for
// HyperTextStyles.Link.

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace Candlelight.UI
{
	/// <summary>
	/// Hyper text link style drawer.
	/// </summary>
	[CustomPropertyDrawer(typeof(HyperTextStyles.Link))]
	public class HyperTextLinkStyleDrawer : PropertyDrawer
	{
		/// <summary>
		/// Gets the height of the property.
		/// </summary>
		/// <value>The height of the property.</value>
		public static float PropertyHeight
		{
			get { return 13f * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing); }
		}

		#region Serialized Properties
		private Dictionary<string, SerializedProperty> m_ColorMultiplier = new Dictionary<string, SerializedProperty>();
		private Dictionary<string, SerializedProperty> m_ColorTintMode = new Dictionary<string, SerializedProperty>();
		private Dictionary<string, SerializedProperty> m_ColorTweenMode = new Dictionary<string, SerializedProperty>();
		private Dictionary<string, SerializedProperty> m_DisabledColor = new Dictionary<string, SerializedProperty>();
		private Dictionary<string, SerializedProperty> m_DadeDuration = new Dictionary<string, SerializedProperty>();
		private Dictionary<string, SerializedProperty> m_HighlightedColor = new Dictionary<string, SerializedProperty>();
		private Dictionary<string, SerializedProperty> m_NormalColor = new Dictionary<string, SerializedProperty>();
		private Dictionary<string, SerializedProperty> m_PressedColor = new Dictionary<string, SerializedProperty>();
		private Dictionary<string, SerializedProperty> m_TextStyle = new Dictionary<string, SerializedProperty>();
		private Dictionary<string, SerializedProperty> m_VerticalOffset = new Dictionary<string, SerializedProperty>();
		#endregion

		/// <summary>
		/// Gets the height of the property.
		/// </summary>
		/// <returns>The property height.</returns>
		/// <param name="property">Property.</param>
		/// <param name="label">Label.</param>
		public override float GetPropertyHeight (SerializedProperty property, GUIContent label)
		{
			return PropertyHeight;
		}

		/// <summary>
		/// Initialize this instance.
		/// </summary>
		/// <param name="property">Property.</param>
		private void Initialize(SerializedProperty property)
		{
			if (!m_ColorMultiplier.ContainsKey(property.propertyPath))
			{
				m_ColorMultiplier.Add(property.propertyPath, property.FindPropertyRelative("m_Colors.m_ColorMultiplier"));
				m_ColorTintMode.Add(property.propertyPath, property.FindPropertyRelative("m_ColorTintMode"));
				m_ColorTweenMode.Add(property.propertyPath, property.FindPropertyRelative("m_ColorTweenMode"));
				m_DisabledColor.Add(property.propertyPath, property.FindPropertyRelative("m_Colors.m_DisabledColor"));
				m_HighlightedColor.Add(
					property.propertyPath, property.FindPropertyRelative("m_Colors.m_HighlightedColor")
				);
				m_NormalColor.Add(property.propertyPath, property.FindPropertyRelative("m_Colors.m_NormalColor"));
				m_PressedColor.Add(property.propertyPath, property.FindPropertyRelative("m_Colors.m_PressedColor"));
				m_DadeDuration.Add(property.propertyPath, property.FindPropertyRelative("m_Colors.m_FadeDuration"));
				m_TextStyle.Add(property.propertyPath, property.FindPropertyRelative("m_TextStyle"));
				m_VerticalOffset.Add(property.propertyPath, property.FindPropertyRelative("m_VerticalOffset"));
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
			Initialize(property);
			EditorGUI.BeginProperty(position, label, property);
			EditorGUI.PrefixLabel(position, label);
			position.height = EditorGUIUtility.singleLineHeight;
			++EditorGUI.indentLevel;
			position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
			EditorGUI.PropertyField(position, m_VerticalOffset[property.propertyPath]);
			position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
			EditorGUI.PropertyField(position, m_TextStyle[property.propertyPath]);
			position.y += (position.height + EditorGUIUtility.standardVerticalSpacing) * 3f;
			EditorGUI.PropertyField(position, m_NormalColor[property.propertyPath]);
			position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
			EditorGUI.PropertyField(position, m_HighlightedColor[property.propertyPath]);
			position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
			EditorGUI.PropertyField(position, m_PressedColor[property.propertyPath]);
			position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
			EditorGUI.PropertyField(position, m_DisabledColor[property.propertyPath]);
			position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
			EditorGUI.PropertyField(position, m_ColorMultiplier[property.propertyPath]);
			position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
			EditorGUI.PropertyField(position, m_ColorTintMode[property.propertyPath]);
			position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
			EditorGUI.PropertyField(position, m_DadeDuration[property.propertyPath]);
			position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
			EditorGUI.PropertyField(position, m_ColorTweenMode[property.propertyPath]);
			--EditorGUI.indentLevel;
			EditorGUI.EndProperty();
		}
	}
}
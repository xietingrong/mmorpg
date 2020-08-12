// 
// HyperTextLinkSubclassDrawer.cs
// 
// Copyright (c) 2014-2015, Candlelight Interactive, LLC
// All rights reserved.
// 
// This file is licensed according to the terms of the Unity Asset Store EULA:
// http://download.unity3d.com/assetstore/customer-eula.pdf
// 
// This file contains a custom property drawer for
// HyperTextStyles.LinkSubclass.

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace Candlelight.UI
{
	/// <summary>
	/// Hyper text link subclass drawer.
	/// </summary>
	[CustomPropertyDrawer(typeof(HyperTextStyles.LinkSubclass))]
	public class HyperTextLinkSubclassDrawer : HyperTextTextStyleDrawer
	{
		#region Labels
		private static readonly GUIContent s_DisabledColorGuiContent =
			new GUIContent("Disabled", "State color for disabled link.");
		private static readonly GUIContent s_FadeDurationGuiContent =
			new GUIContent("Fade", "Length of fade between state colors during transitions.");
		private static readonly GUIContent s_HighlightColorGuiContent =
			new GUIContent("Highlight", "State color for highlighted link.");
		private static readonly GUIContent s_MultiplierGuiContent =
			new GUIContent("Multiplier", "Value multiplied into state color before blending.");
		private static readonly GUIContent s_NormalColorGuiContent =
			new GUIContent("Normal", "State color for normal link.");
		private static readonly GUIContent s_PressedColorGuiContent =
			new GUIContent("Pressed", "State color for pressed link.");
		private static readonly GUIContent s_TintModeGuiContent =
			new GUIContent("Tint", HyperTextStyles.Link.ColorTintModeExplanation);
		private static readonly GUIContent s_TweenModeGuiContent =
			new GUIContent("Tween", "What channels in the state colors should be blended into the base color?");
		#endregion

		/// <summary>
		/// The height of the property.
		/// </summary>
		new public static readonly float propertyHeight =
			8f * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);

		#region Serialized Properties
		private Dictionary<string, SerializedProperty> className = new Dictionary<string, SerializedProperty>();
		private Dictionary<string, SerializedProperty> colorMultiplier = new Dictionary<string, SerializedProperty>();
		private Dictionary<string, SerializedProperty> colorTintMode = new Dictionary<string, SerializedProperty>();
		private Dictionary<string, SerializedProperty> colorTweenMode = new Dictionary<string, SerializedProperty>();
		private Dictionary<string, SerializedProperty> disabledColor = new Dictionary<string, SerializedProperty>();
		private Dictionary<string, SerializedProperty> fadeDuration = new Dictionary<string, SerializedProperty>();
		private Dictionary<string, SerializedProperty> highlightedColor = new Dictionary<string, SerializedProperty>();
		private Dictionary<string, SerializedProperty> normalColor = new Dictionary<string, SerializedProperty>();
		private Dictionary<string, SerializedProperty> pressedColor = new Dictionary<string, SerializedProperty>();
		#endregion

		/// <summary>
		/// Gets the offset property name prefix.
		/// </summary>
		/// <value>The offset property name prefix.</value>
		protected override string OffsetPropertyNamePrefix { get { return "m_Style."; } }
		/// <summary>
		/// Gets the height of the property.
		/// </summary>
		/// <value>The height of the property.</value>
		protected override float PropertyHeight { get { return propertyHeight; } }
		/// <summary>
		/// Gets the size property name prefix.
		/// </summary>
		/// <value>The size property name prefix.</value>
		protected override string SizePropertyNamePrefix
		{
			get { return string.Format("m_Style.{0}", base.SizePropertyNamePrefix); }
		}

		/// <summary>
		/// Displays the custom fields.
		/// </summary>
		/// <returns>The number of lines drawn in the inspector.</returns>
		/// <param name="firstLinePosition">Position of the first line.</param>
		/// <param name="property">Property.</param>
		protected override int DisplayCustomFields(Rect firstLinePosition, SerializedProperty property)
		{
			int numLines = base.DisplayCustomFields(firstLinePosition, property);
			float horizontalMargin = EditorGUIX.standardHorizontalSpacing;
			firstLinePosition.y +=
				numLines * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
			firstLinePosition.width =
				0.5f * (firstLinePosition.width - EditorGUIX.standardHorizontalSpacing);
			EditorGUI.PropertyField(firstLinePosition, normalColor[property.propertyPath], s_NormalColorGuiContent );
			firstLinePosition.x += firstLinePosition.width + horizontalMargin;
			EditorGUI.PropertyField(
				firstLinePosition, highlightedColor[property.propertyPath], s_HighlightColorGuiContent
			);
			firstLinePosition.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
			firstLinePosition.x -= firstLinePosition.width + horizontalMargin;
			EditorGUI.PropertyField(firstLinePosition, pressedColor[property.propertyPath], s_PressedColorGuiContent);
			firstLinePosition.x += firstLinePosition.width + horizontalMargin;
			EditorGUI.PropertyField(firstLinePosition, disabledColor[property.propertyPath], s_DisabledColorGuiContent);
			firstLinePosition.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
			firstLinePosition.x -= firstLinePosition.width + horizontalMargin;
			EditorGUI.PropertyField(firstLinePosition, colorMultiplier[property.propertyPath], s_MultiplierGuiContent);
			firstLinePosition.x += firstLinePosition.width + horizontalMargin;
			EditorGUI.PropertyField(firstLinePosition, colorTintMode[property.propertyPath], s_TintModeGuiContent);
			firstLinePosition.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
			firstLinePosition.x -= firstLinePosition.width + horizontalMargin;
			EditorGUI.PropertyField(firstLinePosition, fadeDuration[property.propertyPath], s_FadeDurationGuiContent);
			firstLinePosition.x += firstLinePosition.width + horizontalMargin;
			EditorGUI.PropertyField(firstLinePosition, colorTweenMode[property.propertyPath], s_TweenModeGuiContent);
			return numLines + 4;
		}

		/// <summary>
		/// Displays the identifier field for this style.
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="property">Property.</param>
		protected override void DisplayIdentifierField(Rect position, SerializedProperty property)
		{
			EditorGUI.PropertyField(position, className[property.propertyPath], classNameGUIContent);
		}

		/// <summary>
		/// Initialize this instance.
		/// </summary>
		/// <param name="property">Property.</param>
		protected override void Initialize (SerializedProperty property)
		{
			base.Initialize(property);
			if (!className.ContainsKey(property.propertyPath))
			{
				className.Add(property.propertyPath, property.FindPropertyRelative("m_ClassName"));
				colorMultiplier.Add(
					property.propertyPath, property.FindPropertyRelative("m_Style.m_Colors.m_ColorMultiplier")
				);
				colorTintMode.Add(property.propertyPath, property.FindPropertyRelative("m_Style.m_ColorTintMode"));
				colorTweenMode.Add(property.propertyPath, property.FindPropertyRelative("m_Style.m_ColorTweenMode"));
				disabledColor.Add(
					property.propertyPath, property.FindPropertyRelative("m_Style.m_Colors.m_DisabledColor")
				);
				highlightedColor.Add(
					property.propertyPath, property.FindPropertyRelative("m_Style.m_Colors.m_HighlightedColor")
				);
				normalColor.Add(property.propertyPath, property.FindPropertyRelative("m_Style.m_Colors.m_NormalColor"));
				pressedColor.Add(
					property.propertyPath, property.FindPropertyRelative("m_Style.m_Colors.m_PressedColor")
				);
				fadeDuration.Add(
					property.propertyPath, property.FindPropertyRelative("m_Style.m_Colors.m_FadeDuration")
				);
			}
		}
	}
}
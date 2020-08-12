// 
// FontStyleDrawer.cs
// 
// Copyright (c) 2014, Candlelight Interactive, LLC
// All rights reserved.
// 
// This file is licensed according to the terms of the Unity Asset Store EULA:
// http://download.unity3d.com/assetstore/customer-eula.pdf
// 
// This file contains a custom property drawer for FontStyle.

using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Candlelight
{
	/// <summary>
	/// Font style drawer.
	/// </summary>
	[CustomPropertyDrawer (typeof(FontStyle))]
	public class FontStyleDrawer : PropertyDrawer
	{
		#region Backing Fields
		private static GUIStyle m_BoldButtonStyle = null;
		private static GUIStyle m_ItalicButtonStyle = null;
		#endregion
		/// <summary>
		/// Gets the bold button style.
		/// </summary>
		/// <value>The bold button style.</value>
		private static GUIStyle BoldButtonStyle
		{
			get
			{
				if (m_BoldButtonStyle == null)
				{
					m_BoldButtonStyle = new GUIStyle(EditorStylesX.MiniButtonLeft);
					m_BoldButtonStyle.fontStyle = FontStyle.Bold;
				}
				return m_BoldButtonStyle;
			}
		}
		/// <summary>
		/// Gets the italic button style.
		/// </summary>
		/// <value>The italic button style.</value>
		private static GUIStyle ItalicButtonStyle
		{
			get
			{
				if (m_ItalicButtonStyle == null)
				{
					m_ItalicButtonStyle = new GUIStyle(EditorStylesX.MiniButtonRight);
					m_ItalicButtonStyle.padding =
						new RectOffset((int)(EditorGUIUtility.singleLineHeight * -0.3f), 0, 0, 0);
					m_ItalicButtonStyle.fontStyle = FontStyle.Italic;
				}
				return m_ItalicButtonStyle;
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
			EditorGUI.BeginProperty(position, label, property);
			{
				if (label != null && label != GUIContent.none)
				{
					EditorGUI.PrefixLabel(position, label);
					position.x = position.x + EditorGUIUtility.labelWidth;
				}
				FontStyle fontStyle = (FontStyle)property.intValue;
				position.width = EditorGUIUtility.singleLineHeight * 1.25f;
				position.height -= 1f;
				EditorGUI.BeginChangeCheck();
				bool bold = fontStyle == FontStyle.Bold || fontStyle == FontStyle.BoldAndItalic;
				if (EditorGUIX.DisplayButton(position, "b", BoldButtonStyle, bold))
				{
					bold = !bold;
				}
				position.x += position.width;
				bool italic = fontStyle == FontStyle.Italic || fontStyle == FontStyle.BoldAndItalic;
				if (EditorGUIX.DisplayButton(position, "i", ItalicButtonStyle, italic))
				{
					italic = !italic;
				}
				if (EditorGUI.EndChangeCheck())
				{
					property.intValue = (int)(
						bold && italic ? FontStyle.BoldAndItalic :
							bold ? FontStyle.Bold : italic ? FontStyle.Italic : FontStyle.Normal
					);
				}
			}
			EditorGUI.EndProperty();
		}
	}
}
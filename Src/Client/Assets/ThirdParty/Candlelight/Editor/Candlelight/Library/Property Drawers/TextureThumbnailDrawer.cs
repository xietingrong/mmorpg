// 
// TextureThumbnailDrawer.cs
// 
// Copyright (c) 2014, Candlelight Interactive, LLC
// All rights reserved.
// 
// This file is licensed according to the terms of the Unity Asset Store EULA:
// http://download.unity3d.com/assetstore/customer-eula.pdf
// 
// This file contains a custom property drawer to display a texture thumbnail
// inline with a Texture or Sprite field.

using UnityEditor;
using UnityEngine;

namespace Candlelight
{
	/// <summary>
	/// Texture thumbnail drawer.
	/// </summary>
	[CustomPropertyDrawer(typeof(TextureThumbnailAttribute))]
	public class TextureThumbnailDrawer : PropertyDrawer
	{
		/// <summary>
		/// Raises the GUI event.
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="property">Property.</param>
		/// <param name="label">Label.</param>
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			position.width -= 2f + EditorGUIUtility.singleLineHeight;
			EditorGUI.PropertyField(position, property, label);
			position.x += position.width + 2f;
			position.width = EditorGUIUtility.singleLineHeight;
			if (property.objectReferenceValue != null)
			{
				if (property.objectReferenceValue is Texture)
				{
					EditorGUI.DrawTextureTransparent(position, property.objectReferenceValue as Texture);
				}
				else if (property.objectReferenceValue is Sprite)
				{
					// TODO: account for sprites cut from an atlas somehow
					EditorGUI.DrawTextureTransparent(position, (property.objectReferenceValue as Sprite).texture);
				}
				else
				{
					Color oldColor = GUI.color;
					GUI.color = Color.red;
					EditorGUI.LabelField(position, new GUIContent("?", "Unknown field type."));
					GUI.color = oldColor;
				}
			}
		}
	}
}
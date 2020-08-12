// 
// InflectedFormDrawer.cs
// 
// Copyright (c) 2014, Candlelight Interactive, LLC
// All rights reserved.
// 
// This file is licensed according to the terms of the Unity Asset Store EULA:
// http://download.unity3d.com/assetstore/customer-eula.pdf
// 
// This file contains a custom property drawer for
// KeywordsGlossary.InflectedForm.

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace Candlelight
{
	/// <summary>
	/// Inflected form drawer.
	/// </summary>
	[CustomPropertyDrawer(typeof(KeywordsGlossary.InflectedForm))]
	public class InflectedFormDrawer : PropertyDrawer
	{
		/// <summary>
		/// The width of the part of speech field.
		/// </summary>
		private static readonly float partOfSpeechFieldWidth = 70f;
		/// <summary>
		/// The margin of the part of speech field.
		/// </summary>
		private static readonly float partOfSpeechFieldMargin = 2f;
		#region Serialized Properties
		private Dictionary<string, SerializedProperty> partOfSpeech = new Dictionary<string, SerializedProperty>();
		private Dictionary<string, SerializedProperty> word = new Dictionary<string, SerializedProperty>();
		#endregion

		/// <summary>
		/// Gets the height of the property.
		/// </summary>
		/// <returns>The property height.</returns>
		/// <param name="property">Property.</param>
		/// <param name="label">Label.</param>
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			Initialize(property);
			return base.GetPropertyHeight(property, label);
		}

		/// <summary>
		/// Initialize the specified property.
		/// </summary>
		/// <param name="property">Property.</param>
		private void Initialize(SerializedProperty property)
		{
			if (!partOfSpeech.ContainsKey(property.propertyPath))
			{
				partOfSpeech.Add(property.propertyPath, property.FindPropertyRelative("m_PartOfSpeech"));
				word.Add(property.propertyPath, property.FindPropertyRelative("m_Word"));
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
#if UNITY_4_6
			// bug 601339
			if (property.isArray && property.propertyType != SerializedPropertyType.String)
			{
				return;
			}
#endif
			Initialize(property);
			position.width -= partOfSpeechFieldWidth + partOfSpeechFieldMargin;
			EditorGUI.PropertyField(position, word[property.propertyPath], GUIContent.none);
			int indent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;
			position.x += position.width + partOfSpeechFieldMargin;
			position.width = partOfSpeechFieldWidth;
			EditorGUI.PropertyField(position, partOfSpeech[property.propertyPath], GUIContent.none);
			EditorGUI.indentLevel = indent;
		}
	}
}
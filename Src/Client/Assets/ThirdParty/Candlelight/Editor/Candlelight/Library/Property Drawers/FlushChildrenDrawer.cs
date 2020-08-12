// 
// FlushChildrenDrawer.cs
// 
// Copyright (c) 2014, Candlelight Interactive, LLC
// All rights reserved.
// 
// This file is licensed according to the terms of the Unity Asset Store EULA:
// http://download.unity3d.com/assetstore/customer-eula.pdf
// 
// This file contains a custom property drawer to display an object's children
// flush with the current indent level.

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace Candlelight
{
	/// <summary>
	/// A property drawer that renders a generic property's children as though they were not child properties.
	/// </summary>
	[CustomPropertyDrawer(typeof(FlushChildrenAttribute))]
	public class FlushChildrenDrawer : PropertyDrawer
	{
		#region Shared Allocations
		private static GUIContent s_Label = new GUIContent();
		#endregion
		/// <summary>
		/// Serialized property types whose default drawers are expandable.
		/// </summary>
		private static HashSet<SerializedPropertyType> s_ExpandableTypes = new HashSet<SerializedPropertyType>(
			new SerializedPropertyType[]
			{
				SerializedPropertyType.Generic, SerializedPropertyType.Quaternion, SerializedPropertyType.Vector4
			}
		);

		#region Shared Allocations
		private SerializedProperty m_SPOnGUI = null;
		#endregion

		/// <summary>
		/// Gets a value indicating whether this <see cref="FlushChildrenDrawer"/> should display foldout.
		/// </summary>
		/// <value><see langword="true"/> if should display foldout; otherwise, <see langword="false"/>.</value>
		public bool ShouldDisplayFoldout { get { return false; } }

		/// <summary>
		/// Gets the height of the property.
		/// </summary>
		/// <returns>The property height.</returns>
		/// <param name="property">Property.</param>
		/// <param name="label">Label.</param>
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			property.isExpanded = true;
			return EditorGUI.GetPropertyHeight(property, label, true) - (
				EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing
			);
		}

		/// <summary>
		/// Raises the GUI event.
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="property">Property.</param>
		/// <param name="label">Label.</param>
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			// if the property is not an expandable type, then use its default drawer
			if (!s_ExpandableTypes.Contains(property.propertyType))
			{
				EditorGUI.PropertyField(position, property, label, property.hasVisibleChildren && property.isExpanded);
			}
			else
			{
				m_SPOnGUI = property.Copy();
				m_SPOnGUI.NextVisible(true);
				Regex match = new Regex(string.Format("^{0}(?=\\.)", Regex.Escape(property.propertyPath)));
				while (match.IsMatch(m_SPOnGUI.propertyPath))
				{
					s_Label.text = m_SPOnGUI.displayName;
					position.height = EditorGUI.GetPropertyHeight(m_SPOnGUI, s_Label, true);
					EditorGUI.PropertyField(position, m_SPOnGUI, true);
					position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
					m_SPOnGUI.NextVisible(false);
				}
			}
		}
	}
}
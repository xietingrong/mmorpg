// 
// HyperTextQuadStyleDrawer.cs
// 
// Copyright (c) 2014, Candlelight Interactive, LLC
// All rights reserved.
// 
// This file is licensed according to the terms of the Unity Asset Store EULA:
// http://download.unity3d.com/assetstore/customer-eula.pdf
// 
// This file contains a custom property drawer for HyperTextStyles.Quad.

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace Candlelight.UI
{
	/// <summary>
	/// Hyper text quad style drawer.
	/// </summary>
	[CustomPropertyDrawer(typeof(HyperTextStyles.Quad))]
	public class HyperTextQuadStyleDrawer : HyperTextStyleDrawer
	{
		#region Labels
		private static readonly GUIContent colorizationGUIContent =
			new GUIContent("Colorize", "Enable if text color styling should be applied to instances of this quad.");
		private static readonly GUIContent linkClassGUIContent = new GUIContent(
			"Link Class", "if not empty, all instances of this quad will use custom link styles of the specified class."
		);
		private static readonly GUIContent linkIdGUIContent = new GUIContent(
			"Link ID", "If not empty, all instances of this quad will be wrapped in a link tag with the specified ID."
		);
		#endregion
		/// <summary>
		/// The height of the property.
		/// </summary>
		public static readonly float propertyHeight =
			5f * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);

		#region Serialized Properties
		private Dictionary<string, SerializedProperty> className = new Dictionary<string, SerializedProperty>();
		private Dictionary<string, SerializedProperty> linkClassName = new Dictionary<string, SerializedProperty>();
		private Dictionary<string, SerializedProperty> linkId = new Dictionary<string, SerializedProperty>();
		private Dictionary<string, SerializedProperty> shouldRespectColorization =
			new Dictionary<string, SerializedProperty>();
		private Dictionary<string, SerializedProperty> sprite = new Dictionary<string, SerializedProperty>();
		#endregion

		/// <summary>
		/// Gets the height of the property.
		/// </summary>
		/// <value>The height of the property.</value>
		protected override float PropertyHeight { get { return propertyHeight; } }

		/// <summary>
		/// Displays the custom fields.
		/// </summary>
		/// <returns>The number of lines drawn in the inspector.</returns>
		/// <param name="firstLinePosition">Position of the first line.</param>
		/// <param name="property">Property.</param>
		protected override int DisplayCustomFields(Rect firstLinePosition, SerializedProperty property)
		{
			EditorGUI.PropertyField(firstLinePosition, sprite[property.propertyPath]);
			firstLinePosition.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
			EditorGUI.PropertyField(
				firstLinePosition, shouldRespectColorization[property.propertyPath], colorizationGUIContent
			);
			firstLinePosition.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
			firstLinePosition.width =
				0.5f * (firstLinePosition.width - EditorGUIX.standardHorizontalSpacing);
			EditorGUI.PropertyField(firstLinePosition, linkId[property.propertyPath], linkIdGUIContent);
			firstLinePosition.x += firstLinePosition.width + EditorGUIX.standardHorizontalSpacing;
			EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(linkId[property.propertyPath].stringValue));
			{
				EditorGUI.PropertyField(firstLinePosition, linkClassName[property.propertyPath], linkClassGUIContent);
			}
			EditorGUI.EndDisabledGroup();
			return 3;
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
		/// Gets the height of the property.
		/// </summary>
		/// <returns>The property height.</returns>
		/// <param name="property">Property.</param>
		/// <param name="label">Label.</param>
		public override float GetPropertyHeight (SerializedProperty property, GUIContent label)
		{
			return propertyHeight;
		}

		/// <summary>
		/// Initialize this instance.
		/// </summary>
		/// <param name="property">Property.</param>
		protected override void Initialize(SerializedProperty property)
		{
			base.Initialize(property);
			if (!className.ContainsKey(property.propertyPath))
			{
				className.Add(property.propertyPath, property.FindPropertyRelative("m_ClassName"));
				linkClassName.Add(property.propertyPath, property.FindPropertyRelative("m_LinkClassName"));
				linkId.Add(property.propertyPath, property.FindPropertyRelative("m_LinkId"));
				shouldRespectColorization.Add(
					property.propertyPath, property.FindPropertyRelative("m_ShouldRespectColorization")
				);
				sprite.Add(property.propertyPath, property.FindPropertyRelative("m_Sprite"));
			}
		}
	}
}
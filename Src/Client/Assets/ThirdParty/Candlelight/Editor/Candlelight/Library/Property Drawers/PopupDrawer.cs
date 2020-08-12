// 
// PopupDrawer.cs
// 
// Copyright (c) 2015, Candlelight Interactive, LLC
// All rights reserved.
// 
// This file is licensed according to the terms of the Unity Asset Store EULA:
// http://download.unity3d.com/assetstore/customer-eula.pdf
// 
// This file contains a custom property drawer for displaying a field as a
// popup.

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;

namespace Candlelight
{
	/// <summary>
	/// A property drawer to display some field as a popup.
	/// </summary>
	[CustomPropertyDrawer(typeof(PopupAttribute), true)]
	public class PopupDrawer : PropertyDrawer
	{
		#region Shared Allocations
		private static readonly object[] s_PopupContentsArgs = new object[2];
		#endregion

		/// <summary>
		/// The popup contents getter.
		/// </summary>
		private MethodInfo m_PopupContentsGetter = null;
		/// <summary>
		/// The popup labels.
		/// </summary>
		private List<GUIContent> m_PopupLabels = new List<GUIContent>();
		/// <summary>
		/// The popup values.
		/// </summary>
		private List<object> m_PopupValues = new List<object>();

		/// <summary>
		/// Gets the <see cref="Candlelight.PopupAttribute"/>.
		/// </summary>
		/// <value>The attribute.</value>
		private PopupAttribute Attribute { get { return attribute as PopupAttribute; } }

		/// <summary>
		/// Raises the GUI event.
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="property">Property.</param>
		/// <param name="label">Label.</param>
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			// initialize the getter method
			if (m_PopupContentsGetter == null)
			{
				m_PopupContentsGetter = fieldInfo.ReflectedType.GetMethod(
					Attribute.PopupContentsGetter,
					ReflectionX.instanceBindingFlags,
					null,
					new System.Type[] { typeof(List<GUIContent>), typeof(List<object>) },
					null
				);
			}
			// if method cannot be found, display error icon
			if (m_PopupContentsGetter == null)
			{
				EditorGUIX.DisplayPropertyFieldWithStatus(
					position,
					property,
					ValidationStatus.Error,
					label,
					true,
					string.Format(
						"Unabled to find method: int {0}.{1} (List<GUIContent> labels, List<object> values)",
						fieldInfo.ReflectedType.FullName, Attribute.PopupContentsGetter
					)
				);
			}
			else if (
				property.propertyType == SerializedPropertyType.Generic ||
				property.propertyType == SerializedPropertyType.Gradient
			)
			{
				EditorGUIX.DisplayPropertyFieldWithStatus(
					position,
					property,
					ValidationStatus.Error,
					label,
					true,
					string.Format(
						"SerializedPropertyType.{0} not currently supported for popup drawer.", property.propertyType
					)
				);
			}
			else
			{
				EditorGUI.BeginProperty(position, label, property);
				{
					s_PopupContentsArgs[0] = m_PopupLabels;
					s_PopupContentsArgs[1] = m_PopupValues;
					int index =
						(int)m_PopupContentsGetter.Invoke(property.serializedObject.targetObject, s_PopupContentsArgs);
					index = EditorGUI.Popup(position, label, index, m_PopupLabels.ToArray());
					property.SetValue(m_PopupValues[index]);
				}
				EditorGUI.EndProperty();
			}
		}
	}
}
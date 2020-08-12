// 
// HyperTextProcessorDrawer.cs
// 
// Copyright (c) 2014, Candlelight Interactive, LLC
// All rights reserved.
// 
// This file is licensed according to the terms of the Unity Asset Store EULA:
// http://download.unity3d.com/assetstore/customer-eula.pdf
// 
// This file contains a custom property drawer for HyperTextProcessor.

using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Candlelight.UI
{
	/// <summary>
	/// A custom property drawer for <see cref="Candlelight.UI.HyperTextProcessor"/> objects.
	/// </summary>
	[CustomPropertyDrawer(typeof(HyperTextProcessor))]
	public class HyperTextProcessorDrawer : PropertyDrawer
	{
		#region Shared Allocations
		private static List<HyperTextStyles.LinkSubclass> s_CascadedLinkStyles =
			new List<HyperTextStyles.LinkSubclass>(64);
		private static List<HyperTextStyles.Quad> s_CascadedQuadStyles = new List<HyperTextStyles.Quad>(64);
		private static List<HyperTextStyles.Text> s_CascadedTextStyles = new List<HyperTextStyles.Text>(64);
		#endregion
		#region Labels
		private static readonly GUIContent s_LinkKeywordIdentifierGUIContent =
			new GUIContent("Class", "Optional class name for custom <a> style with which collection is associated.");
		private static readonly GUIContent s_QuadKeywordIdentifierGUIContent =
			new GUIContent("Class", "Class name for the <quad> style with which this collection is associated.");
		private static readonly GUIContent s_TagKeywordIdentifierGUIContent =
			new GUIContent("Tag", "Tag name for the custom text style with which this collection is associated.");
		#endregion
		/// <summary>
		/// An empty idenfier collection.
		/// </summary>
		private static readonly string[] s_EmptyIdentifierCollection = new string[0];

		/// <summary>
		/// Gets all collections assigned to the specified <see cref="Candlelight.UI.HyperTextProcessor"/>.
		/// </summary>
		/// <returns>
		/// All collections assigned to the specified <see cref="Candlelight.UI.HyperTextProcessor"/>.
		/// </returns>
		/// <param name="hyperTextProcessor">
		/// A SerializedProperty representation of a <see cref="Candlelight.UI.HyperTextProcessor"/>.
		/// </param>
		public static IEnumerable<KeywordCollection> GetAllCollections(SerializedProperty hyperTextProcessor)
		{
			SerializedProperty linkCollections = hyperTextProcessor.FindPropertyRelative("m_LinkKeywordCollections");
			SerializedProperty quadCollections = hyperTextProcessor.FindPropertyRelative("m_QuadKeywordCollections");
			SerializedProperty tagCollections = hyperTextProcessor.FindPropertyRelative("m_TagKeywordCollections");
			return Enumerable.Concat<Object>(
				from idx in Enumerable.Range(0, linkCollections.arraySize)
				select linkCollections.GetArrayElementAtIndex(idx).FindPropertyRelative("m_Collection").objectReferenceValue,
				from idx in Enumerable.Range(0, quadCollections.arraySize)
				select quadCollections.GetArrayElementAtIndex(idx).FindPropertyRelative("m_Collection").objectReferenceValue
			).Concat(
				from idx in Enumerable.Range(0, tagCollections.arraySize)
				select tagCollections.GetArrayElementAtIndex(idx).FindPropertyRelative("m_Collection").objectReferenceValue
			).Cast<KeywordCollection>();
		}

		/// <summary>
		/// Raises the draw keyword collection class entry event.
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="element">Element in the list.</param>
		/// <param name="identifierGUIContent">Identifier GUI content.</param>
		/// <param name="existingIdentifierNames">Existing identifier names.</param>
		/// <param name="defaultStatus">Default status.</param>
		/// <param name="infoTooltip">Info tooltip.</param>
		/// <param name="missingStyleDescriptor">Missing style descriptor.</param>
		/// <param name="styles">Styles assigned to the <see cref="Candlelight.UI.HyperTextProcessor"/>.</param>
		/// <param name="assignedCollections">
		/// All <see cref="Candlelight.KeywordCollection"/> objects assigned to the
		/// <see cref="Candlelight.UI.HyperTextProcessor"/>.
		/// </param>
		private static void OnDrawKeywordCollectionClassEntry(
			Rect position,
			SerializedProperty element,
			GUIContent identifierGUIContent,
			IEnumerable<string> existingIdentifierNames,
			ValidationStatus defaultStatus,
			string infoTooltip,
			string missingStyleDescriptor,
			HyperTextStyles styles,
			IEnumerable<KeywordCollection> assignedCollections
		)
		{
			position.height = EditorGUIUtility.singleLineHeight;
			float oldLabelWidth = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = 60f;
			position.width = (position.width - EditorGUIX.standardHorizontalSpacing) * 0.6f;
			SerializedProperty collectionProperty = element.FindPropertyRelative("m_Collection");
			ValidationStatus collectionStatus = ValidationStatus.Warning;
			if (collectionProperty.objectReferenceValue != null)
			{
				collectionStatus =
					assignedCollections.Count(item => item == collectionProperty.objectReferenceValue) > 1 ?
						ValidationStatus.Error : ValidationStatus.Okay;
			}
			EditorGUIX.DisplayPropertyFieldWithStatus(
				position,
				collectionProperty,
				collectionStatus,
				null,
				true,
				collectionStatus == ValidationStatus.Okay ?
					"" : collectionStatus == ValidationStatus.Error ?
						string.Format(
							"Keyword collection {0} used for multiple different styles on this object.",
							collectionProperty.objectReferenceValue.name
						) :
					"Assign a keyword collection to automatically apply this style to keywords."
			);
			EditorGUIUtility.labelWidth = 40f;
			position.x += position.width;
			position.width *= 0.6666666667f;
			string identifierName = element.FindPropertyRelative("m_ClassName").stringValue;
			ValidationStatus status = defaultStatus;
			if (!string.IsNullOrEmpty(identifierName))
			{
				if (styles == null)
				{
					infoTooltip =
						"No styles assigned to this object. Keywords from this collection will use default style";
					status = ValidationStatus.Warning;
				}
				else
				{
					int matches = existingIdentifierNames.Count(existingId => existingId == identifierName);
					if (matches == 1)
					{
						status = ValidationStatus.Okay;
						infoTooltip = string.Empty;
					}
					else
					{
						status = ValidationStatus.Error;
						infoTooltip = string.Format(
							"No custom {0} {1} found in {2}.",
							missingStyleDescriptor, identifierName, styles.name
						);
					}
				}
			}
			EditorGUIX.DisplayPropertyFieldWithStatus(
				position,
				element.FindPropertyRelative("m_ClassName"),
				status,
				identifierGUIContent,
				true,
				infoTooltip
			);
			EditorGUIUtility.labelWidth = oldLabelWidth;
		}

		/// <summary>
		/// Raises the draw link keyword collections entry event.
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="index">Index.</param>
		/// <param name="hyperTextProcessor">
		/// A SerializedProperty representation of a <see cref="Candlelight.UI.HyperTextProcessor"/>.
		/// </param>
		/// <param name="getAllAssignedCollections">
		/// A method to get all <see cref="Candlelight.KeywordCollection"/> objects assigned to the
		/// <see cref="Candlelight.UI.HyperTextProcessor"/>.
		/// </param>
		public static void OnDrawLinkKeywordCollectionsEntry(
			Rect position,
			int index,
			SerializedProperty hyperTextProcessor,
			System.Func<IEnumerable<KeywordCollection>> getAllAssignedCollections
		)
		{
			HyperTextStyles styles =
				hyperTextProcessor.FindPropertyRelative("m_Styles").objectReferenceValue as HyperTextStyles;
			if (styles != null)
			{
				styles.GetCascadedLinkStyles(ref s_CascadedLinkStyles);
			}
			OnDrawKeywordCollectionClassEntry(
				position,
				hyperTextProcessor.FindPropertyRelative("m_LinkKeywordCollections").GetArrayElementAtIndex(index),
				s_LinkKeywordIdentifierGUIContent,
				styles == null ?
				s_EmptyIdentifierCollection : from style in s_CascadedLinkStyles select style.ClassName,
				ValidationStatus.Info,
				"Optionally specify a class name for the custom <a> style with which this collection is associated.",
				"link style with class name",
				styles,
				getAllAssignedCollections()
			);
		}

		/// <summary>
		/// Raises the draw quad keyword collections entry event.
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="index">Index.</param>
		/// <param name="hyperTextProcessor">
		/// A SerializedProperty representation of a <see cref="Candlelight.UI.HyperTextProcessor"/>.
		/// </param>
		/// <param name="getAllAssignedCollections">
		/// A method to get all <see cref="Candlelight.KeywordCollection"/> objects assigned to the
		/// <see cref="Candlelight.UI.HyperTextProcessor"/>.
		/// </param>
		public static void OnDrawQuadKeywordCollectionsEntry(
			Rect position,
			int index,
			SerializedProperty hyperTextProcessor,
			System.Func<IEnumerable<KeywordCollection>> getAllAssignedCollections
		)
		{
			HyperTextStyles styles =
				hyperTextProcessor.FindPropertyRelative("m_Styles").objectReferenceValue as HyperTextStyles;
			if (styles != null)
			{
				styles.GetCascadedQuadStyles(ref s_CascadedQuadStyles);
			}
			OnDrawKeywordCollectionClassEntry(
				position,
				hyperTextProcessor.FindPropertyRelative("m_QuadKeywordCollections").GetArrayElementAtIndex(index),
				s_QuadKeywordIdentifierGUIContent,
				styles == null ?
					s_EmptyIdentifierCollection : from style in s_CascadedQuadStyles select style.ClassName,
				ValidationStatus.Error,
				"Specify a class name for the custom <quad> style with which this collection is associated.",
				"quad style with class name",
				styles,
				getAllAssignedCollections()
			);
		}

		/// <summary>
		/// Raises the draw tag keyword collections entry event.
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="index">Index.</param>
		/// <param name="hyperTextProcessor">
		/// A SerializedProperty representation of a <see cref="Candlelight.UI.HyperTextProcessor"/>.
		/// </param>
		/// <param name="getAllAssignedCollections">
		/// A method to get all <see cref="Candlelight.KeywordCollection"/> objects assigned to the
		/// <see cref="Candlelight.UI.HyperTextProcessor"/>.
		/// </param>
		public static void OnDrawTagKeywordCollectionsEntry(
			Rect position,
			int index,
			SerializedProperty hyperTextProcessor,
			System.Func<IEnumerable<KeywordCollection>> getAllAssignedCollections
		)
		{
			HyperTextStyles styles =
				hyperTextProcessor.FindPropertyRelative("m_Styles").objectReferenceValue as HyperTextStyles;
			if (styles != null)
			{
				styles.GetCascadedCustomTextStyles(ref s_CascadedTextStyles);
			}
			OnDrawKeywordCollectionClassEntry(
				position,
				hyperTextProcessor.FindPropertyRelative("m_TagKeywordCollections").GetArrayElementAtIndex(index),
				s_TagKeywordIdentifierGUIContent,
				styles == null ? s_EmptyIdentifierCollection : from style in s_CascadedTextStyles select style.Tag,
				ValidationStatus.Error,
				"Specify a tag name for the custom text style with which this collection is associated.",
				"text style with tag name",
				styles,
				getAllAssignedCollections()
			);
		}

		/// <summary>
		/// All collections for each inspected property.
		/// </summary>
		private Dictionary<string, IEnumerable<KeywordCollection>> m_AllCollections =
			new Dictionary<string, IEnumerable<KeywordCollection>>();
		/// <summary>
		/// Link collections for each inspected property.
		/// </summary>
		private Dictionary<string, ReorderableList> m_LinkCollections = new Dictionary<string, ReorderableList>();
		/// <summary>
		/// Quad collections for each inspected property.
		/// </summary>
		private Dictionary<string, ReorderableList> m_QuadCollections = new Dictionary<string, ReorderableList>();
		/// <summary>
		/// Tag collections for each inspected property.
		/// </summary>
		private Dictionary<string, ReorderableList> m_TagCollections = new Dictionary<string, ReorderableList>();

		/// <summary>
		/// Initialize this instance.
		/// </summary>
		/// <param name="property">Property.</param>
		private void Initialize(SerializedProperty property)
		{
			if (!m_AllCollections.ContainsKey(property.propertyPath))
			{
				m_AllCollections.Add(property.propertyPath, GetAllCollections(property));
			}
			else
			{
				m_AllCollections[property.propertyPath] = GetAllCollections(property);
			}
			if (!m_LinkCollections.ContainsKey(property.propertyPath))
			{
				ReorderableList list = new ReorderableList(
					property.serializedObject, property.FindPropertyRelative("m_LinkKeywordCollections")
				);
				list.drawElementCallback = (position, index, isActive, isFocused) => OnDrawLinkKeywordCollectionsEntry(
					position, index, property, () => m_AllCollections[property.propertyPath]
				);
				string displayName = list.serializedProperty.displayName;
				list.drawHeaderCallback = (position) => EditorGUI.LabelField(position, displayName);
				m_LinkCollections.Add(property.propertyPath, list);
			}
			if (!m_QuadCollections.ContainsKey(property.propertyPath))
			{
				ReorderableList list = new ReorderableList(
					property.serializedObject, property.FindPropertyRelative("m_QuadKeywordCollections")
				);
				string displayName = list.serializedProperty.displayName;
				list.drawHeaderCallback = (position) => EditorGUI.LabelField(position, displayName);
				m_QuadCollections.Add(property.propertyPath, list);
				list.drawElementCallback = (position, index, isActive, isFocused) => OnDrawQuadKeywordCollectionsEntry(
					position, index, property, () => m_AllCollections[property.propertyPath]
				);
			}
			if (!m_TagCollections.ContainsKey(property.propertyPath))
			{
				ReorderableList list = new ReorderableList(
					property.serializedObject, property.FindPropertyRelative("m_TagKeywordCollections")
				);
				string displayName = list.serializedProperty.displayName;
				list.drawHeaderCallback = (position) => EditorGUI.LabelField(position, displayName);
				m_TagCollections.Add(property.propertyPath, list);
				list.drawElementCallback = (position, index, isActive, isFocused) => OnDrawTagKeywordCollectionsEntry(
					position, index, property, () => m_AllCollections[property.propertyPath]
				);
			}
		}

		/// <summary>
		/// Gets the height of the property.
		/// </summary>
		/// <returns>The property height.</returns>
		/// <param name="property">Property.</param>
		/// <param name="label">Label.</param>
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			Initialize(property);
			return m_LinkCollections[property.propertyPath].GetHeight() + EditorGUIUtility.standardVerticalSpacing +
				m_QuadCollections[property.propertyPath].GetHeight() + EditorGUIUtility.standardVerticalSpacing +
				m_TagCollections[property.propertyPath].GetHeight() + EditorGUIUtility.standardVerticalSpacing +
				(EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 6f;
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
			SerializedProperty inputText = property.FindPropertyRelative("m_InputText");
			SerializedProperty styles = property.FindPropertyRelative("m_Styles");
			SerializedProperty supportRichText = property.FindPropertyRelative("m_IsRichTextDesired");
			Rect entirePosition = position;
			position.height = (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 2f;
			EditorGUI.PropertyField(position, inputText);
			position.x = entirePosition.x;
			position.width = entirePosition.width;
			position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
			position.height = EditorGUIUtility.singleLineHeight;
			EditorGUIX.DisplayScriptableObjectPropertyFieldWithButton<HyperTextStyles>(position, styles);
			position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
			HyperTextEditor.DisplayOverridableProperty(
				position,
				property.FindPropertyRelative("m_ReferenceFontSize"),
				property.FindPropertyRelative("m_ShouldOverrideStylesFontSize"),
				styles
			);
			position.x = entirePosition.x;
			position.width = entirePosition.width;
			position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
			EditorGUIX.DisplayPropertyFieldWithStatus(
				position,
				supportRichText,
				ValidationStatus.Info,
				new GUIContent("Output Rich Text"),
				true,
				"Disable if the destination does not support rendering rich text."
			);
			position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
			++EditorGUI.indentLevel;
			EditorGUIX.DisplayPropertyFieldWithStatus(
				position,
				property.FindPropertyRelative("m_IsDynamicFontDesired"),
				supportRichText.boolValue ? ValidationStatus.Info : ValidationStatus.Warning,
				new GUIContent("<size> Tags"),
				true,
				string.Format(
					"Disable if the destination uses a non-dynamic font.{0}",
					supportRichText.boolValue ? "" : "\n\nYou must enable rich text to output <size> tags."
				)
			);
			--EditorGUI.indentLevel;
			position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
			position.height = m_LinkCollections[property.propertyPath].GetHeight();
			m_LinkCollections[property.propertyPath].DoList(position);
			position.y += position.height;
			position.height = m_TagCollections[property.propertyPath].GetHeight();
			m_TagCollections[property.propertyPath].DoList(position);
			position.y += position.height;
			position.height = m_QuadCollections[property.propertyPath].GetHeight();
			m_QuadCollections[property.propertyPath].DoList(position);
		}
	}
}
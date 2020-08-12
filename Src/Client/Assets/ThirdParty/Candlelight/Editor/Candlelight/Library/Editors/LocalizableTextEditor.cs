// 
// LocalizableTextEditor.cs
// 
// Copyright (c) 2015, Candlelight Interactive, LLC
// All rights reserved.
// 
// This file is licensed according to the terms of the Unity Asset Store EULA:
// http://download.unity3d.com/assetstore/customer-eula.pdf
// 
// This file contains a custom editor for localizable text objects.

using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Candlelight.UI
{
	/// <summary>
	/// Localizable text input source editor.
	/// </summary>
	[CustomEditor(typeof(LocalizableText), true), CanEditMultipleObjects, InitializeOnLoad]
	public class LocalizableTextEditor : Editor
	{
		/// <summary>
		/// Initializes the <see cref="Candlelight.LocalizableTextEditor"/> class.
		/// </summary>
		static LocalizableTextEditor()
		{
			EditorGizmos.RegisterGizmo(
				EditorGUIUtility.ObjectContent(null, typeof(UnityEngine.TextAsset)).image as Texture2D,
				typeof(LocalizableText).Name
			);
		}

		/// <summary>
		/// Custom <see cref="UnityEditor.PropertyDrawer"/> for <see cref="LocalizableText.LocaleOverrideAttribute"/>.
		/// It is used when drawing the editor for entries below the list.
		/// </summary>
		[CustomPropertyDrawer(typeof(LocalizableText.LocaleOverrideEntryAttribute))]
		private class LocaleOverrideEntryDrawer : PropertyDrawer
		{
			/// <summary>
			/// The EditorGUIUtility.contextWidth property.
			/// </summary>
			private static readonly PropertyInfo s_ContextWidth =
				typeof(EditorGUIUtility).GetProperty("contextWidth", ReflectionX.staticBindingFlags);
			/// <summary>
			/// The number of pixels in one line of text.
			/// </summary>
			private static readonly int s_LineHeight = 13;
			/// <summary>
			/// The maximum number of lines of text to display in the text area.
			/// </summary>
			private static readonly int s_MaxLines = 10;
			/// <summary>
			/// The minimum number of lines of text to display in the text area.
			/// </summary>
			private static readonly int s_MinLines = 3;
			/// <summary>
			/// The EditorGUI.ScrollableTextAreaInternal() method.
			/// </summary>
			private static readonly MethodInfo s_ScrollableTextAreaInternal =
				typeof(EditorGUI).GetMethod("ScrollableTextAreaInternal", ReflectionX.staticBindingFlags);
			/// <summary>
			/// An argument list to use when invoking EditorGUI.ScrollableTextAreaInternal().
			/// </summary>
			private static readonly object[] s_ScrollableTextAreaInternalArgs = new object[4];
			/// <summary>
			/// Temporary GUIContent for determining the height of the text area.
			/// </summary>
			private static readonly GUIContent s_TempGUIContent = new GUIContent();

			/// <summary>
			/// The current scroll position.
			/// </summary>
			private Vector2 m_ScrollPosition = default(Vector2);

			/// <summary>
			/// Gets the height of the property.
			/// </summary>
			/// <returns>The property height.</returns>
			/// <param name="property">Property.</param>
			/// <param name="label">Label.</param>
			public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
			{
				s_TempGUIContent.text = property.FindPropertyRelative("m_Data").stringValue;
				float textAreaHeight =
					EditorStyles.textArea.CalcHeight(s_TempGUIContent, (float)s_ContextWidth.GetValue(null, null));
				int numLines = Mathf.CeilToInt(textAreaHeight / s_LineHeight);
				numLines = Mathf.Clamp(numLines, s_MinLines, s_MaxLines);
				return 32 + (float)((numLines - 1) * s_LineHeight) +
					EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
			}

			/// <summary>
			/// Raises the GUI event.
			/// </summary>
			/// <param name="position">Position.</param>
			/// <param name="property">Property.</param>
			/// <param name="label">Label.</param>
			public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
			{
				Rect localePosition = position;
				localePosition.height = EditorGUIUtility.singleLineHeight;
				label.text = "Locale";
				EditorGUI.PropertyField(localePosition, property.FindPropertyRelative("m_Identifier"), label);
				position.height -= localePosition.height + EditorGUIUtility.standardVerticalSpacing;
				position.y += localePosition.height + EditorGUIUtility.standardVerticalSpacing;
				SerializedProperty text = property.FindPropertyRelative("m_Data");
				label.text = "Text";
				label = EditorGUI.BeginProperty(position, label, text);
				{
					Rect labelPosition = position;
					labelPosition.height = 16;
					position.yMin += labelPosition.height;
					EditorGUI.HandlePrefixLabel(position, labelPosition, label);
					string stringValue = null;
					EditorGUI.BeginChangeCheck();
					{
						s_ScrollableTextAreaInternalArgs[0] = position;
						s_ScrollableTextAreaInternalArgs[1] = text.stringValue;
						s_ScrollableTextAreaInternalArgs[2] = m_ScrollPosition;
						s_ScrollableTextAreaInternalArgs[3] = EditorStyles.textArea;
						stringValue = (string)s_ScrollableTextAreaInternal.Invoke(null, s_ScrollableTextAreaInternalArgs);
						m_ScrollPosition = (Vector2)s_ScrollableTextAreaInternalArgs[2];
					}
					if (EditorGUI.EndChangeCheck())
					{
						text.stringValue = stringValue;
					}
				}
				EditorGUI.EndProperty();
			}
		}

		/// <summary>
		/// The backing field storing the localized string values.
		/// </summary>
		private static readonly FieldInfo s_LocaleOverridesField =
			typeof(LocalizableText).GetField("m_LocaleOverrides", ReflectionX.instanceBindingFlags);
		/// <summary>
		/// Tooltips for the localized text element status icons.
		/// </summary>
		private static readonly Dictionary<ValidationStatus, string> s_LocalizedTextElementTooltips =
			new Dictionary<ValidationStatus, string>()
		{
			{ ValidationStatus.Error, null },
			{ ValidationStatus.Info, null },
			{ ValidationStatus.None, null },
			{ ValidationStatus.Okay, "The text override for this locale is currently being used." },
			{ ValidationStatus.Warning, "No text override specified for this locale." }
		};

		/// <summary>
		/// Creates a new asset in the project.
		/// </summary>
		[UnityEditor.MenuItem("Assets/Create/Candlelight/Localizable Text")]
		public static void CreateNewAssetInProject()
		{
			AssetDatabaseX.CreateNewAssetInCurrentProjectFolder<LocalizableText>();
		}

		/// <summary>
		/// The localized text entries.
		/// </summary>
		private Dictionary<string, string> m_LocaleOverrideEntries = new Dictionary<string, string>();
		/// <summary>
		/// The current status of each entry.
		/// </summary>
		private readonly List<ValidationStatus> m_LocaleOverrideEntryStatuses = new List<ValidationStatus>();

		#region Serialized Properties
		private SerializedProperty m_CurrentLocale;
		private SerializedProperty m_DefaultText;
		private ReorderableList m_LocaleOverrides;
		private SerializedProperty m_Script;
		#endregion

		/// <summary>
		/// Raises the disable event.
		/// </summary>
		private void OnDisable()
		{
			Undo.undoRedoPerformed -= UpdateGUIContents;
		}

		/// <summary>
		/// Raises the display locale override element event.
		/// </summary>
		/// <param name="rect">Rect.</param>
		/// <param name="index">Index.</param>
		/// <param name="isActive">If set to <see langword="true"/> is active.</param>
		/// <param name="isFocused">If set to <see langword="true"/> is focused.</param>
		private void OnDisplayLocaleOverrideElement(Rect rect, int index, bool isActive, bool isFocused)
		{
			if (index >= m_LocaleOverrideEntryStatuses.Count)
			{
				return;
			}
			EditorGUIX.DisplayLabelFieldWithStatus(
				rect,
				m_LocaleOverrides.serializedProperty.GetArrayElementAtIndex(index).FindPropertyRelative("m_Identifier").stringValue,
				null,
				m_LocaleOverrideEntryStatuses[index],
				Color.white,
				s_LocalizedTextElementTooltips[m_LocaleOverrideEntryStatuses[index]]
			);
		}

		/// <summary>
		/// Raises the enable event.
		/// </summary>
		protected virtual void OnEnable()
		{
			m_Script = this.serializedObject.FindProperty("m_Script");
			m_CurrentLocale = this.serializedObject.FindProperty("m_CurrentLocale");
			m_DefaultText = this.serializedObject.FindProperty("m_DefaultText");
			m_LocaleOverrides =
				new ReorderableList(this.serializedObject, this.serializedObject.FindProperty("m_LocaleOverrides"));
			string label = m_LocaleOverrides.serializedProperty.displayName;
			m_LocaleOverrides.drawHeaderCallback = (rect) => EditorGUI.LabelField(rect, label);
			m_LocaleOverrides.drawElementCallback = OnDisplayLocaleOverrideElement;
			UpdateGUIContents();
			Undo.undoRedoPerformed += UpdateGUIContents;
		}

		/// <summary>
		/// Raises the inspector GUI event.
		/// </summary>
		public override void OnInspectorGUI()
		{
			bool needsUpdate = false;
			this.serializedObject.Update();
			EditorGUILayout.PropertyField(m_Script);
			EditorGUI.BeginChangeCheck();
			{
				EditorGUILayout.PropertyField(m_CurrentLocale);
				EditorGUILayout.PropertyField(m_DefaultText);
			}
			if (EditorGUI.EndChangeCheck())
			{
				needsUpdate = true;
			}
			if (EditorGUIX.DisplayButton("Add 10 Most Common Languages"))
			{
				Undo.RecordObjects(this.targets, "Add 10 Most Common Languages");
				foreach (LocalizableText text in this.targets)
				{
					text.GetLocaleOverrides(ref m_LocaleOverrideEntries);
					foreach (string locale in LocalizableText.TenMostCommonLanguages)
					{
						if (!m_LocaleOverrideEntries.ContainsKey(locale))
						{
							m_LocaleOverrideEntries.Add(locale, "");
						}
					}
					text.SetLocaleOverrides(m_LocaleOverrideEntries);
				}
				EditorUtilityX.SetDirty(this.targets);
				UpdateGUIContents();
			}
			if (EditorGUIX.DisplayButton("Alphabetize"))
			{
				Undo.RecordObjects(this.targets, "Alphabetize");
				foreach (LocalizableText text in this.targets)
				{
					System.Collections.IList backingFieldValue =
						(System.Collections.IList)s_LocaleOverridesField.GetValue(text);
					List<IIdentifiable<string>> sortingList = backingFieldValue.Cast<IIdentifiable<string>>().ToList();
					sortingList.Sort((x, y) => x.Identifier.ToLower().CompareTo(y.Identifier.ToLower()));
					for (int i = 0; i < sortingList.Count; ++i)
					{
						backingFieldValue[i] = sortingList[i];
					}
					s_LocaleOverridesField.SetValue(text, backingFieldValue);
				}
				EditorUtilityX.SetDirty(this.targets);
			}
			EditorGUI.BeginChangeCheck();
			{
				EditorGUIX.DisplayListWithElementEditor(m_LocaleOverrides);
			}
			if (EditorGUI.EndChangeCheck())
			{
				needsUpdate = true;
			}
			this.serializedObject.ApplyModifiedProperties();
			if (needsUpdate)
			{
				UpdateGUIContents();
			}
		}

		/// <summary>
		/// Updates the GUI contents.
		/// </summary>
		private void UpdateGUIContents()
		{
			List<IdentifiableBackingFieldCompatibleObjectWrapper<string>> entries = (
				(System.Collections.IList)s_LocaleOverridesField.GetValue(this.target)
			).Cast<IdentifiableBackingFieldCompatibleObjectWrapper<string>>().ToList();
			m_LocaleOverrideEntryStatuses.Clear();
			for (int i = 0; i < entries.Count; ++i)
			{
				ValidationStatus status = ValidationStatus.None;
				if (string.IsNullOrEmpty(entries[i].Data))
				{
					status = ValidationStatus.Warning;
				}
				if (
					entries[i].Identifier == m_CurrentLocale.stringValue &&
					entries.FindIndex(l => l.Identifier == m_CurrentLocale.stringValue) == i
				)
				{
					status = ValidationStatus.Okay;
				}
				m_LocaleOverrideEntryStatuses.Add(status);
			}
		}
	}
}
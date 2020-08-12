// 
// KeywordsGlossaryEditor.cs
// 
// Copyright (c) 2014-2015, Candlelight Interactive, LLC
// All rights reserved.
// 
// This file is licensed according to the terms of the Unity Asset Store EULA:
// http://download.unity3d.com/assetstore/customer-eula.pdf
// 
// This file contains a custom editor for KeywordsGlossary.

using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Candlelight
{
	/// <summary>
	/// Keywords glossary custom editor.
	/// </summary>
	[CustomEditor(typeof(KeywordsGlossary))]
	public class KeywordsGlossaryEditor : KeywordCollectionEditor<KeywordsGlossary>
	{
		/// <summary>
		/// A struct for storing a validation status and accompanying tooltip.
		/// </summary>
		private struct ValidationStatusTooltip
		{
			/// <summary>
			/// Gets the status.
			/// </summary>
			/// <value>The status.</value>
			public ValidationStatus Status { get; private set; }
			/// <summary>
			/// Gets the tooltip.
			/// </summary>
			/// <value>The tooltip.</value>
			public string Tooltip { get; private set; }

			/// <summary>
			/// Initializes a new instance of the <see cref="KeywordsGlossaryEditor.ValidationTooltip"/> struct.
			/// </summary>
			/// <param name="status">Status.</param>
			/// <param name="tooltip">Tooltip.</param>
			public ValidationStatusTooltip(ValidationStatus status, string tooltip)
			{
				this.Status = status;
				this.Tooltip = tooltip;
			}
		}

		/// <summary>
		/// The rebuild keywords method.
		/// </summary>
		private static readonly System.Reflection.MethodInfo s_RebuildKeywordsMethod =
			typeof(KeywordsGlossary).GetMethod("RebuildKeywords", ReflectionX.instanceBindingFlags);

		/// <summary>
		/// The current word.
		/// </summary>
		private string m_CurrentWord = string.Empty;
		/// <summary>
		/// The status for each word form in the glossary.
		/// </summary>
		private Dictionary<string, ValidationStatusTooltip> m_WordFormStatuses =
			new Dictionary<string, ValidationStatusTooltip>();

		#region Serialized Properties
		private SerializedProperty m_CurrentEntry = null;
		private SerializedProperty m_CurrentForm = null;
		private ReorderableList m_Entries = null;
		private Dictionary<string, ReorderableList> m_InflectedForms = new Dictionary<string, ReorderableList>();
		#endregion

		/// <summary>
		/// Creates a new asset in the project.
		/// </summary>
		[MenuItem("Assets/Create/Candlelight/Keyword Collections/Glossary")]
		public static void CreateNewAsset()
		{
			CreateNewAssetInProject();
		}

		/// <summary>
		/// Gets the status of the specified <see cref="KeywordsGlossary.InflectedForm"/>.
		/// </summary>
		/// <returns>The status of the specified <see cref="KeywordsGlossary.InflectedForm"/>.</returns>
		/// <param name="inflectedForm">Inflected form.</param>
		/// <param name="allForms">All forms defined on the <see cref="KeywordsGlossary"/> instance.</param>
		private ValidationStatusTooltip GetInflectedFormStatus(
			KeywordsGlossary.InflectedForm inflectedForm, List<KeywordsGlossary.InflectedForm> allForms
		)
		{
			if (string.IsNullOrEmpty(inflectedForm.Word))
			{
				return new ValidationStatusTooltip(ValidationStatus.Error, "This word form is empty.");
			}
			else if (
				allForms.Count(
					form => form.Word == inflectedForm.Word && form.PartOfSpeech == inflectedForm.PartOfSpeech) > 1
			)
			{
				return new ValidationStatusTooltip(
					ValidationStatus.Error,
					"This word form appears with the same part of speech elsewhere on this instance."
				);
			}
			else if (allForms.Count(form => form.Word == inflectedForm.Word) > 1)
			{
				return new ValidationStatusTooltip(
					ValidationStatus.Warning,
					"This word form appears with a different part of speech elsewhere on this instance."
				);
			}
			return new ValidationStatusTooltip(ValidationStatus.None, null);
		}
		
		/// <summary>
		/// Raises the add new inflected form event.
		/// </summary>
		/// <param name="inflectedFormList">Inflected form list.</param>
		private void OnAddNewInflectedForm(ReorderableList inflectedFormList)
		{
			++inflectedFormList.serializedProperty.arraySize;
			inflectedFormList.serializedProperty.GetArrayElementAtIndex(
				inflectedFormList.serializedProperty.arraySize - 1
			).FindPropertyRelative("m_Word").stringValue = m_CurrentWord;
		}

		/// <summary>
		/// Raises the draw inflected form event.
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="index">Property array index.</param>
		/// <param name="isActive">If set to <see langword="true"/> is active.</param>
		/// <param name="isFocused">If set to <see langword="true"/> is focused.</param>
		private void OnDrawInflectedForm(Rect position, int index, bool isActive, bool isFocused)
		{
			m_CurrentForm =
				m_InflectedForms[m_CurrentEntry.propertyPath].serializedProperty.GetArrayElementAtIndex(index);
			m_CurrentWord = m_CurrentForm.FindPropertyRelative("m_Word").stringValue;
			OnDrawWordForm(position, m_CurrentForm);
		}
		
		/// <summary>
		/// Raises the draw main form event.
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="index">Property array index.</param>
		/// <param name="isActive">If set to <see langword="true"/> is active.</param>
		/// <param name="isFocused">If set to <see langword="true"/> is focused.</param>
		private void OnDrawMainForm(Rect position, int index, bool isActive, bool isFocused)
		{
			m_CurrentForm =
				m_Entries.serializedProperty.GetArrayElementAtIndex(index).FindPropertyRelative("m_MainForm");
			m_CurrentWord = m_CurrentForm.FindPropertyRelative("m_Word").stringValue;
			OnDrawWordForm(
				position,
				m_CurrentForm,
				m_Entries.serializedProperty.GetArrayElementAtIndex(index).FindPropertyRelative("m_OtherForms").arraySize
			);
		}
		
		/// <summary>
		/// Raises the draw word form event.
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="wordFormProperty">Word form property.</param>
		/// <param name="synonymCount">Synonym count, if e.g., the word is a main form.</param>
		private void OnDrawWordForm(
			Rect position, SerializedProperty wordFormProperty, int synonymCount = -1
		)
		{
			float countLabelWidth = 30f;
			position.width -= EditorGUIUtility.singleLineHeight + (synonymCount < 0 ? 0 : countLabelWidth);
			position.height = EditorGUIUtility.singleLineHeight;
			EditorGUI.PropertyField(position, wordFormProperty);
			position.x += position.width;
			position.width = EditorGUIUtility.singleLineHeight;
			EditorGUIX.DisplayValidationStatusIcon(
				position,
				m_WordFormStatuses[wordFormProperty.propertyPath].Status,
				m_WordFormStatuses[wordFormProperty.propertyPath].Tooltip
			);
			if (synonymCount >= 0)
			{
				position.x += position.width;
				position.width = countLabelWidth;
				EditorGUI.LabelField(position, string.Format("({0})", synonymCount), EditorStylesX.LabelRight);
			}
		}
		
		/// <summary>
		/// Initialize properties.
		/// </summary>
		protected override void OnEnable()
		{
			base.OnEnable();
			m_Entries = new ReorderableList(serializedObject, serializedObject.FindProperty("m_Entries"));
			string label = m_Entries.serializedProperty.displayName;
			m_Entries.drawHeaderCallback += (rect) => EditorGUI.LabelField(rect, label);
			m_Entries.drawElementCallback += OnDrawMainForm;
			UpdateGUIContents();
		}

		/// <summary>
		/// Raises the inspector GUI event.
		/// </summary>
		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			DrawDefaultInspector();
			serializedObject.ApplyModifiedProperties();
			EditorGUI.BeginChangeCheck();
			{
				EditorGUI.BeginDisabledGroup(Selection.objects.Length > 1);
				{
					if (GUILayout.Button("Sort Alphabetically"))
					{
						List<KeywordsGlossary.Entry> entries = typeof(KeywordsGlossary).GetField(
							"m_Entries", ReflectionX.instanceBindingFlags
						).GetValue(target) as List<KeywordsGlossary.Entry>;
						foreach (KeywordsGlossary.Entry entry in entries)
						{
							(
								typeof(KeywordsGlossary.Entry).GetField(
									"m_OtherForms", ReflectionX.instanceBindingFlags
								).GetValue(entry) as List<KeywordsGlossary.InflectedForm>
							).Sort((f1, f2) => f1.Word.CompareTo(f2.Word));
						}
						entries.Sort((e1, e2) => e1.MainForm.Word.CompareTo(e2.MainForm.Word));
						m_Entries.list = entries;
						EditorUtility.SetDirty(target);
					}
				}
				EditorGUI.EndDisabledGroup();
				serializedObject.Update();
				m_Entries.DoLayoutList();
				if (m_Entries.count > 0)
				{
					EditorGUILayout.BeginVertical(EditorStylesX.Box);
					{
						m_CurrentEntry = m_Entries.serializedProperty.GetArrayElementAtIndex(m_Entries.index);
						m_CurrentWord = m_CurrentEntry.FindPropertyRelative(
							"m_MainForm"
						).FindPropertyRelative("m_Word").stringValue;
						EditorGUILayout.LabelField(m_CurrentWord, EditorStylesX.BoldTitleBar);
						EditorGUILayout.PropertyField(m_CurrentEntry.FindPropertyRelative("m_Definition"));
						if (!m_InflectedForms.ContainsKey(m_CurrentEntry.propertyPath))
						{
							ReorderableList newList = new ReorderableList(
								serializedObject, m_CurrentEntry.FindPropertyRelative("m_OtherForms")
							);
							newList.drawHeaderCallback = (rect) => EditorGUI.LabelField(rect, "Inflected Forms");
							newList.drawElementCallback = OnDrawInflectedForm;
							newList.onAddCallback = OnAddNewInflectedForm;
							m_InflectedForms.Add(m_CurrentEntry.propertyPath, newList);
						}
						m_InflectedForms[m_CurrentEntry.propertyPath].DoLayoutList();
					}
					EditorGUILayout.EndVertical();
				}
				serializedObject.ApplyModifiedProperties();
			}
			if (EditorGUI.EndChangeCheck())
			{
				UpdateGUIContents();
				s_RebuildKeywordsMethod.Invoke(target, null);
			}
			DisplayKeywordList();
		}

		/// <summary>
		/// Updates the GUI contents when something changes.
		/// </summary>
		private void UpdateGUIContents()
		{
			m_WordFormStatuses.Clear();
			List<KeywordsGlossary.InflectedForm> allInflectedForms = new List<KeywordsGlossary.InflectedForm>();
			SerializedProperty sp = null;
			// get list of all inflected forms
			List<KeywordsGlossary.InflectedForm> altForms = new List<KeywordsGlossary.InflectedForm>();
			for (int i = 0; i < m_Entries.serializedProperty.arraySize; ++i)
			{
				sp = m_Entries.serializedProperty.GetArrayElementAtIndex(i);
				KeywordsGlossary.Entry entry = sp.GetValue<KeywordsGlossary.Entry>();
				entry.GetOtherForms(ref altForms);
				altForms.Insert(0, entry.MainForm);
				allInflectedForms.AddRange(altForms);
			}
			// get status for each property path
			for (int i = 0; i < m_Entries.serializedProperty.arraySize; ++i)
			{
				// get status for entry
				sp = m_Entries.serializedProperty.GetArrayElementAtIndex(i);
				string mainFormPath = sp.FindPropertyRelative("m_MainForm").propertyPath;
				KeywordsGlossary.Entry entry = sp.GetValue<KeywordsGlossary.Entry>();
				ValidationStatusTooltip entryStatus = GetInflectedFormStatus(entry.MainForm, allInflectedForms);
				if (entryStatus.Status == ValidationStatus.None && string.IsNullOrEmpty(entry.Definition))
				{
					entryStatus =
						new ValidationStatusTooltip(ValidationStatus.Warning, "No definition for this entry.");
				}
				// set status of each alternate form
				sp = m_Entries.serializedProperty.GetArrayElementAtIndex(i).FindPropertyRelative("m_OtherForms");
				entry.GetOtherForms(ref altForms);
				SerializedProperty currentFormProperty;
				ValidationStatus worstStatus = ValidationStatus.None;
				for (int j = 0; j < altForms.Count; ++j)
				{
					currentFormProperty = sp.GetArrayElementAtIndex(j);
					ValidationStatusTooltip status = GetInflectedFormStatus(
						currentFormProperty.GetValue<KeywordsGlossary.InflectedForm>(), allInflectedForms
					);
					worstStatus = (ValidationStatus)Mathf.Max((int)worstStatus, (int)status.Status);
					m_WordFormStatuses.Add(currentFormProperty.propertyPath, status);
				}
				// set status for entry
				if ((int)worstStatus >= (int)entryStatus.Status)
				{
					entryStatus = new ValidationStatusTooltip(
						worstStatus, "There is a problem with one of the inflected forms for this entry."
					);
				}
				m_WordFormStatuses.Add(mainFormPath, entryStatus);
			}
		}
	}
}
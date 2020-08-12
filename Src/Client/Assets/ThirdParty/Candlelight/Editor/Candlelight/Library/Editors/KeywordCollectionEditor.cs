// 
// KeywordCollectionEditor.cs
// 
// Copyright (c) 2014, Candlelight Interactive, LLC
// All rights reserved.
// 
// This file is licensed according to the terms of the Unity Asset Store EULA:
// http://download.unity3d.com/assetstore/customer-eula.pdf
// 
// This file contains a base class for custom editors for KeywordCollections.

using UnityEditor;
using UnityEngine;
using System.Reflection;
using System.Reflection.Emit;

namespace Candlelight
{
	/// <summary>
	/// Keyword collection editor base class.
	/// </summary>
	public abstract class KeywordCollectionEditor<T> : Editor where T: KeywordCollection
	{
		/// <summary>
		/// The editor preference to toggle display of keywords in the inspector.
		/// </summary>
		private static readonly EditorPreference<bool, KeywordCollectionEditor<T>> keywordsFoldoutPreference =
			EditorPreference<bool, KeywordCollectionEditor<T>>.ForFoldoutState("keywords", false);

		/// <summary>
		/// The target object as a KeywordCollection.
		/// </summary>
		protected KeywordCollection collection;
		/// <summary>
		/// The case match property.
		/// </summary>
		protected SerializedProperty caseMatchProperty;
		/// <summary>
		/// The priorization property.
		/// </summary>
		protected SerializedProperty priorizationProperty;
		
		/// <summary>
		/// Creates a new asset in the project.
		/// </summary>
		protected static void CreateNewAssetInProject()
		{
			AssetDatabaseX.CreateNewAssetInCurrentProjectFolder<T>();
		}

		/// <summary>
		/// Displays the keyword list.
		/// </summary>
		protected void DisplayKeywordList()
		{
			if (serializedObject.targetObjects.Length == 1)
			{
				int numKeywords = collection.Keywords == null ? 0 : collection.Keywords.Count;
				keywordsFoldoutPreference.CurrentValue = EditorGUILayout.Foldout(
					keywordsFoldoutPreference.CurrentValue,
					string.Format("Extracted Keywords ({0} Unique)", numKeywords)
				);
				if (keywordsFoldoutPreference.CurrentValue && numKeywords > 0)
				{
					EditorGUI.BeginDisabledGroup(true);
					++EditorGUI.indentLevel;
					foreach (string kw in collection.Keywords)
					{
						EditorGUILayout.TextArea(kw);
					}
					--EditorGUI.indentLevel;
					EditorGUI.EndDisabledGroup();
				}
			}
		}
		
		/// <summary>
		/// Initialize properties.
		/// </summary>
		protected virtual void OnEnable()
		{
			collection = target as KeywordCollection;
			caseMatchProperty = serializedObject.FindProperty("m_CaseMatchMode");
			priorizationProperty = serializedObject.FindProperty("m_WordPrioritization");
		}
		
		/// <summary>
		/// Raises the inspector GUI event.
		/// </summary>
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();
			DisplayKeywordList();
		}
	}
}
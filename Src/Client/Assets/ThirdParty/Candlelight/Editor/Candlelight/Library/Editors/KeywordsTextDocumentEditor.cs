// 
// KeywordsTextDocumentEditor.cs
// 
// Copyright (c) 2014, Candlelight Interactive, LLC
// All rights reserved.
// 
// This file is licensed according to the terms of the Unity Asset Store EULA:
// http://download.unity3d.com/assetstore/customer-eula.pdf
// 
// This file contains a custom editor for KeywordsTextDocument.

using UnityEditor;
using UnityEngine;
using System.Reflection;

namespace Candlelight
{
	/// <summary>
	/// Keywords text document custom editor.
	/// </summary>
	[CanEditMultipleObjects, CustomEditor(typeof(KeywordsTextDocument))]
	public class KeywordsTextDocumentEditor : KeywordCollectionEditor<KeywordsTextDocument>
	{
		/// <summary>
		/// Creates a new asset in the project.
		/// </summary>
		[MenuItem("Assets/Create/Candlelight/Keyword Collections/Text Document")]
		public static void CreateNewAsset()
		{
			CreateNewAssetInProject();
		}
	}
}
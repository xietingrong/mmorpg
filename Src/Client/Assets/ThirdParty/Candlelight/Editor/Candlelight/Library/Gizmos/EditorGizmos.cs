// 
// EditorGizmos.cs
// 
// Copyright (c) 2014-2015, Candlelight Interactive, LLC
// All rights reserved.
// 
// This file is licensed according to the terms of the Unity Asset Store EULA:
// http://download.unity3d.com/assetstore/customer-eula.pdf
// 
// This file contains a class for registering gizmos in the editor. Gizmos can
// be registered manually or automatically. In order to automatically register a
// gizmo, simply put a texture in the format "ObjectType Icon.png" in the same
// folder as this script.

using UnityEditor;
using UnityEngine;
using System.Linq;
using System.IO;

namespace Candlelight
{
	/// <summary>
	/// Editor gizmos.
	/// </summary>
	[InitializeOnLoad]
	public static class EditorGizmos
	{
		/// <summary>
		/// Initializes the <see cref="EditorGizmos"/> class.
		/// </summary>
		static EditorGizmos()
		{
			// find the folder with this script
			string scriptFolder = AssetDatabaseX.GetFolderContainingScript(
				typeof(EditorGizmos), "Assets/Plugins/Editor/Candlelight/Library/Gizmos"
			);
			if (string.IsNullOrEmpty(scriptFolder))
			{
				scriptFolder = AssetDatabaseX.GetFolderContainingScript(typeof(EditorGizmos), "Assets/Plugins");
			}
			if (string.IsNullOrEmpty(scriptFolder))
			{
				scriptFolder = AssetDatabaseX.GetFolderContainingScript(typeof(EditorGizmos));
			}
			// if the folder was found, search it for icons and register them
			if (!string.IsNullOrEmpty(scriptFolder))
			{
				foreach (string sourceIconPath in Directory.GetFiles(scriptFolder, "*.png"))
				{
					RegisterGizmo(File.ReadAllBytes(sourceIconPath), Path.GetFileName(sourceIconPath));
				}
			}
		}

		/// <summary>
		/// Registers the gizmo.
		/// </summary>
		/// <param name="texture">Texture.</param>
		/// <param name="typeName">Type name. For example "Dog" will produce "Dog Icon.png".</param>
		public static void RegisterGizmo(Texture2D texture, string typeName)
		{
			Texture2D copy = texture.GetReadableCopy();
			if (copy == null)
			{
				return;
			}
			RegisterGizmo(copy.EncodeToPNG(), string.Format("{0} Icon.png", typeName));
		}

		/// <summary>
		/// Registers the gizmo.
		/// </summary>
		/// <param name="pngData">Png data.</param>
		/// <param name="fileName">Destination file name with extension.</param>
		private static void RegisterGizmo(byte[] pngData, string destinationFileNameWithExtension)
		{
			string destIconPath = Path.Combine("Assets/Gizmos", destinationFileNameWithExtension);
			// skip ahead if icon has already been copied and hasn't changed
			if (File.Exists(destIconPath) /*&& pngData.SequenceEqual(File.ReadAllBytes(destIconPath))*/)
			{
				return;
			}
			// otherwise write the icon into the Gizmos folder
			AssetDatabaseX.CreateFolderIfNecessary("Assets/Gizmos");
			File.WriteAllBytes(destIconPath, pngData);
			// configure the import settings and import the asset
			AssetDatabase.Refresh();
			TextureImporter importer = AssetImporter.GetAtPath(destIconPath) as TextureImporter;
			importer.filterMode = FilterMode.Trilinear;
			importer.textureType = TextureImporterType.GUI;
			importer.textureCompression = TextureImporterCompression.Uncompressed;
			AssetDatabase.ImportAsset(destIconPath);
		}
	}
}
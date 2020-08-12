// 
// AssetDatabaseX.cs
// 
// Copyright (c) 2012-2015, Candlelight Interactive, LLC
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
// 
// 1. Redistributions of source code must retain the above copyright notice,
// this list of conditions and the following disclaimer.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
// POSSIBILITY OF SUCH DAMAGE.
// 
// This file contains a class with static methods for working with the asset
// database.

using UnityEngine;
using UnityEditor;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;

namespace Candlelight
{
	/// <summary>
	/// A utility class for working with the asset database.
	/// </summary>
	public static class AssetDatabaseX
	{
		/// <summary>
		/// Adds and loads the asset.
		/// </summary>
		/// <returns>The new asset, imported from the asset database.</returns>
		/// <param name="asset">Asset to add to the existing path.</param>
		/// <param name="path">A project-relative path to an asset in the form "Assets/MyTextures/hello.png".</param>
		/// <typeparam name="T">The asset's type.</typeparam>
		public static T AddAndLoadAsset<T>(T asset, string path) where T: Object
		{
			CreateFolderIfNecessary(Path.GetDirectoryName(path));
			AssetDatabase.AddObjectToAsset(asset, path);
			AssetDatabase.ImportAsset(path);
			return LoadAssetAtPath<T>(path);
		}
		
		/// <summary>
		/// Creates and loads the asset.
		/// </summary>
		/// <returns>The new asset, imported from the asset database.</returns>
		/// <param name="asset">Asset to add to the database.</param>
		/// <param name="path">A project-relative path to an asset in the form "Assets/MyTextures/hello.png".</param>
		/// <typeparam name="T">The asset's type.</typeparam>
		public static T CreateAndLoadAsset<T>(T asset, string path) where T: Object
		{
			return CreateAndLoadAsset(asset as Object, path) as T;
		}

		/// <summary>
		/// Creates and loads the asset.
		/// </summary>
		/// <returns>The new asset, imported from the asset database.</returns>
		/// <param name="asset">Asset to add to the database.</param>
		/// <param name="path">A project-relative path to an asset in the form "Assets/MyTextures/hello.png".</param>
		public static Object CreateAndLoadAsset(Object asset, string path)
		{
			CreateFolderIfNecessary(Path.GetDirectoryName(path));
			path = AssetDatabase.GenerateUniqueAssetPath(path);
			AssetDatabase.CreateAsset(asset, path);
			AssetDatabase.ImportAsset(path);
			return AssetDatabase.LoadAssetAtPath(path, asset.GetType());
		}
		
		/// <summary>
		/// Creates the specified folder if it does not exist.
		/// </summary>
		/// <param name="folder">A project-relative path to a folder in the form "Assets/MyTextures".</param>
		public static void CreateFolderIfNecessary(string folder)
		{
			string parentFolder = "";
			string[] folders = new Regex("[\\/]").Split(folder);
			string fullPath = Directory.GetParent(Application.dataPath).FullName;
			foreach (string f in folders)
			{
				fullPath = Path.Combine(fullPath, f);
				if (!Directory.Exists(fullPath))
				{
					AssetDatabase.CreateFolder(parentFolder, f);
				}
				parentFolder = Path.Combine(parentFolder, f);
			}
		}
		
		/// <summary>
		/// Creates a new scriptable object asset in the current project folder. Use this method when adding menu items
		/// to Assets/Create.
		/// </summary>
		/// <returns>The new scriptable object asset, imported from the asset database.</returns>
		/// <typeparam name="T">The asset's type.</typeparam>
		public static T CreateNewAssetInCurrentProjectFolder<T>() where T: ScriptableObject
		{
			T newAsset = ScriptableObject.CreateInstance<T>();
			newAsset =
				CreateNewAssetInCurrentProjectFolder<T>(newAsset, string.Format("{0}.asset", typeof(T).Name.ToWords()));
			Selection.activeObject = newAsset;
			return newAsset;
		}

		/// <summary>
		/// Creates a new scriptable object asset in a user-specified path.
		/// </summary>
		/// <returns>The new scriptable object asset, imported from the asset database.</returns>
		/// <param name="title">Title.</param>
		/// <param name="defaultName">Default name.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T CreateNewAssetInUserSpecifiedPath<T>(
			string title = null, string defaultName = null
		) where T: ScriptableObject
		{
			return CreateNewAssetInUserSpecifiedPath(typeof(T), title, defaultName) as T;
		}

		/// <summary>
		/// Creates a new scriptable object asset in a user-specified path.
		/// </summary>
		/// <returns>The new scriptable object asset, imported from the asset database.</returns>
		/// <param name="scriptableObjectType">A type assignable from UnityEngine.ScriptableObject.</param>
		/// <param name="title">Title.</param>
		/// <param name="defaultName">Default name.</param>
		public static ScriptableObject CreateNewAssetInUserSpecifiedPath(
			System.Type scriptableObjectType, string title = null, string defaultName = null
		)
		{
			if (!typeof(ScriptableObject).IsAssignableFrom(scriptableObjectType))
			{
				Debug.LogException(
					new System.ArgumentException(
						string.Format("Type must inherit from {0}.", typeof(ScriptableObject).FullName),
						"scriptableObjectType"
					)
				);
				return null;
			}
			string typeWords = scriptableObjectType.Name.ToWords();
			title = title ?? string.Format("Create new {0}", typeWords);
			defaultName = defaultName ?? string.Format("{0}.asset", typeWords);
			string pathToNewAsset = EditorUtility.SaveFilePanelInProject(
				title, defaultName, "asset", string.Format("Please enter a file name for the new {0}", typeWords)
			);
			return string.IsNullOrEmpty(pathToNewAsset) ?
				null : CreateAndLoadAsset(ScriptableObject.CreateInstance(scriptableObjectType), pathToNewAsset);
		}

		/// <summary>
		/// Creates the new asset in the current project folder. Use this method when adding menu items to
		/// Assets/Create.
		/// </summary>
		/// <returns>The new asset, imported from the asset database.</returns>
		/// <param name="newAsset">New asset.</param>
		/// <param name="newAssetFileName">New asset file name, such as "new asset.asset".</param>
		/// <typeparam name="T">The asset's type.</typeparam>
		public static T CreateNewAssetInCurrentProjectFolder<T>(T newAsset, string newAssetFileName) where T: Object
		{
			string folderName = "Assets";
			if (
				Selection.activeObject != null &&
				(AssetDatabase.IsMainAsset(Selection.activeObject) || AssetDatabase.IsSubAsset(Selection.activeObject))
			)
			{
				folderName = AssetDatabase.GetAssetPath(Selection.activeObject);
				folderName = Directory.Exists(folderName) ?
					folderName : Path.GetDirectoryName(folderName);
			}
			return CreateAndLoadAsset<T>(newAsset, Path.Combine(folderName, newAssetFileName));
		}

		/// <summary>
		/// Tests whether the asset has one of the possible required file extensions.
		/// </summary>
		/// <returns>
		/// <see langword="true"/> if the asset has a possible required file extension;
		/// otherwise, <see langword="false"/>.
		/// </returns>
		/// <param name="assetPath">Asset path.</param>
		/// <param name="possibleExtensions">Possible extensions in the form ".png".</param>
		public static bool DoesAssetHaveRequiredFileExtension(
			string assetPath, ReadOnlyCollection<string> possibleExtensions
		)
		{
			string cmp = Path.GetExtension(assetPath).ToLower();
			foreach (string ext in possibleExtensions)
			{
				if (cmp == ext.ToLower())
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Gets the folder containing the script that defines the specified <see cref="System.Type"/>.
		/// </summary>
		/// <returns>
		/// The folder containing the script that defines the specified <see cref="System.Type"/>. if it could be found;
		/// otherwise, <see langword="null"/>.
		/// </returns>
		/// <param name="folderToSearch">
		/// Optional folder to search in the asset database. Otherwise, entire asset database will be searched.
		/// </param>
		/// <typeparam name="T">The <see cref="System.Type"/> whose script is being sought.</typeparam>
		public static string GetFolderContainingScript<T>(string folderToSearch = "Assets")
		{
			return GetFolderContainingScript(typeof(T), folderToSearch);
		}

		/// <summary>
		/// Gets the folder containing the script that defines the specified <see cref="System.Type"/>.
		/// </summary>
		/// <returns>
		/// The folder containing the script that defines the specified <see cref="System.Type"/>. if it could be found;
		/// otherwise, <see langword="null"/>.
		/// </returns>
		/// <param name="type">The <see cref="System.Type"/> whose script is being sought.</param>
		/// <param name="folderToSearch">
		/// Optional folder to search in the asset database. Otherwise, entire asset database will be searched.
		/// </param>
		public static string GetFolderContainingScript(System.Type type, string folderToSearch = "Assets")
		{
			folderToSearch = string.IsNullOrEmpty(folderToSearch) ? "Assets" : folderToSearch;
			MonoScript script = null;
			if (!Directory.Exists(folderToSearch))
			{
				return null;
			}
			foreach (string filePath in Directory.GetFiles(folderToSearch, "*.cs", SearchOption.AllDirectories))
			{
				script = AssetDatabase.LoadAssetAtPath(filePath, typeof(MonoScript)) as MonoScript;
				if (script != null && script.GetClass() == type)
				{
					return Path.GetDirectoryName(filePath);
				}
			}
			return null;
		}

		/// <summary>
		/// Gets the name of the folder containing the specified asset.
		/// </summary>
		/// <returns>
		/// The name of the folder containing the specified asset. For instance, "Asssets/MyTextures/hello.png" returns
		/// "MyTextures".
		/// </returns>
		/// <param name="assetPath">
		/// A project-relative path to an asset in the form "Assets/MyTextures/hello.png".
		/// </param>
		public static string GetFolderName(string assetPath)
		{
			return Path.GetFileNameWithoutExtension(Path.GetDirectoryName(assetPath));
		}
		
		/// <summary>
		/// Determines if an asset with the specified path is in a folder with the specified name.
		/// </summary>
		/// <returns>
		/// <see langword="true"/> if the asset at the specified path is in a folder with folderName;
		/// otherwise, <see langword="false"/>.
		/// </returns>
		/// <param name="assetPath">
		/// A project-relative path to an asset in the form "Assets/MyTextures/hello.png".
		/// </param>
		/// <param name="folderName">
		/// The name of the folder in which the asset is expected. For instance, "Assets/MyTextures/hello.png" would be
		/// "MyTextures".
		/// </param>
		public static bool IsAssetPathInFolderWithName(string assetPath, string folderName)
		{
			return GetFolderName(assetPath) == folderName;
		}

		/// <summary>
		/// Loads the asset at path with the specified type.
		/// </summary>
		/// <returns>The asset at path with the specified type.</returns>
		/// <param name="assetPath">
		/// A project-relative path to an asset in the form "Assets/MyTextures/hello.png".
		/// </param>
		/// <typeparam name="T">The asset's type.</typeparam>
		public static T LoadAssetAtPath<T>(string assetPath) where T: Object
		{
			return AssetDatabase.LoadAssetAtPath(assetPath, typeof(T)) as T;
		}

		/// <summary>
		/// Prints the selected objects' asset paths and types.
		/// </summary>
		[MenuItem ("Assets/Print Asset Path")]
		private static void PrintSelected()
		{
			foreach (Object obj in Selection.objects)
			{
				if (AssetDatabase.Contains(obj))
				{
					Debug.Log(string.Format("{0} ({1})", AssetDatabase.GetAssetPath(obj), obj.GetType()));
				}
				else
				{
					Debug.LogWarning(string.Format("{0} is not a source asset.", obj));
				}
			}
		}
	}
}
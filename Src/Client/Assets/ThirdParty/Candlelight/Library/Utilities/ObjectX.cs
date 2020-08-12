// 
// ObjectX.cs
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

using System.Collections.Generic;

namespace Candlelight
{
	/// <summary>
	/// An extension class for <see cref="System.Object"/>s and <see cref="UnityEngine.Object"/>s.
	/// </summary>
	public static class ObjectX
	{
		/// <summary>
		/// A regular expression to match an instance's prefab name
		/// </summary>
		private static readonly System.Text.RegularExpressions.Regex s_MatchPrefabName =
			new System.Text.RegularExpressions.Regex(@".+(?=\s*\(Clone\))|.+");

		/// <summary>
		/// Determines if the specified other object is equal to the referenced value type.
		/// </summary>
		/// <remarks>
		/// Use this method to implement <see cref="System.IEquatable{T}.Equals(T)"/> for custom structs.
		/// </remarks>
		/// <returns><see langword="true"/> if the two objects are equal; otherwise, <see langword="false"/>.</returns>
		/// <param name="thisObj">The referenced struct object.</param>
		/// <param name="otherObj">Other object.</param>
		/// <typeparam name="T">The type of the struct.</typeparam>
		public static bool Equals<T>(ref T thisObj, object otherObj) where T : struct
		{
			return otherObj != null && otherObj is T && otherObj.GetHashCode() == thisObj.GetHashCode();
		}

		/// <summary>
		/// Generates a hash code from the hash codes for an object's fields.
		/// </summary>
		/// <returns>A hash code.</returns>
		/// <param name="fieldHashes">Hash codes for the fields on an object being hashed.</param>
		public static int GenerateHashCode(params int[] fieldHashes)
		{
			// NOTE: don't simply call other method because it increases call stack
			int result = 17;
			for (int i = 0; i < fieldHashes.Length; ++i)
			{
				result = result * 23 + fieldHashes[i];
			}
			return result;
		}

		/// <summary>
		/// Generates a hash code from the hash codes for an object's fields.
		/// </summary>
		/// <returns>A hash code.</returns>
		/// <param name="fieldHashes">Hash codes for the fields on an object being hashed.</param>
		public static int GenerateHashCode(IEnumerable<int> fieldHashes)
		{
			int result = 17;
			foreach (int i in fieldHashes)
			{
				result = result * 23 + i;
			}
			return result;
		}

		/// <summary>
		/// Gets the next suitable inactive object in the pool. Instantiates a new one if none is available.
		/// </summary>
		/// <returns>The next suitable inactive object from the pool.</returns>
		/// <param name="pool">Pool.</param>
		/// <param name="prefab">
		/// Prefab to instantiate if no instance is available in pool. If <see langword="null"/>, then an empty GameObject with the
		/// component will be created and used.
		/// </param>
		/// <param name="parent">
		/// Optional object that should become the parent of the retrieved instance. Set this value in particular for
		/// e.g., UI prefabs.
		/// </param>
		/// <param name="isElementSuitable">
		/// Optional predicate to determine if an inactive item in the pool is suitable for use.
		/// </param>
		/// <typeparam name="T">The type of object in the pool.</typeparam>
		public static T GetFromPool<T>(
			List<T> pool, T prefab, UnityEngine.Transform parent = null, System.Predicate<T> isElementSuitable = null
		) where T : UnityEngine.Component
		{
			T result;
			if (isElementSuitable != null)
			{
				result = pool.Find(item => item.gameObject.activeSelf == false && isElementSuitable(item));
			}
			else
			{
				result = pool.Find(item => item != null && item.gameObject.activeSelf == false);
			}
			if (result == null)
			{
				if (prefab == null)
				{
					result = new UnityEngine.GameObject(
						string.Format("<Pooled {0} {1}>", typeof(T).FullName, pool.Count), typeof(T)
					).GetComponent<T>();
				}
				else
				{
					result = UnityEngine.Object.Instantiate(prefab) as T;
				}
				result.transform.SetParent(parent, false);
				result.transform.SetAsLastSibling();
				pool.Add(result);
			}
			result.gameObject.SetActive(true);
			return result;
		}

		/// <summary>
		/// Gets the name of the prefab associated with the supplied instance.
		/// </summary>
		/// <returns>The prefab name.</returns>
		/// <param name="instance">
		/// An <see cref="UnityEngine.Object"/> instance with a name in the form "Object Name (Clone)".
		/// </param>
		public static string GetPrefabName(this UnityEngine.Object instance)
		{
			return s_MatchPrefabName.Match(instance.name).Value;
		}

		/// <summary>
		/// Opens a reference web page generated for the specified object.
		/// </summary>
		/// <remarks>This method assumes the page in question was generated via SHFB.</remarks>
		/// <param name="obj">A <see cref="UnityEngine.Object"/>.</param>
		/// <param name="product">Product name (first parameter in format string).</param>
		/// <param name="urlFormat">
		/// A <see cref="System.String"/> specifying the URL format. Index 0 is for the product name and index 1 is for
		/// the type name.
		/// </param>
		public static void OpenReferencePage(
			this UnityEngine.Object obj,
			string product,
			string urlFormat = "http://candlelightinteractive.com/docs/{0}/html/T_{1}.htm?ref=editor"
		)
		{
			UnityEngine.Application.OpenURL(
				string.Format(urlFormat, product, obj.GetType().FullName.Replace('.', '_'))
			);
		}
	}
}
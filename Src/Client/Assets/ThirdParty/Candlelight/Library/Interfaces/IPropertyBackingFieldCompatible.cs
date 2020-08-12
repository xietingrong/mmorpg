// 
// IPropertyBackingFieldCompatible.cs
// 
// Copyright (c) 2014-2015, Candlelight Interactive, LLC
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
using System.Linq;

namespace Candlelight
{
	/// <summary>
	/// An interface to specify that a serializable type is compatible with <see cref="PropertyBackingFieldAttribute"/>.
	/// </summary>
	public interface IPropertyBackingFieldCompatible : System.ICloneable
	{
		/// <summary>
		/// Gets a hash value that is based on the values of the serialized properties of this instance.
		/// </summary>
		/// <remarks>
		/// Note that any reference type fields should implement and test with this interface;
		/// <see cref="System.Collections.IList"/> fields should generate a value-based hash.
		/// </remarks>
		/// <returns>A hash value based on the values of the serialized properties on this instance.</returns>
		int GetSerializedPropertiesHash();
	}

	/// <summary>
	/// A generic interface to specify that a serializable struct is compatible with
	/// <see cref="PropertyBackingFieldAttribute"/>. It exists mainly to add a compile-time reminder to implement more
	/// optimized equality comparison.
	/// </summary>
	/// <typeparam name="T">The type of struct implementing this interface.</typeparam>
	public interface IPropertyBackingFieldCompatible<T> : IPropertyBackingFieldCompatible, System.IEquatable<T>
		where T : struct
	{

	}

	/// <summary>
	/// Backing field utility class.
	/// </summary>
	public static class BackingFieldUtility
	{
		/// <summary>
		/// Gets the keyed list backing field as a dictionary.
		/// </summary>
		/// <param name="backingField">Backing field of identifiable objects.</param>
		/// <param name="result">Result.</param>
		/// <param name="getData">Method to get the data of interest from the identifiable object.</param>
		/// <typeparam name="T">An identifiable backing field compatible object wrapper type.</typeparam>
		/// <typeparam name="TData">The data type.</typeparam>
		public static void GetKeyedListBackingFieldAsDict<T, TData>(
			List<T> backingField, ref Dictionary<string, TData> result, System.Func<T, TData> getData
		) where T : IIdentifiable<string>
		{
			result = result ?? new Dictionary<string, TData>();
			result.Clear();
			for (int i = 0; i < backingField.Count; ++i)
			{
				result[backingField[i].Identifier] = getData(backingField[i]);
			}
		}

		/// <summary>
		/// Gets the keyed list backing field as a dictionary.
		/// </summary>
		/// <param name="backingField">Backing field of identifiable wrapper objects.</param>
		/// <param name="result">Result.</param>
		/// <typeparam name="TWrapper">An identifiable backing field compatible object wrapper type.</typeparam>
		/// <typeparam name="TData">The data type.</typeparam>
		public static void GetKeyedListBackingFieldAsDict<TWrapper, TData>(
			List<TWrapper> backingField, ref Dictionary<string, TData> result
		) where TWrapper : IdentifiableBackingFieldCompatibleObjectWrapper<TData>
		{
			result = result ?? new Dictionary<string, TData>();
			result.Clear();
			for (int i = 0; i < backingField.Count; ++i)
			{
				result[backingField[i].Identifier] = backingField[i].Data;
			}
		}

		/// <summary>
		/// Sets a backing field for a list of identifiable objects that should have unique <see cref="System.String"/>
		/// keys. Use this method when you need to serialize something that may be deserialized as a dictionary.
		/// </summary>
		/// <returns>
		/// <see langword="true"/> if the new value differs from the old one; otherwise, <see langword="false"/>.
		/// </returns>
		/// <param name="backingField">Backing field of identifiable objects.</param>
		/// <param name="value">Value.</param>
		/// <param name="mutateIdentifier">A method to mutate the identifier on an object if it is not unique.</param>
		/// <param name="ignoreCase">If set to <see langword="true"/>, then all keys will be made lowercased.</param>
		/// <typeparam name="T">A type with a string identifier.</typeparam>
		public static bool SetKeyedListBackingFieldFromArray<T>(
			List<T> backingField, T[] value, System.Func<string, T, T> mutateIdentifier, bool ignoreCase = false
		) where T : IIdentifiable<string>
		{
			if (value == null || value.Length == 0)
			{
				int oldCount = backingField.Count;
				backingField.Clear();
				return oldCount != 0;
			}
			T[] oldValue = backingField.ToArray();
			backingField.Clear();
			Dictionary<string, T> keyedValues = new Dictionary<string, T>();
			string id;
			for (int i = 0; i < value.Length; ++i)
			{
				id = value[i].Identifier ?? "";
				id = (ignoreCase ? id.ToLower() : id).GetUniqueName(keyedValues.Keys);
				keyedValues[id] = value[i];
			}
			foreach (KeyValuePair<string, T> kv in keyedValues)
			{
				backingField.Add(mutateIdentifier(kv.Key, kv.Value));
			}
			return !oldValue.SequenceEqual(backingField);
		}

		/// <summary>
		/// Sets a backing field for a list of identifiable objects that should have unique keys. Use this method when
		/// you need to serialize something that may be deserialized as a dictionary.
		/// </summary>
		/// <returns>
		/// <see langword="true"/> if the new value differs from the old one; otherwise, <see langword="false"/>.
		/// </returns>
		/// <param name="backingField">Backing field of identifiable wrapper objects.</param>
		/// <param name="value">Value.</param>
		/// <param name="factory">
		/// Method to create a new entry instance for the backing field from the key-value pair.
		/// </param>
		/// <typeparam name="TWrapper">An identifiable backing field compatible object wrapper type.</typeparam>
		/// <param name="ignoreCase">If set to <see langword="true"/>, then all keys will be made lowercased.</param>
		/// <typeparam name="TData">The data type.</typeparam>
		public static bool SetKeyedListBackingFieldFromDict<TWrapper, TData>(
			List<TWrapper> backingField,
			Dictionary<string, TData> value,
			System.Func<string, TData,
			TWrapper> factory,
			bool ignoreCase = false
		)
			where TWrapper : IdentifiableBackingFieldCompatibleObjectWrapper<TData>
		{
			if (value == null || value.Count == 0)
			{
				int oldCount = backingField.Count;
				backingField.Clear();
				return oldCount != 0;
			}
			TWrapper[] oldValue = backingField.ToArray();
			backingField.Clear();
			backingField.AddRange(from kv in value select factory(ignoreCase ? kv.Key.ToLower() : kv.Key, kv.Value));
			if (oldValue.Length != backingField.Count)
			{
				return true;
			}
			for (int i = 0; i < oldValue.Length; ++i)
			{
				if (oldValue[i].Data.GetHashCode() != backingField[i].Data.GetHashCode())
				{
					return true;
				}
			}
			return false;
		}
	}

	/// <summary>
	/// Backing field utility class.
	/// </summary>
	/// <typeparam name="T">An <see cref="IPropertyBackingFieldCompatible"/> type.</typeparam>
	public static class BackingFieldUtility<T> where T: IPropertyBackingFieldCompatible
	{
		/// <summary>
		/// An <see cref="System.Collections.Generic.IEqualityComparer{T}"/>, provided as a convenience for evaluating
		/// equality of sequences of <typeparamref name="T"/>.
		/// </summary>
		public class CollectionComparer : IEqualityComparer<T>
		{
			/// <summary>
			/// Determines if the two specified <typeparamref name="T"/> are equivalent in terms of their serialized
			/// properties.
			/// </summary>
			/// <returns>
			/// <see langword="true"/> if the two <typeparamref name="T"/> are equal; otherwise,
			/// <see langword="false"/>.
			/// </returns>
			/// <param name="a">The first <typeparamref name="T"/>.</param>
			/// <param name="b">The second <typeparamref name="T"/>.</param>
			public bool Equals(T a, T b)
			{
				return a.GetSerializedPropertiesHash() == b.GetSerializedPropertiesHash();
			}
			/// <summary>
			/// Gets the hash code of the specified <typeparamref name="T"/> in terms of its serialized properties.
			/// </summary>
			/// <returns>The hash code.</returns>
			/// <param name="obj">Object.</param>
			public int GetHashCode(T obj)
			{
				return obj == null ? 0 : obj.GetSerializedPropertiesHash();
			}
		}

		#region Backing Fields
		private static CollectionComparer s_Comparer = null;
		#endregion
		/// <summary>
		/// Gets the comparer for this class's type.
		/// </summary>
		/// <value>The comparer for this class's type.</value>
		public static CollectionComparer Comparer
		{
			get
			{
				if (s_Comparer == null)
				{
					s_Comparer = new CollectionComparer();
				}
				return s_Comparer;
			}
		}
	}
}
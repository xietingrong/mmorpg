// 
// IdentifiableBackingFieldCompatibleObjectWrapper.cs
// 
// Copyright (c) 2015, Candlelight Interactive, LLC
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

using UnityEngine;

namespace Candlelight
{
	/// <summary>
	/// <para>Base class for a wrapper around serializable, backing field compatible objects that should have a unique
	/// identifier. This class is provided as a means for serializing some dictionary types.</para>
	/// <para>Make sure your subclass adds <see cref="System.SerializableAttribute"/>.</para>
	/// </summary>
	/// <remarks>See also <see cref="BackingFieldUtility"/>.</remarks>
	public abstract class IdentifiableBackingFieldCompatibleObjectWrapper : BackingFieldCompatibleObject
	{

	}

	/// <summary>
	/// <para>Base class for a wrapper around serializable, backing field compatible objects that should have a unique
	/// identifier. This class is provided as a means for serializing some dictionary types.</para>
	/// <para>Make sure your subclass adds <see cref="System.SerializableAttribute"/>.</para>
	/// </summary>
	/// <remarks>See also <see cref="BackingFieldUtility"/>.</remarks>
	/// <typeparam name="T">The type of data being wrapped.</typeparam>
	[System.Serializable]
	public abstract class IdentifiableBackingFieldCompatibleObjectWrapper<T> :
		IdentifiableBackingFieldCompatibleObjectWrapper, IIdentifiable<string>
	{
		#region Backing Fields
		[SerializeField]
		private string m_Identifier;
		[SerializeField]
		private T m_Data;
		#endregion
		/// <summary>
		/// Gets the data.
		/// </summary>
		/// <value>The data.</value>
		public T Data
		{
			get { return m_Data; }
			protected set { m_Data = value; }
		}
		/// <summary>
		/// Gets the identifier.
		/// </summary>
		/// <value>The identifier.</value>
		public string Identifier
		{
			get { return m_Identifier = m_Identifier ?? ""; }
			protected set { m_Identifier = value; }
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="IdentifiableBackingFieldCompatibleObjectWrapper{T}"/>
		/// class.
		/// </summary>
		/// <param name="id">Identifier.</param>
		/// <param name="data">Data.</param>
		protected IdentifiableBackingFieldCompatibleObjectWrapper(string id, T data)
		{
			this.Identifier = id;
			this.Data = data;
		}

		/// <summary>
		/// Gets a hash value that is based on the values of the serialized properties of this instance.
		/// </summary>
		/// <returns>The serialized properties hash.</returns>
		public override int GetSerializedPropertiesHash()
		{
			return ObjectX.GenerateHashCode(this.Identifier.GetHashCode(), this.Data.GetHashCode());
		}
	}
}
// 
// RectOffsetX.cs
// 
// Copyright (c) 2014-2015, Candlelight Interactive, LLC
// All rights reserved.
// 
// This file is licensed according to the terms of the Unity Asset Store EULA:
// http://download.unity3d.com/assetstore/customer-eula.pdf

using UnityEngine;

namespace Candlelight
{
	/// <summary>
	/// An immutable representation of a <see cref="UnityEngine.RectOffset"/>.
	/// </summary>
	[System.Serializable]
	public struct ImmutableRectOffset : IPropertyBackingFieldCompatible<ImmutableRectOffset>
	{
		#region Backing Fields
		[SerializeField]
		private int m_Left;
		[SerializeField]
		private int m_Right;
		[SerializeField]
		private int m_Top;
		[SerializeField]
		private int m_Bottom;
		#endregion

		/// <summary>
		/// Gets the bottom value.
		/// </summary>
		/// <value>The bottom value.</value>
		public int Bottom { get { return m_Bottom; } }
		/// <summary>
		/// Gets the left value.
		/// </summary>
		/// <value>The left value.</value>
		public int Left { get { return m_Left; } }
		/// <summary>
		/// Gets the right value.
		/// </summary>
		/// <value>The right value.</value>
		public int Right { get { return m_Right; } }
		/// <summary>
		/// Gets the top value.
		/// </summary>
		/// <value>The top value.</value>
		public int Top { get { return m_Top; } }

		/// <summary>
		/// Initializes a new instance of the <see cref="ImmutableRectOffset"/> struct.
		/// </summary>
		/// <param name="left">Left offset.</param>
		/// <param name="right">Right offset.</param>
		/// <param name="top">Top offset.</param>
		/// <param name="bottom">Bottom offset.</param>
		public ImmutableRectOffset(int left, int right, int top, int bottom)
		{
			m_Left = left;
			m_Right = right;
			m_Top = top;
			m_Bottom = bottom;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ImmutableRectOffset"/> struct.
		/// </summary>
		/// <param name="rectOffset">Rect offset.</param>
		public ImmutableRectOffset(RectOffset rectOffset)
		{
			m_Left = rectOffset.left;
			m_Right = rectOffset.right;
			m_Top = rectOffset.top;
			m_Bottom = rectOffset.bottom;
		}

		/// <summary>
		/// Clone this instance.
		/// </summary>
		/// <returns>A clone of this instance.</returns>
		public object Clone()
		{
			return this;
		}

		/// <summary>
		/// Determines whether the specified <see cref="System.Object"/> is equal to the current
		/// <see cref="ImmutableRectOffset"/>.
		/// </summary>
		/// <param name="obj">
		/// The <see cref="System.Object"/> to compare with the current <see cref="ImmutableRectOffset"/>.
		/// </param>
		/// <returns>
		/// <see langword="true"/> if the specified <see cref="System.Object"/> is equal to the current
		/// <see cref="ImmutableRectOffset"/>; otherwise, <see langword="false"/>.
		/// </returns>
		public override bool Equals(object obj)
		{
			return ObjectX.Equals(ref this, obj);
		}

		/// <summary>
		/// Determines whether the specified <see cref="ImmutableRectOffset"/> is equal to the current
		/// <see cref="ImmutableRectOffset"/>.
		/// </summary>
		/// <param name="other">
		/// The <see cref="ImmutableRectOffset"/> to compare with the current <see cref="ImmutableRectOffset"/>.
		/// </param>
		/// <returns>
		/// <see langowrd="true"/> if the specified <see cref="ImmutableRectOffset"/> is equal to the current
		/// <see cref="ImmutableRectOffset"/>; otherwise, <see langword="false"/>.</returns>
		public bool Equals(ImmutableRectOffset other)
		{
			return GetHashCode() == other.GetHashCode();
		}

		/// <summary>
		/// Serves as a hash function for a <see cref="ImmutableRectOffset"/> object.
		/// </summary>
		/// <returns>
		/// A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a
		/// hash table.
		/// </returns>
		public override int GetHashCode()
		{
			return ObjectX.GenerateHashCode(
				m_Left.GetHashCode(), m_Right.GetHashCode(), m_Top.GetHashCode(), m_Bottom.GetHashCode()
			);
		}

		/// <summary>
		/// Gets a hash value that is based on the values of the serialized properties of this instance.
		/// </summary>
		/// <returns>The serialized properties hash.</returns>
		public int GetSerializedPropertiesHash()
		{
			return GetHashCode();
		}
	}

	/// <summary>
	/// An extension class for <see cref="UnityEngine.RectOffset"/>.
	/// </summary>
	public static class RectOffsetX
	{
		/// <summary>
		/// Gets the horizontal average.
		/// </summary>
		/// <returns>The horizontal average.</returns>
		/// <param name="rectOffset">A <see cref="UnityEngine.RectOffset"/>.</param>
		public static float GetHorizontalAverage(this RectOffset rectOffset)
		{
			return 0.5f * rectOffset.horizontal;
		}

		/// <summary>
		/// Gets the vertical average.
		/// </summary>
		/// <returns>The vertical average.</returns>
		/// <param name="rectOffset">A <see cref="UnityEngine.RectOffset"/>.</param>
		public static float GetVerticalAverage(this RectOffset rectOffset)
		{
			return 0.5f * rectOffset.vertical;
		}
	}
}
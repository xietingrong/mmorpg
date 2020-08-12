// 
// IndexRange.cs
// 
// Copyright (c) 2014-2015, Candlelight Interactive, LLC
// All rights reserved.
// 
// This file is licensed according to the terms of the Unity Asset Store EULA:
// http://download.unity3d.com/assetstore/customer-eula.pdf

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Candlelight
{
	/// <summary>
	/// A class for describing a range of indices.
	/// </summary>
	public class IndexRange : System.ICloneable, IEnumerable<int>
	{
		/// <summary>
		/// Gets the number of elements encompassed by this instance.
		/// </summary>
		/// <value>The number of elements encompassed by this instance.</value>
		public int Count { get { return Mathf.Abs(this.EndIndex - this.StartIndex) + 1; } }
		/// <summary>
		/// The direction of the range, positive or negative.
		/// </summary>
		private int Direction { get { return this.EndIndex >= this.StartIndex ? 1 : -1; } }
		/// <summary>
		/// Gets or sets the end index.
		/// </summary>
		/// <value>The end index.</value>
		public int EndIndex { get; set; }
		/// <summary>
		/// Gets or sets the start index.
		/// </summary>
		/// <value>The start index.</value>
		public int StartIndex { get; set; }
		/// <summary>
		/// Gets the <see cref="System.Int32"/> at the specified index in the range.
		/// </summary>
		/// <param name="index">Index.</param>
		/// <value>The <see cref="System.Int32"/> at the specified index in the range.</value>
		public int this[int index] { get { return this.StartIndex + index * this.Direction; } }
		
		/// <summary>
		/// Initializes a new instance of the <see cref="IndexRange"/> class.
		/// </summary>
		/// <param name="start">Start.</param>
		/// <param name="end">End.</param>
		public IndexRange(int start, int end)
		{
			this.StartIndex = start;
			this.EndIndex = end;
		}

		/// <summary>
		/// Clone this instance.
		/// </summary>
		/// <returns>A clone of this instance.</returns>
		public object Clone()
		{
			return new IndexRange(this.StartIndex, this.EndIndex);
		}

		/// <summary>
		/// Determines whether or not this instance contains the specified index.
		/// </summary>
		/// <returns>
		/// <see langword="true"/> if this instance contains the specified index; otherwise <see langword="false"/>.
		/// </returns>
		/// <param name="index">Index.</param>
		public bool Contains(int index)
		{
			return this.Direction > 0 ?
				index >= this.StartIndex && index <= this.EndIndex :
				index <= this.StartIndex && index >= this.EndIndex;
		}

		/// <summary>
		/// Determines whether or not this instance contains the specified other <see cref="IndexRange"/>.
		/// </summary>
		/// <returns>
		/// <see langword="true"/> if this instance contains the specified other <see cref="IndexRange"/>; otherwise
		/// <see langword="false"/>.
		/// </returns>
		/// <param name="other">Other.</param>
		public bool Contains(IndexRange other)
		{
			return Contains(other.StartIndex) && Contains(other.EndIndex);
		}
		
		/// <summary>
		/// Gets an enumerator.
		/// </summary>
		/// <returns>An enumerator.</returns>
		public IEnumerator<int> GetEnumerator()
		{
			return (
				from i in Enumerable.Range(0, this.Count) select this.StartIndex + i * this.Direction
			).GetEnumerator();
		}
		
		/// <summary>
		/// Gets an enumerator.
		/// </summary>
		/// <returns>An enumerator.</returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		/// <summary>
		/// Offset this <see cref="IndexRange"/> using the specified delta values.
		/// </summary>
		/// <param name="deltaValues">A collection delta values for each interval in the old range.</param>
		public void Offset(Dictionary<IndexRange, int> deltaValues)
		{
			int direction = this.Direction;
			if (direction < 0)
			{
				Reverse();
			}
			foreach (KeyValuePair<IndexRange, int> delta in deltaValues)
			{
				int deltaEnd = Mathf.Max(delta.Key.StartIndex, delta.Key.EndIndex);
				int deltaStart = Mathf.Min(delta.Key.StartIndex, delta.Key.EndIndex);
				if (deltaEnd <= this.StartIndex)				// ...  |-------|
				{
					this.StartIndex += delta.Value;
					this.EndIndex += delta.Value;
				}
				else if (Contains(deltaStart))			// |--.----|.....
				{
					if (deltaStart == this.StartIndex)		// .-------|.....
					{
						this.StartIndex += delta.Value;
					}
					this.EndIndex += delta.Value;
				}
				else if (Contains(deltaEnd))			// .....|--.----|
				{
					this.StartIndex += delta.Value;
					this.EndIndex += delta.Value;
				}
				else if (								// ...|-------|..
					delta.Key.Contains(this.StartIndex) && delta.Key.Contains(this.EndIndex)
				)
				{
					this.StartIndex += delta.Value;
					this.EndIndex += delta.Value;
				}
			}
			if (direction < 0)
			{
				Reverse();
			}
		}

		/// <summary>
		/// Reverse this instance.
		/// </summary>
		public void Reverse()
		{
			int start = this.StartIndex;
			this.StartIndex = this.EndIndex;
			this.EndIndex = start;
		}
		
		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the current <see cref="IndexRange"/>.
		/// </summary>
		/// <returns>A <see cref="System.String"/> that represents the current <see cref="IndexRange"/>.</returns>
		public override string ToString()
		{
			return string.Format("[{0}, {1}]", this.StartIndex, this.EndIndex);
		}
	}
}
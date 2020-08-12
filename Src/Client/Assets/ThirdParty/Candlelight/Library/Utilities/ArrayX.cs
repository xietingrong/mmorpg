// 
// ArrayX.cs
// 
// Copyright (c) 2014-2015, Candlelight Interactive, LLC
// All rights reserved.
// 
// This file is licensed according to the terms of the Unity Asset Store EULA:
// http://download.unity3d.com/assetstore/customer-eula.pdf
// 
// This file contains a class with extension methods for arrays.

namespace Candlelight
{
	/// <summary>
	/// An extension class for <see cref="System.Array"/>.
	/// </summary>
	public static class ArrayX
	{
		/// <summary>
		/// Populate the specified array with the given value.
		/// </summary>
		/// <param name="array">Array.</param>
		/// <param name="value">Value.</param>
		/// <typeparam name="T">The element type.</typeparam>
		public static void Populate<T>(this T[] array, T value)
		{
			for (int i = 0; i < array.Length; ++i)
			{
				array[i] = value;
			}
		}

		/// <summary>
		/// Scrolls the index of the array to wrap on ends.
		/// </summary>
		/// <returns>The array index.</returns>
		/// <param name="currentIndex">Current index.</param>
		/// <param name="length">Length of the array.</param>
		/// <param name="scrollAmount">Scroll amount.</param>
		public static int ScrollArrayIndex(int currentIndex, int length, int scrollAmount)
		{
			currentIndex += scrollAmount;
			if (currentIndex < 0)
			{
				while (currentIndex < 0)
				{
					currentIndex = (length) + currentIndex;
				}
			}
			else if (currentIndex > length - 1)
			{
				while (currentIndex > length - 1)
				{
					currentIndex -= (length);
				}
			}
			return currentIndex;
		}
	}
}
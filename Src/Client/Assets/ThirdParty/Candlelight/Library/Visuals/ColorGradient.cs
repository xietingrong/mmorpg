// 
// ColorGradient.cs
// 
// Copyright (c) 2011-2015, Candlelight Interactive, LLC
// All rights reserved.
// 
// This file is licensed according to the terms of the Unity Asset Store EULA:
// http://download.unity3d.com/assetstore/customer-eula.pdf

using UnityEngine;

namespace Candlelight
{

	/// <summary>
	/// An enum to specify how intermediate color values should be calculated.
	/// </summary>
	public enum ColorInterpolationSpace
	{
		/// <summary>
		/// Interpolate each color channel (red, blue, and green).
		/// </summary>
		RGB,
		/// <summary>
		/// Interpolate colors along hue, saturation, and value.
		/// </summary>
		HSV
	}

	/// <summary>
	/// A linear gradient between two <see cref="UnityEngine.Color"/>s.
	/// </summary>
	[System.Serializable]
	public struct ColorGradient : IPropertyBackingFieldCompatible<ColorGradient>
	{
		#region Backing Fields
		[SerializeField]
		private readonly Color m_MaxColor;
		[SerializeField]
		private readonly Color m_MinColor;
		[SerializeField]
		private readonly ColorInterpolationSpace m_InterpolationSpace;
		#endregion
		/// <summary>
		/// Gets the end color.
		/// </summary>
		/// <value>The end color.</value>
		public Color MaxColor { get { return m_MaxColor; } }
		/// <summary>
		/// Gets the start color.
		/// </summary>
		/// <value>The start color.</value>
		public Color MinColor { get { return m_MinColor; } }
		/// <summary>
		/// Gets the interpolation space.
		/// </summary>
		/// <value>The interpolation space.</value>
		public ColorInterpolationSpace InterpolationSpace { get { return m_InterpolationSpace; } }
		
		/// <summary>
		/// Initializes a new instance of the <see cref="ColorGradient"/> struct.
		/// </summary>
		/// <param name="minColor">Minimum color.</param>
		/// <param name="maxColor">Maximum color.</param>
		/// <param name="interpolationSpace">Interpolation space.</param>
		public ColorGradient(
			Color minColor, Color maxColor,
			ColorInterpolationSpace interpolationSpace = ColorInterpolationSpace.RGB
		) : this()
		{	
			m_MinColor = minColor;
			m_MaxColor = maxColor;
			m_InterpolationSpace = interpolationSpace;
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
		/// <see cref="ColorGradient"/>.
		/// </summary>
		/// <param name="obj">
		/// The <see cref="System.Object"/> to compare with the current <see cref="ColorGradient"/>.
		/// </param>
		/// <returns>
		/// <see langword="true"/> if the specified <see cref="System.Object"/> is equal to the current
		/// <see cref="ColorGradient"/>; otherwise, <see langword="false"/>.
		/// </returns>
		public override bool Equals(object obj)
		{
			return ObjectX.Equals(ref this, obj);
		}

		/// <summary>
		/// Determines whether the specified <see cref="ColorGradient"/> is equal to the current
		/// <see cref="ColorGradient"/>.
		/// </summary>
		/// <param name="other">
		/// The <see cref="ColorGradient"/> to compare with the current <see cref="ColorGradient"/>.
		/// </param>
		/// <returns>
		/// <see langword="true"/> if the specified <see cref="ColorGradient"/> is equal to the current
		/// <see cref="ColorGradient"/>; otherwise, <see langword="false"/>.
		/// </returns>
		public bool Equals(ColorGradient other)
		{
			return GetHashCode() == other.GetHashCode();
		}

		/// <summary>
		/// Evaluate the color at the specified parameter value.
		/// </summary>
		/// <returns>
		/// The interpolated <see cref="UnityEngine.Color"/> at the specified parameter in this instance.
		/// </returns>
		/// <param name="t">A parameter value in the range [0, 1].</param>
		public Color Evaluate(float t)
		{
			return (this.InterpolationSpace == ColorInterpolationSpace.RGB) ?
				Color.Lerp(this.MinColor, this.MaxColor, t) :
				ColorHSV.Lerp(this.MinColor, this.MaxColor, t);
		}

		/// <summary>
		/// Serves as a hash function for a <see cref="ColorGradient"/> object.
		/// </summary>
		/// <returns>
		/// A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a
		/// hash table.
		/// </returns>
		public override int GetHashCode()
		{
			return ObjectX.GenerateHashCode(
				m_MinColor.GetHashCode(), m_MaxColor.GetHashCode(), m_InterpolationSpace.GetHashCode()
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

		/// <summary>
		/// Gets a value indicating whether or not the two <see cref="ColorGradient"/>s are equal to one another.
		/// </summary>
		/// <returns>
		/// <see langword="true"/> if the two <see cref="ColorGradient"/>s are equal; otherwise,
		/// <see langword="false"/>.
		/// </returns>
		/// <param name="cg1">The first <see cref="ColorGradient"/>.</param>
		/// <param name="cg2">The second <see cref="ColorGradient"/>.</param>
		public static bool operator ==(ColorGradient cg1, ColorGradient cg2)
		{
			return cg1.GetHashCode() == cg2.GetHashCode(); 
		}

		/// <summary>
		/// Gets a value indicating whether or not the two <see cref="ColorGradient"/>s are unequal to one another.
		/// </summary>
		/// <returns>
		/// <see langword="true"/> if the two <see cref="ColorGradient"/>s are unequal; otherwise,
		/// <see langword="false"/>.
		/// </returns>
		/// <param name="cg1">The first <see cref="ColorGradient"/>.</param>
		/// <param name="cg2">The second <see cref="ColorGradient"/>.</param>
		public static bool operator !=(ColorGradient cg1, ColorGradient cg2)
		{
			return !(cg1 == cg2);
		}
	}
}
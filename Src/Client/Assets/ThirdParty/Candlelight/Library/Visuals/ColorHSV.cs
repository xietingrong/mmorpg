// 
// ColorHSV.cs
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
	/// A color defined in HSV space.
	/// </summary>
	[System.Serializable]
	public struct ColorHSV : IPropertyBackingFieldCompatible<ColorHSV>
	{
		/// <summary>
		/// Linearly interpolate the two specified <see cref="ColorHSV"/>s by the specified amount.
		/// </summary>
		/// <returns>A <see cref="ColorHSV"/> interpolated between the two specified values.</returns>
		/// <param name="color1">The first <see cref="ColorHSV"/>.</param>
		/// <param name="color2">The second <see cref="ColorHSV"/>.</param>
		/// <param name="t">A normalized parameter value in the range [0, 1].</param>
		static ColorHSV Lerp(ColorHSV color1, ColorHSV color2, float t)
		{
			return new ColorHSV(
				Mathf.Lerp(color1.Hue, color2.Hue, t),
				Mathf.Lerp(color1.Saturation, color2.Saturation, t),
				Mathf.Lerp(color1.Value, color2.Value, t),
				Mathf.Lerp(color1.Alpha, color2.Alpha, t)
			);
		}

		/// <summary>
		/// Linearly interpolate the two specified <see cref="UnityEngine.Color"/>s by the specified amount in HSV
		/// space.
		/// </summary>
		/// <returns>A <see cref="UnityEngine.Color"/> interpolated between the two specified values.</returns>
		/// <param name="color1">The first <see cref="UnityEngine.Color"/>.</param>
		/// <param name="color2">The second <see cref="UnityEngine.Color"/>.</param>
		/// <param name="t">A normalized parameter value [0, 1].</param>
		public static Color Lerp(Color color1, Color color2, float t)
		{
			return Lerp(new ColorHSV(color1), new ColorHSV(color2), t).ToColor();
		}

		#region Backing Fields
		[SerializeField]
		private readonly float m_Hue;
		[SerializeField]
		private readonly float m_Saturation;
		[SerializeField]
		private readonly float m_Value;
		[SerializeField]
		private readonly float m_Alpha;
		#endregion
		/// <summary>
		/// Gets the hue.
		/// </summary>
		/// <value>The hue.</value>
		public float Hue { get { return m_Hue; } }
		/// <summary>
		/// Gets the saturation.
		/// </summary>
		/// <value>The saturation.</value>
		public float Saturation { get { return m_Saturation; } }
		/// <summary>
		/// Gets the luminance value.
		/// </summary>
		/// <value>The luminance value.</value>
		public float Value { get { return m_Value; } }
		/// <summary>
		/// Gets the alpha.
		/// </summary>
		/// <value>The alpha.</value>
		public float Alpha { get { return m_Alpha; } }
		
		/// <summary>
		/// Initializes a new instance of the <see cref="ColorHSV"/> struct.
		/// </summary>
		/// <param name="h">Hue.</param>
		/// <param name="s">Saturation.</param>
		/// <param name="v">Value.</param>
		/// <param name="a">Alpha.</param>
		public ColorHSV(float h, float s, float v, float a = 1f) : this()
		{
			m_Hue = Mathf.Clamp(h, 0f, 360f);
			m_Saturation = s;
			m_Value = v;
			m_Alpha = a;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ColorHSV"/> struct.
		/// </summary>
		/// <param name="color">A <see cref="UnityEngine.Color"/>.</param>
		public ColorHSV(Color color) : this()
		{
			float min = Mathf.Min(Mathf.Min(color.r, color.g), color.b);
			float max = Mathf.Max(Mathf.Max(color.r, color.g), color.b);
			float delta = max - min;
			m_Alpha = color.a;
			// value is the max color
			m_Value = max;
			// saturation is percent of max
			if (max > 0f)
			{
				m_Saturation = delta / max;
			}
			else
			{
				// all colors are zero, no saturation and hue is undefined
				m_Saturation = 0f;
				m_Hue = -1f;
				return;
			}
			// grayscale image if min and max are the same
			if (min == max)
			{
				m_Value = max;
				m_Saturation = 0f;
				m_Hue = -1f;
				return;
			}
			// hue depends which color is max (creates a rainbow effect)
			if (color.r == max) // between yellow & magenta
			{
				m_Hue = (color.g - color.b) / delta;
			}
			else if (color.g == max) // between cyan & yellow
			{
				m_Hue = 2f + (color.b - color.r) / delta;
			}
			else // between magenta & cyan
			{
				m_Hue = 4f + (color.r - color.g) / delta;
			}
			// turn hue into 0-360 degrees
			m_Hue *= 60f;
			if (m_Hue < 0f)
			{
				m_Hue += 360f;
			}
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
		/// Determines whether the specified <see cref="System.Object"/> is equal to the current <see cref="ColorHSV"/>.
		/// </summary>
		/// <param name="obj">
		/// The <see cref="System.Object"/> to compare with the current <see cref="ColorHSV"/>.
		/// </param>
		/// <returns>
		/// <see langword="true"/> if the specified <see cref="System.Object"/> is equal to the current
		/// <see cref="ColorHSV"/>; otherwise, <see langword="false"/>.
		/// </returns>
		public override bool Equals(object obj)
		{
			return ObjectX.Equals(ref this, obj);
		}

		/// <summary>
		/// Determines whether the specified <see cref="ColorHSV"/> is equal to the current <see cref="ColorHSV"/>.
		/// </summary>
		/// <param name="other">The <see cref="ColorHSV"/> to compare with the current <see cref="ColorHSV"/>.</param>
		/// <returns>
		/// <see langword="true"/> if the specified <see cref="ColorHSV"/> is equal to the current
		/// <see cref="ColorHSV"/>; otherwise, <see langword="false"/>.
		/// </returns>
		public bool Equals(ColorHSV other)
		{
			return GetHashCode() == other.GetHashCode();
		}

		/// <summary>
		/// Serves as a hash function for a <see cref="ColorHSV"/> object.
		/// </summary>
		/// <returns>
		/// A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a
		/// hash table.
		/// </returns>
		public override int GetHashCode()
		{
			return ObjectX.GenerateHashCode(
				m_Hue.GetHashCode(), m_Saturation.GetHashCode(), m_Value.GetHashCode(), m_Alpha.GetHashCode()
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
		/// Converts this instance to a <see cref="UnityEngine.Color"/>.
		/// </summary>
		/// <returns>A <see cref="UnityEngine.Color"/>.</returns>
		public Color ToColor()
		{
			// no saturation is grayscale
			if (this.Saturation == 0f)
			{
				return new Color(this.Value, this.Value, this.Value, this.Alpha);
			}
			// which chunk of the rainbow are we in?
			float sector = this.Hue * 0.016666666666667f; // 1/60;
			// split across the decimal (i.e. 3.87 into 3 and 0.87)
			int i = (int)Mathf.Floor(sector);
			float f = sector - i;
			float v = this.Value;
			float p = v * (1f - this.Saturation);
			float q = v * (1f - this.Saturation * f);
			float t = v * (1f - this.Saturation * (1f - f));
			// build rgb color
			Color color = new Color(0f, 0f, 0f, this.Alpha);
			switch(i)
			{
				case 0:
					color.r = v;
					color.g = t;
					color.b = p;
					break;
				case 1:
					color.r = q;
					color.g = v;
					color.b = p;
					break;
				case 2:
					color.r	 = p;
					color.g	 = v;
					color.b	 = t;
					break;
				case 3:
					color.r	 = p;
					color.g	 = q;
					color.b	 = v;
					break;
				case 4:
					color.r	 = t;
					color.g	 = p;
					color.b	 = v;
					break;
				default:
					color.r	 = v;
					color.g	 = p;
					color.b	 = q;
					break;
			}
			return color;
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the current <see cref="ColorHSV"/>.
		/// </summary>
		/// <returns>A <see cref="System.String"/> that represents the current <see cref="ColorHSV"/>.</returns>
		public override string ToString()
		{
			return string.Format(
				"[ColorHSV: Hue = {0:0.00}, Saturation = {1:0.00}, Value = {2:0.00}, Alpha = {3:0.00}",
				this.Hue, this.Saturation, this.Value, this.Alpha
			);
		}
	}
}
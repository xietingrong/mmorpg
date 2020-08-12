// 
// StatusPropertyAttribute.cs
// 
// Copyright (c) 2014-2015, Candlelight Interactive, LLC
// All rights reserved.
// 
// This file is licensed according to the terms of the Unity Asset Store EULA:
// http://download.unity3d.com/assetstore/customer-eula.pdf

using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using System.Reflection;
#endif

namespace Candlelight
{
	/// <summary>
	/// A <see cref="UnityEngine.PropertyAttribute"/> to specify that a field should display an in-line status icon with
	/// a tooltip.
	/// </summary>
	public class StatusPropertyAttribute : PropertyAttribute
	{
		/// <summary>
		/// Delegate for a status getter method.
		/// </summary>
		public delegate ValidationStatus GetStatusMethod(Object provider, object testValue, out string message);

		/// <summary>
		/// A basic tuple for storing a test value and a comparison value that indicates a bad result.
		/// </summary>
		public struct Comparison
		{
			/// <summary>
			/// Gets the result from <see cref="System.IComparable.CompareTo(object)"/> that indicates a bad value.
			/// </summary>
			/// <value>The bad comparison result.</value>
			public int BadComparisonResult { get; private set; }
			/// <summary>
			/// Gets the value against which to compare.
			/// </summary>
			/// <value>The value against which to compare.</value>
			public object TestValue { get; private set; }

			/// <summary>
			/// Initializes a new instance of the <see cref="StatusPropertyAttribute.Comparison"/> struct.
			/// </summary>
			/// <param name="badComparisonResult">
			/// The result from <see cref="System.IComparable.CompareTo(object)"/> that indicates a bad value.
			/// </param>
			/// <param name="testValue">The value against which to compare.</param>
			public Comparison(int badComparisonResult, object testValue) : this()
			{
				this.BadComparisonResult = Mathf.Clamp(badComparisonResult, -1, 1);
				this.TestValue = testValue;
			}
		}

#if UNITY_EDITOR
		/// <summary>
		/// The status method parameter types.
		/// </summary>
		private static readonly ReadOnlyCollection<Type> s_StatusMethodParamTypes =
			new ReadOnlyCollection<Type>(new [] { typeof(Object), typeof(object), typeof(string).MakeByRefType() });
#endif

		#region Backing Fields
		private readonly Comparison[] m_Comparisons;
		#endregion
		/// <summary>
		/// Gets the type of icon to display if the decorated field's status is bad.
		/// </summary>
		/// <value>The type of icon to display if they decorated field's status is bad.</value>
		public ValidationStatus BadStatusIcon { get; private set; }
		/// <summary>
		/// Gets the tooltip to display on the status icon if the decorated field's status is bad.
		/// </summary>
		/// <value>The tooltip to display on the status icon if the decorated field's status is bad.</value>
		public string BadStatusTooltip { get; private set; }
		/// <summary>
		/// Gets the method to validate the status of the decorated field.
		/// </summary>
		/// <value>The method to validate the status of the decorated field.</value>
		public GetStatusMethod ValidationMethod { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="StatusPropertyAttribute"/> class to display a persistent icon
		///  and tooltip.
		/// </summary>
		/// <param name="persistentIcon">Persistent icon.</param>
		/// <param name="iconTooltip">Icon tooltip.</param>
		/// <param name="fieldType"><see cref="System.Type"/> of the decorated field.</param>
		public StatusPropertyAttribute(ValidationStatus persistentIcon, string iconTooltip, Type fieldType)
		{
			this.BadStatusIcon = persistentIcon;
#if UNITY_EDITOR
			if (fieldType.IsClass) // NOTE: Type.IsClass unavailable in Metro
			{
				m_Comparisons = new [] { new Comparison(0, null), new Comparison(1, null) };
			}
			else
			{
				object defaultValue = Activator.CreateInstance(fieldType);
				m_Comparisons = new []
				{
					new Comparison(-1, defaultValue), new Comparison(0, defaultValue), new Comparison(1, defaultValue)
				};
			}
#endif
			this.BadStatusTooltip = GetBadStatusTooltip(iconTooltip);
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="StatusPropertyAttribute"/> class to test for null values.
		/// </summary>
		/// <param name="badStatusIcon">The icon to use if the decorated field is <see langword="null"/>.</param>
		/// <param name="badStatusTooltip">The tooltip to use if the decorated field is <see langword="null"/>.</param>
		public StatusPropertyAttribute(
			ValidationStatus badStatusIcon = ValidationStatus.Error, string badStatusTooltip = null
		)
		{
			this.BadStatusIcon = badStatusIcon;
			m_Comparisons = new [] { new Comparison(0, null) };
			this.BadStatusTooltip = GetBadStatusTooltip(badStatusTooltip);
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="StatusPropertyAttribute"/> class.
		/// </summary>
		/// <param name="badStatusIcon">The icon to use if the decorated field's status is bad.</param>
		/// <param name="badStatusTooltip">The tooltip to use if the decorated field's status is bad.</param>
		/// <param name="badComparisonResult">
		/// A value in the range [-1, 1] that indicates a failed result of
		/// <see cref="System.IComparable.CompareTo(object)"/> when comparing the decorated field's value to testValue.
		/// For example -1 means the field is bad when its value is less than testValue.
		/// </param>
		/// <param name="testValue">Test value.</param>
		public StatusPropertyAttribute(
			ValidationStatus badStatusIcon, string badStatusTooltip, int badComparisonResult, object testValue
		)
		{
			this.BadStatusIcon = badStatusIcon;
			m_Comparisons = new [] { new Comparison(badComparisonResult, testValue) };
			this.BadStatusTooltip = GetBadStatusTooltip(badStatusTooltip);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="StatusPropertyAttribute"/> class.
		/// </summary>
		/// <param name="badStatusIcon">The icon to use if the decorated field's status is bad.</param>
		/// <param name="badStatusTooltip">The tooltip to use if the decorated field's status is bad.</param>
		/// <param name="comparisonTuples">
		/// An even-length array of alternating <see cref="System.Int32"/>/<see cref="System.Object"/> pairs
		/// corresponding to bad comparison results and test values. In each case, the integer must be in the range
		/// [-1, 1] and indicates a failed result of <see cref="System.IComparable.CompareTo(object)"/> when comparing
		/// the field's value to the corresponding test value. For example -1 means the field is bad when its value is
		/// less than the test value.
		/// </param>
		public StatusPropertyAttribute(
			ValidationStatus badStatusIcon, string badStatusTooltip, params object[] comparisonTuples
		)
		{
			this.BadStatusIcon = badStatusIcon;
			if (comparisonTuples.Length % 2 != 0)
			{
				throw new ArgumentException(
					"You must supply an even number of parameters in the form int, object, int, object...",
					"comparisonTuples"
                );
			}
			m_Comparisons = new Comparison[comparisonTuples.Length];
			for (int i = 0; i < comparisonTuples.Length; i += 2)
			{
				m_Comparisons[i / 2] = new Comparison((int)comparisonTuples[i], comparisonTuples[i + 1]);
			}
			this.BadStatusTooltip = GetBadStatusTooltip(badStatusTooltip);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="StatusPropertyAttribute"/> class that specifies a static method
		/// to get the status of the decorated field.
		/// </summary>
		/// <param name="providerType">The provider type of the validation method.</param>
		/// <param name="methodName">
		/// Name of a static validation method with the signature:
		/// <see cref="ValidationStatus"/> (<see cref="System.Object"/> provider, <see cref="System.Object"/> testValue, out <see cref="System.String"/> message).
		/// </param>
		public StatusPropertyAttribute(Type providerType, string methodName)
		{
#if UNITY_EDITOR
			MethodInfo method = providerType.GetMethod(
				methodName,
				BindingFlags.FlattenHierarchy | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public,
				null,
				s_StatusMethodParamTypes.ToArray<Type>(),
				null
			);
			if (method == null)
			{
				throw new NullReferenceException(
					string.Format(
						"Unable to find static method {0} {1}.{2}({3})",
						typeof(ValidationStatus),
						providerType,
						methodName,
						", ".Join(from t in s_StatusMethodParamTypes select t.ToString())
					)
				);
			}
			else
			{
				this.ValidationMethod = Delegate.CreateDelegate(typeof(GetStatusMethod), method) as GetStatusMethod;
			}
#endif
		}

		/// <summary>
		/// Creates a bad status tooltip from the comparisons if it is null or empty.
		/// </summary>
		/// <param name="tooltip">Tooltip.</param>
		private string GetBadStatusTooltip(string tooltip)
		{
			if (string.IsNullOrEmpty(tooltip))
			{
				switch (this.BadStatusIcon)
				{
				case ValidationStatus.Error:
					System.Text.StringBuilder sb = new System.Text.StringBuilder();
					foreach (Comparison cmp in m_Comparisons)
					{
						sb.AppendFormat(
							", {0} {1}",
							cmp.BadComparisonResult < 0 ?
								"less than" :
								cmp.BadComparisonResult > 0 ?
									"greater than" : "equal to",
								cmp.TestValue == null ? "null" : cmp.TestValue
						);
					}
					return string.Format("This field cannot be {0}.", sb.ToString().Substring(2).TrimEnd(' '));
				case ValidationStatus.Info:
				case ValidationStatus.Warning:
					return "Please check the value of this field.";
				default:
					return string.Empty;
				}
			}
			else
			{
				return tooltip;
			}
		}

		/// <summary>
		/// Gets the comparisons defined on this <see cref="StatusPropertyAttribute"/>.
		/// </summary>
		/// <returns>The comparisons defined on this <see cref="StatusPropertyAttribute"/>.</returns>
		public IEnumerable<Comparison> GetComparisons()
		{
			return m_Comparisons;
		}
	}
}
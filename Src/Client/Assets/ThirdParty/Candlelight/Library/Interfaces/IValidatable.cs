// 
// IValidatable.cs
// 
// Copyright (c) 2014-2015, Candlelight Interactive, LLC
// All rights reserved.
// 
// This file is licensed according to the terms of the Unity Asset Store EULA:
// http://download.unity3d.com/assetstore/customer-eula.pdf

namespace Candlelight
{
	/// <summary>
	/// Different possible status codes for <see cref="IValidatable"/> items.
	/// </summary>
	public enum ValidationStatus
	{
		/// <summary>
		/// No status specified.
		/// </summary>
		None,
		/// <summary>
		/// The item being validated is okay.
		/// </summary>
		Okay,
		/// <summary>
		/// The item being validated is okay, but also provides information.
		/// </summary>
		Info,
		/// <summary>
		/// The item being validated might be okay, but it could have a problem or side-effect.
		/// </summary>
		Warning,
		/// <summary>
		/// The item being validated has some error.
		/// </summary>
		Error
	}

	/// <summary>
	/// An interface to specify an object can have its validity tested.
	/// </summary>
	public interface IValidatable
	{
		/// <summary>
		/// Gets the validation status.
		/// </summary>
		/// <value>The validation status.</value>
		ValidationStatus ValidationStatus { get; }

		/// <summary>
		/// Gets the validation status.
		/// </summary>
		/// <returns>The validation status.</returns>
		/// <param name="message">Message.</param>
		ValidationStatus GetValidationStatus(out string message);
	}
}
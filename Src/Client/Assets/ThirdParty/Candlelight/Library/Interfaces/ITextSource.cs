// 
// ITextSource.cs
// 
// Copyright (c) 2015, Candlelight Interactive, LLC
// All rights reserved.
// 
// This file is licensed according to the terms of the Unity Asset Store EULA:
// http://download.unity3d.com/assetstore/customer-eula.pdf

namespace Candlelight
{
	/// <summary>
	/// An interface to specify an object is a source of text.
	/// </summary>
	public interface ITextSource
	{
		/// <summary>
		/// Gets a callback for whenever the text on this instance has changed.
		/// </summary>
		/// <value>A callback for whenever the text on this instance has changed.</value>
		UnityEngine.Events.UnityEvent OnBecameDirty { get; }
		/// <summary>
		/// Gets the output text.
		/// </summary>
		/// <value>The output text.</value>
		string OutputText { get; }
	}
}
// 
// FlushChildrenAttribute.cs
// 
// Copyright (c) 2014-2015, Candlelight Interactive, LLC
// All rights reserved.
// 
// This file is licensed according to the terms of the Unity Asset Store EULA:
// http://download.unity3d.com/assetstore/customer-eula.pdf

namespace Candlelight
{
	/// <summary>
	/// A <see cref="UnityEngine.PropertyAttribute"/> to specify that a field should have its children displayed at the
	/// current indent level.
	/// </summary>
	public class FlushChildrenAttribute : UnityEngine.PropertyAttribute
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="FlushChildrenAttribute"/> class.
		/// </summary>
		public FlushChildrenAttribute() {}
	}
}
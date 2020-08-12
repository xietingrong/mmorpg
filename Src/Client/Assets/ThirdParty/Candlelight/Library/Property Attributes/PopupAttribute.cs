// 
// PopupAttribute.cs
// 
// Copyright (c) 2015, Candlelight Interactive, LLC
// All rights reserved.
// 
// This file is licensed according to the terms of the Unity Asset Store EULA:
// http://download.unity3d.com/assetstore/customer-eula.pdf

namespace Candlelight
{
	/// <summary>
	/// A custom attribute for specifying that a field should display a popup.
	/// </summary>
	public class PopupAttribute : UnityEngine.PropertyAttribute
	{
		/// <summary>
		/// Gets the popup contents getter.
		/// </summary>
		/// <value>The popup contents getter.</value>
		public string PopupContentsGetter { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="PopupAttribute"/> class.
		/// </summary>
		/// <param name="popupContentsGetter">
		/// Name of the method for getting the current index, labels, and values with signature: <c> <see cref="int"/>
		/// (List&lt;<see cref="UnityEngine.GUIContent"/>&gt; labels, List&lt;<see cref="object"/>&gt; values)</c>.
		/// </param>
		public PopupAttribute(string popupContentsGetter)
		{
			this.PopupContentsGetter = popupContentsGetter;
		}
	}
}
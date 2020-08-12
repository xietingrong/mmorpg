// 
// ISceneGUIContext.cs
// 
// Copyright (c) 2015, Candlelight Interactive, LLC
// All rights reserved.
// 
// This file is licensed according to the terms of the Unity Asset Store EULA:
// http://download.unity3d.com/assetstore/customer-eula.pdf
// 
// This file contains an interface for editors that create scene GUI overlays.

using UnityEditor;
using UnityEngine;

namespace Candlelight
{
	/// <summary>
	/// An interface for custom editors that can create a context for scene GUI overlay.
	/// </summary>
	public interface ISceneGUIContext
	{
		/// <summary>
		/// Gets the first target. This should be a value cached in OnEnable(), as invoking Editor.targets inside of the
		/// OnSceneGUI() callback logs an error message.
		/// </summary>
		/// <value>The first target.</value>
		Object FirstTarget { get; }
		/// <summary>
		/// The Editor calling SceneGUI.Display().
		/// </summary>
		/// <value>The Editor calling SceneGUI.Display().</value>
		Editor SceneGUIContext { get; }
	}
}
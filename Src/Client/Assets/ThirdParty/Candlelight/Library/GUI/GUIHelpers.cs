// 
// GUIHelpers.cs
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
	/// GUI anchor.
	/// </summary>
	public enum GUIAnchor
	{
		/// <summary>
		/// Flag to anchor to the lower left.
		/// </summary>
		LowerLeft,
		/// <summary>
		/// Flag to anchor to the lower right.
		/// </summary>
		LowerRight,
		/// <summary>
		/// Flag to anchor to the upper left.
		/// </summary>
		TopLeft,
		/// <summary>
		/// Flat to anchor to the upper right.
		/// </summary>
		TopRight
	}

	/// <summary>
	/// Mouse state.
	/// </summary>
	public enum MouseState
	{
		/// <summary>
		/// A state of no interest.
		/// </summary>
		None,
		/// <summary>
		/// The left button was just pressed down.
		/// </summary>
		LeftButtonDown,
		/// <summary>
		/// The left button is being held.
		/// </summary>
		LeftButtonHeld,
		/// <summary>
		/// The right button was just pressed down.
		/// </summary>
		RightButtonDown,
		/// <summary>
		/// The right button is being held.
		/// </summary>
		RightButtonHeld
	}

	/// <summary>
	/// GUI helpers.
	/// </summary>
	public static class GUIHelpers
	{		
		/// <summary>
		/// Gets the current state of the mouse.
		/// </summary>
		/// <returns>The current mouse state.</returns>
		public static MouseState GetCurrentMouseState()
		{
			if (Event.current.type == EventType.MouseDown)
			{
				if (Event.current.button == 0)
				{
					return MouseState.LeftButtonDown;
				}
				else if (Event.current.button == 1)
				{
					return MouseState.RightButtonDown;
				}
			}
			else if (Event.current.type == EventType.MouseDrag)
			{
				if (Event.current.button == 0)
				{
					return MouseState.LeftButtonHeld;
				}
				else if (Event.current.button == 1)
				{
					return MouseState.RightButtonHeld;
				}
			}
			return MouseState.None;
		}
	}
}
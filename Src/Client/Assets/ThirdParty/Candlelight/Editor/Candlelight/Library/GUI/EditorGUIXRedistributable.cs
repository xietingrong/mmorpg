// 
// EditorGUIXRedistributable.cs
// 
// Copyright (c) 2012-2015, Candlelight Interactive, LLC
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
// 
// 1. Redistributions of source code must retain the above copyright notice,
// this list of conditions and the following disclaimer.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
// POSSIBILITY OF SUCH DAMAGE.
// 
// This file contains a redistributable part of a static class for working
// with editor GUI.

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace Candlelight
{
	/// <summary>
	/// Editor GUI extensions.
	/// </summary>
	public static partial class EditorGUIX
	{
		/// <summary>
		/// Gets the height of an inline button.
		/// </summary>
		/// <value>The height of an inline button.</value>
		public static float InlineButtonHeight
		{
			get { return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing; }
		}
		/// <summary>
		/// Gets the tinted color of the GUI based on play mode state.
		/// </summary>
		/// <value>The tinted color of the GUI based on play mode state.</value>
		private static Color TintedGUIColor
		{
			get { return GUI.color * (Application.isPlaying ? new Color(0.85f, 0.85f, 0.85f, 1f) : Color.white); }
		}

		/// <summary>
		/// Create a button in the editor GUI layout.
		/// </summary>
		/// <returns><see langword="true"/> if the button is pressed; otherwise, <see langword="false"/>.</returns>
		/// <param name="label">Label.</param>
		/// <param name="style">Optional style override.</param>
		/// <param name="isActive">True if the button should display the active state.</param>
		public static bool DisplayButton(string label, GUIStyle style = null, bool isActive = false)
		{
			return DisplayButton(new GUIContent(label), style, isActive);
		}
		
		/// <summary>
		/// Create a button in the editor GUI layout.
		/// </summary>
		/// <returns><see langword="true"/> if the button is pressed; otherwise, <see langword="false"/>.</returns>
		/// <param name="label">Label.</param>
		/// <param name="style">Optional style override.</param>
		/// <param name="isActive">True if the button should display the active state.</param>
		public static bool DisplayButton(GUIContent label, GUIStyle style = null, bool isActive = false)
		{
			Rect position =
				GUILayoutUtility.GetRect(0f, InlineButtonHeight + EditorGUIUtility.standardVerticalSpacing);
			position = EditorGUI.IndentedRect(position);
			position.height -= EditorGUIUtility.standardVerticalSpacing;
			return DisplayButton(position, label, style, isActive);
		}
		
		/// <summary>
		/// Create a button in the editor GUI.
		/// </summary>
		/// <returns><see langword="true"/> if the button is pressed; otherwise, <see langword="false"/>.</returns>
		/// <param name="position">Position.</param>
		/// <param name="label">Label.</param>
		/// <param name="style">Optional style override.</param>
		/// <param name="isActive">True if the button should display the active state.</param>
		public static bool DisplayButton(Rect position, string label, GUIStyle style = null, bool isActive = false)
		{
			return DisplayButton(position, new GUIContent(label), style, isActive);
		}
		
		/// <summary>
		/// Create a button in the editor GUI.
		/// </summary>
		/// <returns><see langword="true"/> if the button is pressed; otherwise, <see langword="false"/>.</returns>
		/// <param name="position">Position.</param>
		/// <param name="label">Label.</param>
		/// <param name="style">Optional style override.</param>
		/// <param name="isActive">True if the button should display the active state.</param>
		public static bool DisplayButton(Rect position, GUIContent label, GUIStyle style = null, bool isActive = false)
		{
			
			Color oldColor = GUI.color;
			GUI.color = TintedGUIColor;
			bool result = DisplayEditorButton(position, label, style, isActive);
			GUI.color = oldColor;
			return result;
		}

		/// <summary>
		/// Displays an editor button.
		/// </summary>
		/// <returns><see langword="true"/> if editor button is pressed; otherwise, <see langword="false"/>.</returns>
		/// <param name="position">Position.</param>
		/// <param name="label">Label.</param>
		/// <param name="style">Options_InlineButtonHeight</param>
		/// <param name="isActive">True if the button should display the active state.</param>
		private static bool DisplayEditorButton(
			Rect position, GUIContent label, GUIStyle style, bool isActive
		)
		{
			int controlID = GUIUtility.GetControlID(label.text.GetHashCode(), FocusType.Passive, position);
			Event current = Event.current;
			if (
				GUIUtility.keyboardControl == controlID &&
				current.type == EventType.KeyDown && (
					current.keyCode == KeyCode.Space ||
					current.keyCode == KeyCode.KeypadEnter ||
					current.keyCode == KeyCode.Return
				)
			)
			{
				current.Use();
				GUI.changed = true;
			}
			if (
				current.type == EventType.MouseDown &&
				Event.current.button == 0 &&
				position.Contains(Event.current.mousePosition)
			)
			{
				GUIUtility.keyboardControl = controlID;
				EditorGUIUtility.editingTextField = false;
				HandleUtility.Repaint();
			}
			EditorGUI.BeginChangeCheck();
			GUI.Toggle(position, controlID, isActive, label, style == null ? EditorStyles.miniButton : style);
			return EditorGUI.EndChangeCheck();
		}

		/// <summary>
		/// Displays a horizontal line in the current layout.
		/// </summary>
		public static void DisplayHorizontalLine()
		{
			GUI.Box(EditorGUI.IndentedRect(EditorGUILayout.GetControlRect(true, 2f)), GUIContent.none);
		}

		/// <summary>
		/// Displays a selection grid in the current layout.
		/// </summary>
		/// <returns>The currently selected index.</returns>
		/// <param name="currentIndex">The currently selected index.</param>
		/// <param name="labels">Labels.</param>
		/// <param name="xCount">Number of buttons in each grid row.</param>
		/// <param name="style">Optional style override.</param>
		public static int DisplaySelectionGrid(
			int currentIndex, GUIContent[] labels, int xCount = 0, GUIStyle style = null
		)
		{
			xCount = Mathf.Min(xCount < 1 ? labels.Length : xCount, labels.Length);
			int numRows = xCount > 0 ? labels.Length / xCount + (labels.Length % xCount == 0 ? 0 : 1) : 1;
			float ctrlHeight =
				style == null || style.fixedHeight == 0f ? EditorGUIUtility.singleLineHeight : style.fixedHeight;
			float ctrlSpacing = style == null ? EditorGUIUtility.standardVerticalSpacing : style.margin.vertical;
			Rect position =
				EditorGUI.IndentedRect(GUILayoutUtility.GetRect(0f, (ctrlHeight + ctrlSpacing) * numRows));
			return DisplaySelectionGrid(position, currentIndex, labels, xCount, style);
		}

		/// <summary>
		/// Displays a selection grid.
		/// </summary>
		/// <returns>The currently selected index.</returns>
		/// <param name="position">Position.</param>
		/// <param name="currentIndex">The currently selected index.</param>
		/// <param name="labels">Labels.</param>
		/// <param name="xCount">Number of buttons in each grid row.</param>
		/// <param name="style">Optional style override.</param>
		public static int DisplaySelectionGrid(
			Rect position, int currentIndex, GUIContent[] labels, int xCount = 0, GUIStyle style = null
		)
		{
			xCount = Mathf.Min(xCount < 1 ? labels.Length : xCount, labels.Length);
			Color oldColor = GUI.color;
			GUI.color = TintedGUIColor;
			int numRows = xCount > 0 ? labels.Length / xCount + (labels.Length % xCount == 0 ? 0 : 1) : 1;
			float ctrlHeight =
				style == null || style.fixedHeight == 0f ? EditorGUIUtility.singleLineHeight : style.fixedHeight;
			float ctrlSpacing = style == null ? EditorGUIUtility.standardVerticalSpacing : style.margin.vertical;
			position.height = ctrlHeight;
			position.width /= xCount;
			Vector2 anchor = new Vector2(position.x, position.y);
			for (int row = 0; row < numRows; ++row)
			{
				position.y = anchor.y + (position.height + ctrlSpacing) * row;
				for (int col = 0; col < xCount; ++col)
				{
					position.x = anchor.x + position.width * col;
					int i = row * xCount + col;
					if (i < labels.Length && DisplayButton(position, labels[i], style, i == currentIndex))
				    {
						currentIndex = i;
					}
				}
			}
			GUI.color = oldColor;
			return currentIndex;
		}
		
		/// <summary>
		/// Displays a tab group.
		/// </summary>
		/// <returns>The current tab index.</returns>
		/// <param name="currentTab">Current tab index.</param>
		/// <param name="tabs">Tab labels.</param>
		/// <param name="tabContents">GUI callbacks to invoke for each tab.</param>
		/// <param name="xCount">Number of tabs to draw in each row.</param>
		public static int DisplayTabGroup(
			int currentTab, GUIContent[] tabs, Dictionary<int, System.Action> tabContents, int xCount = 0
		)
		{
			currentTab = DisplaySelectionGrid(currentTab, tabs, xCount, EditorStylesX.BrightTab);
			int indent = (int)(EditorGUI.IndentedRect(new Rect(0, 0, 1, 1)).x * 1.6f);
			EditorStylesX.TabAreaBackground.margin.left += indent;
			EditorGUILayout.BeginVertical(EditorStylesX.TabAreaBackground);
			{
				EditorStylesX.TabAreaBackground.margin.left -= indent;
				EditorGUILayout.Separator();
				int oldIndent = EditorGUI.indentLevel;
				EditorGUI.indentLevel = 0;
				if (tabContents.ContainsKey(currentTab) && tabContents[currentTab] != null)
				{
					tabContents[currentTab].Invoke();
				}
				else
				{
					EditorGUILayout.HelpBox(
						string.Format("No draw method supplied for tab {0}", currentTab), MessageType.Error
					);
				}
				EditorGUI.indentLevel = oldIndent;
			}
			EditorGUILayout.EndVertical();
			return currentTab;
		}
	}
}
// 
// EditorGUIX.cs
// 
// Copyright (c) 2012-2015, Candlelight Interactive, LLC
// All rights reserved.
// 
// This file is licensed according to the terms of the Unity Asset Store EULA:
// http://download.unity3d.com/assetstore/customer-eula.pdf
// 
// This file contains a non-redistributable part of a static class for working
// with editor GUI.

using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text.RegularExpressions;

namespace Candlelight
{
	/// <summary>
	/// Editor GUI extensions.
	/// </summary>
	public static partial class EditorGUIX
	{
		/// <summary>
		/// The margin between controls.
		/// </summary>
		public static readonly RectOffset controlMargin = new RectOffset(
			4, 4, (int)EditorGUIUtility.standardVerticalSpacing, (int)EditorGUIUtility.standardVerticalSpacing
		);
		/// <summary>
		/// The label to use for handle size sliders.
		/// </summary>
		private static readonly GUIContent s_HandleSizeLabel = new GUIContent("Size");
		/// <summary>
		/// The pixels per indent level.
		/// </summary>
		public static readonly float pixelsPerIndentLevel = 15f;
		/// <summary>
		/// The standard horizontal spacing.
		/// </summary>
		public static readonly float standardHorizontalSpacing = controlMargin.GetHorizontalAverage();
		/// <summary>
		/// Unity's x handle color.
		/// </summary>
		public static readonly Color xHandleColor =
			GetColorFromUnityPreferences("Scene/X Axis", new Color(0.95f, 0.28f, 0.137f, 1f));
		/// <summary>
		/// Unity's y handle color.
		/// </summary>
		public static readonly Color yHandleColor =
			GetColorFromUnityPreferences("Scene/Y Axis", new Color(0.733f, 0.95f, 0.337f, 1f));
		/// <summary>
		/// Unity's z handle color.
		/// </summary>
		public static readonly Color zHandleColor =
			GetColorFromUnityPreferences("Scene/Z Axis", new Color(0.255f, 0.553f, 0.95f, 1f));
		/// <summary>
		/// An empty icon to use for status fields with no status.
		/// </summary>
		private static Texture2D s_NoStatusIcon = null;
		/// <summary>
		/// The on/off labels.
		/// </summary>
		private static readonly GUIContent[] s_OnOffLabels =
			new GUIContent[] { new GUIContent("Off"), new GUIContent("On") };
		/// <summary>
		/// The slider hash.
		/// </summary>
		private static readonly int s_SliderHash =
			(int)typeof(EditorGUI).GetField("s_SliderHash", ReflectionX.staticBindingFlags).GetValue(null);

		#region MemberInfo
		private static readonly MethodInfo s_GetClosestPowerOfTen =
			typeof(MathUtils).GetMethod("GetClosestPowerOfTen", ReflectionX.staticBindingFlags);
		private static readonly MethodInfo s_RoundBasedOnMinimumDifference = typeof(MathUtils).GetMethod(
			"RoundBasedOnMinimumDifference",
			ReflectionX.staticBindingFlags,
			null,
			new System.Type[] { typeof(float), typeof(float) },
			null
		);
		private static readonly MethodInfo s_RoundToMultipleOf =
			typeof(MathUtils).GetMethod("RoundToMultipleOf", ReflectionX.staticBindingFlags);
		#endregion
		#region Shared Allocations
		private static readonly List<SerializedProperty> s_AffectedObjects = new List<SerializedProperty>(32);
		private static Color s_OldColorCache;
		private static readonly object[] s_Param1 = new object[1];
		private static readonly object[] s_Param2 = new object[2];
		private static float s_SliderMin = 0f;
		private static float s_SliderMax = 5f;
		private static readonly List<Object> s_UndoObjects = new List<Object>(128);
		private static GUIContent s_ValidationStatusIcon = new GUIContent();
		#endregion

		#region Backing Fields
		private const float k_NarrowButtonWidth = 48f;
		private const float k_WideButtonWidth = 80f;
		#endregion
		/// <summary>
		/// The width of the input fields for min/max sliders.
		/// </summary>
		/// <value>The width of the input fields for min/max sliders.</value>
		private static float MinMaxFloatFieldWidth { get { return EditorGUIUtility.wideMode ? 48f : 32f; } }
		/// <summary>
		/// Gets the width of the narrow inline button.
		/// </summary>
		/// <value>The width of the narrow inline button.</value>
		public static float NarrowInlineButtonWidth { get { return k_NarrowButtonWidth; } }
		/// <summary>
		/// Gets the width of the wide inline button.
		/// </summary>
		/// <value>The width of the wide inline button.</value>
		public static float WideInlineButtonWidth { get { return k_WideButtonWidth; } }
		
		/// <summary>
		/// Begins the scene GUI controls area.
		/// </summary>
		/// <returns><see langword="true"/> if the scene GUI is enabled; otherwise, <see langword="false"/>.</returns>
		public static bool BeginSceneGUIControlsArea()
		{
			EditorGUILayout.BeginVertical(EditorStylesX.SceneGUIInspectorBackground);
			DisplaySceneGUIToggle();
			++EditorGUI.indentLevel;
			return SceneGUI.IsEnabled;
		}
		
		/// <summary>
		/// Create an array of buttons in the editor GUI layout.
		/// </summary>
		/// <returns>The index of the button pressed; otherwise, -1.</returns>
		/// <param name="labels">Labels.</param>
		/// <param name="buttonEnabledStates">Optional array to specify enabled states for buttons in the array.</param>
		public static int DisplayButtonArray(string[] labels, bool[] buttonEnabledStates = null)
		{
			GUIContent[] gcLabels = labels == null ? null : new GUIContent[labels.Length];
			if (labels != null)
			{
				for (int i=0; i<gcLabels.Length; ++i)
				{
					gcLabels[i] = new GUIContent(labels[i]);
				}
			}
			return DisplayButtonArray(gcLabels, buttonEnabledStates);
		}
		
		/// <summary>
		/// Create an array of buttons in the editor GUI layout.
		/// </summary>
		/// <returns>The index of the button pressed; otherwise, -1.</returns>
		/// <param name="labels">Labels.</param>
		/// <param name="buttonEnabledStates">Optional array to specify enabled states for buttons in the array.</param>
		public static int DisplayButtonArray(GUIContent[] labels, bool[] buttonEnabledStates = null)
		{
			return DisplayButtonArray(
				GUILayoutUtility.GetRect(0f, InlineButtonHeight + EditorGUIUtility.standardVerticalSpacing),
				labels,
				buttonEnabledStates
			);
		}

		/// <summary>
		/// Create an array of buttons
		/// </summary>
		/// <returns>The button array.</returns>
		/// <param name="position">Position.</param>
		/// <param name="labels">Labels.</param>
		/// <param name="buttonEnabledStates">Optional array to specify enabled states for buttons in the array.</param>
		public static int DisplayButtonArray(Rect position, GUIContent[] labels, bool[] buttonEnabledStates = null)
		{
			int result = -1;
			Color oldColor = GUI.color;
			if (labels == null || labels.Length == 0)
			{
				GUI.color = Color.red;
				EditorGUI.LabelField(position, "No button labels supplied.");
				GUI.color = oldColor;
				return result;
			}
			if (buttonEnabledStates == null)
			{
				buttonEnabledStates = new bool[labels.Length];
				buttonEnabledStates.Populate(true);
			}
			GUI.color = TintedGUIColor;
			position = EditorGUI.IndentedRect(position);
			position.height -= EditorGUIUtility.standardVerticalSpacing;
			position.width -= EditorGUIUtility.standardVerticalSpacing * 0.5f * (labels.Length - 1);
			position.width = position.width / labels.Length;
			for (int i = 0; i < labels.Length; ++i)
			{
				EditorGUI.BeginDisabledGroup(!buttonEnabledStates[i]);
				{
					if (DisplayEditorButton(position, labels[i], null, false))
					{
						result = i;
					}
				}
				EditorGUI.EndDisabledGroup();
				position.x += position.width + EditorGUIUtility.standardVerticalSpacing * 0.5f;
			}
			GUI.color = oldColor;
			return result;
		}

		/// <summary>
		/// Displays a selection grid for an enumerated type.
		/// </summary>
		/// <returns>The currently selected value.</returns>
		/// <param name="currentValue">The currently selected value.</param>
		/// <param name="labels">Labels.</param>
		/// <param name="xCount">Number of buttons in each grid row.</param>
		/// <param name="style">Optional style override.</param>
		/// <typeparam name="T">An enumerated type.</typeparam>
		public static T DisplayEnumSelectionGrid<T>(
			T currentValue, GUIContent[] labels = null, int xCount = 0, GUIStyle style = null
		) where T : struct, System.IComparable, System.IConvertible, System.IFormattable
		{
			if (!typeof(T).IsEnum) 
			{
				string message = "T must be an enumerated type";
				Debug.LogException(new System.ArgumentException(message, "T"));
				EditorGUILayout.HelpBox(message, MessageType.Error);
				return currentValue;
			}
			labels = labels ??
				(from name in System.Enum.GetNames(typeof(T)) select new GUIContent(name.ToWords())).ToArray();
			return (T)(object)DisplaySelectionGrid(
				System.Convert.ToInt32(currentValue), labels, xCount, style
			);
		}
		
		/// <summary>
		/// Displays a tab group for an enumerated type.
		/// </summary>
		/// <returns>The current tab.</returns>
		/// <param name="currentTab">Current tab.</param>
		/// <param name="tabContents">GUI callbacks to invoke for each tab.</param>
		/// <param name="labels">Labels.</param>
		/// <param name="xCount">Number of tabs to draw in each row.</param>
		/// <typeparam name="T">An enumerated type.</typeparam>
		public static T DisplayEnumTabGroup<T>(
			T currentTab, Dictionary<T, System.Action> tabContents, GUIContent[] labels = null, int xCount = 0
		) where T : struct, System.IComparable, System.IConvertible, System.IFormattable
		{
			if (!typeof(T).IsEnum) 
			{
				string message = "T must be an enumerated type";
				Debug.LogException(new System.ArgumentException(message, "T"));
				EditorGUILayout.HelpBox(message, MessageType.Error);
				return currentTab;
			}
			labels = labels ?? (
				from name in System.Enum.GetNames(typeof(T)) select new GUIContent(name.ToWords(), name.ToWords())
			).ToArray();
			Dictionary<int, System.Action> contents = new Dictionary<int, System.Action>();
			List<int> values = new List<int>((int[])System.Enum.GetValues(typeof(T)));
			foreach (KeyValuePair<T, System.Action> kv in tabContents)
			{
				contents.Add(values.IndexOf(System.Convert.ToInt32(kv.Key)), kv.Value);
			}
			return (T)(object)DisplayTabGroup(System.Convert.ToInt32(currentTab), labels, contents, xCount);
		}
		
		/// <summary>
		/// Displays a field using the current GUI skin, if a default skin is not being used.
		/// </summary>
		/// <param name="label">Label.</param>
		/// <param name="value">Value</param>
		/// <param name="drawMethod"></param>
		public static T DisplayField<T>(GUIContent label, T value, System.Func<Rect, GUIContent, T, T> drawMethod)
		{
			return DisplayField<T>(EditorGUILayout.GetControlRect(), label, value, drawMethod);
		}

		/// <summary>
		/// Displays a field using the current GUI skin, if a default skin is not being used.
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="label">Label.</param>
		/// <param name="value">Value</param>
		/// <param name="drawMethod"></param>
		public static T DisplayField<T>(
			Rect position, GUIContent label, T value, System.Func<Rect, GUIContent, T, T> drawMethod
		)
		{
			label = label ?? GUIContent.none;
#if !UNITY_4_6
			if (EditorStylesX.IsUsingBuiltinSkin)
			{
				EditorGUI.PrefixLabel(position, label);
			}
			else
			{
				EditorGUI.PrefixLabel(position, label, GUI.skin.label);
			}
#else
			EditorGUI.PrefixLabel(position, label);
#endif
			position.width -= EditorGUIUtility.labelWidth;
			position.x += EditorGUIUtility.labelWidth;
			int oldIndent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;
			value = drawMethod(position, GUIContent.none, value);
			EditorGUI.indentLevel = oldIndent;
			return value;
		}
		
		/// <summary>
		/// Displays a float slider using the current GUI skin, if a default skin is not being used.
		/// </summary>
		/// <param name="label">Label.</param>
		/// <param name="size">Size.</param>
		/// <param name="min">Minimum size.</param>
		/// <param name="max">Maximum size.</param>
		public static float DisplayFloatSlider(GUIContent label, float value, float min, float max)
		{
			s_SliderMin = min;
			s_SliderMax = max;
			return DisplayField<float>(label, value, OnFloatSliderField);
		}
		
		/// <summary>
		/// Displays a label field with a status icon.
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="label">Label.</param>
		/// <param name="text">Text.</param>
		/// <param name="status">Status.</param>
		/// <param name="statusTooltip">Status tooltip.</param>
		public static void DisplayLabelFieldWithStatus(
			Rect position, string label, string text, ValidationStatus status, Color tint, string statusTooltip = ""
		)
		{
			s_OldColorCache = GUI.color;
			GUI.color = tint;
			position.width -= EditorGUIUtility.singleLineHeight;
			EditorGUI.LabelField(position, label, text);
			GUI.color = s_OldColorCache;
			position.x += position.width;
			position.width = position.height = EditorGUIUtility.singleLineHeight;
			DisplayValidationStatusIcon(position, status, statusTooltip);
		}

		/// <summary>
		/// Displays a reorderable list in a box followed by an editor for the currently selected element. Use this
		/// method with lists of objects that are <see cref="UnityEditor.SerializedPropertyType.Generic"/>.
		/// </summary>
		/// <param name="list">List.</param>
		public static void DisplayListWithElementEditor(ReorderableList list)
		{
			EditorGUILayout.BeginVertical(EditorStylesX.Box);
			{
				Rect rect = EditorGUILayout.GetControlRect(false, list.GetHeight());
				list.DoList(rect);
				if (list.count > 0)
				{
					int selected = Mathf.Clamp(list.index, 0, list.serializedProperty.arraySize - 1);
					EditorGUILayout.Space();
					EditorGUILayout.PropertyField(list.serializedProperty.GetArrayElementAtIndex(selected), true);
				}
			}
			EditorGUILayout.EndVertical();
		}

		/// <summary>
		/// Displays the handle property editor.
		/// </summary>
		/// <param name="handleName">Handle name (e.g. "Vision").</param>
		/// <param name="toggle">Toggle.</param>
		/// <param name="size">Size.</param>
		/// <param name="minSize">Minimum size.</param>
		/// <param name="maxSize">Max size.</param>
		/// <typeparam name="TEditor">The 1st type parameter.</typeparam>
		public static void DisplayHandlePropertyEditor<TEditor>(
			string handleName,
			EditorPreference<bool, TEditor> toggle,
			EditorPreference<float, TEditor> size,
			float minSize = 0f,
			float maxSize = 5f
		)
		{
			EditorGUI.BeginChangeCheck();
			{
				toggle.CurrentValue = DisplayOnOffToggle(string.Format("{0} Handle", handleName), toggle.CurrentValue);
				if (toggle.CurrentValue)
				{
					++EditorGUI.indentLevel;
					size.CurrentValue = DisplayFloatSlider(s_HandleSizeLabel, size.CurrentValue, minSize, maxSize);
					--EditorGUI.indentLevel;
				}
			}
			if (EditorGUI.EndChangeCheck())
			{
				SceneView.RepaintAll();
			}
		}

		/// <summary>
		/// Displays the handle property editor.
		/// </summary>
		/// <param name="handleName">Handle name (e.g. "Vision").</param>
		/// <param name="toggle">Toggle.</param>
		/// <param name="color">Color.</param>
		/// <param name="size">Optional size preference.</param>
		/// <param name="minSize">Minimum size.</param>
		/// <param name="maxSize">Maximum size.</param>
		/// <typeparam name="TEditor">The editor type.</typeparam>
		public static void DisplayHandlePropertyEditor<TEditor>(
			string handleName,
			EditorPreference<bool, TEditor> toggle,
			EditorPreference<Color, TEditor> color,
			EditorPreference<float, TEditor> size = null,
			float minSize = 0f,
			float maxSize = 5f
		)
		{
			EditorGUI.BeginChangeCheck();
			{
				toggle.CurrentValue = DisplayOnOffToggle(string.Format("{0} Handle", handleName), toggle.CurrentValue);
				if (toggle.CurrentValue)
				{
					++EditorGUI.indentLevel;
					Color col = color.CurrentValue;
					try
					{
						col = EditorGUILayout.ColorField(col);
					}
					catch (ExitGUIException)
					{
						col = color.CurrentValue;
					}
					color.CurrentValue = col;
					if (size != null)
					{
						size.CurrentValue = DisplayFloatSlider(s_HandleSizeLabel, size.CurrentValue, minSize, maxSize);
					}
					--EditorGUI.indentLevel;
				}
			}
			if (EditorGUI.EndChangeCheck())
			{
				SceneView.RepaintAll();
			}
		}
		
		/// <summary>
		/// Displays the handle property editor.
		/// </summary>
		/// <param name="handleName">Handle name (e.g. "Vision").</param>
		/// <param name="toggle">Toggle.</param>
		/// <param name="color">Color.</param>
		/// <param name="size">Optional size preference.</param>
		/// <param name="minSize">Minimum size.</param>
		/// <param name="maxSize">Maximum size.</param>
		/// <typeparam name="TEditor">The editor type.</typeparam>
		public static void DisplayHandlePropertyEditor<TEditor>(
			string handleName,
			EditorPreference<bool, TEditor> toggle,
			EditorPreference<ColorGradient, TEditor> color,
			EditorPreference<float, TEditor> size = null,
			float minSize = 0f,
			float maxSize = 5f
		)
		{
			EditorGUI.BeginChangeCheck();
			{
				toggle.CurrentValue = DisplayOnOffToggle(string.Format("{0} Handle", handleName), toggle.CurrentValue);
				if (toggle.CurrentValue)
				{
					++EditorGUI.indentLevel;
					Color minColor = color.CurrentValue.MinColor;
					Color maxColor = color.CurrentValue.MaxColor;
					ColorInterpolationSpace interpolationSpace;
					Rect colorPickerPosition = EditorGUI.IndentedRect(EditorGUILayout.GetControlRect());
					int indent = EditorGUI.indentLevel;
					EditorGUI.indentLevel = 0;
					colorPickerPosition.width = (colorPickerPosition.width - standardHorizontalSpacing) * 0.5f;
					try
					{
						minColor = EditorGUI.ColorField(colorPickerPosition, minColor);
					}
					catch (ExitGUIException)
					{
						minColor = color.CurrentValue.MinColor;
					}
					colorPickerPosition.x += colorPickerPosition.width + standardHorizontalSpacing;
					try
					{
						maxColor = EditorGUI.ColorField(colorPickerPosition, maxColor);
					}
					catch (ExitGUIException)
					{
						maxColor = color.CurrentValue.MaxColor;
					}
					EditorGUI.indentLevel = indent;
					interpolationSpace = (ColorInterpolationSpace)EditorGUILayout.EnumPopup(
						"Interpolation", color.CurrentValue.InterpolationSpace
					);
					color.CurrentValue = new ColorGradient(minColor, maxColor, interpolationSpace);
					if (size != null)
					{
						size.CurrentValue = DisplayFloatSlider(s_HandleSizeLabel, size.CurrentValue, minSize, maxSize);
					}
					--EditorGUI.indentLevel;
				}
			}
			if (EditorGUI.EndChangeCheck())
			{
				SceneView.RepaintAll();
			}
		}

		/// <summary>
		/// Displays a min/max slider with input fields at either end.
		/// </summary>
		/// <param name="position">Control position.</param>
		/// <param name="label">Label.</param>
		/// <param name="minProp">Minimum value property.</param>
		/// <param name="maxProp">Maximum value property.</param>
		/// <param name="sliderMin">Minimum slider value.</param>
		/// <param name="sliderMax">Maximum slider value.</param>
		public static void DisplayMinMaxPropertiesWithSlider(
			Rect position,
			GUIContent label,
			SerializedProperty minProp,
			SerializedProperty maxProp,
			float sliderMin,
			float sliderMax
		)
		{
			Rect sliderPos = position;
			float indent = label == null || label == GUIContent.none ? 0f : EditorGUIUtility.labelWidth;
			sliderPos.x += indent + MinMaxFloatFieldWidth + standardHorizontalSpacing;
			sliderPos.width -= indent + 2f * (MinMaxFloatFieldWidth + standardHorizontalSpacing);
			Rect maxPos = sliderPos;
			maxPos.x += sliderPos.width + standardHorizontalSpacing;
			maxPos.width = MinMaxFloatFieldWidth;
			Rect minPos = position;
			minPos.width -= sliderPos.width + maxPos.width + 2f * standardHorizontalSpacing;
			bool drawSlider=  sliderPos.width > 32f;
			if (!drawSlider)
			{
				minPos.width = (position.width - standardHorizontalSpacing) * 0.5f;
				maxPos.x -= minPos.width - maxPos.width;
				maxPos.width = minPos.width;
			}
			EditorGUI.BeginChangeCheck();
			{
				EditorGUI.PropertyField(minPos, minProp, label);
			}
			if (EditorGUI.EndChangeCheck())
			{
				maxProp.floatValue = Mathf.Max(minProp.floatValue, maxProp.floatValue);
			}
			float max = maxProp.floatValue;
			float min = minProp.floatValue;
			int indentLevel = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;
			if (drawSlider)
			{
				EditorGUI.MinMaxSlider(
					sliderPos, GUIContent.none, ref min, ref max, Mathf.Min(sliderMin, min), Mathf.Max(sliderMax, max)
				);
			}
			if (maxProp.floatValue != max)
			{
				maxProp.floatValue = max;
				minProp.floatValue = Mathf.Min(min, max);
			}
			if (minProp.floatValue != min)
			{
				minProp.floatValue = min;
				maxProp.floatValue = Mathf.Max(min, max);
			}
			EditorGUI.BeginChangeCheck();
			{
				EditorGUI.PropertyField(maxPos, maxProp, GUIContent.none);
			}
			if (EditorGUI.EndChangeCheck())
			{
				minProp.floatValue = Mathf.Min(minProp.floatValue, maxProp.floatValue);
			}
			EditorGUI.indentLevel = indentLevel;
		}

		/// <summary>
		/// Displays the on/off toggle.
		/// </summary>
		/// <returns>The value of the on/off toggle.</returns>
		/// <param name="label">Label.</param>
		/// <param name="val">Value.</param>
		public static bool DisplayOnOffToggle(string label, bool val)
		{
			return DisplayOnOffToggle(new GUIContent(label), val);
		}

		/// <summary>
		/// Displays the on/off toggle.
		/// </summary>
		/// <returns>The value of the on/off toggle.</returns>
		/// <param name="label">Label.</param>
		/// <param name="val">Value.</param>
		public static bool DisplayOnOffToggle(GUIContent label, bool val)
		{
			Rect position = EditorGUILayout.GetControlRect(true);
			Rect controlPosition, buttonPosition;
			GetRectsForControlWithInlineButton(position, out controlPosition, out buttonPosition);
			float oldLabelWidth = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = controlPosition.width;
#if !UNITY_4_6
			if (EditorStylesX.IsUsingBuiltinSkin)
			{
				EditorGUI.PrefixLabel(controlPosition, label);
			}
			else
			{
				EditorGUI.PrefixLabel(controlPosition, label, GUI.skin.label);
			}
#else
			EditorGUI.PrefixLabel(controlPosition, label);
#endif
			EditorGUIUtility.labelWidth = oldLabelWidth;
			return DisplaySelectionGrid(buttonPosition, val ? 1 : 0, s_OnOffLabels, 2) == 1;
		}

		/// <summary>
		/// Displays a property field using the current GUI skin, if a default skin is not being used.
		/// </summary>
		/// <param name="property">Property.</param>
		/// <param name="includeChildren">If set to <see langword="true"/> include children.</param>
		/// <param name="label">Label.</param>
		public static void DisplayPropertyField(
			SerializedProperty property, bool includeChildren = true, GUIContent label = null
		)
		{
			DisplayPropertyField(EditorGUILayout.GetControlRect(), property, includeChildren, label);
		}

		/// <summary>
		/// Displays a property field using the current GUI skin, if a default skin is not being used.
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="property">Property.</param>
		/// <param name="includeChildren">If set to <see langword="true"/> include children.</param>
		/// <param name="label">Label.</param>
		public static void DisplayPropertyField(
			Rect position, SerializedProperty property, bool includeChildren = true, GUIContent label = null
		)
		{
			label = label ?? new GUIContent(property.displayName);
#if !UNITY_4_6
			if (EditorStylesX.IsUsingBuiltinSkin)
			{
				EditorGUI.PrefixLabel(position, label);
			}
			else
			{
				EditorGUI.PrefixLabel(position, label, GUI.skin.label);
			}
#else
			EditorGUI.PrefixLabel(position, label);
#endif
			position.width -= EditorGUIUtility.labelWidth;
			position.x += EditorGUIUtility.labelWidth;
			int oldIndent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;
			EditorGUI.PropertyField(position, property, GUIContent.none, includeChildren);
			EditorGUI.indentLevel = oldIndent;
		}

		/// <summary>
		/// Displays a property field with a button to apply its value to many items, if at least one item has a
		/// different value for the property of interest.
		/// </summary>
		/// <returns><see langword="true"/> if the button was pressed; otherwise, <see langword="false"/>.</returns>
		/// <param name="property">The <see cref="UnityEditor.SerializedProperty"/> used to set others.</param>
		/// <param name="label">Label.</param>
		/// <param name="affectedObjects">
		/// The <see cref="UnityEditor.SerializedProperty"/> for all objects affected by changes to the property.
		/// </param>
		/// <param name="buttonNarrow">Button width to use in narrow mode.</param>
		/// <param name="buttonWide">Button width to use in wide mode.</param>
		/// <param name="areValuesEqual">
		/// If not <see langword="null"/> then specifies a method for testing equality of the values of the property and
		/// a potentially matching affected object; otherwise, the method simply compares their hash codes and, if they
		/// are enums, their type string as well.
		/// </param>
		/// <param name="onSetProperty">
		/// <para>If not <see langword="null"/> then specifies a method to pass the new property value to, when the
		/// property changes or the button is clicked; otherwise, the property's new value is applied directly to all
		/// affected objects.</para>
		/// <para>In the current implementation, if the property is of type Generic, then you must specify this method.
		/// </para>
		/// </param>
		public static bool DisplayPropertyWithSetManyButton(
			SerializedProperty property,
			GUIContent label,
			SerializedProperty affectedObjects,
			string buttonText = "Set All",
			float buttonNarrow = k_NarrowButtonWidth,
			float buttonWide = k_NarrowButtonWidth,
			System.Func<object, object, bool> areValuesEqual = null,
			System.Action<object> onSetProperty = null
		)
		{
			Rect position = EditorGUILayout.GetControlRect(label != null, EditorGUI.GetPropertyHeight(property));
			return DisplayPropertyWithSetManyButton(
				position, property, label, affectedObjects, buttonText,
				buttonNarrow, buttonWide, areValuesEqual, onSetProperty
			);
		}

		/// <summary>
		/// Displays a property field with a button to apply its value to many items, if at least one item has a
		/// different value for the property of interest.
		/// </summary>
		/// <returns><see langword="true"/> if the button was pressed; otherwise, <see langword="false"/>.</returns>
		/// <param name="position">Position.</param>
		/// <param name="property">The <see cref="UnityEditor.SerializedProperty"/> used to set others.</param>
		/// <param name="label">Label.</param>
		/// <param name="affectedObjects">
		/// The <see cref="UnityEditor.SerializedProperty"/> for all objects affected by changes to the property.
		/// </param>
		/// <param name="buttonNarrow">Button width to use in narrow mode.</param>
		/// <param name="buttonWide">Button width to use in wide mode.</param>
		/// <param name="areValuesEqual">
		/// If not <see langword="null"/> then specifies a method for testing equality of the values of the property and
		/// a potentially matching affected object; otherwise, the method simply compares their hash codes and, if they
		/// are enums, their type string as well.
		/// </param>
		/// <param name="onSetProperty">
		/// <para>If not <see langword="null"/> then specifies a method to pass the new property value to, when the
		/// property changes or the button is clicked; otherwise, the property's new value is applied directly to all
		/// affected objects.</para>
		/// <para>In the current implementation, if the property is of type Generic, then you must specify this method.
		/// </para>
		/// </param>
		public static bool DisplayPropertyWithSetManyButton(
			Rect position,
			SerializedProperty property,
			GUIContent label,
			SerializedProperty affectedObjects,
			string buttonText = "Set All",
			float buttonNarrow = k_NarrowButtonWidth,
			float buttonWide = k_NarrowButtonWidth,
			System.Func<object, object, bool> areValuesEqual = null,
			System.Action<object> onSetProperty = null
		)
		{
			s_AffectedObjects.Clear();
			s_AffectedObjects.Add(affectedObjects);
			return DisplayPropertyWithSetManyButton(
				position, property, label, s_AffectedObjects, buttonText,
				buttonNarrow, buttonWide, areValuesEqual, onSetProperty
			);
		}

		/// <summary>
		/// Displays a property field with a button to apply its value to many items, if at least one item has a
		/// different value for the property of interest.
		/// </summary>
		/// <returns><see langword="true"/> if the button was pressed; otherwise, <see langword="false"/>.</returns>
		/// <param name="property">The <see cref="UnityEditor.SerializedProperty"/> used to set others.</param>
		/// <param name="label">Label.</param>
		/// <param name="affectedObjects">
		/// A list of <see cref="UnityEditor.SerializedProperty"/> for all objects affected by changes to the property.
		/// </param>
		/// <param name="buttonNarrow">Button width to use in narrow mode.</param>
		/// <param name="buttonWide">Button width to use in wide mode.</param>
		/// <param name="areValuesEqual">
		/// If not <see langword="null"/> then specifies a method for testing equality of the values of the property and
		/// a potentially matching affected object; otherwise, the method simply compares their hash codes and, if they
		/// are enums, their type string as well.
		/// </param>
		/// <param name="onSetProperty">
		/// <para>If not <see langword="null"/> then specifies a method to pass the new property value to, when the
		/// property changes or the button is clicked; otherwise, the property's new value is applied directly to all
		/// affected objects.</para>
		/// <para>In the current implementation, if the property is of type Generic, then you must specify this method.
		/// </para>
		/// </param>
		public static bool DisplayPropertyWithSetManyButton(
			SerializedProperty property,
			GUIContent label,
			IList<SerializedProperty> affectedObjects,
			string buttonText = "Set All",
			float buttonNarrow = k_NarrowButtonWidth,
			float buttonWide = k_NarrowButtonWidth,
			System.Func<object, object, bool> areValuesEqual = null,
			System.Action<object> onSetProperty = null
		)
		{
			Rect position = EditorGUILayout.GetControlRect(label != null, EditorGUI.GetPropertyHeight(property));
			return DisplayPropertyWithSetManyButton(
				position, property, label, affectedObjects, buttonText,
				buttonNarrow, buttonWide, areValuesEqual, onSetProperty
			);
		}

		/// <summary>
		/// Displays a property field with a button to apply its value to many items, if at least one item has a
		/// different value for the property of interest.
		/// </summary>
		/// <returns><see langword="true"/> if the button was pressed; otherwise, <see langword="false"/>.</returns>
		/// <param name="position">Position.</param>
		/// <param name="property">The <see cref="UnityEditor.SerializedProperty"/> used to set others.</param>
		/// <param name="label">Label.</param>
		/// <param name="affectedObjects">
		/// A list of <see cref="UnityEditor.SerializedProperty"/> for all objects affected by changes to the property.
		/// </param>
		/// <param name="buttonNarrow">Button width to use in narrow mode.</param>
		/// <param name="buttonWide">Button width to use in wide mode.</param>
		/// <param name="areValuesEqual">
		/// If not <see langword="null"/> then specifies a method for testing equality of the values of the property and
		/// a potentially matching affected object; otherwise, the method simply compares their hash codes and, if they
		/// are enums, their type string as well.
		/// </param>
		/// <param name="onSetProperty">
		/// <para>If not <see langword="null"/> then specifies a method to pass the new property value to, when the
		/// property changes or the button is clicked; otherwise, the property's new value is applied directly to all
		/// affected objects.</para>
		/// <para>In the current implementation, if the property is of type Generic, then you must specify this method.
		/// </para>
		/// </param>
		public static bool DisplayPropertyWithSetManyButton(
			Rect position,
			SerializedProperty property,
			GUIContent label,
			IList<SerializedProperty> affectedObjects,
			string buttonText = "Set All",
			float buttonNarrow = k_NarrowButtonWidth,
			float buttonWide = k_NarrowButtonWidth,
			System.Func<object, object, bool> areValuesEqual = null,
			System.Action<object> onSetProperty = null
		)
		{
			// determine if they all match (and hence whether button should be displayed)
			bool doAllMatch = affectedObjects == null || affectedObjects.Count < 1;
			if (!doAllMatch)
			{
				doAllMatch = affectedObjects.All(p => !p.hasMultipleDifferentValues);
				if (doAllMatch)
				{
					if (areValuesEqual != null)
					{
						doAllMatch &= areValuesEqual(property.GetValue(true), affectedObjects[0].GetValue(true));
					}
					else
					{
						bool isEnum = property.propertyType == SerializedPropertyType.Enum;
						doAllMatch &= affectedObjects.All(
							p => p.propertyType == property.propertyType && (isEnum ? p.type == property.type : true)
						);
						object propertyValue = property.GetValue(true);
						object compareValue = affectedObjects[0].GetValue(true);
						doAllMatch &= propertyValue == null ?
							compareValue == null :
							(compareValue == null ? false : propertyValue.GetHashCode() == compareValue.GetHashCode());
					}
				}
			}
			Rect controlPosition, buttonPosition;
			if (!doAllMatch)
			{
				GetRectsForControlWithInlineButton(
					position, out controlPosition, out buttonPosition, buttonNarrow, buttonWide
				);
			}
			else
			{
				controlPosition = position;
				buttonPosition = new Rect();
			}
			// display property field
			EditorGUI.BeginChangeCheck();
			{
				DisplayPropertyField(controlPosition, property, true, label);
			}
			bool didChange = EditorGUI.EndChangeCheck();
			// display button if not all values match
			bool didClick = false;
			if (!doAllMatch)
			{
				didClick = DisplayButton(buttonPosition, buttonText);
			}
			// apply changes to all affected objects
			if (didChange || didClick)
			{
				property.serializedObject.ApplyModifiedProperties();
				object propertyValue = property.GetValue(true);
				if (onSetProperty != null)
				{
					s_UndoObjects.Clear();
					s_UndoObjects.AddRange(property.serializedObject.targetObjects.Where(o => o != null));
					for (int i = 0; i < affectedObjects.Count; ++i)
					{
						s_UndoObjects.AddRange(affectedObjects[i].serializedObject.targetObjects.Where(o => o != null));
					}
					Undo.RecordObjects(
						s_UndoObjects.ToArray(),
						string.Format("{0} {1}", buttonText, buttonText == "Set All" ? property.displayName : "")
					);
					onSetProperty(propertyValue);
					EditorUtilityX.SetDirty(s_UndoObjects);
				}
				else
				{
					for (int i = 0; i < affectedObjects.Count; ++i)
					{
						affectedObjects[i].SetValue(propertyValue);
						affectedObjects[i].serializedObject.ApplyModifiedProperties();
					}
				}
			}
			return didClick;
		}

		/// <summary>
		/// Displays a property field with a status icon using EditorGUILayout.
		/// </summary>
		/// <param name="property">Property.</param>
		/// <param name="status">Status.</param>
		/// <param name="label">Label.</param>
		/// <param name="includeChildren">If set to <see langword="true"/> include children.</param>
		/// <param name="statusTooltip">Status tooltip.</param>
		public static void DisplayPropertyFieldWithStatus(
			SerializedProperty property, ValidationStatus status,
			GUIContent label = null, bool includeChildren = true, string statusTooltip = ""
		)
		{
			DisplayPropertyFieldWithStatus(
				EditorGUILayout.GetControlRect(), property, status, GUI.color, label, includeChildren, statusTooltip
			);
		}

		/// <summary>
		/// Displays a property field with a status icon.
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="property">Property.</param>
		/// <param name="status">Status.</param>
		/// <param name="label">Label.</param>
		/// <param name="includeChildren">If set to <see langword="true"/> include children.</param>
		/// <param name="statusTooltip">Status tooltip.</param>
		public static void DisplayPropertyFieldWithStatus(
			Rect position, SerializedProperty property, ValidationStatus status,
			GUIContent label = null, bool includeChildren = true, string statusTooltip = ""
		)
		{
			DisplayPropertyFieldWithStatus(
				position, property, status, GUI.color, label, includeChildren, statusTooltip
			);
		}

		/// <summary>
		/// Displays a property field with a status icon.
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="property">Property.</param>
		/// <param name="status">Status.</param>
		/// <param name="label">Label.</param>
		/// <param name="includeChildren">If set to <see langword="true"/> include children.</param>
		/// <param name="statusTooltip">Status tooltip.</param>
		public static void DisplayPropertyFieldWithStatus(
			Rect position, SerializedProperty property, ValidationStatus status, Color tint,
			GUIContent label = null, bool includeChildren = true, string statusTooltip = ""
		)
		{
			s_OldColorCache = GUI.color;
			GUI.color = tint;
			position.width -= EditorGUIUtility.singleLineHeight;
			label = label ?? new GUIContent(property.displayName);
			bool hasTooltip = !string.IsNullOrEmpty(label.tooltip);
			if (!hasTooltip)
			{
				label.tooltip = statusTooltip;
			}
			EditorGUI.PropertyField(position, property, label, includeChildren);
			if (!hasTooltip)
			{
				label.tooltip = string.Empty;
			}
			GUI.color = s_OldColorCache;
			position.x += position.width;
			position.width = position.height = EditorGUIUtility.singleLineHeight;
			DisplayValidationStatusIcon(position, status, statusTooltip);
		}

		/// <summary>
		/// Displays a property with a toggle. Use this method for e.g., override properties that only take effect when
		/// the user explicitly enabled them.
		/// </summary>
		/// <returns><see langword="true"/> if the property is enabled; otherwise, <see langword="false"/>.</returns>
		/// <param name="position">Position.</param>
		/// <param name="label">Label.</param>
		/// <param name="toggle">Property specifying whether or not the value will be used.</param>
		/// <param name="property">Property specifying the value.</param>
		public static bool DisplayPropertyWithToggle(
			Rect position, GUIContent label, SerializedProperty toggle, SerializedProperty property
		)
		{
			float totalWidth = position.width;
			position.width = EditorGUIUtility.labelWidth + 14f + standardHorizontalSpacing;
			EditorGUI.PropertyField(position, toggle, label);
			EditorGUI.BeginDisabledGroup(!toggle.boolValue);
			{
				int indent = EditorGUI.indentLevel;
				EditorGUI.indentLevel = 0;
				if (property.propertyType != SerializedPropertyType.Generic) // NOTE: quick fix assuming FlushChildrenAttribute
				{
					position.x += position.width;
					position.width = totalWidth - position.width;
				}
				else
				{
					position.width = totalWidth;
				}
				EditorGUI.PropertyField(position, property, GUIContent.none);
				EditorGUI.indentLevel = indent;
			}
			EditorGUI.EndDisabledGroup();
			return toggle.boolValue;
		}
		
		/// <summary>
		/// Displays a property group.
		/// </summary>
		/// <returns><see langword="true"/> if the group is expanded; otherwise, <see langword="false"/>.</returns>
		/// <param name="label">Label for the group.</param>
		/// <param name="foldout">Foldout preference.</param>
		/// <param name="contents">Method to draw the contents of the group.</param>
		/// <typeparam name="TEditor">The editor type.</typeparam>
		public static bool DisplayPropertyGroup<TEditor>(
			string label, EditorPreference<bool, TEditor> foldout, System.Action contents
		)
		{
			foldout.CurrentValue = EditorGUILayout.Foldout(foldout.CurrentValue, label);
			if (foldout.CurrentValue)
			{
				int indent = EditorGUI.indentLevel;
				GUILayout.BeginHorizontal();
				{
					Rect position = EditorGUI.IndentedRect(new Rect());
					GUILayout.Space(position.x);
					EditorGUI.indentLevel = 0;
					EditorGUILayout.BeginVertical("box");
					{
						contents.Invoke();
					}
					EditorGUILayout.EndVertical();
				}
				GUILayout.EndHorizontal();
				EditorGUI.indentLevel = indent;
			}
			return foldout.CurrentValue;
		}
		
		/// <summary>
		/// Displays the scene GUI toggle.
		/// </summary>
		public static void DisplaySceneGUIToggle()
		{
			SceneGUI.IsEnabled = DisplayOnOffToggle("Scene GUI", SceneGUI.IsEnabled);
		}

		/// <summary>
		/// Displays a property field for a scriptable object with a button alongside to create a new instance or select
		/// the assigned instance.
		/// </summary>
		/// <returns>
		/// <see langword="true"/> if a new instance was created; otherwise, <see langword="false"/>.
		/// </returns>
		/// <param name="property">Property.</param>
		/// <param name="label">Label.</param>
		/// <typeparam name="T">The concrete type associated with this property.</typeparam>
		public static bool DisplayScriptableObjectPropertyFieldWithButton<T>(
			SerializedProperty property, GUIContent label = null
		) where T: ScriptableObject
		{
			return DisplayScriptableObjectPropertyFieldWithButton<T>(
				EditorGUILayout.GetControlRect(true, EditorGUI.GetPropertyHeight(property)), property, label
			);
		}

		/// <summary>
		/// Displays a property field for a scriptable object with a button alongside to create a new instance or select
		/// the assigned instance.
		/// </summary>
		/// <returns>
		/// <see langword="true"/> if a new instance was created; otherwise, <see langword="false"/>.
		/// </returns>
		/// <param name="position">Position.</param>
		/// <param name="property">Property.</param>
		/// <param name="label">Label.</param>
		/// <typeparam name="T">The concrete type associated with this property.</typeparam>
		public static bool DisplayScriptableObjectPropertyFieldWithButton<T>(
			Rect position, SerializedProperty property, GUIContent label = null
		) where T: ScriptableObject
		{
			bool didCreateNewAsset = false;
			if (property.propertyType != SerializedPropertyType.ObjectReference)
			{
				DisplayPropertyField(position, property, true);
			}
			else
			{
				Rect controlPosition, buttonPosition;
				GetRectsForControlWithInlineButton(
					position,
					out controlPosition,
					out buttonPosition,
					Mathf.Min(NarrowInlineButtonWidth, (position.width - EditorGUIUtility.labelWidth) * 0.5f),
					WideInlineButtonWidth
				);
				DisplayPropertyField(controlPosition, property, false, label);
				if (property.objectReferenceValue != null)
				{
					if (DisplayButton(buttonPosition, "Select"))
					{
						Selection.objects = new Object[] { property.objectReferenceValue };
					}
				}
				else if (
					DisplayButton(
						buttonPosition, buttonPosition.width > NarrowInlineButtonWidth ? "Create New" : "Create"
					)
				)
				{
					Object[] selection = Selection.objects;
					ScriptableObject newObject = AssetDatabaseX.CreateNewAssetInUserSpecifiedPath<T>();
					property.objectReferenceValue = newObject;
					Selection.objects = selection;
					didCreateNewAsset = true;
				}
			}
			return didCreateNewAsset;
		}

		/// <summary>
		/// Displays a slider whose type-in field allows values outside the slider range.
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="label">Label.</param>
		/// <param name="property">Property.</param>
		/// <param name="sliderMin">Slider minimum.</param>
		/// <param name="sliderMax">Slider maximum.</param>
		public static void DisplaySoftSlider(
			Rect position, GUIContent label, SerializedProperty property, float sliderMin, float sliderMax
		)
		{
			switch (property.propertyType)
			{
			case SerializedPropertyType.Float:
				label = EditorGUI.BeginProperty(position, label, property);
				{
					float floatValue = property.floatValue;
					EditorGUI.BeginChangeCheck();
					{
						floatValue = DisplaySoftSlider(position, label, floatValue, sliderMin, sliderMax);
					}
					if (EditorGUI.EndChangeCheck())
					{
						property.floatValue = floatValue;
					}
				}
				EditorGUI.EndProperty();
				break;
			case SerializedPropertyType.Integer:
				label = EditorGUI.BeginProperty(position, label, property);
				{
					int intValue = property.intValue;
					EditorGUI.BeginChangeCheck();
					{
						intValue = DisplaySoftSlider(position, label, intValue, (int)sliderMin, (int)sliderMax);
					}
					if (EditorGUI.EndChangeCheck())
					{
						property.intValue = intValue;
					}
				}
				EditorGUI.EndProperty();
				break;
			default:
				DisplayPropertyFieldWithStatus(
					position,
					property,
					ValidationStatus.Error,
					label,
					true,
					string.Format(
						"Property must be of type {0} or {1} to use soft slider.",
						SerializedPropertyType.Float, SerializedPropertyType.Integer
					)
				);
				break;
			}
		}

		/// <summary>
		/// Displays a slider whose type-in field allows values outside the slider range.
		/// </summary>
		/// <returns>The soft slider.</returns>
		/// <param name="position">Position.</param>
		/// <param name="label">Label.</param>
		/// <param name="value">Current slider value.</param>
		/// <param name="sliderMin">Slider minimum.</param>
		/// <param name="sliderMax">Slider maximum.</param>
		public static float DisplaySoftSlider(
			Rect position, GUIContent label, float value, float sliderMin, float sliderMax
		)
		{
			return DisplaySoftSlider(position, label, value, sliderMin, sliderMax, false);
		}

		/// <summary>
		/// Displays a slider whose type-in field allows values outside the slider range.
		/// </summary>
		/// <returns>The soft slider.</returns>
		/// <param name="position">Position.</param>
		/// <param name="label">Label.</param>
		/// <param name="value">Current slider value.</param>
		/// <param name="sliderMin">Slider minimum.</param>
		/// <param name="sliderMax">Slider maximum.</param>
		public static int DisplaySoftSlider(Rect position, GUIContent label, int value, int sliderMin, int sliderMax)
		{
			return Mathf.RoundToInt(
				DisplaySoftSlider(position, label, (float)value, (float)sliderMin, (float)sliderMax)
			);
		}

		/// <summary>
		/// Displays a slider whose type-in field allows values outside the slider range.
		/// </summary>
		/// <returns>The soft slider.</returns>
		/// <param name="position">Position.</param>
		/// <param name="label">Label.</param>
		/// <param name="value">Current slider value.</param>
		/// <param name="sliderMin">Slider minimum.</param>
		/// <param name="sliderMax">Slider maximum.</param>
		/// <param name="formatString">Format string for input field.</param>
		private static float DisplaySoftSlider(
			Rect position, GUIContent label, float value, float sliderMin, float sliderMax, bool isInt
		)
		{
			// modified from EditorGUI.DoSlider()
			if (position.width >= 65f + EditorGUIUtility.fieldWidth)
			{
				int id = GUIUtility.GetControlID(s_SliderHash, FocusType.Passive, position);
				position = EditorGUI.PrefixLabel(position, id, label);
				position.width -= 5f + EditorGUIUtility.fieldWidth;
				EditorGUI.BeginChangeCheck();
				{
					value = GUI.Slider(
						position,
						value,
						0f,
						sliderMin,
						sliderMax,
						GUI.skin.horizontalSlider,
						(!EditorGUI.showMixedValue) ? GUI.skin.horizontalSliderThumb : "SliderMixed", true, id
					);
					if (GUIUtility.hotControl == id)
					{
						GUIUtility.keyboardControl = id;
					}
					if (
						GUIUtility.keyboardControl == id &&
						Event.current.type == EventType.KeyDown &&
						(Event.current.keyCode == KeyCode.LeftArrow || Event.current.keyCode == KeyCode.RightArrow)
					)
					{
						s_Param1[0] = Mathf.Abs((sliderMax - sliderMin) * 0.01f);
						float step = (float)s_GetClosestPowerOfTen.Invoke(null, s_Param1);
						if (isInt && step < 1f)
						{
							step = 1f;
						}
						if (Event.current.shift)
						{
							step *= 10f;
						}
						if (Event.current.keyCode == KeyCode.LeftArrow)
						{
							value -= step * 0.5001f;
						}
						else
						{
							value += step * 0.5001f;
						}
						s_Param2[0] = value;
						s_Param2[1] = step;
						value = (float)s_RoundToMultipleOf.Invoke(null, s_Param2);
						GUI.changed = true;
						Event.current.Use();
					}
				}
				if (EditorGUI.EndChangeCheck())
				{
					float f = (sliderMax - sliderMin) / (
						position.width -
						(float)GUI.skin.horizontalSlider.padding.horizontal -
						GUI.skin.horizontalSliderThumb.fixedWidth
					);
					s_Param2[0] = value;
					s_Param2[1] = Mathf.Abs(f);
					value = (float)s_RoundBasedOnMinimumDifference.Invoke(null, s_Param2);
				}
				position.x += position.width + 5f;
				position.width = EditorGUIUtility.fieldWidth;
				int indent = EditorGUI.indentLevel;
				EditorGUI.indentLevel = 0;
				value = isInt ? (float)EditorGUI.IntField(position, (int)value) : EditorGUI.FloatField(position, value);
				EditorGUI.indentLevel = indent;
			}
			else
			{
				position.width = Mathf.Min(EditorGUIUtility.fieldWidth, position.width);
				position.x = position.xMax - position.width;
				value = EditorGUI.FloatField(position, label, value);
			}
			return value;
		}

		/// <summary>
		/// Displays a title bar.
		/// </summary>
		/// <param name="label">Label.</param>
		public static void DisplayTitleBar(string label)
		{
			EditorGUILayout.LabelField(label, EditorStylesX.BoldTitleBar);
			GUILayoutUtility.GetRect(0f, EditorGUIUtility.standardVerticalSpacing);
		}
		
		/// <summary>
		/// Displays a validation status icon.
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="status">Status.</param>
		/// <param name="tooltip">Tooltip (optional).</param>
		public static void DisplayValidationStatusIcon(Rect position, ValidationStatus status, string tooltip = "")
		{
			s_ValidationStatusIcon.image = null;
			s_ValidationStatusIcon.text = string.Empty;
			s_ValidationStatusIcon.tooltip = tooltip;
			switch (status)
			{
			case ValidationStatus.Error:
				s_ValidationStatusIcon.image = EditorStylesX.ErrorIcon;
				break;
			case ValidationStatus.Info:
				s_ValidationStatusIcon.image = EditorStylesX.InfoIcon;
				break;
			case ValidationStatus.None:
				// display empty icon to prevent control IDs changing when status icons dynamically update
				if (s_NoStatusIcon == null)
				{
					s_NoStatusIcon = new Texture2D(0, 0);
					s_NoStatusIcon.hideFlags = HideFlags.HideAndDontSave;
				}
				s_ValidationStatusIcon.image = s_NoStatusIcon;
				break;
			case ValidationStatus.Okay:
				s_ValidationStatusIcon.image = EditorStylesX.OkayIcon;
				break;
			case ValidationStatus.Warning:
				s_ValidationStatusIcon.image = EditorStylesX.WarningIcon;
				break;
			}
			if (s_ValidationStatusIcon.image != null)
			{
				GUI.Box(position, s_ValidationStatusIcon, EditorStylesX.StatusIconStyle);
			}
		}
		
		/// <summary>
		/// Ends the scene GUI controls area.
		/// </summary>
		public static void EndSceneGUIControlsArea()
		{
			--EditorGUI.indentLevel;
			EditorGUILayout.EndVertical();
		}
		
		/// <summary>
		/// Gets the color from Unity preferences.
		/// </summary>
		/// <remarks>
		/// Unity editor colors use their own, exotic serialization scheme.
		/// </remarks>
		/// <returns>The color from Unity preferences.</returns>
		/// <param name="propName">Property name.</param>
		/// <param name="defaultColor">Default color.</param>
		private static Color GetColorFromUnityPreferences(string propName, Color defaultColor)
		{
			string s = EditorPrefs.GetString(propName, "");
			if (string.IsNullOrEmpty(s))
			{
				return defaultColor;
			}
			string[] values = s.Split(';');
			try
			{
				return new Color(
					float.Parse(values[values.Length-4]),
					float.Parse(values[values.Length-3]),
					float.Parse(values[values.Length-2]),
					float.Parse(values[values.Length-1])
				);
			}
			catch (System.Exception)
			{
				return defaultColor;
			}
		}

		/// <summary>
		/// Gets the rects for a control and an inline button next to it. You can specify desired button widths for wide
		/// and not wide modes, and the width will not exceed the field width.
		/// </summary>
		/// <param name="position">Position in which to draw.</param>
		/// <param name="controlRect">Control rect.</param>
		/// <param name="buttonRect">Button rect.</param>
		/// <param name="buttonNarrow">Button width to use in narrow mode.</param>
		/// <param name="buttonWide">Button width to use in wide mode.</param>
		public static void GetRectsForControlWithInlineButton(
			Rect position,
			out Rect controlRect,
			out Rect buttonRect,
			float buttonNarrow = k_WideButtonWidth,
			float buttonWide = k_WideButtonWidth
		)
		{
			float buttonWidth = Mathf.Min(
				EditorGUIUtility.wideMode ? buttonWide : buttonNarrow, position.width - EditorGUIUtility.labelWidth
			);
			controlRect = position;
			controlRect.width -= buttonWidth + standardHorizontalSpacing;
			buttonRect = new Rect(
				controlRect.x + controlRect.width + standardHorizontalSpacing,
				position.y,
				buttonWidth,
				EditorGUIUtility.singleLineHeight
			);
		}
		
		/// <summary>
		/// A wrapper for EditorGUI.Slider() that matches the method signature requirement of EditorGUIX.DisplayField().
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="label">Label.</param>
		/// <param name="value">Value.</param>
		private static float OnFloatSliderField(Rect position, GUIContent label, float value)
		{
			return EditorGUI.Slider(position, label, value, s_SliderMin, s_SliderMax);
		}
	}
}
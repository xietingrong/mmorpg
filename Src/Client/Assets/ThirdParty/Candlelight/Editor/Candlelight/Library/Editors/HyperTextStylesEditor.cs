// 
// HyperTextStyles.cs
// 
// Copyright (c) 2014-2015, Candlelight Interactive, LLC
// All rights reserved.
// 
// This file is licensed according to the terms of the Unity Asset Store EULA:
// http://download.unity3d.com/assetstore/customer-eula.pdf
// 
// This file contains a custom editor for HyperTextStyles.

using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Candlelight.UI
{
	/// <summary>
	/// A custom editor for <see cref="HyperTextStyles"/> objects.
	/// </summary>
	[CustomEditor(typeof(HyperTextStyles)), CanEditMultipleObjects, InitializeOnLoad]
	public class HyperTextStylesEditor : Editor
	{
		/// <summary>
		/// An enum to specify which custom style tab is currently selected.
		/// </summary>
		public enum CustomStyleTab { LinkStyles, CustomTextStyles, QuadStyles }

		/// <summary>
		/// A <see cref="System.Delegate"/> to get a list of styles from a style sheet.
		/// </summary>
		/// <typeparam name="T">The style type.</typeparam>
		/// <param name="styleSheet">The style sheet from which to get styles.</param>
		/// <param name="styles">A list to populate with the defined styles.</param>
		private delegate void StylesGetter<T>(HyperTextStyles styleSheet, List<T> styles);

		/// <summary>
		/// A <see cref="System.Delegate"/> to set a list of styles on a style sheet.
		/// </summary>
		/// <typeparam name="T">The style type.</typeparam>
		/// <param name="styleSheet">The style sheet on which styles should be set.</param>
		/// <param name="styles">The collection of new styles to set.</param>
		private delegate void StylesSetter<T>(HyperTextStyles styleSheet, IEnumerable<T> styles);

		/// <summary>
		/// A class for storing GUIContent related to a custom style.
		/// </summary>
		internal class CustomStyleGUIContent
		{
			/// <summary>
			/// Gets or sets the label.
			/// </summary>
			/// <value>The label.</value>
			public GUIContent Label { get; set; }
			/// <summary>
			/// Gets or sets the status.
			/// </summary>
			/// <value>The status.</value>
			public ValidationStatus Status { get; set; }
			/// <summary>
			/// Gets or sets the status tooltip.
			/// </summary>
			/// <value>The status tooltip.</value>
			public string StatusTooltip { get; set; }
		}

		#region Shared Allocations
		private static List<HyperTextStyles> s_InheritedStyles = new List<HyperTextStyles>();
		private static readonly GUIContent s_ReusableLabel = new GUIContent();
		#endregion

		/// <summary>
		/// The custom style tab labels.
		/// </summary>
		private static readonly GUIContent[] s_CustomStyleTabLabels =
			(from name in System.Enum.GetNames(typeof(CustomStyleTab)) select new GUIContent(name.ToWords())).ToArray();
		/// <summary>
		/// An editor preference to record which custom style tab is currently selected.
		/// </summary>
		private static EditorPreference<CustomStyleTab, HyperTextStylesEditor> s_CustomStyleTabPreference =
			new EditorPreference<CustomStyleTab, HyperTextStylesEditor>("customStyleTab", CustomStyleTab.LinkStyles);

		/// <summary>
		/// Creates a new asset in the project.
		/// </summary>
		[UnityEditor.MenuItem("Assets/Create/Candlelight/HyperText Styles")]
		public static void CreateNewAssetInProject()
		{
			AssetDatabaseX.CreateNewAssetInCurrentProjectFolder<HyperTextStyles>();
		}

		/// <summary>
		/// Displays a validation icon next to a style's label in the inspector.
		/// </summary>
		/// <param name="elementDrawPosition">Element draw position.</param>
		/// <param name="info">Validation info.</param>
		private static void DisplayStyleIdentifierValidationIcon(Rect elementDrawPosition, CustomStyleGUIContent info)
		{
			if (
				elementDrawPosition.height <
				(EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 2f
			)
			{
				elementDrawPosition.x += elementDrawPosition.width - EditorGUIUtility.singleLineHeight;
			}
			else
			{
				elementDrawPosition.x += EditorGUIUtility.labelWidth -
					EditorGUIUtility.singleLineHeight -
					EditorGUIX.standardHorizontalSpacing;
			}
			elementDrawPosition.height = elementDrawPosition.width = EditorGUIUtility.singleLineHeight;
			info.Status = info.Status == ValidationStatus.Okay ? ValidationStatus.None : info.Status;
			EditorGUIX.DisplayValidationStatusIcon(elementDrawPosition, info.Status, info.StatusTooltip);
		}

		#region Serialized Properties
		private ReorderableList m_CustomTextStyles;
		private SerializedProperty m_DefaultFontStyleProperty;
		private SerializedProperty m_DefaultFontStyleOverrideProperty;
		private SerializedProperty m_DefaultLinkStyleProperty;
		private SerializedProperty m_DefaultLinkStyleOverrideProperty;
		private SerializedProperty m_DefaultTextColorProperty;
		private SerializedProperty m_DefaultTextColorOverrideProperty;
		private SerializedProperty m_FontProperty;
		private SerializedProperty m_FontSizeProperty;
		private SerializedProperty m_FontSizeOverrideProperty;
		private ReorderableList m_InheritedStyles;
		private SerializedProperty m_LineSpacingProperty;
		private SerializedProperty m_LineSpacingOverrideProperty;
		private SerializedProperty m_LinkHitboxPaddingProperty;
		private SerializedProperty m_LinkHitboxPaddingOverrideProperty;
		private ReorderableList m_LinkStyles;
		private SerializedProperty m_NameProperty;
		private ReorderableList m_QuadStyles;
		private SerializedProperty m_ScriptProperty;
		#endregion
		
		/// <summary>
		/// The inherited link styles.
		/// </summary>
		private Dictionary <HyperTextStyles.LinkSubclass, HyperTextStyles> m_InheritedLinks =
			new Dictionary<HyperTextStyles.LinkSubclass, HyperTextStyles>();
		/// <summary>
		/// The inherited quad styles.
		/// </summary>
		private Dictionary<HyperTextStyles.Quad, HyperTextStyles> m_InheritedQuads =
			new Dictionary<HyperTextStyles.Quad, HyperTextStyles>();
		/// <summary>
		/// The inherited custom text styles.
		/// </summary>
		private Dictionary<HyperTextStyles.Text, HyperTextStyles> m_InheritedTags =
			new Dictionary<HyperTextStyles.Text, HyperTextStyles>();
		/// <summary>
		/// The names of inherited styles relevant to the current tab.
		/// </summary>
		private List<string> m_CurrentTabInheritedStyleNames = new List<string>();
		/// <summary>
		/// The methods for drawing the custom style tabs.
		/// </summary>
		private Dictionary<int, System.Action> m_CustomStyleTabContents = new Dictionary<int, System.Action>();
		/// <summary>
		/// An allocation for the method to display the currently selected style entry for editing.
		/// </summary>
		private System.Action<Rect> m_DisplayCurrentStyle = null;
		/// <summary>
		/// A table for storing the GUI contents for each custom link class.
		/// </summary>
		private Dictionary <int, CustomStyleGUIContent> m_LinkGUIContents =
			new Dictionary<int, CustomStyleGUIContent>();
		/// <summary>
		/// Tooltip predicate for properties inheriting a style from a parent.
		/// </summary>
		private string m_InheritTooltip;
		/// <summary>
		/// Tooltip predicate for properties overriding a parent style.
		/// </summary>
		private string m_OverrideTooltip;
		/// <summary>
		/// The parent style from which defaults will be inherited.
		/// </summary>
		private HyperTextStyles m_ParentStyle = null;
		/// <summary>
		/// A table for storing the GUI contents for each custom quad class.
		/// </summary>
		private Dictionary <int, CustomStyleGUIContent> m_QuadGUIContents =
			new Dictionary<int, CustomStyleGUIContent>();
		/// <summary>
		/// A table for storing the GUI contents for each custom tag.
		/// </summary>
		private Dictionary <int, CustomStyleGUIContent> m_TagGUIContents = new Dictionary<int, CustomStyleGUIContent>();

		/// <summary>
		/// Displays a property field with an override property checkbox and status icon if a parent style is found.
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="property">Property to display.</param>
		/// <param name="overrideProperty">
		/// Property specifying whether the other property is overriding an inherited one. Object reference properties
		/// are assumed to override if they have a value assigned.
		/// </param>
		private void DisplayOverridableProperty(
			Rect position, SerializedProperty property, SerializedProperty overrideProperty
		)
		{
			HyperTextEditor.DisplayOverridableProperty(
				position: position,
				property: property,
				overrideProperty: overrideProperty,
				displayCheckbox: m_ParentStyle != null,
				inheritTooltip: m_InheritTooltip,
				overrideTooltip: m_OverrideTooltip
			);
		}

		/// <summary>
		/// Raises the add custom text style event.
		/// </summary>
		/// <param name="list">List.</param>
		private void OnAddCustomTextStyle(ReorderableList list)
		{
			++list.serializedProperty.arraySize;
			if (list.serializedProperty.arraySize == 1)
			{
				RichTextStyle defaultStyle = RichTextStyle.DefaultStyle;
				SerializedProperty newEntry = list.serializedProperty.GetArrayElementAtIndex(list.count - 1);
				newEntry.FindPropertyRelative("m_TextStyle.m_ReplacementColor").colorValue =
					defaultStyle.ReplacementColor;
				newEntry.FindPropertyRelative("m_TextStyle.m_SizeScalar").floatValue = defaultStyle.SizeScalar;
			}
			list.index = list.serializedProperty.arraySize - 1;
			this.serializedObject.ApplyModifiedProperties();
			UpdateCustomStyleGUIContents();
		}

		/// <summary>
		/// Raises the add link style event.
		/// </summary>
		/// <param name="list">List.</param>
		private void OnAddLinkStyle(ReorderableList list)
		{
			++list.serializedProperty.arraySize;
			if (list.serializedProperty.arraySize == 1)
			{
				HyperTextStyles.Link defaultStyle = HyperTextStyles.Link.DefaultStyle;
				SerializedProperty newEntry = list.serializedProperty.GetArrayElementAtIndex(list.count - 1);
				newEntry.FindPropertyRelative("m_Style.m_TextStyle.m_ReplacementColor").colorValue =
					defaultStyle.TextStyle.ReplacementColor;
				newEntry.FindPropertyRelative("m_Style.m_TextStyle.m_SizeScalar").floatValue =
					defaultStyle.TextStyle.SizeScalar;
				newEntry.FindPropertyRelative("m_Style.m_ColorTintMode").enumValueIndex =
					(int)defaultStyle.ColorTintMode;
				newEntry.FindPropertyRelative("m_Style.m_ColorTweenMode").enumValueIndex =
					(int)defaultStyle.ColorTweenMode;
				newEntry.FindPropertyRelative("m_Style.m_Colors.m_ColorMultiplier").floatValue =
					defaultStyle.Colors.colorMultiplier;
				newEntry.FindPropertyRelative("m_Style.m_Colors.m_FadeDuration").floatValue =
					defaultStyle.Colors.fadeDuration;
				newEntry.FindPropertyRelative("m_Style.m_Colors.m_NormalColor").colorValue =
					defaultStyle.Colors.normalColor;
				newEntry.FindPropertyRelative("m_Style.m_Colors.m_HighlightedColor").colorValue =
					defaultStyle.Colors.highlightedColor;
				newEntry.FindPropertyRelative("m_Style.m_Colors.m_PressedColor").colorValue =
					defaultStyle.Colors.pressedColor;
				newEntry.FindPropertyRelative("m_Style.m_Colors.m_DisabledColor").colorValue =
					defaultStyle.Colors.disabledColor;
			}
			this.serializedObject.ApplyModifiedProperties();
			UpdateCustomStyleGUIContents();
		}

		/// <summary>
		/// Raises the add quad style event.
		/// </summary>
		/// <param name="list">List.</param>
		private void OnAddQuadStyle(ReorderableList list)
		{
			++list.serializedProperty.arraySize;
			if (list.serializedProperty.arraySize == 1)
			{
				SerializedProperty newEntry = list.serializedProperty.GetArrayElementAtIndex(list.count - 1);
				newEntry.FindPropertyRelative("m_ShouldRespectColorization").boolValue = true;
				newEntry.FindPropertyRelative("m_SizeScalar").floatValue = 1f;
			}
			this.serializedObject.ApplyModifiedProperties();
			UpdateCustomStyleGUIContents();
		}

		/// <summary>
		/// Raises the draw custom style entry event.
		/// </summary>
		/// <param name="position">Position of the entry.</param>
		/// <param name="list">List to which the entry belongs.</param>
		/// <param name="index">Index of the entry being drawn.</param>
		/// <param name="guiContentsTable">Table of GUI contents used by the element being drawn.</param>
		/// <param name="inheritedStyles">Table of inherited styles of the element's type.</param>
		/// <param name="getStyles">Method to get styles of the element's type from a HyperTextStyles.</param>
		/// <param name="setStyles">Method to set styles of the element's type on a HyperTextStyles.</param>
		/// <typeparam name="T">A custom style type.</typeparam>
		private void OnDrawCustomStyleEntry<T>(
			Rect position,
			ReorderableList list,
			int index,
			Dictionary<int, CustomStyleGUIContent> guiContentsTable,
			Dictionary<T, HyperTextStyles> inheritedStyles,
			StylesGetter<T> getStyles,
			StylesSetter<T> setStyles
		) where T: IIdentifiable<string>
		{
			index = Mathf.Clamp(index, 0, list.serializedProperty.arraySize);
			if (!guiContentsTable.ContainsKey(index))
			{
				return;
			}
			SerializedProperty element = // NOTE: undoing list increase can cause GetArrayElementAtIndex() to fail
				list.serializedProperty.FindPropertyRelative(string.Format("Array.data[{0}]", index));
			if (element == null)
			{
				return;
			}
			if (position.height < (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 2f)
			{
				EditorGUI.LabelField(position, guiContentsTable[index].Label);
			}
			else
			{
				EditorGUI.PropertyField(position, element, guiContentsTable[index].Label);
				if (guiContentsTable[index].Status == ValidationStatus.Warning)
				{
					Rect buttonPosition = position;
					buttonPosition.height = EditorGUIUtility.singleLineHeight;
					buttonPosition.y += buttonPosition.height + EditorGUIUtility.standardVerticalSpacing;
					buttonPosition.width = EditorGUIUtility.labelWidth;
					if (EditorGUIX.DisplayButton(buttonPosition, "Paste Inherited"))
					{
						string identifier = element.GetValue<T>().Identifier;
						T inheritedStyle = inheritedStyles.Keys.Where(s => s.Identifier == identifier).FirstOrDefault();
						Undo.RecordObjects(this.targets, "Paste HyperText Style");
						foreach (HyperTextStyles styleSheet in this.targets)
						{
							List<T> styles = new List<T>();
							getStyles(styleSheet, styles);
							styles[index] = inheritedStyle;
							setStyles(styleSheet, styles);
						}
					}
				}
			}
			DisplayStyleIdentifierValidationIcon(position, guiContentsTable[index]);
		}

		/// <summary>
		/// Raises the draw custom text style entry event.
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="index">Index.</param>
		/// <param name="isActive">If set to <see langword="true"/> is active.</param>
		/// <param name="isFocused">If set to <see langword="true"/> is focused.</param>
		private void OnDrawCustomTextStyleEntry(Rect position, int index, bool isActive, bool isFocused)
		{
			OnDrawCustomStyleEntry<HyperTextStyles.Text>(
				position,
				m_CustomTextStyles,
				index,
				m_TagGUIContents,
				m_InheritedTags,
				(styles, texts) => styles.GetCustomTextStyles(ref texts),
				(styles, texts) => styles.SetCustomTextStyles(texts)
			);
		}

		/// <summary>
		/// Raises the draw inherited style entry event.
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="index">Index.</param>
		/// <param name="isActive">If set to <see langword="true"/> is active.</param>
		/// <param name="isFocused">If set to <see langword="true"/> is focused.</param>
		private void OnDrawInheritedStyleEntry(Rect position, int index, bool isActive, bool isFocused)
		{
			position.height = EditorGUIUtility.singleLineHeight;
			s_ReusableLabel.text =
				string.Format("Style {0}{1}", index, index == m_InheritedStyles.count - 1 ? " (Parent)" : "");
			EditorGUI.BeginChangeCheck();
			{
				EditorGUIX.DisplayScriptableObjectPropertyFieldWithButton<HyperTextStyles>(
					position, m_InheritedStyles.serializedProperty.GetArrayElementAtIndex(index), s_ReusableLabel
				);
			}
			if (EditorGUI.EndChangeCheck())
			{
				UpdateCustomStyleGUIContents();
			}
		}

		/// <summary>
		/// Raises the draw link style entry event.
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="index">Index.</param>
		/// <param name="isActive">If set to <see langword="true"/> is active.</param>
		/// <param name="isFocused">If set to <see langword="true"/> is focused.</param>
		private void OnDrawLinkStyleEntry(Rect position, int index, bool isActive, bool isFocused)
		{
			OnDrawCustomStyleEntry<HyperTextStyles.LinkSubclass>(
				position,
				m_LinkStyles,
				index,
				m_LinkGUIContents,
				m_InheritedLinks,
				(styles, links) => styles.GetLinkStyles(ref links),
				(styles, links) => styles.SetLinkStyles(links)
			);
		}

		/// <summary>
		/// Raises the draw quad style entry event.
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="index">Index.</param>
		/// <param name="isActive">If set to <see langword="true"/> is active.</param>
		/// <param name="isFocused">If set to <see langword="true"/> is focused.</param>
		private void OnDrawQuadStyleEntry(Rect position, int index, bool isActive, bool isFocused)
		{
			OnDrawCustomStyleEntry<HyperTextStyles.Quad>(
				position,
				m_QuadStyles,
				index,
				m_QuadGUIContents,
				m_InheritedQuads,
				(styles, quads) => styles.GetQuadStyles(ref quads),
				(styles, quads) => styles.SetQuadStyles(quads)
			);
		}

		/// <summary>
		/// Raises the disable event.
		/// </summary>
		private void OnDisable()
		{
			(this.target as HyperTextStyles).OnStylesChanged.RemoveListener(UpdateCustomStyleGUIContents);
		}

		/// <summary>
		/// Raises the enable event.
		/// </summary>
		private void OnEnable()
		{
			m_DefaultFontStyleProperty = this.serializedObject.FindProperty("m_DefaultFontStyle");
			m_DefaultFontStyleOverrideProperty =
				this.serializedObject.FindProperty("m_ShouldOverrideInheritedDefaultFontStyle");
			m_DefaultLinkStyleProperty = this.serializedObject.FindProperty("m_DefaultLinkStyle");
			m_DefaultLinkStyleOverrideProperty =
				this.serializedObject.FindProperty("m_ShouldOverrideInheritedDefaultLinkStyle");
			m_DefaultTextColorProperty = this.serializedObject.FindProperty("m_DefaultTextColor");
			m_DefaultTextColorOverrideProperty =
				this.serializedObject.FindProperty("m_ShouldOverrideInheritedDefaultTextColor");
			m_FontProperty = this.serializedObject.FindProperty("m_Font");
			m_FontSizeProperty = this.serializedObject.FindProperty("m_FontSize");
			m_FontSizeOverrideProperty = this.serializedObject.FindProperty("m_ShouldOverrideInheritedFontSize");
			m_LineSpacingProperty = this.serializedObject.FindProperty("m_LineSpacing");
			m_LineSpacingOverrideProperty = this.serializedObject.FindProperty("m_ShouldOverrideInheritedLineSpacing");
			m_LinkHitboxPaddingProperty = this.serializedObject.FindProperty("m_LinkHitboxPadding");
			m_LinkHitboxPaddingOverrideProperty =
				this.serializedObject.FindProperty("m_ShouldOverrideInheritedLinkHitboxPadding");
			m_ScriptProperty = this.serializedObject.FindProperty("m_Script");
			m_NameProperty = this.serializedObject.FindProperty("m_Name");
			m_CustomTextStyles =
				new ReorderableList(this.serializedObject, this.serializedObject.FindProperty("m_CustomTextStyles"));
			string displayName1 = m_CustomTextStyles.serializedProperty.displayName;
			m_CustomTextStyles.drawHeaderCallback = (position) => EditorGUI.LabelField(position, displayName1);
			m_CustomTextStyles.drawElementCallback = OnDrawCustomTextStyleEntry;
			m_CustomTextStyles.onAddCallback = OnAddCustomTextStyle;
			m_LinkStyles =
				new ReorderableList(this.serializedObject, this.serializedObject.FindProperty("m_LinkStyles"));
			string displayName2 = m_LinkStyles.serializedProperty.displayName;
			m_LinkStyles.drawHeaderCallback = (position) => EditorGUI.LabelField(position, displayName2);
			m_LinkStyles.drawElementCallback = OnDrawLinkStyleEntry;
			m_LinkStyles.onAddCallback = OnAddLinkStyle;
			m_QuadStyles =
				 new ReorderableList(this.serializedObject, this.serializedObject.FindProperty("m_QuadStyles"));
			string displayName3 = m_QuadStyles.serializedProperty.displayName;
			m_QuadStyles.drawHeaderCallback = (position) => EditorGUI.LabelField(position, displayName3);
			m_QuadStyles.drawElementCallback = OnDrawQuadStyleEntry;
			m_QuadStyles.onAddCallback = OnAddQuadStyle;
			m_CustomStyleTabContents.Add((int)CustomStyleTab.CustomTextStyles, DisplayCustomStyleTab);
			m_CustomStyleTabContents.Add((int)CustomStyleTab.LinkStyles, DisplayCustomStyleTab);
			m_CustomStyleTabContents.Add((int)CustomStyleTab.QuadStyles, DisplayCustomStyleTab);
			m_InheritedStyles =
				new ReorderableList(this.serializedObject, this.serializedObject.FindProperty("m_InheritedStyles"));
			string displayName4 = m_InheritedStyles.serializedProperty.displayName;
			m_InheritedStyles.drawHeaderCallback = (position) => EditorGUI.LabelField(position, displayName4);
			m_InheritedStyles.drawElementCallback = OnDrawInheritedStyleEntry;
			(this.target as HyperTextStyles).OnStylesChanged.AddListener(UpdateCustomStyleGUIContents);
			UpdateCustomStyleGUIContents();
		}

		/// <summary>
		/// Displays the current custom style tab.
		/// </summary>
		private void DisplayCustomStyleTab()
		{
			float currentlySelectedStyleHeight = 0f;
			ReorderableList list = null;
			switch (s_CustomStyleTabPreference.CurrentValue)
			{
			case CustomStyleTab.CustomTextStyles:
				list = m_CustomTextStyles;
				currentlySelectedStyleHeight = HyperTextTextStyleDrawer.propertyHeight;
				m_DisplayCurrentStyle = (pos) => OnDrawCustomTextStyleEntry(pos, list.index, true, true);
				break;
			case CustomStyleTab.LinkStyles:
				list = m_LinkStyles;
				currentlySelectedStyleHeight = HyperTextLinkSubclassDrawer.propertyHeight;
				m_DisplayCurrentStyle = (pos) => OnDrawLinkStyleEntry(pos, list.index, true, true);
				break;
			case CustomStyleTab.QuadStyles:
				list = m_QuadStyles;
				currentlySelectedStyleHeight = HyperTextQuadStyleDrawer.propertyHeight;
				m_DisplayCurrentStyle = (pos) => OnDrawQuadStyleEntry(pos, list.index, true, true);
				break;
			}
			if (list.count > 0)
			{
				EditorGUILayout.LabelField(
					string.Format("Modify Style {0}", Mathf.Max(0, list.index)), EditorStylesX.BoldTitleBar
				);
				EditorGUILayout.Space();
				if (m_DisplayCurrentStyle != null)
				{
					m_DisplayCurrentStyle(EditorGUILayout.GetControlRect(true, currentlySelectedStyleHeight));
				}
			}
			else
			{
				EditorGUILayout.LabelField("Modify Styles", EditorStylesX.BoldTitleBar);
				EditorGUILayout.HelpBox("Add some styles and edit them here.", MessageType.Info);
			}
			EditorGUILayout.LabelField("Select, Add, or Remove Styles", EditorStylesX.BoldTitleBar);
			EditorGUILayout.Space();
			if (list != null)
			{
				list.DoLayoutList();
			}
			if (
				!this.serializedObject.isEditingMultipleObjects &&
				m_ParentStyle != null &&
				m_CurrentTabInheritedStyleNames.Count > 0
			)
			{
				EditorGUILayout.LabelField("Inherited Styles", EditorStylesX.BoldTitleBar);
				foreach (string styleName in m_CurrentTabInheritedStyleNames)
				{
					EditorGUILayout.LabelField(styleName);
				}
			}
		}

		/// <summary>
		/// Raises the inspector GUI event.
		/// </summary>
		public override void OnInspectorGUI()
		{
			this.serializedObject.Update();
			EditorGUILayout.PropertyField(m_ScriptProperty);
			EditorGUILayout.PropertyField(m_NameProperty);
			EditorGUI.BeginChangeCheck();
			{
				m_InheritedStyles.DoLayoutList();
			}
			if (EditorGUI.EndChangeCheck())
			{
				UpdateCustomStyleGUIContents();
			}
			Rect position = EditorGUILayout.GetControlRect();
			HyperTextEditor.DisplayFontProperty(position, m_FontProperty, m_ParentStyle);
			position = EditorGUILayout.GetControlRect();
			DisplayOverridableProperty(position, m_FontSizeProperty, m_FontSizeOverrideProperty);
			position = EditorGUILayout.GetControlRect();
			DisplayOverridableProperty(position, m_LineSpacingProperty, m_LineSpacingOverrideProperty);
			position = EditorGUILayout.GetControlRect();
			DisplayOverridableProperty(position, m_DefaultFontStyleProperty, m_DefaultFontStyleOverrideProperty);
			position = EditorGUILayout.GetControlRect();
			DisplayOverridableProperty(position, m_DefaultTextColorProperty, m_DefaultTextColorOverrideProperty);
			DisplayOverridableProperty(
				EditorGUILayout.GetControlRect(true, HyperTextLinkStyleDrawer.PropertyHeight),
				m_DefaultLinkStyleProperty,
				m_DefaultLinkStyleOverrideProperty
			);
			position = EditorGUILayout.GetControlRect(true, EditorGUI.GetPropertyHeight(m_LinkHitboxPaddingProperty));
			DisplayOverridableProperty(position, m_LinkHitboxPaddingProperty, m_LinkHitboxPaddingOverrideProperty);
			EditorGUILayout.LabelField("Custom Styles");
			++EditorGUI.indentLevel;
			CustomStyleTab oldTab = s_CustomStyleTabPreference.CurrentValue;
			EditorGUI.BeginChangeCheck();
			{
				s_CustomStyleTabPreference.CurrentValue = (CustomStyleTab)EditorGUIX.DisplayTabGroup(
					(int)s_CustomStyleTabPreference.CurrentValue,
					s_CustomStyleTabLabels,
					m_CustomStyleTabContents
				);
			}
			if (EditorGUI.EndChangeCheck() && oldTab != s_CustomStyleTabPreference.CurrentValue)
			{
				UpdateCustomStyleGUIContents();
			}
			--EditorGUI.indentLevel;
			this.serializedObject.ApplyModifiedProperties();
		}

		/// <summary>
		/// Updates the validation statuses, labels, and tooltips for custom styles.
		/// </summary>
		private void UpdateCustomStyleGUIContents()
		{
			// rebuild inheritance
			m_InheritedLinks.Clear();
			m_InheritedQuads.Clear();
			m_InheritedTags.Clear();
			m_InheritTooltip = "";
			m_OverrideTooltip = "";
			HyperTextStyles styles = this.target as HyperTextStyles;
			styles.GetInheritedStyles(ref s_InheritedStyles);
			m_ParentStyle = s_InheritedStyles.Where(s => s != null).LastOrDefault();
			if (m_ParentStyle != null)
			{
				m_InheritedLinks = styles.GetInheritedLinkStyles();
				m_InheritedQuads = styles.GetInheritedQuadStyles();
				m_InheritedTags = styles.GetInheritedCustomTextStyles();
				m_InheritTooltip = string.Format("will inherit from {0}", m_ParentStyle.name);
				m_OverrideTooltip = string.Format("overriding style inherited from {0}", m_ParentStyle.name);
			}
			// rebuild tooltips and icons
			m_LinkGUIContents.Clear();
			List<HyperTextStyles.LinkSubclass> links = null;
			styles.GetLinkStyles(ref links);
			for (int i = 0; i < links.Count; ++i)
			{
				m_LinkGUIContents[i] = ValidateIdentifier(links[i].Identifier, links);
				m_LinkGUIContents[i].Label = new GUIContent(
					string.IsNullOrEmpty(links[i].Identifier) ?
						string.Format("Link Style {0}", i) : string.Format("<a class=\"{0}\">", links[i].Identifier)
				);
				UpdateValidatationStatusIfOverridingInheritedStyle(
					m_LinkGUIContents[i], links[i].Identifier, m_InheritedLinks
				);
			}
			m_QuadGUIContents.Clear();
			List<HyperTextStyles.Quad> quads = null;
			styles.GetQuadStyles(ref quads);
			for (int i = 0; i < quads.Count; ++i)
			{
				m_QuadGUIContents[i] = ValidateIdentifier(quads[i].Identifier, quads);
				m_QuadGUIContents[i].Label = new GUIContent(
					string.IsNullOrEmpty(quads[i].Identifier) ?
						string.Format("Quad Style {0}", i) : string.Format("<quad class=\"{0}\">", quads[i].Identifier)
				);
				if (
					m_QuadGUIContents[i].Status == ValidationStatus.Okay &&
					!string.IsNullOrEmpty(quads[i].LinkClassName) &&
					!string.IsNullOrEmpty(quads[i].LinkId)
				)
				{
					if (links.Count(link => link.Identifier.ToLower() == quads[i].ClassName.ToLower()) == 0)
					{
						m_QuadGUIContents[i].Status = ValidationStatus.Error;
						m_QuadGUIContents[i].StatusTooltip =
							string.Format("No link style with class name {0} exists.", quads[i].LinkClassName);
					}
				}
				UpdateValidatationStatusIfOverridingInheritedStyle(
					m_QuadGUIContents[i], quads[i].Identifier, m_InheritedQuads
				);
			}
			m_TagGUIContents.Clear();
			List<HyperTextStyles.Text> tags = null;
			styles.GetCustomTextStyles(ref tags);
			for (int i = 0; i < tags.Count; ++i)
			{
				m_TagGUIContents[i] = ValidateIdentifier(tags[i].Identifier, tags, "Tag");
				m_TagGUIContents[i].Label = new GUIContent(
					string.IsNullOrEmpty(tags[i].Identifier) ?
						string.Format("Text Style {0}", i) : string.Format("<{0}>", tags[i].Identifier)
				);
				UpdateValidatationStatusIfOverridingInheritedStyle(
					m_TagGUIContents[i], tags[i].Identifier, m_InheritedTags
				);
			}
			// update list of inherited style names based on current tab
			m_CurrentTabInheritedStyleNames.Clear();
			switch (s_CustomStyleTabPreference.CurrentValue)
			{
			case CustomStyleTab.CustomTextStyles:
				m_CurrentTabInheritedStyleNames = (
					from kv in m_InheritedTags
					where kv.Value != this.target
					select string.Format("{0} ({1})", kv.Key.Identifier, kv.Value.name)
				).ToList();
				break;
			case CustomStyleTab.LinkStyles:
				m_CurrentTabInheritedStyleNames = (
					from kv in m_InheritedLinks
					where kv.Value != this.target
					select string.Format("{0} ({1})", kv.Key.Identifier, kv.Value.name)
				).ToList();
				break;
			case CustomStyleTab.QuadStyles:
				m_CurrentTabInheritedStyleNames = (
					from kv in m_InheritedQuads
					where kv.Value != this.target
					select string.Format("{0} ({1})", kv.Key.Identifier, kv.Value.name)
				).ToList();
				break;
			}
		}

		/// <summary>
		/// Updates the validatation status if the identifier already eixsts in the table of inherited styles.
		/// </summary>
		/// <param name="guiContent">CustomStyleGUIContent for a particular style entry.</param>
		/// <param name="styleIdentifier">Style identifier.</param>
		/// <param name="inheritedStyles">Inherited styles.</param>
		/// <typeparam name="T">The style type.</typeparam>
		private void UpdateValidatationStatusIfOverridingInheritedStyle<T>(
			CustomStyleGUIContent guiContent, string styleIdentifier, Dictionary<T, HyperTextStyles> inheritedStyles
		) where T: IIdentifiable<string>
		{
			if (guiContent.Status != ValidationStatus.Okay)
			{
				return;
			}
			T existingStyle =
				inheritedStyles.Keys.Where(s => s.Identifier.ToLower() == styleIdentifier.ToLower()).FirstOrDefault();
			if (existingStyle.Identifier.ToLower() == styleIdentifier.ToLower())
			{
				guiContent.Status = ValidationStatus.Warning;
				guiContent.StatusTooltip = string.Format(
					"Overrides inherited style {0}.{1}.", inheritedStyles[existingStyle].name, existingStyle.Identifier
				);
			}
		}

		/// <summary>
		/// Validates the name of the identifier in the supplied list of styles.
		/// </summary>
		/// <returns>The validation info for the supplied identifier.</returns>
		/// <param name="identifier">Identifier to validate against the collection.</param>
		/// <param name="list">List.</param>
		/// <param name="identifierLabel">Label of the identifier to appear in the tooltip.</param>
		private CustomStyleGUIContent ValidateIdentifier<T>(
			string identifier, List<T> items, string identifierLabel = "Class name"
		)
			where T: IIdentifiable<string>
		{
			CustomStyleGUIContent result = new CustomStyleGUIContent();
			result.Label = new GUIContent();
			result.Status = string.IsNullOrEmpty(identifier) ? ValidationStatus.Error : ValidationStatus.Okay;
			result.StatusTooltip = string.IsNullOrEmpty(identifier) ?
				string.Format("{0} must be specified.", identifierLabel) : "";
			if (
				result.Status == ValidationStatus.Okay &&
				items.Where(t => t.Identifier.ToLower() == identifier.ToLower()).Count() > 1
			)
			{
				result.StatusTooltip = string.Format(
					"{0} \"{1}\" occurs more than once in the list of styles.", identifierLabel, identifier
				);
				result.Status = ValidationStatus.Error;
			}
			return result;
		}
	}
}	
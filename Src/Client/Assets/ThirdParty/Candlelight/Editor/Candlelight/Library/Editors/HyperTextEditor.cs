// 
// HyperTextEditor.cs
// 
// Copyright (c) 2014-2015, Candlelight Interactive, LLC
// All rights reserved.
// 
// This file is licensed according to the terms of the Unity Asset Store EULA:
// http://download.unity3d.com/assetstore/customer-eula.pdf
// 
// This file contains a custom editor for HyperText.

#if UNITY_4_6 || UNITY_5_0 || UNITY_5_1
#define IS_VBO_UI_VERTEX
#endif

using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
#if IS_VBO_UI_VERTEX
using System.Linq;
#endif

namespace Candlelight.UI
{
	/// <summary>
	/// A custom editor for <see cref="HyperText"/> objects.
	/// </summary>
	[CanEditMultipleObjects, CustomEditor(typeof(HyperText)), InitializeOnLoad]
	public class HyperTextEditor : UnityEditor.UI.TextEditor
	{
		/// <summary>
		/// An enum with different debug display modes
		/// </summary>
		public enum DebugSceneMode { None, VertexIndices }

		/// <summary>
		/// Initializes the <see cref="Candlelight.UI.HyperTextEditor"/> class.
		/// </summary>
		static HyperTextEditor()
		{
			EditorPreferenceMenu.AddPreferenceMenuItem(
				HyperTextDefineSymbols.preferenceMenuFeatureGroup, OnPreferenceMenuGUI
			);
			EditorPreferenceMenu.AddAssetStoreUrl(HyperTextDefineSymbols.preferenceMenuFeatureGroup, 21252);
			EditorPreferenceMenu.AddSupportForumUrl(HyperTextDefineSymbols.preferenceMenuFeatureGroup, "uas-hypertext");
		}

		#region Labels
		private static readonly GUIContent s_InputTextSourceLabel =
			new GUIContent("Override Text Source", "Assigning a text input source overrides the text on this object.");
		private static readonly GUIContent s_MaterialLabel = new GUIContent("Material");
		#endregion
		#region Shared Allocations
		private static List<Vector3> s_DebugSceneModeVertices = new List<Vector3>(4096);
		private static readonly GUIContent s_ReusableLabel = new GUIContent();
		#endregion
		/// <summary>
		/// The hitbox color preference.
		/// </summary>
		private static readonly EditorPreference<Color, HyperTextEditor> s_HitboxColorPreference =
			new EditorPreference<Color, HyperTextEditor>("hitboxesColor", Color.magenta);
		/// <summary>
		/// The hitbox toggle preference.
		/// </summary>
		private static readonly EditorPreference<bool, HyperTextEditor> s_HitboxTogglePreference =
			EditorPreference<bool, HyperTextEditor>.ForToggle("hitboxes", true);
		/// <summary>
		/// The debug mode preference
		/// </summary>
		private static readonly EditorPreference<DebugSceneMode, HyperTextEditor> s_DebugSceneModePreference =
			new EditorPreference<DebugSceneMode, HyperTextEditor>("debugSceneMode", DebugSceneMode.None);

		/// <summary>
		/// Creates a new HyperText in the scene.
		/// </summary>
		/// <param name="menuCommand">The menu command being executed.</param>
		[MenuItem("GameObject/UI/Candlelight/HyperText")]
		public static void CreateNew(MenuCommand menuCommand)
		{
			EditorApplication.ExecuteMenuItem("GameObject/UI/Text");
			UnityEngine.UI.Text text = Selection.activeGameObject.GetComponent<UnityEngine.UI.Text>();
			Color color = text.color;
			GameObject.DestroyImmediate(Selection.activeGameObject.GetComponent<UnityEngine.UI.Shadow>(), true);
			GameObject.DestroyImmediate(text, true);
			Selection.activeGameObject.name = "HyperText";
			HyperText hyperText = Selection.activeGameObject.AddComponent<HyperText>();
			hyperText.color = color;
			hyperText.text = "New <a name=\"link\">HyperText</a>";
			// BUG: for some reason parenting behavior is not inherited when executing built-in menu command
			GameObject parent = menuCommand.context as GameObject;
			if (parent != null && parent.GetComponentInParent<Canvas>() != null)
			{
#if !UNITY_4_6
				hyperText.gameObject.name =
					GameObjectUtility.GetUniqueNameForSibling(parent.transform, hyperText.gameObject.name);
#endif
				GameObjectUtility.SetParentAndAlign(hyperText.gameObject, parent);
			}
		}

		/// <summary>
		/// Displays a font property field.
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="fontProperty">Font property.</param>
		/// <param name="inheritingFrom">Style sheet from which the font is potentially inheriting.</param>
		public static void DisplayFontProperty(
			Rect position, SerializedProperty fontProperty, HyperTextStyles inheritingFrom
		)
		{
			ValidationStatus status = ValidationStatus.None;
			s_ReusableLabel.text = fontProperty.displayName;
			s_ReusableLabel.tooltip = string.Empty;
			if (fontProperty.objectReferenceValue == null)
			{
				if (inheritingFrom != null && inheritingFrom.CascadedFont != null)
				{
					s_ReusableLabel.tooltip = string.Format(
						"Inheriting Font {0} from {1}.", inheritingFrom.CascadedFont.name, inheritingFrom.name
					);
				}
				else
				{
					s_ReusableLabel.tooltip = "Font cannot be null.";
					status = ValidationStatus.Error;
				}
			}
			else if (!(fontProperty.objectReferenceValue as Font).dynamic)
			{
				s_ReusableLabel.tooltip = "Font size and style settings are only supported for dynamic fonts. " +
					"Only colors and offsets will be applied.";
				status = ValidationStatus.Warning;
			}
			else if (inheritingFrom != null && inheritingFrom.CascadedFont != null)
			{
				s_ReusableLabel.tooltip = string.Format(
					"Overriding Font {0} inherited from {1}.", inheritingFrom.CascadedFont.name, inheritingFrom.name
				);
				status = ValidationStatus.Warning;
			}
			if (
				string.IsNullOrEmpty(s_ReusableLabel.tooltip) &&
				inheritingFrom != null &&
				inheritingFrom.CascadedFont != null
			)
			{
				s_ReusableLabel.tooltip = string.Format(
					"Assign a value to override Font {0} inherited from {1}",
					inheritingFrom.CascadedFont.name,
					inheritingFrom.name
				);
			}
			switch (status)
			{
			case ValidationStatus.None:
				EditorGUI.PropertyField(position, fontProperty, s_ReusableLabel);
				break;
			default:
				EditorGUIX.DisplayPropertyFieldWithStatus(
					position, fontProperty, status, s_ReusableLabel, false, s_ReusableLabel.tooltip
				);
				break;
			}
		}

		/// <summary>
		/// Displays a property field with an override property checkbox and status icon if styles are assigned.
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="property">Property to display.</param>
		/// <<param name="overrideProperty">
		/// Property specifying whether the other property is overriding an inherited one. Object reference properties
		/// are assumed to override if they have a value assigned.
		/// </param>
		/// <param name="stylesProperty">Property with the reference to a style sheet.</param>
		public static void DisplayOverridableProperty(
			Rect position,
			SerializedProperty property,
			SerializedProperty overrideProperty,
			SerializedProperty stylesProperty
		)
		{
			DisplayOverridableProperty(
				position: position,
				property: property,
				overrideProperty: overrideProperty,
				displayCheckbox:
					stylesProperty.objectReferenceValue != null || stylesProperty.hasMultipleDifferentValues,
				inheritTooltip: "controlled by styles on this object",
				overrideTooltip: "overridden by this object"
			);
		}

		/// <summary>
		/// Displays a property field with an optional override property checkbox and status icon as needed.
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="property">Property.</param>
		/// <param name="overrideProperty">
		/// Property specifying whether the other property is overriding an inherited one. Object reference properties
		/// are assumed to override if they have a value assigned.
		/// </param>
		/// <param name="displayCheckbox">If set to <see langword="true"/> display checkbox/tooltip as needed.</param>
		/// <param name="inheritTooltip">Predicate fragment of the tooltip when property is inheriting.</param>
		/// <param name="overrideTooltip">Predicate fragment of the tooltip when property is overridden.</param>
		public static void DisplayOverridableProperty(
			Rect position,
			SerializedProperty property,
			SerializedProperty overrideProperty,
			bool displayCheckbox,
			string inheritTooltip,
			string overrideTooltip
		)
		{
			if (displayCheckbox)
			{
				ValidationStatus status = ValidationStatus.None;
				if (property.propertyType == SerializedPropertyType.ObjectReference)
				{
					status = property.objectReferenceValue == null ? ValidationStatus.None : ValidationStatus.Warning;
				}
				else
				{
					status = overrideProperty.hasMultipleDifferentValues || overrideProperty.boolValue ?
						ValidationStatus.Warning : ValidationStatus.None;
				}
				s_ReusableLabel.text = property.displayName;
				s_ReusableLabel.tooltip = string.Format(
					property.serializedObject.isEditingMultipleObjects ?
						"{0} {1} on at least one selected object." : "{0} {1}.",
					property.displayName,
					status == ValidationStatus.Warning ? overrideTooltip : inheritTooltip
				);
				if (status != ValidationStatus.None)
				{
					Rect iconPosition = position;
					iconPosition.x += position.width - EditorGUIUtility.singleLineHeight;
					iconPosition.width = iconPosition.height = EditorGUIUtility.singleLineHeight;
					if (property.propertyType != SerializedPropertyType.Generic)
					{
						position.width -= iconPosition.width;
					}
					EditorGUIX.DisplayValidationStatusIcon(iconPosition, status, s_ReusableLabel.tooltip);
				}
				if (property.propertyType == SerializedPropertyType.ObjectReference)
				{
					EditorGUI.PropertyField(position, property, s_ReusableLabel);
				}
				else
				{
					EditorGUIX.DisplayPropertyWithToggle(position, s_ReusableLabel, overrideProperty, property);
				}
			}
			else
			{
				EditorGUI.PropertyField(position, property);
			}
		}

		/// <summary>
		/// Raises the preference menu GUI event.
		/// </summary>
		private static void OnPreferenceMenuGUI()
		{
			EditorGUIX.DisplayHandlePropertyEditor<HyperTextEditor>(
				"Hitboxes", s_HitboxTogglePreference, s_HitboxColorPreference
			);
			EditorGUI.BeginChangeCheck();
			{
				s_DebugSceneModePreference.CurrentValue = (DebugSceneMode)EditorGUILayout.EnumPopup(
					"Debug Scene Mode", s_DebugSceneModePreference.CurrentValue
				);
			}
			if (EditorGUI.EndChangeCheck())
			{
				SceneView.RepaintAll();
			}
		}

		#region Serialized Properties
		private SerializedProperty m_AlignmentProperty;
		private SerializedProperty m_BestFitProperty;
		private SerializedProperty m_ColorProperty;
		private SerializedProperty m_FontDataProperty;
		private SerializedProperty m_FontProperty;
		private SerializedProperty m_FontSizeProperty;
		private SerializedProperty m_FontStyleProperty;
		private SerializedProperty m_HorizontalOverflowProperty;
		private SerializedProperty m_InputTextSourceProperty;
		private SerializedProperty m_InteractableProperty;
		private SerializedProperty m_LinkHitboxPaddingProperty;
		private SerializedProperty m_LineSpacingProperty;
		private ReorderableList m_LinkKeywordCollections = null;
		private SerializedProperty m_MaterialProperty;
		private SerializedProperty m_OnClickProperty;
		private SerializedProperty m_OnEnterProperty;
		private SerializedProperty m_OnExitProperty;
		private SerializedProperty m_OnPressProperty;
		private SerializedProperty m_OnReleaseProperty;
		private SerializedProperty m_OverrideFontColorProperty;
		private SerializedProperty m_OverrideFontStyleProperty;
		private SerializedProperty m_OverrideFontSizeProperty;
		private SerializedProperty m_OverrideLineSpacingProperty;
		private SerializedProperty m_OverrideLinkHitboxProperty;
		private ReorderableList m_QuadKeywordCollections = null;
		private SerializedProperty m_RichTextProperty;
		private SerializedProperty m_ScriptProperty;
		private SerializedProperty m_StylesProperty;
		private ReorderableList m_TagKeywordCollections = null;
		private SerializedProperty m_TextProperty;
		private SerializedProperty m_TextProcessorProperty;
		private SerializedProperty m_VerticalOverflowProperty;
		#endregion
		/// <summary>
		/// All keyword collections assigned to this object in some list or other.
		/// </summary>
		private IEnumerable<KeywordCollection> m_AssignedCollections = null;
		/// <summary>
		/// The index of the currently highlighted vertex.
		/// </summary>
		private int m_CurrentlyHighlightedVertexIndex = 0;
		/// <summary>
		/// The link hitboxes.
		/// </summary>
		private Dictionary<HyperText.LinkInfo, List<Rect>> m_LinkHitboxes =
			new Dictionary<HyperText.LinkInfo, List<Rect>>();
		
		/// <summary>
		/// Displays the vertex indices in the scene.
		/// </summary>
		/// <param name="hyperText">Hyper text.</param>
		private void DisplayVertexIndices(HyperText hyperText)
		{
			System.Collections.Generic.List<UIVertex> vertices =
				hyperText.GetFieldValue<System.Collections.Generic.List<UIVertex>>("uiVertices");
			for (int index = 0; index < vertices.Count; ++index)
			{
				Handles.Label(vertices[index].position, index.ToString());
			}
		}

		/// <summary>
		/// Encircle the specified world position and label the index at the top of the scene view.
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="index">Index.</param>
		private void HighlightIndex(Vector3 position, int index)
		{
			Color oldColor = Handles.color;
			Handles.color = Color.green;
			Handles.DrawWireDisc(position, Vector3.forward, 2f);
			Handles.color = oldColor;
			Handles.BeginGUI();
			{
				oldColor = GUI.color;
				GUI.color = Color.green;
				Rect rect = Camera.current.pixelRect;
				rect.x += rect.width * 0.5f - 20f;
				rect.width = 40f;
				GUI.Label(rect, index.ToString(), EditorStylesX.BoldLabel);
				GUI.color = oldColor;
			}
			Handles.EndGUI();
		}

		/// <summary>
		/// Raises the enable event.
		/// </summary>
		new private void OnEnable()
		{
			base.OnEnable();
			m_InteractableProperty = this.serializedObject.FindProperty("m_Interactable");
			m_ColorProperty = this.serializedObject.FindProperty("m_Color");
			m_FontDataProperty = this.serializedObject.FindProperty("m_FontData");
			m_FontProperty = m_FontDataProperty.FindPropertyRelative("m_Font");
			m_FontStyleProperty = m_FontDataProperty.FindPropertyRelative("m_FontStyle");
			m_FontSizeProperty = m_FontDataProperty.FindPropertyRelative("m_FontSize");
			m_LineSpacingProperty = m_FontDataProperty.FindPropertyRelative("m_LineSpacing");
			m_RichTextProperty = m_FontDataProperty.FindPropertyRelative("m_RichText");
			m_AlignmentProperty = m_FontDataProperty.FindPropertyRelative("m_Alignment");
			m_HorizontalOverflowProperty = m_FontDataProperty.FindPropertyRelative("m_HorizontalOverflow");
			m_VerticalOverflowProperty = m_FontDataProperty.FindPropertyRelative("m_VerticalOverflow");
			m_BestFitProperty = m_FontDataProperty.FindPropertyRelative("m_BestFit");
			m_LinkHitboxPaddingProperty = this.serializedObject.FindProperty("m_LinkHitboxPadding");
			m_MaterialProperty = this.serializedObject.FindProperty("m_Material");
			m_OnClickProperty = this.serializedObject.FindProperty("m_OnClick");
			m_OnEnterProperty = this.serializedObject.FindProperty("m_OnEnter");
			m_OnExitProperty = this.serializedObject.FindProperty("m_OnExit");
			m_OnPressProperty = this.serializedObject.FindProperty("m_OnPress");
			m_OnReleaseProperty = this.serializedObject.FindProperty("m_OnRelease");
			m_OverrideFontColorProperty = this.serializedObject.FindProperty("m_ShouldOverrideStylesFontColor");
			m_OverrideFontStyleProperty = this.serializedObject.FindProperty("m_ShouldOverrideStylesFontStyle");
			m_OverrideFontSizeProperty =
				this.serializedObject.FindProperty("m_TextProcessor.m_ShouldOverrideStylesFontSize");
			m_OverrideLineSpacingProperty = this.serializedObject.FindProperty("m_ShouldOverrideStylesLineSpacing");
			m_OverrideLinkHitboxProperty =
				this.serializedObject.FindProperty("m_ShouldOverrideStylesLinkHitboxPadding");
			m_ScriptProperty = this.serializedObject.FindProperty("m_Script");
			m_StylesProperty = this.serializedObject.FindProperty("m_TextProcessor.m_Styles");
			m_TextProperty = this.serializedObject.FindProperty("m_Text");
			m_TextProcessorProperty = this.serializedObject.FindProperty("m_TextProcessor");
			m_InputTextSourceProperty = this.serializedObject.FindProperty("m_TextProcessor.m_InputTextSourceObject");
			m_LinkKeywordCollections = new ReorderableList(
				this.serializedObject, this.serializedObject.FindProperty("m_TextProcessor.m_LinkKeywordCollections")
			);
			string displayName1 = m_LinkKeywordCollections.serializedProperty.displayName;
			m_LinkKeywordCollections.drawHeaderCallback = (position) => EditorGUI.LabelField(position, displayName1);
			m_LinkKeywordCollections.drawElementCallback = (position, index, isActive, isFocused) =>
				HyperTextProcessorDrawer.OnDrawLinkKeywordCollectionsEntry(
					position, index, m_TextProcessorProperty, () => m_AssignedCollections
				);
			m_QuadKeywordCollections = new ReorderableList(
				this.serializedObject, this.serializedObject.FindProperty("m_TextProcessor.m_QuadKeywordCollections")
			);
			string displayName2 = m_QuadKeywordCollections.serializedProperty.displayName;
			m_QuadKeywordCollections.drawHeaderCallback = (position) => EditorGUI.LabelField(position, displayName2);
			m_QuadKeywordCollections.drawElementCallback = (position, index, isActive, isFocused) =>
				HyperTextProcessorDrawer.OnDrawQuadKeywordCollectionsEntry(
					position, index, m_TextProcessorProperty, () => m_AssignedCollections
				);
			m_TagKeywordCollections = new ReorderableList(
				this.serializedObject, this.serializedObject.FindProperty("m_TextProcessor.m_TagKeywordCollections")
			);
			string displayName3 = m_TagKeywordCollections.serializedProperty.displayName;
			m_TagKeywordCollections.drawHeaderCallback = (position) => EditorGUI.LabelField(position, displayName3);
			m_TagKeywordCollections.drawElementCallback = (position, index, isActive, isFocused) =>
				HyperTextProcessorDrawer.OnDrawTagKeywordCollectionsEntry(
					position, index, m_TextProcessorProperty, () => m_AssignedCollections
				);
		}

		/// <summary>
		/// Raises the inspector GUI event.
		/// </summary>
		public override void OnInspectorGUI()
		{
			this.serializedObject.Update();
			m_AssignedCollections = HyperTextProcessorDrawer.GetAllCollections(m_TextProcessorProperty);
			EditorGUILayout.PropertyField(m_ScriptProperty);
			EditorGUILayout.PropertyField(m_InteractableProperty);
			Rect position =
				EditorGUILayout.GetControlRect(true, EditorGUI.GetPropertyHeight(m_LinkHitboxPaddingProperty));
			DisplayOverridableProperty(
				position, m_LinkHitboxPaddingProperty, m_OverrideLinkHitboxProperty, m_StylesProperty
			);
			bool hadStyles = m_StylesProperty.objectReferenceValue != null;
			if (EditorGUIX.DisplayScriptableObjectPropertyFieldWithButton<HyperTextStyles>(m_StylesProperty))
			{
				HyperTextStyles newStyles = m_StylesProperty.objectReferenceValue as HyperTextStyles;
				if (newStyles != null && m_FontDataProperty.FindPropertyRelative("m_Font").objectReferenceValue != null)
				{
					newStyles.Font = m_FontDataProperty.FindPropertyRelative("m_Font").objectReferenceValue as Font;
				}
			}
			if (
				!hadStyles &&
				m_StylesProperty.objectReferenceValue != null &&
				(m_StylesProperty.objectReferenceValue as HyperTextStyles).CascadedFont != null
			)
			{
				m_FontDataProperty.FindPropertyRelative("m_Font").objectReferenceValue = null;
			}
			// NOTE: LayoutList() doesn't use proper vertical spacing
			Rect rect = EditorGUILayout.GetControlRect(false, m_LinkKeywordCollections.GetHeight());
			m_LinkKeywordCollections.DoList(rect);
			rect = EditorGUILayout.GetControlRect(false, m_TagKeywordCollections.GetHeight());
			m_TagKeywordCollections.DoList(rect);
			rect = EditorGUILayout.GetControlRect(false, m_QuadKeywordCollections.GetHeight());
			m_QuadKeywordCollections.DoList(rect);
			bool isTextInputSourceAssigned =
				m_InputTextSourceProperty.objectReferenceValue != null ||
				(this.target as HyperText).InputTextSource != null;
			EditorGUI.BeginDisabledGroup(isTextInputSourceAssigned);
			{
				EditorGUILayout.PropertyField(m_TextProperty);
			}
			EditorGUI.EndDisabledGroup();
			if (isTextInputSourceAssigned)
			{
				EditorGUIX.DisplayPropertyFieldWithStatus(
					m_InputTextSourceProperty,
					ValidationStatus.Warning,
					s_InputTextSourceLabel,
					false,
					s_InputTextSourceLabel.tooltip
				);
			}
			else
			{
				EditorGUILayout.PropertyField(m_InputTextSourceProperty, s_InputTextSourceLabel);
			}
			EditorGUILayout.LabelField("Character", EditorStyles.boldLabel);
			++EditorGUI.indentLevel;
			position = EditorGUILayout.GetControlRect();
			DisplayFontProperty(position, m_FontProperty, m_StylesProperty.objectReferenceValue as HyperTextStyles);
			position = EditorGUILayout.GetControlRect();
			DisplayOverridableProperty(position, m_FontStyleProperty, m_OverrideFontStyleProperty, m_StylesProperty);
			position = EditorGUILayout.GetControlRect();
			DisplayOverridableProperty(position, m_FontSizeProperty, m_OverrideFontSizeProperty, m_StylesProperty);
			position = EditorGUILayout.GetControlRect();
			DisplayOverridableProperty(
				position, m_LineSpacingProperty, m_OverrideLineSpacingProperty, m_StylesProperty
			);
			EditorGUILayout.PropertyField(m_RichTextProperty);
			--EditorGUI.indentLevel;
			EditorGUILayout.LabelField("Paragraph", EditorStyles.boldLabel);
			++EditorGUI.indentLevel;
			EditorGUILayout.PropertyField(m_AlignmentProperty);
			EditorGUILayout.PropertyField(m_HorizontalOverflowProperty);
			EditorGUILayout.PropertyField(m_VerticalOverflowProperty);
			EditorGUILayout.PropertyField(m_BestFitProperty);
			--EditorGUI.indentLevel;
			position = EditorGUILayout.GetControlRect();
			DisplayOverridableProperty(position, m_ColorProperty, m_OverrideFontColorProperty, m_StylesProperty);
			EditorGUILayout.PropertyField(m_MaterialProperty, s_MaterialLabel);
			EditorGUILayout.LabelField("Events", EditorStyles.boldLabel);
			EditorGUILayout.PropertyField(m_OnClickProperty);
			EditorGUILayout.PropertyField(m_OnEnterProperty);
			EditorGUILayout.PropertyField(m_OnExitProperty);
			EditorGUILayout.PropertyField(m_OnPressProperty);
			EditorGUILayout.PropertyField(m_OnReleaseProperty);
			this.serializedObject.ApplyModifiedProperties();
		}

		/// <summary>
		/// Raises the scene GUI event.
		/// </summary>
		void OnSceneGUI()
		{
			if (!s_HitboxTogglePreference.CurrentValue)
			{
				return;
			}
			Matrix4x4 oldMatrix = Handles.matrix;
			Color oldColor = Handles.color;
			Handles.color = s_HitboxColorPreference.CurrentValue;
			int[] wrap = new int[] { 1, 2, 3, 0 };
			HyperText hyperText;
			foreach (GameObject go in Selection.gameObjects)
			{
				hyperText = go.GetComponent<HyperText>();
				if (hyperText == null)
				{
					continue;
				}
				Handles.matrix = hyperText.transform.localToWorldMatrix;
				hyperText.GetLinkHitboxes(ref m_LinkHitboxes);
				foreach (KeyValuePair<HyperText.LinkInfo, List<Rect>> linkHitboxes in m_LinkHitboxes)
				{
					Vector2 center = Vector2.zero;
					foreach (Rect hitbox in linkHitboxes.Value)
					{
						Vector3[] vertices = new Vector3[]
						{
							Vector3.right * hitbox.xMin + Vector3.up * hitbox.yMax,
							Vector3.right * hitbox.xMax + Vector3.up * hitbox.yMax,
							Vector3.right * hitbox.xMax + Vector3.up * hitbox.yMin,
							Vector3.right * hitbox.xMin + Vector3.up * hitbox.yMin,
						};
						// draw a box around each hitbox
						for (int i = 0; i < vertices.Length; ++i)
						{
							Handles.DrawLine(vertices[i], vertices[wrap[i]]);
						}
						center += hitbox.center;
					}
					center /= linkHitboxes.Value.Count;
					// indicate the name for each link
					Handles.Label(
						center,
						string.Format(
							"{0}{1}",
							linkHitboxes.Key.Name,
							string.IsNullOrEmpty(linkHitboxes.Key.ClassName) ?
								"" : string.Format(" ({0})", linkHitboxes.Key.ClassName)
						)
					);
				}
				int scrollAmt = 0;
				m_CurrentlyHighlightedVertexIndex =
					Mathf.Clamp(m_CurrentlyHighlightedVertexIndex, 0, s_DebugSceneModeVertices.Count);
				if (s_DebugSceneModePreference.CurrentValue == DebugSceneMode.VertexIndices)
				{
					scrollAmt = 1;
					s_DebugSceneModeVertices.Clear();
#if IS_VBO_UI_VERTEX
					s_DebugSceneModeVertices.AddRange(
						from v in hyperText.GetFieldValue<List<UIVertex>>("m_UIVertices") select v.position
					);
#else
					s_DebugSceneModeVertices.AddRange(hyperText.GetFieldValue<Mesh>("m_GlyphMesh").vertices);
#endif
					for (int i = 0; i < s_DebugSceneModeVertices.Count; ++i)
					{
						Handles.Label(s_DebugSceneModeVertices[i], i.ToString());
						if (i == m_CurrentlyHighlightedVertexIndex)
						{
							HighlightIndex(s_DebugSceneModeVertices[i], i);
						}
					}
					if (Event.current.isKey && Event.current.type == EventType.KeyDown)
					{
						switch (Event.current.keyCode)
						{
						case KeyCode.Comma:
						case KeyCode.Less:
							scrollAmt *= -1 * (Event.current.shift ? 12 : 1);
							break;
						case KeyCode.Period:
						case KeyCode.Greater:
							scrollAmt *= 1 * (Event.current.shift ? 12 : 1);
							break;
						default:
							scrollAmt = 0;
							break;
						}
						Event.current.Use();
						m_CurrentlyHighlightedVertexIndex = ArrayX.ScrollArrayIndex(
							m_CurrentlyHighlightedVertexIndex, s_DebugSceneModeVertices.Count, scrollAmt
						);
					}
				}
			}
			Handles.color = oldColor;
			Handles.matrix = oldMatrix;
		}
	}
}
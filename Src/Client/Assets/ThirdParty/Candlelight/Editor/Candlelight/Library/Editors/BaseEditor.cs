// 
// BaseEditor.cs
// 
// Copyright (c) 2015, Candlelight Interactive, LLC
// All rights reserved.
// 
// This file is licensed according to the terms of the Unity Asset Store EULA:
// http://download.unity3d.com/assetstore/customer-eula.pdf
// 
// This file contains a base class for custom editors that implement complex
// inspectors and/or scene GUI.

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Candlelight
{
	/// <summary>
	/// A utility class to register all <see cref="BaseEditor"/> classes in the editor preferences menu.
	/// </summary>
	[InitializeOnLoad]
	public static class BaseEditorUtility
	{
		/// <summary>
		/// Initializes the <see cref="BaseEditorUtility"/> class.
		/// </summary>
		static BaseEditorUtility()
		{
			foreach (System.Type type in ReflectionX.AllTypes)
			{
				if (!type.IsAbstract && typeof(BaseEditor).IsAssignableFrom(type))
				{
					MethodInfo initializeMethod = type.GetMethod("InitializeClass", ReflectionX.staticBindingFlags);
					if (initializeMethod != null)
					{
						initializeMethod.Invoke(null, null);
					}
					PropertyInfo featureGroupProp = type.GetProperty("FeatureGroup", ReflectionX.staticBindingFlags);
					if (featureGroupProp == null)
					{
						continue;
					}
					string featureGroup = featureGroupProp.GetValue(null, null) as string;
					if (string.IsNullOrEmpty(featureGroup))
					{
						continue;
					}
					MethodInfo prefMenuMethod =
						type.GetMethod("DisplayHandlePreferences", ReflectionX.staticBindingFlags);
					if (prefMenuMethod == null || prefMenuMethod.DeclaringType != type)
					{
						continue;
					}
					EditorPreferenceMenu.AddPreferenceMenuItem(featureGroup, prefMenuMethod);
				}
			}
		}
	}

	/// <summary>
	/// Base editor class for objects to register preferences and scene GUI callbacks.
	/// </summary>
	public abstract class BaseEditor : Editor, ISceneGUIContext
	{
		/// <summary>
		/// Gets the feature group. Override this in a subclass to specify its location in the preference menu.
		/// </summary>
		/// <value>The feature group.</value>
		protected static string FeatureGroup { get { return string.Empty; } }

		/// <summary>
		/// Displays the handle preferences. They will be displayed in the preference menu and the top of the inspector.
		/// </summary>
		protected static void DisplayHandlePreferences()
		{

		}

		/// <summary>
		/// Initializes the class. Override this method to perform any special functions when the class is loaded.
		/// </summary>
		protected static void InitializeClass()
		{
			
		}

		/// <summary>
		/// Static method for displaying handle preferences.
		/// </summary>
		private MethodInfo m_DisplayHandlePreferencesMethod;
		/// <summary>
		/// A table of serialized object representations of the currently selected objects.
		/// </summary>
		private readonly Dictionary<Object, SerializedObject> m_InspectedObjects =
			new Dictionary<Object, SerializedObject>();

		#region Backing Fields
		private Object m_FirstTarget;
		#endregion

		/// <summary>
		/// Gets the first target. This should be a value cached in OnEnable(), as invoking Editor.targets inside of the
		/// OnSceneGUI() callback logs an error message.
		/// </summary>
		/// <value>The first target.</value>
		public Object FirstTarget { get { return m_FirstTarget; } }
		/// <summary>
		/// Gets the handle matrix.
		/// </summary>
		/// <value>The handle matrix.</value>
		protected virtual Matrix4x4 HandleMatrix { get { return Matrix4x4.identity; } }
		/// <summary>
		/// Gets a value indicating whether this <see cref="BaseEditor{T}"/> implements a scene GUI handles.
		/// </summary>
		/// <value><see langword="true"/> if implements a scene GUI handles; otherwise, <see langword="false"/>.</value>
		protected abstract bool ImplementsSceneGUIHandles { get; }
		/// <summary>
		/// Gets a value indicating whether this <see cref="BaseEditor{T}"/> implements a scene GUI overlay.
		/// </summary>
		/// <value><see langword="true"/> if implements a scene GUI overlay; otherwise, <see langword="false"/>.</value>
		protected abstract bool ImplementsSceneGUIOverlay { get; }
		/// <summary>
		/// The Editor calling SceneGUI.Display().
		/// </summary>
		/// <value>The Editor calling SceneGUI.Display().</value>
		public Editor SceneGUIContext { get { return this; } }
		/// <summary>
		/// The current target object represented as a serialized object. Use this property when interacting with
		/// serialized properties from within OnSceneGUI() to prevent errors related to accessing targets array.
		/// </summary>
		/// <value>The serialized target.</value>
		protected SerializedObject SerializedTarget { get { return m_InspectedObjects[this.target]; } }
		
		/// <summary>
		/// Displays the inspector.
		/// </summary>
		protected virtual void DisplayInspector()
		{
			base.OnInspectorGUI();
		}
		
		/// <summary>
		/// Displays the scene GUI controls. This group appears after handle toggles.
		/// </summary>
		protected virtual void DisplaySceneGUIControls()
		{

		}
		
		/// <summary>
		/// Displays the scene GUI handle toggles. This group appears at the top of the scene GUI overlay.
		/// </summary>
		protected virtual void DisplaySceneGUIHandleToggles()
		{
			
		}
		
		/// <summary>
		/// Displays the scene GUI handles.
		/// </summary>
		protected virtual void DisplaySceneGUIHandles()
		{
			
		}

		/// <summary>
		/// Displays a field for a property in the scene GUI.
		/// </summary>
		/// <param name="propertyPath">Property path.</param>
		protected void DisplaySceneGUIPropertyField(string propertyPath)
		{
			EditorGUI.BeginChangeCheck();
			{
				EditorGUIX.DisplayPropertyField(m_InspectedObjects[this.target].FindProperty(propertyPath));
			}
			if (EditorGUI.EndChangeCheck())
			{
				SerializedObject so = new SerializedObject(m_InspectedObjects.Keys.ToArray());
				switch (so.FindProperty(propertyPath).propertyType)
				{
				case SerializedPropertyType.AnimationCurve:
					so.FindProperty(propertyPath).animationCurveValue =
						m_InspectedObjects[this.target].FindProperty(propertyPath).animationCurveValue;
					break;
				case SerializedPropertyType.ArraySize:
				case SerializedPropertyType.Integer:
				case SerializedPropertyType.LayerMask:
				case SerializedPropertyType.Character:
					so.FindProperty(propertyPath).intValue =
						m_InspectedObjects[this.target].FindProperty(propertyPath).intValue;
					break;
				case SerializedPropertyType.Boolean:
					so.FindProperty(propertyPath).boolValue =
						m_InspectedObjects[this.target].FindProperty(propertyPath).boolValue;
					break;
				case SerializedPropertyType.Bounds:
					so.FindProperty(propertyPath).boundsValue =
						m_InspectedObjects[this.target].FindProperty(propertyPath).boundsValue;
					break;
				case SerializedPropertyType.Color:
					so.FindProperty(propertyPath).colorValue =
						m_InspectedObjects[this.target].FindProperty(propertyPath).colorValue;
					break;
				case SerializedPropertyType.Enum:
					so.FindProperty(propertyPath).enumValueIndex =
						m_InspectedObjects[this.target].FindProperty(propertyPath).enumValueIndex;
					break;
				case SerializedPropertyType.Float:
					so.FindProperty(propertyPath).floatValue =
						m_InspectedObjects[this.target].FindProperty(propertyPath).floatValue;
					break;
				case SerializedPropertyType.Generic:
					Debug.LogError("Generic properties not implemented.");
					break;
				case SerializedPropertyType.Gradient:
					Debug.LogError("Gradient properties not implemented");
					break;
				case SerializedPropertyType.ObjectReference:
					so.FindProperty(propertyPath).objectReferenceValue =
						m_InspectedObjects[this.target].FindProperty(propertyPath).objectReferenceValue;
					break;
				case SerializedPropertyType.Quaternion:
					so.FindProperty(propertyPath).quaternionValue =
						m_InspectedObjects[this.target].FindProperty(propertyPath).quaternionValue;
					break;
				case SerializedPropertyType.Rect:
					so.FindProperty(propertyPath).rectValue =
						m_InspectedObjects[this.target].FindProperty(propertyPath).rectValue;
					break;
				case SerializedPropertyType.String:
					so.FindProperty(propertyPath).stringValue =
						m_InspectedObjects[this.target].FindProperty(propertyPath).stringValue;
					break;
				case SerializedPropertyType.Vector2:
					so.FindProperty(propertyPath).vector2Value =
						m_InspectedObjects[this.target].FindProperty(propertyPath).vector2Value;
					break;
				case SerializedPropertyType.Vector3:
					so.FindProperty(propertyPath).vector3Value =
						m_InspectedObjects[this.target].FindProperty(propertyPath).vector3Value;
					break;
				case SerializedPropertyType.Vector4:
					so.FindProperty(propertyPath).vector4Value =
						m_InspectedObjects[this.target].FindProperty(propertyPath).vector4Value;
					break;
				}
				so.ApplyModifiedProperties();
			}
		}

		/// <summary>
		/// Gets the cached targets. Use this method when interacting with serialized properties from within
		/// OnSceneGUI() to prevent errors related to accessing targets array.
		/// </summary>
		/// <returns>The cached targets.</returns>
		protected Object[] GetCachedTargets()
		{
			return m_InspectedObjects.Keys.ToArray();
		}

		/// <summary>
		/// Raises the disable event.
		/// </summary>
		protected virtual void OnDisable()
		{
			SceneGUI.DeregisterObjectGUICallback(this as ISceneGUIContext);
			Undo.undoRedoPerformed -= UpdateGUIContents;
		}

		/// <summary>
		/// Raises the enable event.
		/// </summary>
		protected virtual void OnEnable()
		{
			m_DisplayHandlePreferencesMethod = GetType().GetMethod(
				"DisplayHandlePreferences", ReflectionX.staticBindingFlags
			);
			m_FirstTarget = this.target;
			foreach (Object t in this.targets)
			{
				if (t != null)
				{
					m_InspectedObjects.Add(t, new SerializedObject(t));
				}
			}
			if (this.ImplementsSceneGUIOverlay)
			{
				SceneGUI.RegisterObjectGUICallback(this as ISceneGUIContext, OnSceneGUIOverlay);
			}
			Undo.undoRedoPerformed += UpdateGUIContents;
		}
		
		/// <summary>
		/// Raises the inspector GUI event.
		/// </summary>
		public override void OnInspectorGUI()
		{
			// early out if the target is null, e.g., if it was destroyed in an earlier callback this frame
			if (this.target == null)
			{
				return;
			}
			if (this.ImplementsSceneGUIOverlay || this.ImplementsSceneGUIHandles)
			{
				if (EditorGUIX.BeginSceneGUIControlsArea())
				{
					m_DisplayHandlePreferencesMethod.Invoke(null, null);
				}
				EditorGUIX.EndSceneGUIControlsArea();
			}
			DisplayInspector();
		}

		/// <summary>
		/// Raises the scene GUI event.
		/// </summary>
		protected virtual void OnSceneGUI()
		{
			// early out if the target is null, e.g., if it was destroyed in an earlier callback this frame or if scene gui is disabled
			if (this.target == null || !SceneGUI.IsEnabled)
			{
				return;
			}
			if (this.ImplementsSceneGUIOverlay)
			{
				SceneGUI.Display(this);
			}
			if (this.ImplementsSceneGUIHandles)
			{
				Color oldColor = Handles.color;
				Matrix4x4 oldMatrix = Handles.matrix;
				Handles.matrix = this.HandleMatrix;
				DisplaySceneGUIHandles();
				Handles.color = oldColor;
				Handles.matrix = oldMatrix;
			}
		}

		/// <summary>
		/// Raises the scene GUI overlay event.
		/// </summary>
		private void OnSceneGUIOverlay()
		{
			DisplaySceneGUIHandleToggles();
			m_InspectedObjects[this.target].Update();
			DisplaySceneGUIControls();
			m_InspectedObjects[this.target].ApplyModifiedProperties();
		}

		/// <summary>
		/// Updates any necessary GUI contents when something has changed.
		/// </summary>
		protected virtual void UpdateGUIContents()
		{

		}
	}

	/// <summary>
	/// Base editor class for objects of a particular type to register preferences and scene GUI callbacks.
	/// </summary>
	public abstract class BaseEditor<T> : BaseEditor where T : Object
	{
		/// <summary>
		/// A flag indicating whether the inspected type is a component.
		/// </summary>
		private bool m_IsComponentType = false;

		/// <summary>
		/// Gets the handle matrix.
		/// </summary>
		/// <value>The handle matrix.</value>
		protected override Matrix4x4 HandleMatrix
		{
			get
			{
				return m_IsComponentType ? (this.Target as Component).transform.localToWorldMatrix : Matrix4x4.identity;
			}
		}
		/// <summary>
		/// Gets the target.
		/// </summary>
		/// <value>The target.</value>
		protected T Target { get { return this.target as T; } }

		/// <summary>
		/// For a specified target, gets all objects that are dirtied by its property changes.
		/// </summary>
		/// <returns>An array of objects to record for undoing.</returns>
		/// <param name="obj">Target.</param>
		protected virtual Object[] GetUndoObjects(T obj)
		{
			return obj == null ? new Object[0] : new Object[] { obj };
		}

		/// <summary>
		/// Raises the enable event.
		/// </summary>
		protected override void OnEnable()
		{
			base.OnEnable();
			m_IsComponentType = typeof(Component).IsAssignableFrom(typeof(T));
		}
	}
}
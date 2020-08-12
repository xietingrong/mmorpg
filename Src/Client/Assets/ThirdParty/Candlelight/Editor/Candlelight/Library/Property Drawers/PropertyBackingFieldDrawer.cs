// 
// PropertyBackingFieldDrawer.cs
// 
// Copyright (c) 2014-2015, Candlelight Interactive, LLC
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
// This file contains a custom PropertyDrawer for a serialized backing field of
// a getter/setter.

using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Candlelight
{
	/// <summary>
	/// A custom property drawer for a property backing field that will use the corresponding setter to validate data.
	/// </summary>
	[InitializeOnLoad, CustomPropertyDrawer(typeof(PropertyBackingFieldAttribute))]
	public class PropertyBackingFieldDrawer : PropertyDrawer
	{
		/// <summary>
		/// A hashable structure for storing a serialized property.
		/// </summary>
		private struct HashableSerializedProperty : System.IEquatable<HashableSerializedProperty>
		{
			#region Backing Fields
			private int m_Hash;
			private string m_PropertyPath;
			private Object m_TargetObject;
			#endregion

			/// <summary>
			/// Gets the property path.
			/// </summary>
			/// <value>The property path.</value>
			public string PropertyPath { get { return m_PropertyPath; } }

			/// <summary>
			/// Gets the serialized property.
			/// </summary>
			/// <value>The serialized property.</value>
			public SerializedProperty SerializedProperty
			{
				get
				{
					if (m_TargetObject == null)
					{
						return null;
					}
					SerializedObject so = new SerializedObject(m_TargetObject);
					if (so == null)
					{
						return null;
					}
					return so.FindProperty(m_PropertyPath);
				}
			}
			
			/// <summary>
			/// Gets the target object.
			/// </summary>
			/// <value>The target object.</value>
			public Object TargetObject { get { return m_TargetObject; } }

			/// <summary>
			/// Initializes a new instance of the <see cref="PropertyBackingFieldDrawer.HashableSerializedProperty"/>
			/// struct.
			/// </summary>
			/// <param name="path">Path.</param>
			/// <param name="target">Target.</param>
			public HashableSerializedProperty(string path, Object target) : this()
			{
				m_PropertyPath = path;
				m_TargetObject = target;
				// store the hash upon construction, in case the object is killed later
				m_Hash = ObjectX.GenerateHashCode(m_PropertyPath.GetHashCode(), m_TargetObject.GetHashCode());
			}

			/// <summary>
			/// Initializes a new instance of the <see cref="PropertyBackingFieldDrawer.HashableSerializedProperty"/>
			/// struct.
			/// </summary>
			/// <param name="mod">Property modification.</param>
			public HashableSerializedProperty(PropertyModification mod) : this(mod.propertyPath, mod.target)
			{

			}

			/// <summary>
			/// Determines whether the specified <see cref="System.Object"/> is equal to the current
			/// <see cref="PropertyBackingFieldDrawer.HashableSerializedProperty"/>.
			/// </summary>
			/// <param name="obj">
			/// The <see cref="System.Object"/> to compare with the current
			/// <see cref="PropertyBackingFieldDrawer.HashableSerializedProperty"/>.
			/// </param>
			/// <returns>
			/// <see langword="true"/> if the specified <see cref="System.Object"/> is equal to the current
			/// <see cref="PropertyBackingFieldDrawer.HashableSerializedProperty"/>; otherwise,
			/// <see langword="false"/>.
			/// </returns>
			public override bool Equals(object obj)
			{
				return ObjectX.Equals<HashableSerializedProperty>(ref this, obj);
			}

			/// <summary>
			/// Determines whether the specified
			/// <see cref="PropertyBackingFieldDrawer.HashableSerializedProperty"/> is equal to the current
			/// <see cref="PropertyBackingFieldDrawer.HashableSerializedProperty"/>.
			/// </summary>
			/// <param name="other">
			/// The <see cref="PropertyBackingFieldDrawer.HashableSerializedProperty"/> to compare with the current
			/// <see cref="PropertyBackingFieldDrawer.HashableSerializedProperty"/>.
			/// </param>
			/// <returns>
			/// <see langword="true"/> if the specified
			/// <see cref="PropertyBackingFieldDrawer.HashableSerializedProperty"/> is equal to the current
			/// <see cref="PropertyBackingFieldDrawer.HashableSerializedProperty"/>; otherwise,
			/// <see langword="false"/>.
			/// </returns>
			public bool Equals(HashableSerializedProperty other)
			{
				return GetHashCode() == other.GetHashCode();
			}

			/// <summary>
			/// Serves as a hash function for a <see cref="PropertyBackingFieldDrawer.HashableSerializedProperty"/>
			/// object.
			/// </summary>
			/// <returns>
			/// A hash code for this instance that is suitable for use in hashing algorithms and data structures such as
			/// a hash table.
			/// </returns>
			public override int GetHashCode()
			{
				return m_Hash;
			}

			/// <summary>
			/// Returns a <see cref="System.String"/> that represents the current
			/// <see cref="PropertyBackingFieldDrawer.HashableSerializedProperty"/>.
			/// </summary>
			/// <returns>
			/// A <see cref="System.String"/> that represents the current
			/// <see cref="PropertyBackingFieldDrawer.HashableSerializedProperty"/>.
			/// </returns>
			public override string ToString()
			{
				return string.Format(
					"[HashableSerializedProperty: TargetObject={0}, PropertyPath={1}]", m_TargetObject, m_PropertyPath
				);
			}
		}

		/// <summary>
		/// Initializes the <see cref="PropertyBackingFieldDrawer"/> class.
		/// </summary>
		static PropertyBackingFieldDrawer()
		{
			// add preference menu item
			string featureGroup = "Property Backing Field";
			EditorPreferenceMenu.AddPreferenceMenuItem(
				featureGroup, () => EditorGUILayout.LabelField("Thanks for your bug reports!")
			);
			EditorPreferenceMenu.AddAssetStoreUrl(featureGroup, 18253);
			EditorPreferenceMenu.AddSupportForumUrl(featureGroup, "free-and-open-source");
			// register callbacks with UnityEditor.Undo
			Undo.postprocessModifications += OnPerformUndoableAction;
			Undo.undoRedoPerformed += OnUndoRedo;
			// get all types incompatible with this feature
			List<System.SerializableAttribute> serializableAttrs = new List<System.SerializableAttribute>(1);
			HashSet<System.Type> typesThatCannotBeBackingFields = new HashSet<System.Type>(
				ReflectionX.AllTypes.Where(
					t =>
						(t.IsClass || (t.IsValueType && !t.IsEnum && !t.IsPrimitive)) &&
						t.GetCustomAttributes(ref serializableAttrs) > 0 &&
						!typeof(IPropertyBackingFieldCompatible).IsAssignableFrom(t)
				)
			);
			typesThatCannotBeBackingFields.Remove(typeof(string));
			// collect T[] and List<T> for each incompatible type
			HashSet<System.Type> sequenceTypesThatCannotBeBackingFields = new HashSet<System.Type>();
			foreach (System.Type incompatibleType in typesThatCannotBeBackingFields)
			{
				sequenceTypesThatCannotBeBackingFields.Add(incompatibleType.MakeArrayType());
				sequenceTypesThatCannotBeBackingFields.Add(
					typeof(List<>).MakeGenericType(new System.Type[] { incompatibleType })
				);
			}
			// collect any fields that will cause problems with types that cannot be marked as backing fields
			Dictionary<System.Type, List<FieldInfo>> problematicFields = new Dictionary<System.Type, List<FieldInfo>>();
			// examine all fields on the scripted types to find any problematic usages
			List<SerializeField> serializeFieldAttrs = new List<SerializeField>(1);
			List<PropertyBackingFieldAttribute> pbfAttrs = new List<PropertyBackingFieldAttribute>(1);
			foreach (System.Type providerType in ReflectionX.AllTypes)
			{
				foreach (FieldInfo field in providerType.GetFields(ReflectionX.instanceBindingFlags))
				{
					System.Type typeToValidate = field.FieldType;
					// skip the field if it is known to be compatible
					if (
						!typesThatCannotBeBackingFields.Contains(typeToValidate) &&
						!sequenceTypesThatCannotBeBackingFields.Contains(typeToValidate)
					)
					{
						continue;
					}
					// skip the field if it is a built-in Unity type
					if (ReflectionX.UnityRuntimeAssemblies.Contains(typeToValidate.Assembly))
					{
						continue;
					}
					// skip the field if it is not serialized
					if (field.IsPrivate && field.GetCustomAttributes(ref serializeFieldAttrs) == 0)
					{
						continue;
					}
					// skip the field if it is not designated as a backing field
					if (field.GetCustomAttributes(ref pbfAttrs) == 0)
					{
						continue;
					}
					// add the type to the problem table
					if (typeToValidate.IsArray)
					{
						typeToValidate = typeToValidate.GetElementType();
					}
					else if (typeToValidate.IsGenericType)
					{
						typeToValidate = typeToValidate.GetGenericArguments()[0];
					}
					if (!problematicFields.ContainsKey(typeToValidate))
					{
						problematicFields.Add(typeToValidate, new List<FieldInfo>());
					}
					// add the field to the type's list of problematic usages
					problematicFields[typeToValidate].Add(field);
				}
			}
			// display messages for any problems
			foreach (KeyValuePair<System.Type, List<FieldInfo>> problematicType in problematicFields)
			{
				Debug.LogError(
					string.Format(
						"<b>{0}</b> must implement <b>{1}<{0}></b>, because it is marked with <b>{2}</b> on the " +
						"following fields:\n{3}",
						problematicType.Key,
						typeof(IPropertyBackingFieldCompatible).FullName,
						typeof(PropertyBackingFieldAttribute).FullName,
						string.Join(
							"\n",
							(
								from field in problematicType.Value
								select string.Format(
									"    - <i>{0}.{1}</i>",
									field.DeclaringType, field.Name
								)
							).ToArray()
						)
					)
				);
			}
			// make sure all types indicated as backing field compatible are also serializable
			HashSet<System.Type> compatibleTypesThatAreNotSerializable = new HashSet<System.Type>(
				ReflectionX.AllTypes.Where(
					t =>
						!t.IsAbstract &&
						!t.IsInterface &&
						typeof(IPropertyBackingFieldCompatible).IsAssignableFrom(t) &&
					t.GetCustomAttributes(ref serializableAttrs) == 0
				)
			);
			foreach (System.Type problematicType in compatibleTypesThatAreNotSerializable)
			{
				Debug.LogError(
					string.Format(
						"<b>{0}</b> implements <b>{1}</b>, but is not also marked with <b>{2}</b>",
						problematicType,
						typeof(IPropertyBackingFieldCompatible).FullName,
						typeof(System.SerializableAttribute).FullName
					)
				);
			}
		}

		#region Shared Allocations
		private static List<PropertyAttribute> s_PropertyAttrs = new List<PropertyAttribute>(2);
		#endregion

		/// <summary>
		/// The current selection.
		/// </summary>
		private static readonly HashSet<Object> s_CurrentSelection = new HashSet<Object>();
		/// <summary>
		/// The method for displaying a default property field.
		/// </summary>
		private static readonly MethodInfo s_DefaultPropertyField =
			typeof(EditorGUI).GetMethod("DefaultPropertyField", ReflectionX.staticBindingFlags);
		/// <summary>
		/// A reusable parameter array for invoking the default property field method via reflection.
		/// </summary>
		private static readonly object[] s_DefaultPropertyFieldArgs = new object[3];
		/// <summary>
		/// A suffix to automatically search for editor-only property wrappers. Suffix wrapper methods with this string
		/// if they wrap virtual properties that may be overridden in child classes.
		/// </summary>
		private static readonly string s_EditorPropertySuffix = "_PBFEditorProperty";
		/// <summary>
		/// A regex pattern to guess a property name from a backing field name.
		/// </summary>
		private static readonly Regex s_MatchPropertyNameInBackingField =
			new Regex(@"(?<=m_|_)\w+");
		/// <summary>
		/// GUIContent to use for the status icon if no setter is found.
		/// </summary>
		private static GUIContent s_NoSetterStatusIcon = new GUIContent(EditorStylesX.ErrorIcon);
		/// <summary>
		/// A reusable set to collect objects to undo when a setter is invoked.
		/// </summary>
		private static readonly HashSet<Object> s_ObjectsToUndo = new HashSet<Object>();
		/// <summary>
		/// A reusable <see cref="UnityEditor.SerializedProperty"/>.
		/// </summary>
		private static SerializedProperty s_Property;
		/// <summary>
		/// The properties affected by undo property modifications.
		/// </summary>
		private static readonly HashSet<HashableSerializedProperty> s_PropertyModifications =
			new HashSet<HashableSerializedProperty>();
		/// <summary>
		/// The property setter callback for each hashable property.
		/// </summary>
		private static Dictionary<HashableSerializedProperty, System.Action> s_PropertySetterCallbacks =
			new Dictionary<HashableSerializedProperty, System.Action>();
		/// <summary>
		/// A reusable cache of serialized properties for each hashed version, to prevent additional allocations.
		/// </summary>
		private static Dictionary<HashableSerializedProperty, SerializedProperty> s_SerializedPropertyCache =
			new Dictionary<HashableSerializedProperty, SerializedProperty>();
		/// <summary>
		/// A reusable parameter array for invoking setters via reflection.
		/// </summary>
		private static readonly object[] s_SetterArgs = new object[1];
		/// <summary>
		/// A reusable list for sorting modifications so children can be invoked first.
		/// </summary>
		private static readonly List<HashableSerializedProperty> s_SortedModifications =
			new List<HashableSerializedProperty>(256);
		/// <summary>
		/// A reusable set of all hashed properties that triggered a modification.
		/// </summary>
		private static readonly HashSet<HashableSerializedProperty> s_TriggeredModifications =
			new HashSet<HashableSerializedProperty>();
		/// <summary>
		/// A reusable list for finding a property's upstream parent properties.
		/// </summary>
		private static readonly List<HashableSerializedProperty> s_UpstreamProperties =
			new List<HashableSerializedProperty>(32);
		/// <summary>
		/// For each registered property, a cached backing field value. Used to flush the backing field right before the
		/// setter is invoked.
		/// </summary>
		private static Dictionary<HashableSerializedProperty, object> s_ValueCache =
			new Dictionary<HashableSerializedProperty, object>();
		/// <summary>
		/// The warning format string.
		/// </summary>
		private static readonly string s_WarningFormatString = "CA1819: Properties should not return arrays.\n\n" +
			"Consider implementing methods {0} Get{1}() and void Set{1}({0}) in class {2}.";

		/// <summary>
		/// Gets any properties upstream of the supplied property. This method is used to determine when a parent setter
		/// might need to be invoked.
		/// </summary>
		/// <param name="property">Property.</param>
		/// <param name="upstreamProperties">The upstream properties.</param>
		/// <param name="propertyCache">
		/// Cached <see cref="UnityEditor.SerializedProperties"/> to prevent further allocations.
		/// </param>
		private static void GetUpstreamProperties(
			HashableSerializedProperty property,
			List<HashableSerializedProperty> upstreamProperties,
			Dictionary<HashableSerializedProperty, SerializedProperty> propertyCache
		)
		{
			upstreamProperties.Clear();
			s_Property = propertyCache[property];
			if (s_Property != null)
			{
				s_Property = s_Property.GetParentProperty();
				while (s_Property != null)
				{
					upstreamProperties.Add(
						new HashableSerializedProperty(s_Property.propertyPath, property.TargetObject)
					);
					s_Property = s_Property.GetParentProperty();
				}
			}
		}

		/// <summary>
		/// Invokes the supplied property setter for the supplied hashable serialized property and update its value
		/// cache.
		/// </summary>
		/// <param name="hashableProperty">A hashable representation of the property's serialized backing field.</param>
		/// <param name="getter">A getter method that takes the provider as its argument.</param>
		/// <param name="setter">A setter method that takes the provider and new value as its arguments.</param> 
		/// <param name="propertyType">The type returned by the getter and specified in the setter signature.</param>
		private static void InvokeSetter(
			HashableSerializedProperty hashableProperty,
			System.Func<object, object> getter,
			System.Action<object, object> setter,
			System.Type propertyType
		)
		{
			SerializedProperty sp = hashableProperty.SerializedProperty;
			// mark for undo
			s_ObjectsToUndo.Clear();
			s_ObjectsToUndo.Add(hashableProperty.TargetObject);
			string undoName = string.Format("Change {0}", sp.displayName);
			// if it's on a monobehaviour, it may affect other objects in the hierarchy
			if (hashableProperty.TargetObject is MonoBehaviour)
			{
				MonoBehaviour monoBehaviour = hashableProperty.TargetObject as MonoBehaviour;
				foreach (Transform t in monoBehaviour.transform.root.GetComponentsInChildren<Transform>(true))
				{
					s_ObjectsToUndo.Add(t.gameObject);
					foreach (Component c in t.GetComponents<Component>())
					{
						if (c != null) // skip MonoBehaviours with unassigned script reference
						{
							s_ObjectsToUndo.Add(c);
						}
					}
				}
			}
			Undo.RecordObjects(s_ObjectsToUndo.ToArray(), undoName);
			// get the providers
			FieldInfo fieldInfo;
			object provider = sp.GetProvider(out fieldInfo);
			// get the element index of the property being set, if any
			int elementIndex = hashableProperty.SerializedProperty.GetElementIndex();
			// flush inspector changes and store pending values
			sp.serializedObject.ApplyModifiedProperties();
			object pendingValue = null;
			// ensure IList backing field values are converted to the type expected by the setter
			if (elementIndex < 0 && typeof(IList).IsAssignableFrom(propertyType) && propertyType != fieldInfo.FieldType)
			{
				pendingValue = sp.GetConvertedIListValues(propertyType);
			}
			else
			{
				pendingValue = sp.GetValue();
			}
			// reset backing field to old values
			if (elementIndex >= 0)
			{
				(fieldInfo.GetValue(provider) as IList)[elementIndex] = s_ValueCache[hashableProperty];
			}
			else
			{
				fieldInfo.SetValue(provider, s_ValueCache[hashableProperty]);
			}
			// invoke the setter
			if (elementIndex >= 0)
			{
				// clone the result of the getter
				IList arrayValues = (IList)getter.Invoke(provider);
				if (typeof(System.Array).IsAssignableFrom(propertyType))
				{
					arrayValues = (IList)((System.ICloneable)arrayValues).Clone();
				}
				else
				{
					IList srcValues = arrayValues;
					arrayValues = (IList)System.Activator.CreateInstance(propertyType);
					for (int idx = 0; idx < srcValues.Count; ++idx)
					{
						arrayValues.Add(srcValues[idx]);
					}
				}
				// assign the pending element value
				arrayValues[elementIndex] = pendingValue;
				// invoke setter
				setter.Invoke(provider, arrayValues);
			}
			else
			{
				setter.Invoke(provider, pendingValue);
				// if the provider is a struct, copy its changes back up to the next reference type
				if (provider.GetType().IsValueType)
				{
					object newValue = provider;
					SerializedProperty parent = sp.GetParentProperty();
					while (parent != null && !parent.isArray)
					{
						provider = parent.GetProvider(out fieldInfo);
						if (typeof(IList).IsAssignableFrom(fieldInfo.FieldType))
						{
							elementIndex = parent.GetElementIndex();
							(fieldInfo.GetValue(provider) as IList)[elementIndex] = newValue;
						}
						else
						{
							fieldInfo.SetValue(provider, newValue);
						}
						if (provider.GetType().IsValueType)
						{
							newValue = provider;
							parent = parent.GetParentProperty();
						}
						else
						{
							parent = null;
						}
					}
				}
			}
			// set dirty
			EditorUtilityX.SetDirty(s_ObjectsToUndo.ToArray());
			// update cache
			s_ValueCache[hashableProperty] = sp.GetValue();
		}

		/// <summary>
		/// Raises the perform undoable action event. Invoked when inspector is modified or context menu item is
		/// selected (e.g., Revert Value to Prefab, Set to Value of).
		/// </summary>
		/// <param name="modifications">Information about what properties were modified by the action.</param>
		private static UndoPropertyModification[] OnPerformUndoableAction(UndoPropertyModification[] modifications)
		{
			s_PropertyModifications.Clear();
			s_SerializedPropertyCache.Clear();
			for (int i = 0; i < modifications.Length; ++i)
			{
				HashableSerializedProperty hashed =
#if UNITY_4_6 || UNITY_5_0
					new HashableSerializedProperty(modifications[i].propertyModification);
#else
					new HashableSerializedProperty(modifications[i].currentValue);
#endif
				s_SerializedPropertyCache[hashed] = hashed.SerializedProperty;
				if (s_SerializedPropertyCache[hashed] != null)
				{
					s_PropertyModifications.Add(hashed);
				}
			}
			// add all upstream properties so parent setters are called
			foreach (HashableSerializedProperty modification in s_PropertyModifications.ToArray())
			{
				GetUpstreamProperties(modification, s_UpstreamProperties, s_SerializedPropertyCache);
				for (int i = 0; i < s_UpstreamProperties.Count; ++i)
				{
					s_PropertyModifications.Add(s_UpstreamProperties[i]);
				}
			}
			// trigger setters for all modified properties
			TriggerSettersForKnownModifications(s_PropertyModifications, s_SerializedPropertyCache);
			return modifications;
		}

		/// <summary>
		/// Raises the undo/redo event. This event contains no information about what was undone, so each hashed
		/// property must test its current value against its cached value.
		/// </summary>
		private static void OnUndoRedo()
		{
			// trigger setters for all modified properties
			s_SerializedPropertyCache.Clear();
			foreach (HashableSerializedProperty hashed in s_PropertySetterCallbacks.Keys)
			{
				s_SerializedPropertyCache[hashed] = hashed.SerializedProperty;
			}
			TriggerSettersForKnownModifications(
				s_PropertySetterCallbacks.Keys.Where(
					p => s_SerializedPropertyCache[p] != null &&
					!s_SerializedPropertyCache[p].IsValueEqualTo(s_ValueCache[p])
				), s_SerializedPropertyCache
			);
		}
		
		/// <summary>
		/// Raises the trigger property setter event.
		/// </summary>
		/// <param name="hashableProperty">Hashable property.</param>
		/// <param name="getter">Getter.</param>
		/// <param name="setter">Setter.</param>
		/// <param name="propertyType">Property type.</param>
		/// <param name="oldValue">Old value.</param>
		private static void OnTriggerPropertySetter(
			HashableSerializedProperty hashableProperty,
			System.Func<object, object> getter,
			System.Action<object, object> setter,
			System.Type propertyType
		)
		{
			// clean up lookup tables and early out if property is dead
			SerializedProperty sp = hashableProperty.SerializedProperty;
			if (sp == null)
			{
				s_PropertySetterCallbacks.Remove(hashableProperty);
				if (s_ValueCache.ContainsKey(hashableProperty))
				{
					s_ValueCache.Remove(hashableProperty);
				}
				return;
			}
			// invoke the setter
			InvokeSetter(hashableProperty, getter, setter, propertyType);
			// when any part of an array changes, ensure array, elements, and size are all up to date
			bool doElementsNeedUpdate = false;
			bool doesArrayNeedUpdate = false;
			bool doesSizeNeedUpdate = false;
			if (sp.isArray && sp.propertyType != SerializedPropertyType.String)
			{
				doElementsNeedUpdate = true;
				doesSizeNeedUpdate = true;
			}
			else if (sp.IsArrayElement())
			{
				doesArrayNeedUpdate = true;
				doesSizeNeedUpdate = true;
				sp = sp.GetParentProperty();
			}
			else if (sp.IsArraySize())
			{
				doesArrayNeedUpdate = true;
				doElementsNeedUpdate = true;
				sp = sp.GetParentProperty();
			}
			if (doElementsNeedUpdate)
			{
				for (int elementIndex = 0; elementIndex < sp.arraySize; ++elementIndex)
				{
					HashableSerializedProperty hashableElement = new HashableSerializedProperty(
						string.Format("{0}.Array.data[{1}]", sp.propertyPath, elementIndex),
						hashableProperty.TargetObject
					);
					if (s_ValueCache.ContainsKey(hashableElement))
					{
						s_ValueCache[hashableElement] = hashableElement.SerializedProperty.GetValue();
					}
					else // for added elements when list grows
					{
						s_ValueCache.Add(hashableElement, hashableElement.SerializedProperty.GetValue());
					}
				}
			}
			if (doesArrayNeedUpdate)
			{
				HashableSerializedProperty hashableArray =
					new HashableSerializedProperty(sp.propertyPath, hashableProperty.TargetObject);
				s_ValueCache[hashableArray] = hashableArray.SerializedProperty.GetValue();
			}
			if (doesSizeNeedUpdate)
			{
				HashableSerializedProperty hashableSize = new HashableSerializedProperty(
					string.Format("{0}.Array.size", sp.propertyPath), hashableProperty.TargetObject
				);
				s_ValueCache[hashableSize] = hashableSize.SerializedProperty.GetValue();
			}
		}

		/// <summary>
		/// Registers the array property and its size property.
		/// </summary>
		/// <param name="arrayProperty">Array property.</param>
		/// <param name="getter">Getter.</param>
		/// <param name="setter">Setter.</param>
		/// <param name="propertyType">Property type.</param>
		private static void RegisterArrayProperty(
			SerializedProperty arrayProperty,
			System.Func<object, object> getter,
			System.Action<object, object> setter,
			System.Type propertyType
		)
		{
			HashableSerializedProperty hashableArrayProperty =
				new HashableSerializedProperty(arrayProperty.propertyPath, arrayProperty.serializedObject.targetObject);
			HashableSerializedProperty hashableArraySizeProperty = new HashableSerializedProperty(
				arrayProperty.propertyPath + ".Array.size", arrayProperty.serializedObject.targetObject
			);
			RegisterPropertyIfNeeded(hashableArrayProperty, hashableArrayProperty, getter, setter, propertyType);
			RegisterPropertyIfNeeded(hashableArraySizeProperty, hashableArrayProperty, getter, setter, propertyType);
			FieldInfo field;
			arrayProperty.GetProvider(out field);
			s_ValueCache[hashableArrayProperty] = field.FieldType.IsArray ?
				System.Array.CreateInstance(field.FieldType.GetElementType(), 0) :
				System.Activator.CreateInstance(field.FieldType);
			s_ValueCache[hashableArraySizeProperty] = 0;
		}

		/// <summary>
		/// Registers the supplied hashable property's initial backing field value and setter callback.
		/// </summary>
		/// <param name="trigger">Hashable property whose value changes will trigger the callback.</param>
		/// <param name="propertyToInvoke">
		/// Hashable property whose setter to invoke. Differs from trigger only when trigger is an array size property
		/// that triggers the setter in the array property.
		/// </param>
		/// <param name="getter">Getter.</param>
		/// <param name="setter">Setter.</param>
		/// <param name="propertyType">Property type.</param>
		private static void RegisterPropertyIfNeeded(
			HashableSerializedProperty trigger,
			HashableSerializedProperty propertyToInvoke,
			System.Func<object, object> getter,
			System.Action<object, object> setter,
			System.Type propertyType
		)
		{
			if (propertyType == null)
			{
				return;
			}
			// ensure concrete property type is registered
			if (propertyType.IsGenericType)
			{
				FieldInfo field;
				propertyToInvoke.SerializedProperty.GetProvider(out field);
				propertyType = field.FieldType;
			}
			// initialize value cache
			if (!s_ValueCache.ContainsKey(trigger))
			{
				s_ValueCache.Add(trigger, trigger.SerializedProperty.GetValue());
				// if it is an array element, ensure the array and the size properties are registered
				SerializedProperty sp = trigger.SerializedProperty;
				if (sp.IsArrayElement())
				{
					RegisterArrayProperty(sp.GetParentProperty(), getter, setter, propertyType);
				}
			}
			if (!s_ValueCache.ContainsKey(propertyToInvoke))
			{
				s_ValueCache.Add(propertyToInvoke, propertyToInvoke.SerializedProperty.GetValue());
			}
			// add callbacks associated with the trigger
			if (!s_PropertySetterCallbacks.ContainsKey(trigger))
			{
				s_PropertySetterCallbacks.Add(
					trigger, () => OnTriggerPropertySetter(propertyToInvoke, getter, setter, propertyType)
				);
			}
		}

		/// <summary>
		/// Triggers the setter when the supplied property is known to have been modified. Properties are triggered
		/// according to depth, so that all children properties (e.g., elements, members) are triggered before their
		/// parents (e.g., arrays, classes, structs).
		/// </summary>
		/// <param name="modifiedProperties">Properties known to be modified.</param>
		/// <param name="propertyCache">
		/// Cached <see cref="UnityEditor.SerializedProperties"/> to prevent further allocations.
		/// </param>
		private static void TriggerSettersForKnownModifications(
			IEnumerable<HashableSerializedProperty> modifiedProperties,
			Dictionary<HashableSerializedProperty, SerializedProperty> propertyCache
		)
		{
			s_TriggeredModifications.Clear();
			foreach (HashableSerializedProperty hashed in modifiedProperties)
			{
				s_TriggeredModifications.Add(hashed);
				if (!propertyCache.ContainsKey(hashed))
				{
					propertyCache[hashed] = hashed.SerializedProperty;
				}
			}
			// sort by depth, so children setters are invoked first
			s_SortedModifications.Clear();
			s_SortedModifications.AddRange(s_TriggeredModifications);
			s_SortedModifications.Sort((a, b) => propertyCache[a].depth.CompareTo(propertyCache[b].depth));
			s_SortedModifications.Reverse();
			// invoke setters
			for (int i = 0; i < s_SortedModifications.Count; ++i)
			{
				if (s_PropertySetterCallbacks.ContainsKey(s_SortedModifications[i]))
				{
					s_PropertySetterCallbacks[s_SortedModifications[i]]();
				}
			}
		}
		
		/// <summary>
		/// heights of any decorators preceding properties with the specified paths.
		/// </summary>
		private Dictionary<string, float> m_DecoratorHeights = new Dictionary<string, float>();
		/// <summary>
		/// The getter.
		/// </summary>
		private System.Func<object, object> m_Getter = null;
		/// <summary>
		/// A value indicating whether this instance is initialized.
		/// </summary>
		private bool m_IsInitialized = false;
		/// <summary>
		/// The return type of the property getter.
		/// </summary>
		private System.Type m_PropertyType = null;
		/// <summary>
		/// The setter.
		/// </summary>
		public System.Action<object, object> m_Setter = null;
		#region Backing Fields
		private PropertyBackingFieldAttribute m_Attribute = null;
		private PropertyDrawer m_DrawerToUse = null;
		#endregion
		
		/// <summary>
		/// Gets the drawer to use.
		/// </summary>
		/// <value>
		/// The drawer to use if an override is specified, otherwise <see langword="null"/> if default drawer should be used.
		/// </value>
		private PropertyDrawer DrawerToUse
		{
			get
			{
				if (m_DrawerToUse == null)
				{
					m_DrawerToUse = SerializedPropertyX.GetPropertyDrawer(this.fieldInfo, this.Attribute.OverrideAttribute);
				}
				return m_DrawerToUse;
			}
		}
		
		/// <summary>
		/// Gets the backing field attribute.
		/// </summary>
		/// <value>The backing field attribute.</value>
		private PropertyBackingFieldAttribute Attribute
		{
			get
			{
				if (m_Attribute == null)
				{
					m_Attribute = this.attribute as PropertyBackingFieldAttribute;
				}
				if (!m_IsInitialized)
				{
					Initialize(this.fieldInfo, m_Attribute);
				}
				return m_Attribute;
			}
		}

		/// <summary>
		/// Gets the concrete method representation of the supplied method on the supplied provider.
		/// </summary>
		/// <returns>
		/// A concrete representation of the supplied method if it is defined on a generic type; otherwise, the method
		/// itself.
		/// </returns>
		/// <param name="method">Method.</param>
		/// <param name="provider">Provider.</param>
		private MethodInfo GetConcreteMethod(MethodInfo method, object provider)
		{
			if (method.ReflectedType.IsGenericType)
			{
				// just use base type because generic MonoBehaviours and ScriptableObjects only serialize when they are
				// concrete subclasses
				method = provider.GetType().BaseType.GetMethod(method.Name, ReflectionX.instanceBindingFlags);
			}
			return method;
		}
		
		/// <summary>
		/// Gets the height of the property.
		/// </summary>
		/// <returns>The property height.</returns>
		/// <param name="property">Property.</param>
		/// <param name="label">Label.</param>
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			// ensure property's decorator height is registered
			if (!m_DecoratorHeights.ContainsKey(property.propertyPath))
			{
				m_DecoratorHeights.Add(property.propertyPath, 0f);
			}
			m_DecoratorHeights[property.propertyPath] = 0f;
			if (this.DrawerToUse != null)
			{
				return this.DrawerToUse.GetPropertyHeight(property, label);
			}
			else
			{
				float result = EditorGUI.GetPropertyHeight(property, label, true);
				if (!property.IsArrayElement())
				{
					this.fieldInfo.GetCustomAttributes(ref s_PropertyAttrs);
					for (int i = 0; i < s_PropertyAttrs.Count; ++i)
					{
						GUIDrawer drawer = SerializedPropertyX.GetGUIDrawer(this.fieldInfo, s_PropertyAttrs[i]);
						if (drawer is DecoratorDrawer)
						{
							float height = (drawer as DecoratorDrawer).GetHeight();
							result -= height;
							m_DecoratorHeights[property.propertyPath] += height;
						}
					}
				}
				return result;
			}
		}

		/// <summary>
		/// Initializes this <see cref="PropertySetterBackingDrawer"/>.
		/// </summary>
		/// <param name="backingField">The backing field for the property being drawn.</param>
		/// <param name="propertyAttribute">
		/// The attribute decorating the backing field.
		/// </param>
		private void Initialize(FieldInfo backingField, PropertyBackingFieldAttribute propertyAttribute)
		{
			if (m_IsInitialized)
			{
				return;
			}
			System.Type providerType = backingField.ReflectedType;
			string propertyName = string.IsNullOrEmpty(propertyAttribute.PropertyName) ?
				s_MatchPropertyNameInBackingField.Match(backingField.Name).Value : propertyAttribute.PropertyName;
			if (string.IsNullOrEmpty(propertyAttribute.PropertyName))
			{
				propertyAttribute.PropertyName = propertyName;
			}
			MethodInfo getter = null;
			MethodInfo setter = null;
			string getterName = propertyName;
			string setterName = propertyName;
			// first see if there is an editor-only property wrapper
			PropertyInfo property =
				providerType.GetProperty(propertyName + s_EditorPropertySuffix, ReflectionX.instanceBindingFlags);
			if (property == null)
			{
				property = providerType.GetProperty(propertyName, ReflectionX.instanceBindingFlags);
			}
			if (property != null)
			{
				getter = property.GetGetMethod(true);
				setter = property.GetSetMethod(true);
				m_PropertyType = property.PropertyType;
				if (typeof(IList).IsAssignableFrom(m_PropertyType))
				{
					Debug.LogWarning(string.Format(s_WarningFormatString, m_PropertyType, propertyName, providerType));
				}
			}
			else
			{
				// first see if there is an editor-only property wrapper
				getterName = string.Format("Get{0}{1}", propertyName, s_EditorPropertySuffix);
				setterName = string.Format("Set{0}{1}", propertyName, s_EditorPropertySuffix);
				getter = providerType.GetMethod(
					getterName, ReflectionX.instanceBindingFlags, null, System.Type.EmptyTypes, null
				);
				if (getter == null)
				{
					getterName = string.Format("Get{0}", propertyName);
					setterName = string.Format("Set{0}", propertyName);
					getter = providerType.GetMethod(
						getterName, ReflectionX.instanceBindingFlags, null, System.Type.EmptyTypes, null
					);
				}
				// prefer set method with parameter type matching return type of get method
				if (getter != null)
				{
					System.Type[] setterArgs = new System.Type[] { getter.ReturnType };
					setter =
						providerType.GetMethod(setterName, ReflectionX.instanceBindingFlags, null, setterArgs, null);
					m_PropertyType = getter.ReturnType;
				}
				else
				{
					setter = providerType.GetMethod(setterName, ReflectionX.instanceBindingFlags);
				}
			}
			if (getter != null)
			{
				m_Getter = provider => GetConcreteMethod(getter, provider).Invoke(provider, null);
			}
			if (setter != null)
			{
				m_Setter = delegate(object provider, object value)
				{
					MethodInfo concreteSetter = GetConcreteMethod(setter, provider);
					System.Type concreteParameterType = concreteSetter.GetParameters().First().ParameterType;
					s_SetterArgs[0] =
						value == null || !concreteParameterType.IsAssignableFrom(value.GetType()) ? null : value;
					concreteSetter.Invoke(provider, s_SetterArgs);
				};
			}
			m_IsInitialized = true;
		}
		
		/// <summary>
		/// Raises the GUI event.
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="property">Property.</param>
		/// <param name="label">Label.</param>
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
#if UNITY_4_6
			// bug 601339
			if (property.isArray && property.propertyType != SerializedPropertyType.String)
			{
				return;
			}
#endif
			// ensure property's decorator height is registered
			if (!m_DecoratorHeights.ContainsKey(property.propertyPath))
			{
				m_DecoratorHeights.Add(property.propertyPath, 0f);
			}
			// clear all callbacks and cached values if the selection has changed
			if (!s_CurrentSelection.SetEquals(Selection.objects))
			{
				s_PropertySetterCallbacks.Clear();
				s_ValueCache.Clear();
				s_CurrentSelection.Clear();
				foreach (Object obj in Selection.objects)
				{
					s_CurrentSelection.Add(obj);
				}
			}
			// ensure all properties are registered
			foreach (Object target in property.serializedObject.targetObjects)
			{
				HashableSerializedProperty hashableProperty =
					new HashableSerializedProperty(property.propertyPath, target);
				// register property if needed
				if (hashableProperty.SerializedProperty != null) // newly added array elements might not yet exist
				{
					RegisterPropertyIfNeeded(hashableProperty, hashableProperty, m_Getter, m_Setter, m_PropertyType);
				}
			}
			// display field
			bool hasSetter = m_Setter != null;
			EditorGUI.BeginDisabledGroup(!hasSetter);
			{
				if (!hasSetter)
				{
					position.width -= EditorGUIUtility.singleLineHeight;
				}
				if (this.DrawerToUse == null)
				{
					// for generic types, just back up and draw the decorator again
					if (
						property.propertyType == SerializedPropertyType.Generic ||
						property.propertyType == SerializedPropertyType.ObjectReference
					)
					{
						position.y -= m_DecoratorHeights[property.propertyPath];
						EditorGUI.PropertyField(position, property, label, true);
						// TODO: did Unity 5.1.0b3 fix this?
					}
					// for other types, use the default property field
					else
					{
						s_DefaultPropertyFieldArgs[0] = position;
						s_DefaultPropertyFieldArgs[1] = property;
						s_DefaultPropertyFieldArgs[2] = label;
						s_DefaultPropertyField.Invoke(null, s_DefaultPropertyFieldArgs);
					}
				}
				else
				{
					this.DrawerToUse.OnGUI(position, property, label);
				}
			}
			EditorGUI.EndDisabledGroup();
			if (!hasSetter)
			{
				position.x += position.width;
				position.height = EditorGUIUtility.singleLineHeight;
				position.width = EditorGUIUtility.singleLineHeight;
				s_NoSetterStatusIcon.tooltip = string.Format("No setter found for {0}.", this.Attribute.PropertyName);
				GUI.Box(position, s_NoSetterStatusIcon, EditorStylesX.StatusIconStyle);
			}
		}
	}
}
// 
// LocalizableText.cs
// 
// Copyright (c) 2015, Candlelight Interactive, LLC
// All rights reserved.
// 
// This file is licensed according to the terms of the Unity Asset Store EULA:
// http://download.unity3d.com/assetstore/customer-eula.pdf

using UnityEngine;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Candlelight.UI
{
	/// <summary>
	/// A class for storing text values for different locales.
	/// </summary>
	public class LocalizableText : ScriptableObject, ITextSource
	{
		/// <summary>
		/// Locale override entry attribute.
		/// </summary>
		public class LocaleOverrideEntryAttribute : PropertyAttribute {}

		/// <summary>
		/// A basic class to wrap and identify a string to be used as an override for a particular locale.
		/// </summary>
		[System.Serializable]
		private class LocaleOverride : IdentifiableBackingFieldCompatibleObjectWrapper<string>
		{
			/// <summary>
			/// Initializes a new instance of the <see cref="LocalizableText.LocaleOverride"/> class.
			/// </summary>
			/// <param name="locale">Locale.</param>
			/// <param name="text">Text.</param>
			public LocaleOverride(string locale, string text) : base(locale, text) {}

			/// <summary>
			/// Clone this instance.
			/// </summary>
			/// <returns>A clone of this instance.</returns>
			public override object Clone()
			{
				return new LocaleOverride(this.Identifier, this.Data);
			}
		}

		/// <summary>
		/// Default locale string.
		/// </summary>
		private static readonly string s_DefaultLocale = "[DEFAULT]";

		#region Backing Fields
		private static readonly ReadOnlyCollection<string> s_TenMostCommonLanguages = new ReadOnlyCollection<string>(
			new [] { "en", "ja", "ko", "zh", "de", "fr", "pt", "es", "it", "ms" }
		);
		#endregion

		/// <summary>
		/// Gets locale strings for the ten most common languages for mobile apps according to
		/// http://todaysweb.net/top-mobile-apps-games-localization-languages-maximum-revenue/
		/// </summary>
		/// <value>The ten most common languages for mobile apps.</value>
		public static ReadOnlyCollection<string> TenMostCommonLanguages { get { return s_TenMostCommonLanguages; } }

		#region Backing Fields
		[SerializeField, PropertyBackingField(typeof(PopupAttribute), "GetCurrentLocalePopupContents")]
		private string m_CurrentLocale = s_DefaultLocale;
		[SerializeField, PropertyBackingField(typeof(TextAreaAttribute), 3, 10)]
		private string m_DefaultText = "";
		[SerializeField, PropertyBackingField(typeof(LocaleOverrideEntryAttribute))]
		private List<LocaleOverride> m_LocaleOverrides = new List<LocaleOverride>();
		private UnityEngine.Events.UnityEvent m_OnBecameDirty = new UnityEngine.Events.UnityEvent();
		#endregion

		/// <summary>
		/// Gets or sets the current locale.
		/// </summary>
		/// <value>The current locale.</value>
		public string CurrentLocale
		{
			get { return m_CurrentLocale; }
			set
			{
				value = value ?? "";
				if (m_CurrentLocale != value)
				{
					m_CurrentLocale = value;
					m_OnBecameDirty.Invoke();
				}
			}
		}
		/// <summary>
		/// Gets or sets the default text.
		/// </summary>
		/// <value>The default text.</value>
		public string DefaultText
		{
			get { return m_DefaultText; }
			set
			{
				value = value ?? "";
				if (m_DefaultText != value)
				{
					m_DefaultText = value;
					m_OnBecameDirty.Invoke();
				}
			}
		}
		/// <summary>
		/// Gets a callback for whenever the text on this instance has changed.
		/// </summary>
		/// <value>A callback for whenever the text on this instance has changed.</value>
		public UnityEngine.Events.UnityEvent OnBecameDirty { get { return m_OnBecameDirty; } }
		/// <summary>
		/// Gets the output text.
		/// </summary>
		/// <value>The output text.</value>
		public string OutputText
		{
			get
			{
				int index = m_LocaleOverrides.FindIndex(k => k.Identifier == m_CurrentLocale);
				return index < 0 ? m_DefaultText : m_LocaleOverrides[index].Data;
			}
		}

		/// <summary>
		/// Gets the current locale popup contents. Included for inspector.
		/// </summary>
		/// <returns>The current locale popup contents.</returns>
		/// <param name="labels">Labels.</param>
		/// <param name="values">Values.</param>
		private int GetCurrentLocalePopupContents(List<GUIContent> labels, List<object> values)
		{
			labels.Clear();
			values.Clear();
			int currentIndex = -1;
			for (int i = m_LocaleOverrides.Count - 1; i >= 0; --i)
			{
				labels.Add(new GUIContent(m_LocaleOverrides[i].Identifier));
				values.Add(m_LocaleOverrides[i].Identifier);
				currentIndex = m_LocaleOverrides[i].Identifier == m_CurrentLocale ? i : currentIndex;
			}
			labels.Add(new GUIContent(s_DefaultLocale));
			values.Add(s_DefaultLocale);
			++currentIndex;
			labels.Reverse();
			values.Reverse();
			return currentIndex;
		}

		/// <summary>
		/// Gets the localized text.
		/// </summary>
		/// <remarks>Included for inspector.</remarks>
		/// <returns>The localized text.</returns>
		private LocaleOverride[] GetLocaleOverrides()
		{
			return m_LocaleOverrides.ToArray();
		}

		/// <summary>
		/// Gets the localized text.
		/// </summary>
		/// <param name="localizedText">A dictionary of string for different locales to populate.</param>
		public void GetLocaleOverrides(ref Dictionary<string, string> localizedText)
		{
			BackingFieldUtility.GetKeyedListBackingFieldAsDict(m_LocaleOverrides, ref localizedText);
		}

		/// <summary>
		/// Raises the enable event.
		/// </summary>
		protected virtual void OnEnable()
		{
			if (m_LocalizedText.Count == 0)
			{
				return;
			}
			m_LocaleOverrides.Clear();
			for (int i = 0; i < m_LocalizedText.Count; ++i)
			{
				m_LocaleOverrides.Add(new LocaleOverride(m_LocalizedText[i].Locale, m_LocalizedText[i].Text));
			}
			m_LocalizedText.Clear();
			#if UNITY_EDITOR
			UnityEditor.EditorUtility.SetDirty(this);
			Debug.LogWarning(
				"Updated serialization layout. Save your project and commit this object to version control.",
				this
			);
			#endif
		}

		/// <summary>
		/// Opens the API reference page.
		/// </summary>
		[ContextMenu("API Reference")]
		private void OpenAPIReferencePage()
		{
			this.OpenReferencePage("uas-hypertext");
		}

		/// <summary>
		/// Sets the localized text.
		/// </summary>
		/// <remarks>Included for inspector.</remarks>
		/// <param name="value">Value.</param>
		private void SetLocaleOverrides(LocaleOverride[] value)
		{
			if (
				BackingFieldUtility.SetKeyedListBackingFieldFromArray(
					m_LocaleOverrides, value, (locale, wrapper) => new LocaleOverride(locale, wrapper.Data)
				)
			)
			{
				m_OnBecameDirty.Invoke();
			}
		}

		/// <summary>
		/// Sets the localized text.
		/// </summary>
		/// <param name="value">
		/// A dictionary of different possible <see cref="OutputText"/> values for this instance, keyed by locale.
		/// </param>
		public void SetLocaleOverrides(Dictionary<string, string> value)
		{
			if (
				BackingFieldUtility.SetKeyedListBackingFieldFromDict(
					m_LocaleOverrides, value, (locale, text) => new LocaleOverride(locale, text)
				)
			)
			{
				m_OnBecameDirty.Invoke();
			}
		}

		#region Obsolete
		[System.Serializable, System.Obsolete]
		public struct LocalizedText : IPropertyBackingFieldCompatible<LocalizedText>
		{
			#region Backing Fields
			[SerializeField]
			private string m_Locale;
			[SerializeField, TextArea(3, 10)]
			private string m_Text;
			#endregion
			public string Locale { get { return m_Locale = m_Locale ?? string.Empty; } }
			public string Text { get { return m_Text = m_Text ?? string.Empty; } }
			public LocalizedText(string locale, string text)
			{
				m_Locale = locale;
				m_Text = text;
			}
			public object Clone()
			{
				return this;
			}
			public override bool Equals(object obj)
			{
				return ObjectX.Equals(ref this, obj);
			}
			public bool Equals(LocalizedText other)
			{
				return GetHashCode() == other.GetHashCode();
			}
			public override int GetHashCode()
			{
				return ObjectX.GenerateHashCode(this.Locale.GetHashCode(), this.Text.GetHashCode());
			}
			public int GetSerializedPropertiesHash()
			{
				return GetHashCode();
			}
		}
		#pragma warning disable 612
		[SerializeField]
		private List<LocalizedText> m_LocalizedText = new List<LocalizedText>();
		[System.Obsolete("Use LocalizableText.GetLocaleOverrides(ref Dictionary<string, string>)", true)]
		public void SetLocalizedText(ref List<LocalizedText> localizedText) {}
		[System.Obsolete("Use LocalizableText.SetLocaleOverrides(Dictionary<string, string>)", true)]
		public void GetLocalizedText(IEnumerable<LocalizedText> value) {}
		#pragma warning restore 612
		#endregion
	}
}
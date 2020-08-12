// 
// HyperTextProcessor.cs
// 
// Copyright (c) 2014-2015, Candlelight Interactive, LLC
// All rights reserved.
// 
// This file is licensed according to the terms of the Unity Asset Store EULA:
// http://download.unity3d.com/assetstore/customer-eula.pdf

using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;

namespace Candlelight.UI
{
	/// <summary>
	/// A class for processing an input string as hyper text.
	/// </summary>
	/// <remarks>
	/// <para>This class's primary function is to extract &lt;a&gt; tags and their relevant information from the value
	/// supplied to the <see cref="HyperTextProcessor.InputText"/> property, and format the resulting string for the
	/// <see cref="HyperTextProcessor.OutputText"/> property. You can then use the
	/// <see cref="M:Candlelight.UI.HyperTextProcessor.GetLinks(System.Collections.Generic.List{Candlelight.UI.HyperTextProcessor.Link}@)" />
	/// method to get information about the links that were found, such as their character indices in
	/// <see cref="HyperTextProcessor.OutputText"/>. The minimal syntactical requirement for an &lt;a&gt; tag is the
	/// <c>name</c> attribute. For example, the input text <c>"Here is a &lt;a name="some_link"&gt;link&lt;/a&gt;"</c>
	/// will result in the output text <c>"Here is a link".</c></para>
	/// <para>Assigning a <see cref="HyperTextStyles"/> object to the <see cref="HyperTextProcessor.Styles"/> property
	/// also allows for some additional processing. If <see cref="HyperTextProcessor.IsRichTextDesired"/> is
	/// <see langword="true"/>, then it will automatically convert any custom tags and quads found in
	/// <see cref="HyperTextProcessor.InputText"/>, as well as insert &lt;color&gt; tags as needed. If
	/// <see cref="HyperTextProcessor.IsDynamicFontDesired"/> is <see langword="true"/>, then &lt;size&gt; tags will 
	/// automatically be inserted for links, custom tags, and quads. The value of &lt;size&gt; tags depends on either
	/// the font size specified in the styles or the <see cref="HyperTextProcessor.ReferenceFontSize"/> property if no
	/// styles are assigned, as well as the <see cref="HyperTextProcessor.ScaleFactor"/> property. Information about 
	/// custom tags and quads can then be extracted via
	/// <see cref="M:Candlelight.UI.HyperTextProcessor.GetCustomTags(System.Collections.Generic.List{Candlelight.UI.HyperTextProcessor.CustomTag}@)" />
	/// and
	/// <see cref="M:Candlelight.UI.HyperTextProcessor.GetQuads(System.Collections.Generic.List{Candlelight.UI.HyperTextProcessor.Quad}@)" />. 
	/// The syntactical requirements for custom styles are:</para>
	/// <para>Link Classes: <c>&lt;a name="some_link" class="class_name"&gt;link&lt;/a&gt;</c></para>
	/// <para>Tags: <c>&lt;custom&gt;text&lt;/custom&gt;</c></para>
	/// <para>Quads: <c>&lt;quad class="class_name" /&gt;</c></para>
	/// <para>You can also assign <see cref="KeywordCollection"/> objects to automatically detect and tag keywords
	/// appearing in <see cref="HyperTextProcessor.InputText"/> as either links or custom tags. Any links automatically
	/// detected in this way will have a <c>name</c> attribute equal to the keyword. For example, the word <c>"dog"</c>
	/// would become <c>"&lt;a name="dog"&gt;dog&lt;/a&gt;"</c>.</para>
	/// <para>The class also allows specification of sizes as percentages rather than raw values. For example, you can 
	/// use the pattern: <c>"&lt;size=120%&gt;BIG TEXT&lt;/size&gt;"</c>.</para>
	/// </remarks>
	[System.Serializable]
	public class HyperTextProcessor : System.IDisposable, ITextSource
	{
		/// <summary>
		/// A class for storing information about a custom tag indicated in the text.
		/// </summary>
		public class CustomTag : TagCharacterData
		{
			/// <summary>
			/// Gets the style.
			/// </summary>
			/// <value>The style.</value>
			public HyperTextStyles.Text Style { get; private set; }

			/// <summary>
			/// Initializes a new instance of the <see cref="HyperTextProcessor.CustomTag"/> class.
			/// </summary>
			/// <param name="indexRange">Index range.</param>
			/// <param name="style">Style.</param>
			public CustomTag(IndexRange indexRange, HyperTextStyles.Text style) : base(indexRange)
			{
				this.Style = style;
			}

			/// <summary>
			/// Clone this instance.
			/// </summary>
			/// <returns>A clone of this instance.</returns>
			public override object Clone()
			{
				return new CustomTag((IndexRange)this.CharacterIndices.Clone(), this.Style);
			}
		}
		
		/// <summary>
		/// A structure for storing a keyword collection and its associated class. It is used to create associations
		/// between keyword collections and styles specified in the style sheet.
		/// </summary>
		[System.Serializable]
		public struct KeywordCollectionClass : IPropertyBackingFieldCompatible<KeywordCollectionClass>
		{
			#region Backing Fields
			[SerializeField]
			private string m_ClassName;
			[SerializeField]
			private KeywordCollection m_Collection;
			#endregion
			
			/// <summary>
			/// Gets the name of the class.
			/// </summary>
			/// <value>The name of the class.</value>
			public string ClassName { get { return m_ClassName = m_ClassName ?? string.Empty; } }
			/// <summary>
			/// Gets the collection.
			/// </summary>
			/// <value>The collection.</value>
			public KeywordCollection Collection { get { return m_Collection; } }

			/// <summary>
			/// Initializes a new instance of the <see cref="KeywordCollectionClass"/> struct.
			/// </summary>
			/// <param name="className">Class name.</param>
			/// <param name="collection">Collection.</param>
			/// <exception cref="System.ArgumentNullException">Thrown if className is null.</exception>
			/// <exception cref="System.ArgumentException">Thrown if className is empty.</exception>
			public KeywordCollectionClass(string className, KeywordCollection collection) : this()
			{
				if (className == null)
				{
					throw new System.ArgumentNullException("className", "Class name cannot be null or empty.");
				}
				else if (
#if UNITY_EDITOR
					Application.isPlaying &&
#endif
					className == string.Empty
				)
				{
					throw new System.ArgumentException("Class name cannot be null or empty", "className");
				}
				m_ClassName = className;
				m_Collection = collection;
			}
			
			/// <summary>
			/// Clone this instance.
			/// </summary>
			/// <returns>A clone of this instance.</returns>
			public object Clone()
			{
				return this;
			}

			/// <summary>
			/// Determines whether the specified <see cref="System.Object"/> is equal to the current
			/// <see cref="KeywordCollectionClass"/>.
			/// </summary>
			/// <param name="obj">
			/// The <see cref="System.Object"/> to compare with the current <see cref="KeywordCollectionClass"/>.
			/// </param>
			/// <returns>
			/// <see langword="true"/> if the specified <see cref="System.Object"/> is equal to the current
			/// <see cref="KeywordCollectionClass"/>; otherwise, <see langword="false"/>.
			/// </returns>
			public override bool Equals(object obj)
			{
				return ObjectX.Equals(ref this, obj);
			}

			/// <summary>
			/// Determines whether the specified <see cref="KeywordCollectionClass"/> is equal to the current
			/// <see cref="KeywordCollectionClass"/>.
			/// </summary>
			/// <param name="other">
			/// The <see cref="KeywordCollectionClass"/> to compare with the current
			///  <see cref="HyperTextProcessor.KeywordCollectionClass"/>.
			/// </param>
			/// <returns>
			/// <see langword="true"/> if the specified <see cref="KeywordCollectionClass"/> is equal to the current
			/// <see cref="KeywordCollectionClass"/>; otherwise, <see langword="false"/>.
			/// </returns>
			public bool Equals(KeywordCollectionClass other)
			{
				return GetHashCode() == other.GetHashCode();
			}

			/// <summary>
			/// Serves as a hash function for a <see cref="KeywordCollectionClass"/> object.
			/// </summary>
			/// <returns>
			/// A hash code for this instance that is suitable for use in hashing algorithms and data structures such as
			/// a hash table.
			/// </returns>
			public override int GetHashCode()
			{
				return ObjectX.GenerateHashCode(this.ClassName.GetHashCode(), m_Collection.GetHashCode());
			}
			
			/// <summary>
			/// Gets a hash value that is based on the values of the serialized properties of this instance.
			/// </summary>
			/// <returns>A hash value based on the values of the serialized properties on this instance.</returns>
			public int GetSerializedPropertiesHash()
			{
				return GetHashCode();
			}
		}
		
		/// <summary>
		/// A class for storing information about a link indicated in the text.
		/// </summary>
		public class Link : TagCharacterData
		{
			/// <summary>
			/// Gets the name of the class.
			/// </summary>
			/// <value>The name of the class.</value>
			public string ClassName { get; private set; }
			/// <summary>
			/// Gets the value of the <c>name</c> attribute.
			/// </summary>
			/// <value>The value of the <c>name</c> attribute.</value>
			public string Name { get; private set; }
			/// <summary>
			/// Gets or sets the style.
			/// </summary>
			/// <value>The style.</value>
			public HyperTextStyles.Link Style { get; private set; }

			/// <summary>
			/// Initializes a new instance of the <see cref="HyperTextProcessor.Link"/> class.
			/// </summary>
			/// <param name="linkName">Value of the link's <c>name</c> attribute.</param>
			/// <param name="className">Class name.</param>
			/// <param name="characterIndices">Character indices.</param>
			/// <param name="style">Style.</param>
			public Link(
				string linkName, string className, IndexRange characterIndices, HyperTextStyles.Link style
			) : base(characterIndices)
			{
				this.Name = linkName;
				this.ClassName = className;
				this.Style = style;
			}

			/// <summary>
			/// Clone this instance.
			/// </summary>
			/// <returns>A clone of this instance.</returns>
			public override object Clone()
			{
				return new Link(this.Name, this.ClassName, (IndexRange)this.CharacterIndices.Clone(), this.Style);
			}

			#region Obsolete
			[System.Obsolete("Use HyperTextProcessor.Link.Name")]
			public string Id { get { return this.Name; } }
			#endregion
		}
		
		/// <summary>
		/// A class for storing information about a quad indicated in the text.
		/// </summary>
		public class Quad : TagCharacterData
		{
			/// <summary>
			/// Gets the style.
			/// </summary>
			/// <value>The style.</value>
			public HyperTextStyles.Quad Style { get; private set; }

			/// <summary>
			/// Initializes a new instance of the <see cref="HyperTextProcessor.Quad"/> class.
			/// </summary>
			/// <param name="indexRange">Index range.</param>
			/// <param name="style">Style.</param>
			public Quad(IndexRange indexRange, HyperTextStyles.Quad style) : base(indexRange)
			{
				this.Style = style;
			}

			/// <summary>
			/// Clone this instance.
			/// </summary>
			/// <returns>A clone of this instance.</returns>
			public override object Clone()
			{
				return new Quad((IndexRange)this.CharacterIndices.Clone(), this.Style);
			}
		}
		
		/// <summary>
		/// A base class for storing data about the characters for a tag appearing in the text.
		/// </summary>
		public abstract class TagCharacterData : System.ICloneable
		{
			/// <summary>
			/// Gets or sets the character indices.
			/// </summary>
			/// <value>The character indices.</value>
			public IndexRange CharacterIndices { get; set; }

			/// <summary>
			/// Initializes a new instance of the <see cref="HyperTextProcessor.TagCharacterData"/>
			/// class.
			/// </summary>
			/// <param name="indexRange">Index range.</param>
			protected TagCharacterData(IndexRange indexRange)
			{
				this.CharacterIndices = indexRange;
			}

			/// <summary>
			/// Clone this instance.
			/// </summary>
			/// <returns>A clone of this instance.</returns>
			public abstract object Clone();
		}

		/// <summary>
		/// A regular expression to extract an &lt;a&gt; tag, its arguments, and enclosed text in postprocessed text.
		/// </summary>
		private static readonly Regex s_PostprocessedLinkTagRegex = new Regex(
			string.Format(
				"<a name\\s*=\\s*\"(?<{0}>.*?)\"(\\s+class\\s*=\\s*\"(?<{1}>.*?)\")?>(?<{2}>.*?)(?<{3}></a>)",
				AttributeValueCaptureGroup, ClassNameCaptureGroup, TextCaptureGroup, CloseTagCaptureGroup
			),
			RegexOptions.Singleline | RegexOptions.IgnoreCase
		);
		/// <summary>
		/// A regular expression to extract the attribute value of a &lt;size&gt; tag or the size attribute of a
		/// &lt;quad&gt; tag.
		/// </summary>
		private static readonly Regex s_PostProcessedSizeAttributeRegex = new Regex(
			string.Format(
				@"(?<{0}><size\s*=\s*)(?<{1}>\d+)(?<{2}>>)|(?<{0}><quad\b[^>]*?\bsize=)(?<{1}>\d+)(?<{2}>[^>]*?>)",
				OpenTagCaptureGroup, AttributeValueCaptureGroup, CloseTagCaptureGroup
			),
			RegexOptions.Singleline | RegexOptions.IgnoreCase
		);
		/// <summary>
		/// The base match pattern for any rich text tag in preprocessed text (used when supportRichText = true).
		/// </summary>
		private static readonly string s_PreprocessedAnyTagMatchPattern =
			"</?a\b.*?>|" +
			"<quad\b.*?>|" +
			"</?color\b.*?>|" +
			"</?i>|" +
			"</?b>|" +
			"</?size\b.*?>|" +
			"</?material\b.*?>";
		/// <summary>
		/// A regular expression to match only &lt;a&gt; tags in preprocessed text (used when supportRichText = false).
		/// </summary>
		private static readonly Regex s_PreprocessedLinkTagRegex =
			new Regex("</?a\b.*?>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
		/// <summary>
		/// A regular expression to extract a &lt;size&gt; tag and its arguments in preprocessed text.
		/// </summary>
		private static readonly Regex s_PreprocessedSizeTagRegex = new Regex(
			string.Format(
				"<size\\s*=\\s*(?<{0}>\\d*\\.?\\d+%?)>(?<{1}>.+?)</size>", AttributeValueCaptureGroup, TextCaptureGroup
			),
			RegexOptions.Singleline | RegexOptions.IgnoreCase
		);
		/// <summary>
		/// A regular expression to extract a &lt;quad&gt; tag in text.
		/// </summary>
		private static readonly Regex s_QuadTagRegex = new Regex(
			string.Format("<quad class\\s*=\\s*\"(?<{0}>.+?)\"\\s*.*?/?>", ClassNameCaptureGroup),
			RegexOptions.IgnoreCase
		);

		#region Shared Allocations
		private static List<HyperTextStyles.LinkSubclass> s_CascadedLinkStyles =
			new List<HyperTextStyles.LinkSubclass>(64);
		private static List<HyperTextStyles.Quad> s_CascadedQuadStyles = new List<HyperTextStyles.Quad>(64);
		private static List<HyperTextStyles.Text> s_CascadedTextStyles = new List<HyperTextStyles.Text>(64);
		private static string s_CloseTag = string.Empty;
		private static readonly Dictionary<IndexRange, int> s_IndexRangeOffsets = new Dictionary<IndexRange, int>();
		private static string s_OpenTag = string.Empty;
		private static readonly HashSet<string> s_ProcessedKeywords = new HashSet<string>();
		private static StringBuilder s_ProcessedTextBuilder;
		private static string s_Segment = string.Empty;
		private static string s_TextCache;
		#endregion
		#region Backing Fields
		private static readonly ReadOnlyCollection<string> s_ReservedTags = new ReadOnlyCollection<string>(
			new [] { "a", "b", "color", "i", "material", "quad", "size" }
		);
		#endregion

		/// <summary>
		/// Gets the name of the capture group used for a tag's attribute value of interest.
		/// </summary>
		/// <value>The name of the capture group used for a tag's attribute value of interest.</value>
		private static string AttributeValueCaptureGroup { get { return "attributeValue"; } }
		/// <summary>
		/// Gets the name of the capture group used for a tag's class attribute value.
		/// </summary>
		/// <value>The name of the capture group used for a tag's class attribute value.</value>
		private static string ClassNameCaptureGroup { get { return "className"; } }
		/// <summary>
		/// Gets the name of the capture group used for a close tag in a piece of text.
		/// </summary>
		/// <value>The name of the capture group used for a close tag in a piece of text.</value>
		public static string CloseTagCaptureGroup { get { return "closeTag"; } }
		/// <summary>
		/// Gets the name of the capture group used for an open tag in a piece of text.
		/// </summary>
		/// <value>The name of the capture group used for an open tag in a piece of text.</value>
		public static string OpenTagCaptureGroup { get { return "openTag"; } }
		/// <summary>
		/// Gets the reserved tags.
		/// </summary>
		/// <value>The reserved tags.</value>
		public static ReadOnlyCollection<string> ReservedTags { get { return s_ReservedTags; } }
		/// <summary>
		/// Gets the name of the capture group used for text enclosed in a tag.
		/// </summary>
		/// <value>The name of the capture group used for text enclosed in a tag.</value>
		public static string TextCaptureGroup { get { return "text"; } }
		#region Backing Fields
		private List<TagCharacterData> m_CustomTags = new List<TagCharacterData>();
		[SerializeField, PropertyBackingField]
		private List<KeywordCollectionClass> m_LinkKeywordCollections = new List<KeywordCollectionClass>();
		private List<Link> m_Links = new List<Link>();
		[SerializeField, PropertyBackingField]
		private string m_InputText = string.Empty;
		[SerializeField, PropertyBackingField]
		private Object m_InputTextSourceObject = null;
		private ITextSource m_InputTextSource;
		[SerializeField, PropertyBackingField]
		private bool m_IsDynamicFontDesired = true;
		[SerializeField, PropertyBackingField]
		private bool m_IsRichTextDesired = true;
		private UnityEngine.Events.UnityEvent m_OnBecameDirty = new UnityEngine.Events.UnityEvent();
		[SerializeField, HideInInspector] // serialize this so editor undo/redo bypasses lazy evaluation
		private string m_OutputText = string.Empty;
		[SerializeField, PropertyBackingField]
		private List<KeywordCollectionClass> m_QuadKeywordCollections = new List<KeywordCollectionClass>();
		private List<Quad> m_Quads = new List<Quad>();
		[SerializeField, PropertyBackingField]
		private int m_ReferenceFontSize = 14;
		private float m_ScaleFactor = 1f;
		[SerializeField, PropertyBackingField]
		private bool m_ShouldOverrideStylesFontSize = false;
		[SerializeField, PropertyBackingField]
		private HyperTextStyles m_Styles = null;
		[SerializeField, PropertyBackingField]
		private List<KeywordCollectionClass> m_TagKeywordCollections = new List<KeywordCollectionClass>();
		#endregion
		/// <summary>
		/// A value indicating whether or not m_ProcessedText is currently dirty.
		/// </summary>
		private bool m_IsDirty = true;

		/// <summary>
		/// Gets the default link style.
		/// </summary>
		/// <value>The default link style.</value>
		private HyperTextStyles.Link DefaultLinkStyle
		{
			get { return m_Styles == null ? HyperTextStyles.Link.DefaultStyle : m_Styles.DefaultLinkStyle; }
		}
		/// <summary>
		/// Gets the font size to use.
		/// </summary>
		/// <value>The font size to use.</value>
		private int FontSizeToUse
		{
			get
			{
				return m_ShouldOverrideStylesFontSize || m_Styles == null ?
					this.ReferenceFontSize : m_Styles.CascadedFontSize;
			}
		}
		/// <summary>
		/// Gets a GUIStyle for the current property values.
		/// </summary>
		/// <value>A GUIStyle for the current property values.</value>
		public GUIStyle GUIStyle
		{
			get
			{
				GUIStyle result = new GUIStyle();
				result.fontSize = this.FontSizeToUse;
				result.fontStyle = m_Styles == null ? FontStyle.Normal : m_Styles.DefaultFontStyle;
				result.normal.textColor = m_Styles == null ? Color.white : m_Styles.DefaultTextColor;
				result.richText = this.IsRichTextDesired;
				return result;
			}
		}
		/// <summary>
		/// Gets or sets the input text.
		/// </summary>
		/// <value>The input text.</value>
		public string InputText
		{
			get { return m_InputText; }
			set
			{
				if (m_InputText != value)
				{
					m_InputText = value;
					SetDirty();
				}
			}
		}
		/// <summary>
		/// Gets or sets the input text source. If a value is assigned, its OutputText will be used in place of the
		/// value in the InputText property of this <see cref="HyperTextProcessor"/>.
		/// </summary>
		/// <value>The input text source.</value>
		public ITextSource InputTextSource
		{
			get { return m_InputTextSource = m_InputTextSource ?? m_InputTextSourceObject as ITextSource; }
			set
			{
				if (value != m_InputTextSource)
				{
					if (m_InputTextSource != null)
					{
						m_InputTextSource.OnBecameDirty.RemoveListener(SetDirty);
					}
					m_InputTextSource = value;
					m_InputTextSourceObject = value as Object; // NOTE: store for inspector/serialization
					if (m_InputTextSource != null)
					{
						m_InputTextSource.OnBecameDirty.AddListener(SetDirty);
					}
					SetDirty();
				}
			}
		}
		/// <summary>
		/// Gets or sets the input text source object. This property only exists for the inspector.
		/// </summary>
		/// <remarks>Included for inspector.</remarks>
		/// <value>The input text source object.</value>
		private Object InputTextSourceObject
		{
			get { return m_InputTextSourceObject; }
			set
			{
				if (value != null && !(value is ITextSource))
				{
					Debug.LogError(
						string.Format("{0} must implement {1}.", value.GetType().FullName, typeof(ITextSource).FullName)
					);
				}
				value = (value is ITextSource) ? value : null;
				m_InputTextSourceObject = value;
				this.InputTextSource = m_InputTextSourceObject as ITextSource;
			}
		}
		/// <summary>
		/// Gets the input text to use.
		/// </summary>
		/// <value>The input text to use.</value>
		private string InputTextToUse
		{
			get { return this.InputTextSource != null ? m_InputTextSource.OutputText : m_InputText; }
		}
		/// <summary>
		/// Gets or sets a value indicating whether dynamic font output is desired on this instance.
		/// </summary>
		/// <value>
		/// <see langword="true"/> if this dynamic font output is desired on this instance; otherwise,
		/// <see langword="false"/>.
		/// </value>
		public bool IsDynamicFontDesired
		{
			get { return m_IsDynamicFontDesired; }
			set
			{
				if (m_IsDynamicFontDesired != value)
				{
					m_IsDynamicFontDesired = value;
					SetDirty();
				}
			}
		}
		/// <summary>
		/// Gets a value indicating whether dynamic font output is enabled.
		/// </summary>
		/// <value><see langword="true"/> if dynamic font output is enabled; otherwise, <see langword="false"/>.</value>
		public bool IsDynamicFontEnabled { get { return m_IsDynamicFontDesired && m_IsRichTextDesired; } }
		/// <summary>
		/// Gets or sets a value indicating whether rich text is desired on this instance.
		/// </summary>
		/// <value>
		/// <see langword="true"/> if rich text is desired on this instance; otherwise, <see langword="false"/>.
		/// </value>
		public bool IsRichTextDesired
		{
			get { return m_IsRichTextDesired; }
			set
			{
				if (m_IsRichTextDesired != value)
				{
					m_IsRichTextDesired = value;
					SetDirty();
				}
			}
		}
		/// <summary>
		/// Gets a value indicating whether rich text is enabled on this instance.
		/// </summary>
		/// <value><see langword="true"/> if rich text is enabled; otherwise, <see langword="false"/>.</value>
		public bool IsRichTextEnabled { get { return m_IsRichTextDesired && m_Styles != null; } }
		/// <summary>
		/// Gets a callback for whenever a value on this instance has changed.
		/// </summary>
		/// <value>A callback for whenever a value on this instance has changed.</value>
		public UnityEngine.Events.UnityEvent OnBecameDirty
		{
			get
			{
				if (m_OnBecameDirty == null)
				{
					m_OnBecameDirty = new UnityEngine.Events.UnityEvent();
				}
				return m_OnBecameDirty;
			}
		}
		/// <summary>
		/// Gets the output text.
		/// </summary>
		/// <value>The output text.</value>
		public string OutputText
		{
			get
			{
				ProcessInputText();
				return m_OutputText;
			}
		}
		/// <summary>
		/// Gets or sets the reference font size. It should correspond to the font size where OutputText will be sent.
		/// </summary>
		/// <value>The reference font size.</value>
		public int ReferenceFontSize
		{
			get { return m_ReferenceFontSize; }
			set
			{
				value = Mathf.Max(value, 0);
				if (m_ReferenceFontSize != value)
				{
					m_ReferenceFontSize = value;
					SetDirty();
				}
			}
		}
		/// <summary>
		/// Gets or sets the scale factor.
		/// </summary>
		/// <value>The scale factor.</value>
		public float ScaleFactor
		{
			get { return m_ScaleFactor; }
			set
			{
				if (m_ScaleFactor != value)
				{
					m_ScaleFactor = value;
					SetDirty();
				}
			}
		}
		/// <summary>
		/// Gets the size of the font multiplied by the DPI.
		/// </summary>
		/// <value>The size of the font multiplied by the DPI.</value>
		private int ScaledFontSize { get { return (int)(this.FontSizeToUse * this.ScaleFactor); } }
		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="HyperTextProcessor"/> should override the font size 
		/// specified in styles, if one is assigned.
		/// </summary>
		/// <value>
		/// <see langword="true"/> if should override the font size specified in styles; otherwise, 
		/// <see langword="false"/>.
		/// </value>
		public bool ShouldOverrideStylesFontSize
		{
			get { return m_ShouldOverrideStylesFontSize; }
			set
			{
				if (m_ShouldOverrideStylesFontSize != value)
				{
					m_ShouldOverrideStylesFontSize = value;
					SetDirty();
				}
			}
		}
		/// <summary>
		/// Gets or sets the styles.
		/// </summary>
		/// <value>The styles.</value>
		public HyperTextStyles Styles
		{
			get { return m_Styles; }
			set
			{
				if (m_Styles == value)
				{
					return;
				}
				if (m_Styles != null)
				{
					m_Styles.OnStylesChanged.RemoveListener(SetDirty);
				}
				m_Styles = value;
				if (m_Styles != null)
				{
					m_Styles.OnStylesChanged.AddListener(SetDirty);
				}
				SetDirty();
			}
		}

		/// <summary>
		/// Releases all resource used by the <see cref="HyperTextProcessor"/> object.
		/// </summary>
		/// <remarks>
		/// Call <see cref="Dispose"/> when you are finished using the <see cref="HyperTextProcessor"/>. The
		/// <see cref="Dispose"/> method leaves the <see cref="HyperTextProcessor"/> in an unusable state. After calling 
		/// <see cref="Dispose"/>, you must release all references to the <see cref="HyperTextProcessor"/> so the
		/// garbage collector can reclaim the memory that the <see cref="HyperTextProcessor"/> was occupying.
		/// </remarks>
		public void Dispose()
		{
			m_OnBecameDirty.RemoveAllListeners();
		}

		/// <summary>
		/// Gets the custom tags extracted from the text.
		/// </summary>
		/// <param name="tags">Tags.</param>
		public void GetCustomTags(ref List<CustomTag> tags)
		{
			ProcessInputText();
			tags = tags ?? new List<CustomTag>(m_CustomTags.Count);
			tags.Clear();
			tags.AddRange(from customTag in m_CustomTags select (CustomTag)customTag.Clone());
		}

		/// <summary>
		/// Gets the link keyword collections.
		/// </summary>
		/// <remarks>Included for inspector.</remarks>
		/// <returns>The link keyword collections.</returns>
		private KeywordCollectionClass[] GetLinkKeywordCollections()
		{
			return m_LinkKeywordCollections.ToArray();
		}

		/// <summary>
		/// Gets the link keyword collections.
		/// </summary>
		/// <param name="collections">Collections.</param>
		public void GetLinkKeywordCollections(ref List<KeywordCollectionClass> collections)
		{
			collections = collections ?? new List<KeywordCollectionClass>(m_LinkKeywordCollections.Count);
			collections.Clear();
			collections.AddRange(m_LinkKeywordCollections);
		}

		/// <summary>
		/// Gets the links extracted from the text.
		/// </summary>
		/// <param name="links">Links.</param>
		public void GetLinks(ref List<Link> links)
		{
			ProcessInputText();
			links = links ?? new List<Link>(m_Links.Count);
			links.Clear();
			links.AddRange(from link in m_Links select (Link)link.Clone());
		}
		
		/// <summary>
		/// Gets a version of the quad tag corresponding to the supplied Match with all of its arguments injected.
		/// </summary>
		/// <returns>The postprocessed quad tag corresponding to the supplied Match.</returns>
		/// <param name="quadTagMatch">Quad tag match.</param>
		/// <param name="quadTemplates">The list of quad styles specified on the styles object.</param>
		private string GetPostprocessedQuadTag(Match quadTagMatch, List<HyperTextStyles.Quad> quadTemplates)
		{
			string quadName = quadTagMatch.Groups[ClassNameCaptureGroup].Value;
			string linkOpenTag = "";
			float sizeScalar = 1f;
			float width, height;
			Vector4 padding;
			float aspect = 1f;
			int templateIndex = quadTemplates.FindIndex(quad => quad.ClassName == quadName);
			if (templateIndex >= 0)
			{
				if (!string.IsNullOrEmpty(quadTemplates[templateIndex].LinkId))
				{
					linkOpenTag = string.Format(
						"<a name=\"{0}\"{1}>",
						quadTemplates[templateIndex].LinkId,
						string.IsNullOrEmpty(quadTemplates[templateIndex].LinkClassName) ?
							"" : string.Format(" class=\"{0}\"", quadTemplates[templateIndex].LinkClassName)
					);
				}
				if (quadTemplates[templateIndex].Sprite != null)
				{
					padding = UnityEngine.Sprites.DataUtility.GetPadding(quadTemplates[templateIndex].Sprite);
					Rect rect = quadTemplates[templateIndex].Sprite.rect;
					width = rect.width - padding.z - padding.x;
					height = rect.height - padding.w - padding.y;
					aspect = height == 0f ? 0f : width / height;
				}
				sizeScalar = quadTemplates[templateIndex].SizeScalar;
			}
			return string.Format(
				"{0}<quad class=\"{1}\" size={2} width={3}>{4}",
				linkOpenTag,
				quadName,
				sizeScalar * this.ScaledFontSize,
				aspect,
				string.IsNullOrEmpty(linkOpenTag) ? "" : "</a>"
			);
		}

		/// <summary>
		/// Gets the quad keyword collections.
		/// </summary>
		/// <remarks>Included for inspector.</remarks>
		/// <returns>The quad keyword collections.</returns>
		private KeywordCollectionClass[] GetQuadKeywordCollections()
		{
			return m_QuadKeywordCollections.ToArray();
		}

		/// <summary>
		/// Gets the quad keyword collections.
		/// </summary>
		/// <param name="collections">Collections.</param>
		public void GetQuadKeywordCollections(ref List<KeywordCollectionClass> collections)
		{
			collections = collections ?? new List<KeywordCollectionClass>(m_QuadKeywordCollections.Count);
			collections.Clear();
			collections.AddRange(m_QuadKeywordCollections);
		}

		/// <summary>
		/// Gets the quads extracted from the text.
		/// </summary>
		/// <param name="quads">Quads.</param>
		public void GetQuads(ref List<Quad> quads)
		{
			ProcessInputText();
			quads = quads ?? new List<Quad>(m_Quads.Count);
			quads.Clear();
			quads.AddRange(from quad in m_Quads select (Quad)quad.Clone());
		}
		
		/// <summary>
		/// Gets the tag keyword collections.
		/// </summary>
		/// <remarks>Included for inspector.</remarks>
		/// <returns>The tag keyword collections.</returns>
		private KeywordCollectionClass[] GetTagKeywordCollections()
		{
			return m_TagKeywordCollections.ToArray();
		}

		/// <summary>
		/// Gets the tag keyword collections.
		/// </summary>
		/// <param name="collections">Collections.</param>
		public void GetTagKeywordCollections(ref List<KeywordCollectionClass> collections)
		{
			collections = collections ?? new List<KeywordCollectionClass>(m_TagKeywordCollections.Count);
			collections.Clear();
			collections.AddRange(m_TagKeywordCollections);
		}

		/// <summary>
		/// Initializes the keyword collection callbacks for the specified backing field.
		/// </summary>
		/// <param name="backingField">Backing field.</param>
		private void InitializeKeywordCollectionCallbacks(List<KeywordCollectionClass> backingField)
		{
			for (int i = 0; i < backingField.Count; ++i)
			{
				if (backingField[i].Collection != null)
				{
					backingField[i].Collection.OnRebuildKeywords.RemoveListener(SetDirty);
					backingField[i].Collection.OnRebuildKeywords.AddListener(SetDirty);
				}
			}
		}
		
		/// <summary>
		/// Inserts tags arround the supplied keyword into the text segment.
		/// </summary>
		/// <returns>The text segment with custom tags inserted.</returns>
		/// <param name="textSegment">Segment of text to modify.</param>
		/// <param name="keyword">Keyword.</param>
		/// <param name="tag">Tag.</param>
		/// <param name="matchMode">Match mode.</param>
		private string InsertCustomTagsIntoSegment(
			string textSegment, string keyword, string tag, CaseMatchMode matchMode
		)
		{
			if (string.IsNullOrEmpty(tag))
			{
				return textSegment;
			}
			Regex regex = new Regex(
				string.Format("(?<=^|\\W){0}(?=\\W|$)", Regex.Escape(keyword)),
				matchMode == CaseMatchMode.IgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None
			);
			return regex.Replace(textSegment, string.Format("<{0}>{1}</{0}>", tag, regex.Match(textSegment).Value));
		}
		
		/// <summary>
		/// Inserts links for the supplied keyword into the text segment.
		/// </summary>
		/// <returns>The text segment with keyword links inserted.</returns>
		/// <param name="textSegment">Segment of text to modify.</param>
		/// <param name="keyword">Keyword.</param>
		/// <param name="className">Class name.</param>
		/// <param name="matchMode">Match mode.</param>
		private string InsertKeywordLinksIntoSegment(
			string textSegment, string keyword, string className, CaseMatchMode matchMode
		)
		{
			Regex regex = new Regex(
				string.Format("(?<=^|\\W){0}(?=\\W|$)", Regex.Escape(keyword)),
				matchMode == CaseMatchMode.IgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None
			);
			return regex.Replace(
				textSegment,
				string.Format(
					"<a name=\"{0}\"{1}>{2}</a>",
					keyword,
					string.IsNullOrEmpty(className) || !this.IsRichTextEnabled ?
						"" : string.Format(" class=\"{0}\"", className),
					regex.Match(textSegment).Value
				)
			);
		}

		/// <summary>
		/// Inserts a quad tag for the supplied keyword into the text segment.
		/// </summary>
		/// <returns>The text segment with quad tags inserted.</returns>
		/// <param name="textSegment">Segment of text to modify.</param>
		/// <param name="keyword">Keyword.</param>
		/// <param name="className">Class name.</param>
		/// <param name="matchMode">Match mode.</param>
		private string InsertQuadTagIntoSegment(
			string textSegment, string keyword, string className, CaseMatchMode matchMode
		)
		{
			if (string.IsNullOrEmpty(className))
			{
				return textSegment;
			}
			Regex regex = new Regex(
				string.Format("(?<=^|\\W){0}(?=\\W|$)", Regex.Escape(keyword)),
				matchMode == CaseMatchMode.IgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None
			);
			return regex.Replace(textSegment, string.Format("<quad class=\"{0}\">", className));
		}

		/// <summary>
		/// Raises the enable event.
		/// </summary>
		public void OnEnable()
		{
			if (m_Styles != null)
			{
				m_Styles.OnStylesChanged.RemoveListener(SetDirty);
				m_Styles.OnStylesChanged.AddListener(SetDirty);
			}
			InitializeKeywordCollectionCallbacks(m_LinkKeywordCollections);
			InitializeKeywordCollectionCallbacks(m_QuadKeywordCollections);
			InitializeKeywordCollectionCallbacks(m_TagKeywordCollections);
			if (this.InputTextSource != null)
			{
				m_InputTextSource.OnBecameDirty.RemoveListener(SetDirty);
				m_InputTextSource.OnBecameDirty.AddListener(SetDirty);
			}
			SetDirty();
		}

		/// <summary>
		/// Processes the input text.
		/// </summary>
		private void ProcessInputText()
		{
			// early out if already up to date
			if (!m_IsDirty)
			{
				return;
			}
			// initialize variables used throughout this method
			s_ProcessedTextBuilder = new StringBuilder();
			int indexInRawString = 0;
			Dictionary<string, HyperTextStyles.Text> customTags = new Dictionary<string, HyperTextStyles.Text>();
			if (m_Styles != null)
			{
				m_Styles.GetCascadedCustomTextStyles(ref s_CascadedTextStyles);
				for (int i = 0; i < s_CascadedTextStyles.Count; ++i)
				{
					if (
						!string.IsNullOrEmpty(s_CascadedTextStyles[i].Tag) &&
						!customTags.ContainsKey(s_CascadedTextStyles[i].Tag)
					)
					{
						customTags.Add(s_CascadedTextStyles[i].Tag, s_CascadedTextStyles[i]);
					}
				}
			}
			// insert tags in text for words present in keyword collections
			s_TextCache = SubstituteTagsInForKeywords(this.InputTextToUse);
			// if rich text is enabled, substitute quad arguments, discrete sizes, and custom tag styles into text
			Dictionary<string, HyperTextStyles.Link> linkStyles = new Dictionary<string, HyperTextStyles.Link>();
			if (m_Styles == null)
			{
				s_CascadedQuadStyles.Clear();
			}
			else
			{
				m_Styles.GetCascadedQuadStyles(ref s_CascadedQuadStyles);
			}
			m_CustomTags.Clear();
			m_Quads.Clear();
			Dictionary<IndexRange, float> processedIndexRangesAndScalars = new Dictionary<IndexRange, float>();
			if (this.IsRichTextEnabled)
			{
				// sub quad arguments into text
				s_TextCache =
					s_QuadTagRegex.Replace(s_TextCache, match => GetPostprocessedQuadTag(match, s_CascadedQuadStyles));
				// substitute sizes in for percentages
				s_TextCache = s_PreprocessedSizeTagRegex.Replace(
					s_TextCache,
					match => string.Format(
						"<size={0}>{1}</size>",
						match.Groups[AttributeValueCaptureGroup].Value.EndsWith("%") ?
							(int)(
								float.Parse(
									match.Groups[AttributeValueCaptureGroup].Value.Substring(
										0, match.Groups[AttributeValueCaptureGroup].Value.Length - 1
									)
								) * this.ScaledFontSize * 0.01f
							) : (
								(int)float.Parse(match.Groups[AttributeValueCaptureGroup].Value) > 0 ?
								(int)float.Parse(match.Groups[AttributeValueCaptureGroup].Value) : this.ScaledFontSize
							),
							match.Groups[TextCaptureGroup].Value
					)
				);
				// substitute text styles in for custom tags
				foreach (HyperTextStyles.Text style in customTags.Values)
				{
					RichTextStyle textStyle =
						this.IsDynamicFontEnabled ? style.TextStyle : style.TextStyle.NonDynamicVersion;
					while (style.TagRegex.IsMatch(s_TextCache))
					{
						s_TextCache = style.TagRegex.Replace(
							s_TextCache,
							delegate(Match match)
							{
								s_OpenTag = textStyle.ToStartTag(ScaledFontSize);
								s_Segment = match.Groups[TextCaptureGroup].Value;
								s_CloseTag = textStyle.ToEndTag();
								IndexRange characterIndices = new IndexRange(
									match.Index + s_OpenTag.Length, match.Index + s_OpenTag.Length + s_Segment.Length - 1
								);
								s_IndexRangeOffsets.Clear();
								s_IndexRangeOffsets.Add(
									// start range one after match so start indices of enclosing tags aren't affected
									new IndexRange(match.Index + 1, match.Groups[CloseTagCaptureGroup].Index - 1),
									s_OpenTag.Length - match.Groups[OpenTagCaptureGroup].Length
								);
								s_IndexRangeOffsets.Add(
									new IndexRange(match.Groups[CloseTagCaptureGroup].Index, s_TextCache.Length),
									s_CloseTag.Length - match.Groups[CloseTagCaptureGroup].Length
								);
								foreach (IndexRange range in processedIndexRangesAndScalars.Keys)
								{
									range.Offset(s_IndexRangeOffsets);
								}
								processedIndexRangesAndScalars.Add(characterIndices, style.TextStyle.SizeScalar);
								CustomTag tag = new CustomTag(characterIndices, customTags[style.Tag]);
								m_CustomTags.Add(tag);
								return string.Format("{0}{1}{2}", s_OpenTag, s_Segment, s_CloseTag);
							},
							1 // only replace first instance so indices are properly set for any subsequent matches
						);
					}
				}
				// collect link styles
				this.Styles.GetCascadedLinkStyles(ref s_CascadedLinkStyles);
				for (int i = 0; i < s_CascadedLinkStyles.Count; ++i)
				{
					if (
						!string.IsNullOrEmpty(s_CascadedLinkStyles[i].ClassName) &&
						!linkStyles.ContainsKey(s_CascadedLinkStyles[i].ClassName)
					)
					{
						linkStyles.Add(s_CascadedLinkStyles[i].ClassName, s_CascadedLinkStyles[i].Style);
					}
				}
			}
			// remove <a> tags from processed text and record the link character indices
			string className;
			string textCapture;
			m_Links.Clear();
			foreach (Match match in s_PostprocessedLinkTagRegex.Matches(s_TextCache))
			{
				// append everything since last append
				s_ProcessedTextBuilder.Append(s_TextCache.Substring(indexInRawString, match.Index - indexInRawString));
				// get link class and style from match
				HyperTextStyles.Link linkStyle = this.DefaultLinkStyle;
				className = match.Groups[ClassNameCaptureGroup].Value;
				if (
					match.Groups[ClassNameCaptureGroup].Success &&
					linkStyles.ContainsKey(match.Groups[ClassNameCaptureGroup].Value)
				)
				{
					linkStyle = linkStyles[className];
				}
				// create the result for the substitution
				RichTextStyle textStyle =
					this.IsDynamicFontEnabled ? linkStyle.TextStyle : linkStyle.TextStyle.NonDynamicVersion;
				s_OpenTag = textStyle.ToStartTag(this.ScaledFontSize);
				s_CloseTag = textStyle.ToEndTag();
				textCapture = match.Groups[TextCaptureGroup].Value;
				string result = textCapture;
				if (this.IsRichTextEnabled)
				{
					result = string.Format("{0}{1}{2}", s_OpenTag, textCapture, s_CloseTag);
				}
				// append substitution
				s_ProcessedTextBuilder.Append(result);
				indexInRawString = match.Index + match.Length;
				// store the data for the link
				int startPosition = s_ProcessedTextBuilder.Length -
					(this.IsRichTextEnabled ? s_CloseTag.Length : 0) -
					textCapture.Length;
				Link newLink = new Link(
					match.Groups[AttributeValueCaptureGroup].Value,
					className,
					new IndexRange(startPosition, startPosition + textCapture.Length - 1),
					linkStyle
				);
				m_Links.Add(newLink);
				// offset existing index ranges as needed
				s_IndexRangeOffsets.Clear();
				// add close tag first in case it shifts range backward
				s_IndexRangeOffsets.Add(
					new IndexRange(match.Groups[CloseTagCaptureGroup].Index, s_TextCache.Length),
					s_CloseTag.Length - match.Groups[CloseTagCaptureGroup].Length
				);
				s_IndexRangeOffsets.Add(
					// start range one after match so that start indices of enclosing tags aren't affected
					new IndexRange(match.Index + 1, match.Groups[CloseTagCaptureGroup].Index - 1),
					s_OpenTag.Length - (match.Groups[TextCaptureGroup].Index - match.Index)
				);
				foreach (IndexRange range in processedIndexRangesAndScalars.Keys)
				{
					range.Offset(s_IndexRangeOffsets);
				}
			}
			s_ProcessedTextBuilder.Append(s_TextCache.Substring(indexInRawString, s_TextCache.Length - indexInRawString));
			m_OutputText = s_ProcessedTextBuilder.ToString();
            // pull out data for quads and finalize sizes if rich text is enabled
            if (this.IsRichTextEnabled)
            {
                // multiply out overlapping sizes if dynamic font is enabled
                if (this.IsDynamicFontEnabled)
                {
                    foreach (Link link in m_Links)
                    {
                        processedIndexRangesAndScalars.Add(link.CharacterIndices, link.Style.TextStyle.SizeScalar);
                    }
                    foreach (KeyValuePair<IndexRange, float> rangeScalar in processedIndexRangesAndScalars)
                    {
                        if (rangeScalar.Value <= 0f || rangeScalar.Value == 1f)
                        {
                            continue;
                        }
                        s_Segment = m_OutputText.Substring(rangeScalar.Key.StartIndex, rangeScalar.Key.Count);
                        int oldLength = s_Segment.Length;
                        if (s_PostProcessedSizeAttributeRegex.IsMatch(s_Segment))
                        {
                            s_ProcessedTextBuilder = new StringBuilder();
                            s_ProcessedTextBuilder.Append(m_OutputText.Substring(0, rangeScalar.Key.StartIndex));
                            s_Segment = s_PostProcessedSizeAttributeRegex.Replace(
                                s_Segment,
                                match => string.Format(
                                    "{0}{1}{2}",
                                    match.Groups[OpenTagCaptureGroup].Value,
                                    (int)(
                                        rangeScalar.Value * float.Parse(match.Groups[AttributeValueCaptureGroup].Value)
                                    ),
                                    match.Groups[CloseTagCaptureGroup].Value
                                )
                            );
                            s_ProcessedTextBuilder.Append(s_Segment);
                            s_ProcessedTextBuilder.Append(m_OutputText.Substring(rangeScalar.Key.EndIndex + 1));
                            m_OutputText = s_ProcessedTextBuilder.ToString();
                            int delta = s_Segment.Length - oldLength;
                            if (delta != 0)
                            {
                                s_IndexRangeOffsets.Clear();
                                s_IndexRangeOffsets.Add(rangeScalar.Key, delta);
                                foreach (IndexRange range in processedIndexRangesAndScalars.Keys)
                                {
                                    if (range == rangeScalar.Key)
                                    {
                                        continue;
                                    }
                                    range.Offset(s_IndexRangeOffsets);
                                }
                                rangeScalar.Key.EndIndex += delta;
                            }
                        }
                    }
                }
                // pull out quad data
                string quadName;
                foreach (Match match in s_QuadTagRegex.Matches(m_OutputText))
                {
                    // add new quad data to list if its class is known
                    quadName = match.Groups[ClassNameCaptureGroup].Value;
                    int templateIndex = s_CascadedQuadStyles.FindIndex(quad => quad.ClassName == quadName);
                    int quadGeomIndex = match.Index;
#if !UNITY_5_3_OR_NEWER
                    quadGeomIndex += match.Length - 1;
#endif
                    if (templateIndex >= 0)
                    {
                        m_Quads.Add(
                            new Quad(new IndexRange(quadGeomIndex, quadGeomIndex), s_CascadedQuadStyles[templateIndex])
                        );
                    }
                }
            }
			m_CustomTags.Sort((x, y) => x.CharacterIndices.StartIndex.CompareTo(y.CharacterIndices.StartIndex));
			m_Links.Sort((x, y) => x.CharacterIndices.StartIndex.CompareTo(y.CharacterIndices.StartIndex));
			m_Quads.Sort((x, y) => x.CharacterIndices.StartIndex.CompareTo(y.CharacterIndices.StartIndex));
			m_IsDirty = false;
		}
		
		/// <summary>
		/// Sets this instance dirty in order to force a became dirty callback.
		/// </summary>
		private void SetDirty()
		{
			m_IsDirty = true;
			m_OnBecameDirty.Invoke();
		}

		/// <summary>
		/// Sets the keyword collection backing field.
		/// </summary>
		/// <param name="backingField">Backing field.</param>
		/// <param name="value">Value.</param>
		private void SetKeywordCollectionBackingField(
			List<KeywordCollectionClass> backingField, IEnumerable<KeywordCollectionClass> value
		)
		{
			for (int i = 0; i < backingField.Count; ++i)
			{
				if (backingField[i].Collection != null)
				{
					backingField[i].Collection.OnRebuildKeywords.RemoveListener(SetDirty);
				}
			}
			backingField.Clear();
			backingField.AddRange(value);
			for (int i = 0; i < backingField.Count; ++i)
			{
				if (backingField[i].Collection != null)
				{
					backingField[i].Collection.OnRebuildKeywords.AddListener(SetDirty);
				}
			}
			SetDirty();
		}

		/// <summary>
		/// Sets the link keyword collections.
		/// </summary>
		/// <remarks>Included for inspector.</remarks>
		/// <param name="value">Value.</param>
		private void SetLinkKeywordCollections(KeywordCollectionClass[] value)
		{
			SetLinkKeywordCollections(value as IEnumerable<KeywordCollectionClass>);
		}

		/// <summary>
		/// Sets the link keyword collections.
		/// </summary>
		/// <param name="value">Value.</param>
		public void SetLinkKeywordCollections(IEnumerable<KeywordCollectionClass> value)
		{
			SetKeywordCollectionBackingField(m_LinkKeywordCollections, value);
		}

		/// <summary>
		/// Sets the quad keyword collections.
		/// </summary>
		/// <remarks>Included for inspector.</remarks>
		/// <param name="value">Value.</param>
		private void SetQuadKeywordCollections(KeywordCollectionClass[] value)
		{
			SetQuadKeywordCollections(value as IEnumerable<KeywordCollectionClass>);
		}

		/// <summary>
		/// Sets the quad keyword collections.
		/// </summary>
		/// <param name="value">Value.</param>
		public void SetQuadKeywordCollections(IEnumerable<KeywordCollectionClass> value)
		{
			SetKeywordCollectionBackingField(m_QuadKeywordCollections, value);
		}

		/// <summary>
		/// Sets the tag keyword collections.
		/// </summary>
		/// <remarks>Included for inspector.</remarks>
		/// <param name="value">Value.</param>
		private void SetTagKeywordCollections(KeywordCollectionClass[] value)
		{
			SetTagKeywordCollections(value as IEnumerable<KeywordCollectionClass>);
		}

		/// <summary>
		/// Sets the tag keyword collections.
		/// </summary>
		/// <param name="value">Value.</param>
		public void SetTagKeywordCollections(IEnumerable<KeywordCollectionClass> value)
		{
			SetKeywordCollectionBackingField(m_TagKeywordCollections, value);
		}

		/// <summary>
		/// Substitutes the tags in for detected keywords.
		/// </summary>
		/// <returns>Input text with tags patched in around keywords as needed.</returns>
		/// <param name="input">Input.</param>
		private string SubstituteTagsInForKeywords(string input)
		{
			// gather up all keyword collections and their corresponding substitution methods
			Dictionary<KeywordCollection, System.Func<string, string, string>> keywordCollections =
				new Dictionary<KeywordCollection, System.Func<string, string, string>>();
			List<KeywordCollectionClass> allCollectionClasses =
				new List<KeywordCollectionClass>(m_LinkKeywordCollections);
			List<System.Func<string, string, string, CaseMatchMode, string>> collectionSubstitutionMethods =
				new List<System.Func<string, string, string, CaseMatchMode, string>>();
			for (int index = 0; index < m_LinkKeywordCollections.Count; ++index)
			{
				collectionSubstitutionMethods.Add(InsertKeywordLinksIntoSegment);
			}
			if (this.IsRichTextEnabled)
			{
				allCollectionClasses.AddRange(m_QuadKeywordCollections);
				for (int index = 0; index < m_QuadKeywordCollections.Count; ++index)
				{
					collectionSubstitutionMethods.Add(InsertQuadTagIntoSegment);
				}
				allCollectionClasses.AddRange(m_TagKeywordCollections);
				for (int index = 0; index < m_TagKeywordCollections.Count; ++index)
				{
					collectionSubstitutionMethods.Add(InsertCustomTagsIntoSegment);
				}
			}
			for (int index = 0; index < allCollectionClasses.Count; ++index)
			{
				if (allCollectionClasses[index].Collection == null)
				{
					continue;
				}
				if (keywordCollections.ContainsKey(allCollectionClasses[index].Collection))
				{
#if UNITY_EDITOR
					if (Application.isPlaying)
#endif
					{
						Debug.LogError(
							string.Format(
								"Keyword collection {0} used for multiple different styles.",
								allCollectionClasses[index].Collection.name
							)
						);
					}
				}
				else
				{
					System.Func<string, string, string, CaseMatchMode, string> substitutionMethod =
						collectionSubstitutionMethods[index];
					string identifierName = allCollectionClasses[index].ClassName;
					CaseMatchMode caseMatchMode = allCollectionClasses[index].Collection.CaseMatchMode;
					keywordCollections.Add(
						allCollectionClasses[index].Collection,
						(segment, keyword) => substitutionMethod(segment, keyword, identifierName, caseMatchMode)
					);
				}
			}
			// get a regular expression to match tags
			string customTagsMatchPattern = "|".Join(
				from style in m_TagKeywordCollections
				select string.IsNullOrEmpty(style.ClassName) ?
					"" : string.Format("</?{0}\b.*?>", Regex.Escape(style.ClassName))
			);
			Regex tagMatcher = this.IsRichTextEnabled ?
				new Regex(
					string.Format(
						"{0}{1}",
						s_PreprocessedAnyTagMatchPattern,
						string.IsNullOrEmpty(customTagsMatchPattern) ?
							"" : string.Format("|{0}", customTagsMatchPattern)
					),
					RegexOptions.Singleline | RegexOptions.IgnoreCase
				) : s_PreprocessedLinkTagRegex;
			// sub in tags for each keyword
			s_ProcessedKeywords.Clear();
			foreach (KeyValuePair<KeywordCollection, System.Func<string, string, string>> kv in keywordCollections)
			{
				foreach (string keyword in kv.Key.Keywords)
				{
					if (s_ProcessedKeywords.Contains(keyword) || string.IsNullOrEmpty(keyword))
					{
						continue;
					}
					s_ProcessedKeywords.Add(keyword);
					int start;
					MatchCollection tagMatches = tagMatcher.Matches(input);
					// preserve all text inside of tags
					if (tagMatches.Count > 0)
					{
						s_ProcessedTextBuilder = new StringBuilder();
						for (int matchIndex = 0; matchIndex < tagMatches.Count; ++matchIndex)
						{
							start = matchIndex == 0 ?
								0 : tagMatches[matchIndex - 1].Index + tagMatches[matchIndex - 1].Length;
							// append text preceding tag
							s_Segment = input.Substring(start, tagMatches[matchIndex].Index - start);
							// append patched text
							s_ProcessedTextBuilder.Append(kv.Value(s_Segment, keyword));
							// append tag
							s_ProcessedTextBuilder.Append(tagMatches[matchIndex].Value);
							// append segment following final tag
							if (matchIndex == tagMatches.Count - 1)
							{
								s_Segment =
									input.Substring(tagMatches[matchIndex].Index + tagMatches[matchIndex].Length);
								s_ProcessedTextBuilder.Append(kv.Value(s_Segment, keyword));
							}
						}
						input = s_ProcessedTextBuilder.ToString();
					}
					// perform simple substitution if there are no tags present
					else
					{
						input = kv.Value(input, keyword);
					}
				}
				// reset string builder
				s_ProcessedTextBuilder = new StringBuilder();
			}
			return input;
		}
	}
}
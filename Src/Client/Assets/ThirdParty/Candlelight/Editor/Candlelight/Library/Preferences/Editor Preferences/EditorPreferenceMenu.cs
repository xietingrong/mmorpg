// 
// EditorPreferenceMenu.cs
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
// This file contains a class for the Candlelight editor preferences menu.

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Candlelight
{
	/// <summary>
	/// Editor preference menu.
	/// </summary>
	public class EditorPreferenceMenu : Singleton<EditorPreferenceMenu>
	{
		/// <summary>
		/// The asset store urls for each feature.
		/// </summary>
		private static Dictionary<string, string> s_AssetStoreUrls = new Dictionary<string, string>();
#pragma warning disable 414
		/// <summary>
		/// The asset store window.
		/// </summary>
		private static EditorWindow s_AssetStoreWindow = null;
		/// <summary>
		/// The type of the asset store window.
		/// </summary>
		private static readonly System.Type s_AssetStoreWindowType =
			typeof(EditorWindow).Assembly.GetTypes().FirstOrDefault(t => t.Name == "AssetStoreWindow");
#pragma warning restore 414
		/// <summary>
		/// The bug report email address.
		/// </summary>
		private static readonly string s_BugReportEmailAddress = "bugs@candlelightinteractive.com";
		/// <summary>
		/// The labels for the feature groups.
		/// </summary>
		private static GUIContent[] s_FeatureGroupLabels = new GUIContent[0];
		/// <summary>
		/// The menu items for each feature.
		/// </summary>
		private static Dictionary<string, List<MethodInfo>> s_MenuItems = new Dictionary<string, List<MethodInfo>>();
		/// <summary>
		/// The publisher page url in the asset store.
		/// </summary>
		private static readonly string s_PublisherPage = "com.unity3d.kharma:publisher/8";
		/// <summary>
		/// The support forum base URL.
		/// </summary>
		private static readonly string s_SupportForumBaseUrl =
			"http://groups.google.com/a/candlelightinteractive.com/forum/#!categories/developer-support";
		/// <summary>
		/// The support forum urls for each feature.
		/// </summary>
		private static Dictionary<string, string> s_SupportForumUrls = new Dictionary<string, string>();
		/// <summary>
		/// The tab pages.
		/// </summary>
		[System.NonSerialized]
		private static readonly Dictionary<int, System.Action> s_TabPages = new Dictionary<int, System.Action>();

		#region Backing Fields
		private static GUIStyle m_TabAreaStyle = null;
		#endregion

		/// <summary>
		/// Gets the tab area style.
		/// </summary>
		/// <value>The tab area style.</value>
		private static GUIStyle TabAreaStyle
		{
			get
			{
				if (m_TabAreaStyle == null)
				{
					m_TabAreaStyle = new GUIStyle();
					m_TabAreaStyle.padding = new RectOffset(3, 3, 0, 0); // otherwise tabs spill over edges of box
				}
				return m_TabAreaStyle;
			}
		}

		/// <summary>
		/// The current tab.
		/// </summary>
		[SerializeField]
		private int m_CurrentTab;
		/// <summary>
		/// The scroll position.
		/// </summary>
		[SerializeField]
		private Vector2 m_ScrollPosition;

		/// <summary>
		/// Adds the asset store URL for the product with the specified ID to the specified feature group.
		/// </summary>
		/// <param name="featureGroup">Feature group.</param>
		/// <param name="productId">Product identifier.</param>
		public static void AddAssetStoreUrl(string featureGroup, int productId)
		{
			s_AssetStoreUrls.Add(featureGroup, string.Format("com.unity3d.kharma:content/{0}", productId));
		}

		/// <summary>
		/// Adds the preference menu item.
		/// </summary>
		/// <param name="preferenceMenu">Preference menu.</param>
		/// <param name="method">Method.</param>
		public static void AddPreferenceMenuItem(string featureGroup, System.Action method)
		{
			AddPreferenceMenuItem(featureGroup, method.Method);
		}

		/// <summary>
		/// Adds the preference menu item.
		/// </summary>
		/// <param name="featureGroup">Feature group.</param>
		/// <param name="method">Method.</param>
		public static void AddPreferenceMenuItem(string featureGroup, MethodInfo method)
		{
			if (!s_MenuItems.ContainsKey(featureGroup))
			{
				s_MenuItems.Add(featureGroup, new List<MethodInfo>());
			}
			s_MenuItems[featureGroup].Add(method);
			s_MenuItems[featureGroup].Sort(
				(a, b) => string.Format("{0}.{1}", a.ReflectedType, a.Name).CompareTo(
					string.Format("{0}.{1}", b.ReflectedType, b.Name)
				)
			);
			List<GUIContent> labels = new List<GUIContent>();
			labels.AddRange(from tabName in s_MenuItems.Keys select new GUIContent(tabName));
			labels.Sort((x, y) => x.text.CompareTo(y.text));
			s_FeatureGroupLabels = labels.ToArray();
			s_TabPages.Clear();
			for (int i = 0; i < s_FeatureGroupLabels.Length; ++i)
			{
				s_TabPages.Add(i, () => Instance.DisplayPreferences(s_FeatureGroupLabels[Instance.m_CurrentTab]));
			}
		}

		/// <summary>
		/// Adds the support forum URL for the specified feature group.
		/// </summary>
		/// <param name="featureGroup">Feature group.</param>
		/// <param name="url">URL.</param>
		public static void AddSupportForumUrl(string featureGroup, string subForum)
		{
			s_SupportForumUrls.Add(featureGroup, string.Format("{0}/{1}", s_SupportForumBaseUrl, subForum));
		}

		/// <summary>
		/// Displays the preference GUI.
		/// </summary>
		[PreferenceItem("Candlelight")]
		public static void DisplayPreferenceGUI()
		{
			GUILayout.BeginArea(new Rect(134f, 39f, 352f, 352f)); // the rect in the preference window is bizarre...
			{
#if IS_CANDLELIGHT_SCENE_GUI_AVAILABLE
				EditorGUIX.DisplaySceneGUIToggle();
#endif
				EditorGUILayout.BeginVertical(TabAreaStyle, GUILayout.ExpandWidth(false));
				{
					Instance.m_CurrentTab = EditorGUIX.DisplayTabGroup(
						Instance.m_CurrentTab,
						s_FeatureGroupLabels,
						s_TabPages,
						4
					);
				}
				EditorGUILayout.EndVertical();
			}
			GUILayout.EndArea();
		}
		
		/// <summary>
		/// Displays the bug report button.
		/// </summary>
		/// <param name="featureLabel">Feature label.</param>
		private static void DisplayBugReportButton(GUIContent featureLabel)
		{
			if (EditorGUIX.DisplayButton(string.Format("Report a Problem with {0}", featureLabel.text)))
			{
				OpenUrl(
					string.Format(
						"mailto:{0}?subject={1} Bug Report&body=1) What happened?\n\n2) How often does it happen?\n\n" +
						"3) How can I reproduce it using the example you attached?",
						s_BugReportEmailAddress, featureLabel.text
					),
					"Error Creating Bug Report",
					"Please ensure an application is associated with email links."
				);
			}
		}
		
		/// <summary>
		/// Displays the preferences for a feature.
		/// </summary>
		/// <param name="featureLabel">Feature label.</param>
		private void DisplayPreferences(GUIContent featureLabel)
		{
			m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition);
			{
				foreach (MethodInfo method in s_MenuItems[featureLabel.text])
				{
					EditorGUILayout.LabelField(
						method.ReflectedType.IsGenericType ?
							string.Format(
								"{0} ({1})",
								method.ReflectedType.Name.ToWords().Range(0, -2),
								", ".Join(from t in method.ReflectedType.GetGenericArguments() select t.Name.ToWords())
							) : method.ReflectedType.Name.ToWords(),
						EditorStyles.boldLabel
					);
					EditorGUIX.DisplayHorizontalLine();
					EditorGUI.indentLevel += 1;
					method.Invoke(null, null);
					EditorGUI.indentLevel -= 1;
				}
			}
			EditorGUILayout.EndScrollView();
			// bug report button
			DisplayBugReportButton(featureLabel);
			// forum link button
			if (
				s_SupportForumUrls.ContainsKey(featureLabel.text) &&
				!string.IsNullOrEmpty(s_SupportForumUrls[featureLabel.text]) &&
				EditorGUIX.DisplayButton(string.Format("Get Help with {0}", featureLabel.text))
			)
			{
				OpenUrl(s_SupportForumUrls[featureLabel.text]);
			}
			// asset store page
			if (
				s_AssetStoreUrls.ContainsKey(featureLabel.text) &&
				!string.IsNullOrEmpty(s_AssetStoreUrls[featureLabel.text]) &&
				EditorGUIX.DisplayButton(string.Format("Review {0} on the Unity Asset Store", featureLabel.text))
			)
			{
				OpenUrl(s_AssetStoreUrls[featureLabel.text]);
			}
			// products page
			if (EditorGUIX.DisplayButton("More Products by Candlelight Interactive"))
			{
				OpenUrl(s_PublisherPage);
			}
		}

		/// <summary>
		/// Opens the URL.
		/// </summary>
		/// <param name="url">URL.</param>
		/// <param name="errorDialogTitle">Error dialog title.</param>
		/// <param name="errorDialogMessage">Error dialog message.</param>
		private static void OpenUrl(
			string url,
			string errorDialogTitle = "Error Opening URL",
			string errorDialogMessage = "Please ensure an application is associated with web links."
		)
		{
			try
			{
				if (url.StartsWith("mailto:"))
				{
					System.Diagnostics.Process.Start(url);
				}
				else
				{
					Application.OpenURL(url);
				}
			}
			catch
			{
				EditorUtility.DisplayDialog(errorDialogTitle, errorDialogMessage, "OK");
			}
		}
	}
}
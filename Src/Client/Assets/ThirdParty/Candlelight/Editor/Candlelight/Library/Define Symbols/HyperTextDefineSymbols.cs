// 
// HyperTextDefineSymbols.cs
// 
// Copyright (c) 2014, Candlelight Interactive, LLC
// All rights reserved.
// 
// This file is licensed according to the terms of the Unity Asset Store EULA:
// http://download.unity3d.com/assetstore/customer-eula.pdf
// 
// This file contains a class to register define symbols for the HyperText
// component. If you are distributing a tool that depends on this package,
// simply use the IS_CANDLELIGHT_HYPERTEXT_AVAILABLE symbol to conditionally
// compile dependent code.
// 
// Put this script in an Editor folder and it should automatically work when
// loaded.

using UnityEditor;

namespace Candlelight
{
	/// <summary>
	/// Hyper text define symbols.
	/// </summary>
	[InitializeOnLoad]
	public sealed class HyperTextDefineSymbols
	{
		/// <summary>
		/// The preference menu feature group.
		/// </summary>
		public static readonly string preferenceMenuFeatureGroup = "HyperText";

		/// <summary>
		/// Initializes the <see cref="HyperTextDefineSymbols"/> class.
		/// </summary>
		static HyperTextDefineSymbols()
		{
#if UNITY_5_3_OR_NEWER
            EditorApplication.playModeStateChanged += EditorApplication_playModeStateChanged;
#endif
        }

        private static void EditorApplication_playModeStateChanged(PlayModeStateChange obj)
        {
            if(obj == PlayModeStateChange.ExitingPlayMode)
                UnityFeatureDefineSymbols.AddSymbolForAllBuildTargets("IS_CANDLELIGHT_HYPERTEXT_AVAILABLE");
        }
    }
}
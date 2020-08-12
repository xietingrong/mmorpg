// 
// UnityFeatureDefineSymbols.cs
// 
// Copyright (c) 2013-2015, Candlelight Interactive, LLC
// All rights reserved.
// 
// This file is licensed according to the terms of the Unity Asset Store EULA:
// http://download.unity3d.com/assetstore/customer-eula.pdf
// 
// This file contains a class to register define symbols for Unity features.
// 
// Put this script in an Editor folder and it should automatically work when
// loaded.

using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace Candlelight
{
    /// <summary>
    /// Unity feature define symbols.
    /// </summary>
    [InitializeOnLoad]
    public sealed class UnityFeatureDefineSymbols
    {
        /// <summary>
        /// Initializes the <see cref="UnityFeatureDefineSymbols"/> class.
        /// </summary>
        static UnityFeatureDefineSymbols()
        {
#if UNITY_5_3_OR_NEWER
            EditorApplication.projectChanged += () =>
            {
                AddSymbolForAllBuildTargets("IS_UNITYEDITOR_ANIMATIONS_AVAILABLE", t => true);
            };
#endif
        }

        /// <summary>
        /// Adds the symbol for all build targets.
        /// </summary>
        /// <param name="symbol">Symbol.</param>
        /// <param name="condition">
        /// The condition under which the symbol should be added. If <see langword="null"/>, then the symbol will be added for all
        /// build targets. Otherwise, if the condition evaluates to <see langword="true"/> for the particular target, it will be
        /// added; if the condition evaluates to<see langword="false"/> for the particular target, it will be removed.
        /// </param>
        public static void AddSymbolForAllBuildTargets(
            string symbol, System.Predicate<BuildTargetGroup> condition = null
        )
        {
            foreach (BuildTargetGroup target in System.Enum.GetValues(typeof(BuildTargetGroup)))
            {
                // prevent editor spam in Unity 5.x
                if (target == BuildTargetGroup.Unknown)
                {
                    continue;
                }
                object[] objAttrs = target.GetType().GetField(target.ToString()).GetCustomAttributes(typeof(System.ObsoleteAttribute), true);
                if (objAttrs != null &&
                    objAttrs.Length > 0)
                {
                    continue;
                }
                HashSet<string> symbols =
                new HashSet<string>(PlayerSettings.GetScriptingDefineSymbolsForGroup(target).Split(';'));
                if (condition == null || condition(target))
                {
                    symbols.Add(symbol);
                }
                else if (symbols.Contains(symbol))
                {
                    symbols.Remove(symbol);
                }
                PlayerSettings.SetScriptingDefineSymbolsForGroup(target, string.Join(";", symbols.ToArray()));
            }
        }
    }
}
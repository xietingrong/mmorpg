// 
// SceneGUI.cs
// 
// Copyright (c) 2011-2015, Candlelight Interactive, LLC
// All rights reserved.
// 
// This file is licensed according to the terms of the Unity Asset Store EULA:
// http://download.unity3d.com/assetstore/customer-eula.pdf
// 
// This file contains a static class with methods for working with scene GUI.

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Candlelight
{
    /// <summary>
    /// Scene GUI.
    /// </summary>
    [InitializeOnLoad]
    public static class SceneGUI
    {
        /// <summary>
        /// A basic class for storing information about a registered context for drawing scene GUI overlay.
        /// </summary>
        internal class RegisteredContext
        {
            public System.Action Callback { get; set; }
            public Editor Context { get; set; }
            public Object Target { get; set; }
        }

        /// <summary>
        /// Handle context exception.
        /// </summary>
        public class HandleContextException : System.Exception
        {
            public HandleContextException() : base(
                "You must close the current handle context before beginning a new one."
            )
            { }
        }

        /// <summary>
        /// The wrap values for the joint frame calculation notice.
        /// </summary>
        private static readonly float[] s_JointCalculationMessageWrapValues =
            new float[] { 1482f, 830f, 618f, 507f, 443f };
        /// <summary>
        /// The target object triggering display of scene GUI overlay.
        /// </summary>
        private static Object s_DisplayInvoker = null;
        /// <summary>
        /// The state of the mouse before entering a handle context.
        /// </summary>
        private static MouseState s_EnterContextMouseState = MouseState.None;
        /// <summary>
        /// The color of the handle.
        /// </summary>
        private static Color s_HandleColor;
        /// <summary>
        /// Flag indicating whether a handle context is currently open.
        /// </summary>
        private static bool s_IsInHandleContext = false;
        /// <summary>
        /// The most recent target objects in the handle context.
        /// </summary>
        private static Object[] s_MostRecentTargetObjects = new Object[0];
        /// <summary>
        /// The most recent undo message.
        /// </summary>
        private static string s_MostRecentUndoMessage;
        /// <summary>
        /// A list of all registered contexts.
        /// </summary>
        private static List<RegisteredContext> s_RegisteredContexts = new List<RegisteredContext>();
        /// <summary>
        /// The scroll position of the scene GUI area.
        /// </summary>
        private static Vector2 s_ScrollPosition = Vector2.zero;
        /// <summary>
        /// The toggle preference.
        /// </summary>
        private static readonly EditorPreference<bool, EditorApplication> s_TogglePreference =
            EditorPreference<bool, EditorApplication>.ForToggle("sceneGUI", true);
        /// <summary>
        /// The viewport padding.
        /// </summary>
        private static readonly float s_ViewportPadding = 5f;

        #region Backing Fields
        private static GUISkin s_Skin = null;
        #endregion

        /// <summary>
        /// Gets the current alpha scalar.
        /// </summary>
        /// <value>The current alpha scalar.</value>
        public static Color CurrentAlphaScalar { get { return new Color(1f, 1f, 1f, Handles.color.a); } }
        /// <summary>
        /// Gets the size of the dot handle.
        /// </summary>
        /// <value>The size of the dot handle.</value>
        public static float DotHandleSize { get { return 0.03f; } }
        /// <summary>
        /// Gets the fill alpha scalar.
        /// </summary>
        /// <value>The fill alpha scalar.</value>
        public static float FillAlphaScalar { get { return 0.1f; } }
        /// <summary>
        /// Gets or sets a value indicating whether the scene GUI is enabled.
        /// </summary>
        /// <value><see langword="true"/> if is enabled; otherwise, <see langword="false"/>.</value>
        public static bool IsEnabled
        {
            get { return s_TogglePreference.CurrentValue; }
            set
            {
                if (value != s_TogglePreference.CurrentValue)
                {
                    s_TogglePreference.CurrentValue = value;
                    SceneView.RepaintAll();
                }
            }
        }
        /// <summary>
        /// Gets the line alpha scalar.
        /// </summary>
        /// <value>The line alpha scalar.</value>
        public static float LineAlphaScalar { get { return 0.5f; } }
        /// <summary>
        /// Gets the skin.
        /// </summary>
        /// <value>The skin.</value>
        private static GUISkin Skin
        {
            get
            {
                if (s_Skin == null)
                {
                    s_Skin = ScriptableObject.CreateInstance<GUISkin>();
                    EditorUtility.CopySerialized(EditorStylesX.CurrentBuiltinSkin, s_Skin);
                    // for non-pro skin, use color from IN Label focus background texture
                    Color focusColor =
                        EditorGUIUtility.isProSkin ? new Color32(255, 255, 255, 255) : new Color32(62, 95, 150, 255);
                    // without a focus background, focus text color won't be applied...
                    Texture2D focusBackground = new Texture2D(1, 1);
                    focusBackground.SetPixels32(new Color32[] { new Color32(255, 255, 255, 0) });
                    focusBackground.Apply();
                    focusBackground.hideFlags = HideFlags.HideAndDontSave;
                    //					s_Skin.box = null;
                    s_Skin.button = EditorStylesX.MiniButton;
                    s_Skin.font = EditorStylesX.MiniLabel.font;
                    //					s_Skin.horizontalScrollbar = null;
                    //					s_Skin.horizontalScrollbarLeftButton = null;
                    //					s_Skin.horizontalScrollbarRightButton = null;
                    //					s_Skin.horizontalScrollbarThumb = null;
                    //					s_Skin.horizontalSlider = null;
                    //					s_Skin.horizontalSliderThumb = null;
                    s_Skin.label = new GUIStyle(EditorStylesX.MiniLabel);
                    s_Skin.label.focused.textColor = focusColor;
                    s_Skin.label.focused.background = focusBackground;
                    //					s_Skin.scrollView = null;
                    //					s_Skin.textArea = null;
                    //					s_Skin.textField = null;
                    //					s_Skin.toggle = null;
                    //					s_Skin.verticalScrollbar = null;
                    //					s_Skin.verticalScrollbarDownButton = null;
                    //					s_Skin.verticalScrollbarThumb = null;
                    //					s_Skin.verticalScrollbarUpButton = null;
                    //					s_Skin.verticalSlider = null;
                    //					s_Skin.verticalSliderThumb = null;
                    //					s_Skin.window = null;
                    s_Skin.name = typeof(SceneGUI).FullName;
                    s_Skin.hideFlags = HideFlags.HideAndDontSave;
                }
                return s_Skin;
            }
        }

        /// <summary>
        /// Initializes the <see cref="SceneGUI"/> class.
        /// </summary>
        static SceneGUI()
        {

        }

        /// <summary>
        /// Begins a context around custom handle calls to store an undo snapshot.
        /// </summary>
        /// <remarks>
        /// Wraps BeginHandlesChangeCheck().
        /// </remarks>
        /// <returns><see langword="true"/> if the Scene GUI is enabled; otherwise, <see langword="false"/>.</returns>
        /// <param name="target">Target.</param>
        /// <param name="undoMessage">Undo message.</param>
        public static bool BeginHandles(Object target, string undoMessage)
        {
            return BeginHandles(new Object[] { target }, undoMessage);
        }

        /// <summary>
        /// Begins a context around custom handle calls to store an undo snapshot.
        /// </summary>
        /// <remarks>
        /// Wraps EndHandlesChangeCheck().
        /// </remarks>
        /// <returns><see langword="true"/> if the Scene GUI is enabled; otherwise, <see langword="false"/>.</returns>
        /// <param name="targets">Targets.</param>
        /// <param name="undoMessage">Undo message.</param>
        public static bool BeginHandles(Object[] targets, string undoMessage)
        {
            // store targets
            s_MostRecentTargetObjects = targets;
            // store undo message
            s_MostRecentUndoMessage = undoMessage;
            return BeginHandlesChangeCheck();
        }

        /// <summary>
        /// Begins the handles change check context.
        /// </summary>
        /// <returns><see langword="true"/> if the Scene GUI is enabled; otherwise, <see langword="false"/>.</returns>
        private static bool BeginHandlesChangeCheck()
        {
            // throw an exception if a previous context is still open
            if (s_IsInHandleContext)
            {
                Debug.LogException(new HandleContextException());
            }
            // set flag to indicate handle context has begun
            s_IsInHandleContext = true;
            // capture current event before it is used by a handle
            s_EnterContextMouseState = GUIHelpers.GetCurrentMouseState();
            return IsEnabled;
        }

        /// <summary>
        /// Deregisters the scene GUI callback for the supplied object.
        /// </summary>
        /// <param name="ctx">Editor for the object whose scene GUI should no longer be drawn.</param>
        public static void DeregisterObjectGUICallback(ISceneGUIContext ctx)
        {
            s_RegisteredContexts.RemoveAll(k => ctx.SceneGUIContext.targets.Contains(k.Target));
        }

        /// <summary>
        /// Executes any registered scene GUI callbacks.
        /// </summary>
        /// <param name="ctx">Object from whose context this method is called.</param>
        /// <param name="width">Desired width of scene GUI area. 281 is minimum width for sliders to appear.</param>
        /// <param name="anchor">Corner of the viewport to which the controls should be anchored.</param>
        public static void Display(ISceneGUIContext ctx, float width = 281f, GUIAnchor anchor = GUIAnchor.TopLeft)
        {
            // early out if scene gui is disabled or there are no valid registered contexts
            if (
                !IsEnabled ||
                s_RegisteredContexts.Count == 0 ||
                s_RegisteredContexts.Count(k => k.Target != null && k.Callback != null) == 0
            )
            {
                return;
            }
            // ensure only the first context invoking this method during a layout phase will display the GUI
            if (Event.current.type == EventType.Layout && s_DisplayInvoker == null)
            {
                s_DisplayInvoker = ctx.SceneGUIContext.target;
            }
            if (s_DisplayInvoker != ctx.SceneGUIContext.target)
            {
                return;
            }
            if (Event.current.type != EventType.Layout)
            {
                s_DisplayInvoker = null;
            }
            // begin gui matrix
            Handles.BeginGUI();
            // begin the area
            width = Mathf.Min(width, Screen.width - 2f * s_ViewportPadding);
            float height = Screen.height - 2f * s_ViewportPadding - 38f; // 38 is height of tab/top menu in scene
            GUISkin oldSkin = GUI.skin;
            GUI.skin = Skin;
            GUILayout.BeginArea(
                new Rect(
                    (anchor == GUIAnchor.LowerLeft || anchor == GUIAnchor.TopLeft) ?
                        s_ViewportPadding : Screen.width - s_ViewportPadding - width,
                    (anchor == GUIAnchor.TopLeft || anchor == GUIAnchor.TopRight) ?
                        s_ViewportPadding : Screen.height - s_ViewportPadding - height,
                    width,
                    height
                )
            );
            {
                Color oldColor = GUI.color;
                GUI.color = new Color(oldColor.r, oldColor.g, oldColor.b, oldColor.a * 0.65f);
                EditorGUILayout.BeginVertical(EditorStylesX.SceneBox);
                {
                    GUI.color = oldColor;
                    s_ScrollPosition = EditorGUILayout.BeginScrollView(s_ScrollPosition, GUILayout.ExpandHeight(false));
                    {
                        for (int i = 0; i < s_RegisteredContexts.Count; ++i)
                        {
                            if (s_RegisteredContexts[i].Target == null || s_RegisteredContexts[i].Callback == null)
                            {
                                continue;
                            }
                            FontStyle fontStyle = GUI.skin.label.fontStyle;
                            GUI.skin.label.fontStyle = FontStyle.Bold;
                            EditorGUILayout.LabelField(
                                s_RegisteredContexts[i].Target.GetType().Name.ToWords(), GUI.skin.label
                            );
                            GUI.skin.label.fontStyle = fontStyle;
                            EditorGUIX.DisplayHorizontalLine();
                            ++EditorGUI.indentLevel;
                            s_RegisteredContexts[i].Callback.Invoke();
                            --EditorGUI.indentLevel;
                        }
                    }
                    EditorGUILayout.EndScrollView();
                }
                EditorGUILayout.EndVertical();
            }
            GUILayout.EndArea();
            GUI.skin = oldSkin;
            Handles.EndGUI();
        }

        /// <summary>
        /// Displays a button with a cap function.
        /// </summary>
        /// <returns><see langword="true"/> if cap button was clicked; otherwise, <see langword="false"/>.</returns>
        /// <param name="id">Identifier for the control.</param>
        /// <param name="position">Position.</param>
        /// <param name="orientation">Orientation.</param>
        /// <param name="size">Size.</param>
        /// <param name="cap">Cap draw function.</param>
        public static bool DisplayCapButton(
            int id, Vector3 position, Quaternion orientation, float size, Handles.CapFunction cap
        )
        {
            return DisplayCapButton(id, position, orientation, size, cap, Handles.color);
        }

        /// <summary>
        /// Displays a button with a cap function.
        /// </summary>
        /// <returns><see langword="true"/> if cap button was clicked; otherwise, <see langword="false"/>.</returns>
        /// <param name="id">Identifier for the control.</param>
        /// <param name="position">Position.</param>
        /// <param name="orientation">Orientation.</param>
        /// <param name="size">Size.</param>
        /// <param name="cap">Cap draw function.</param>
        /// <param name="clickColor">Color to use when the cap is clicked.</param>
        private static bool DisplayCapButton(
            int id, Vector3 position, Quaternion orientation, float size, Handles.CapFunction cap, Color clickColor
        )
        {
            bool result = false;
            Event current = Event.current;
            switch (current.GetTypeForControl(id))
            {
                case EventType.MouseDown:
                    if (
                        (
                            (HandleUtility.nearestControl == id && current.button == 0) ||
                            (GUIUtility.keyboardControl == id && current.button == 2)
                        ) && GUIUtility.hotControl == 0
                    )
                    {
                        GUIUtility.keyboardControl = id;
                        GUIUtility.hotControl = id;
                        current.Use();
                        EditorGUIUtility.SetWantsMouseJumping(1);
                    }
                    break;
                case EventType.MouseUp:
                    if (GUIUtility.hotControl == id && (current.button == 0 || current.button == 2))
                    {
                        GUIUtility.hotControl = 0;
                        current.Use();
                        EditorGUIUtility.SetWantsMouseJumping(0);
                        result = true;
                    }
                    break;
                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == id)
                    {
                        GUI.changed = true;
                        current.Use();
                    }
                    break;
                case EventType.Repaint:
                    {
                        // TODO: implement click color
                        cap(id, position, orientation, size, EventType.Repaint);
                        break;
                    }
                case EventType.Layout:
                    HandleUtility.AddControl(id, HandleUtility.DistanceToCircle(position, size * 0.2f));
                    break;
            }
            return result;
        }

        /// <summary>
        /// Displays the joint frame calculation notification message.
        /// </summary>
        public static void DisplayJointFrameCalculationNotification()
        {
            DisplayNotification(
                "One or more selected joints did not exist before play mode was entered. Handle orientation cannot be determined.",
                s_JointCalculationMessageWrapValues
            );
        }

        /// <summary>
        /// Displays a notification message in the scene view.
        /// </summary>
        /// <remarks>
        /// Not necessary to wrap in BeginGUI() / EndGUI().
        /// </remarks>
        /// <param name="message">Message to display.</param>
        /// <param name="wrapValues">Scene view pixel width values at which the text line height increments.</param>
        /// <param name="defaultHeight">Starting height for notification rect. Defaults to two lines of text.</param>
        public static void DisplayNotification(string message, float[] wrapValues, float defaultHeight = 110f)
        {
            Handles.BeginGUI();
            {
                Rect notificationRect = new Rect(30f, 0f, Camera.current.pixelWidth - 60f, defaultHeight);
                foreach (float wrap in wrapValues)
                {
                    notificationRect.height += Camera.current.pixelWidth <= wrap ? 30f : 0f;
                }
                notificationRect.y = 0.7f * (Camera.current.pixelHeight - notificationRect.height);
                GUI.Label(notificationRect, message, EditorStylesX.SceneNotification);
            }
            Handles.EndGUI();
        }

        /// <summary>
        /// Ends a context around custom handle calls to register undo snapshot and
        /// set dirty as needed.
        /// </summary>
        /// <returns><see langword="true"/> if changes were detected; otherwise, <see langword="false"/>.</returns>
        public static bool EndHandles()
        {
            bool isChangeDetected = EndHandlesChangeCheck();
            // see if event was eaten in a hold
            if (isChangeDetected)
            {
                Undo.RecordObjects(s_MostRecentTargetObjects, s_MostRecentUndoMessage);
                EditorUtilityX.SetDirty(s_MostRecentTargetObjects);
            }
            // clean up context
            s_MostRecentTargetObjects = new Object[0];
            return isChangeDetected;
        }

        /// <summary>
        /// Ends the handles change check context.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if changes were detected; otherwise, <see langword="false"/>.
        /// </returns>
        private static bool EndHandlesChangeCheck()
        {
            bool isChangeDetected = false;
            // see if event was eaten in a hold
            if (s_EnterContextMouseState == MouseState.LeftButtonHeld && Event.current.type == EventType.Used)
            {
                isChangeDetected = true;
            }
            // clean up context
            s_IsInHandleContext = false;
            s_EnterContextMouseState = MouseState.None;
            return IsEnabled ? isChangeDetected : false;
        }

        /// <summary>
        /// Gets the size of the fixed handle, given its current handle-space position.
        /// </summary>
        /// <returns>The fixed handle size.</returns>
        /// <param name="handleSpacePosition">Handle space position.</param>
        /// <param name="desiredSize">Desired size.</param>
        public static float GetFixedHandleSize(Vector3 handleSpacePosition, float desiredSize)
        {
            return HandleUtility.GetHandleSize(handleSpacePosition) * desiredSize;
        }

        /// <summary>
        /// Registers a scene GUI callback for the target object of the supplied editor context.
        /// </summary>
        /// <param name="ctx">Editor for the object whose scene GUI should be drawn.</param>
        /// <param name="action">GUI calback for the object.</param>
        public static void RegisterObjectGUICallback(ISceneGUIContext ctx, System.Action action)
        {
            UnityFeatureDefineSymbols.AddSymbolForAllBuildTargets("IS_CANDLELIGHT_SCENE_GUI_AVAILABLE");

            Object target = ctx.SceneGUIContext.target;
            // don't register same target more than once
            if (s_RegisteredContexts.Count(r => r.Target == target) > 0)
            {
                return;
            }
            // don't register targets of the same type if they're on another object
            System.Type targetType = target.GetType();
            for (int i = 0; i < s_RegisteredContexts.Count; ++i)
            {
                if (targetType == s_RegisteredContexts[i].Target.GetType())
                {
                    if (target is ScriptableObject)
                    {
                        return;
                    }
                    else if (
                        (target as Component).gameObject != (s_RegisteredContexts[i].Target as Component).gameObject
                    )
                    {
                        return;
                    }
                }
            }
            s_RegisteredContexts.Add(
                new RegisteredContext
                {
                    Callback = action,
                    Context = ctx.SceneGUIContext,
                    Target = target
                }
            );
        }

        /// <summary>
        /// Sets the handle alpha.
        /// </summary>
        /// <param name="alpha">Alpha.</param>
        public static void SetHandleAlpha(float alpha)
        {
            s_HandleColor = Handles.color;
            Handles.color = new Color(s_HandleColor.r, s_HandleColor.g, s_HandleColor.b, alpha);
        }

        #region Obsolete
        [System.Obsolete]
        public const int nullControlId = -1;
        #endregion
    }
}
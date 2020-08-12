// 
// HyperTextQuad.cs
// 
// Copyright (c) 2015, Candlelight Interactive, LLC
// All rights reserved.
// 
// This file is licensed according to the terms of the Unity Asset Store EULA:
// http://download.unity3d.com/assetstore/customer-eula.pdf

#if !(UNITY_4_6 || UNITY_5_0 || UNITY_5_1)
#define IS_VBO_MESH
#endif

using UnityEngine;
using System.Collections.Generic;

namespace Candlelight.UI
{
	/// <summary>
	/// This component exists to make sure that <see cref="UnityEngine.CanvasRenderer"/>s created for quads on a
	/// <see cref="HyperText"/> component are compatible with <see cref="UnityEngine.UI.RectMask2D"/>.
	/// </summary>
	[RequireComponent(typeof(CanvasRenderer))]
	public class HyperTextQuad : UnityEngine.EventSystems.UIBehaviour
#if IS_VBO_MESH
		, UnityEngine.UI.IClippable
#endif
	{
#if IS_VBO_MESH
		#region Shared Allocations
		private static readonly List<Canvas> s_Canvases = new List<Canvas>(8);
		private static readonly Vector3[] s_Corners = new Vector3[4];
		#endregion

		/// <summary>
		/// The<see cref="UnityEngine.UI.RectMask2D"/> affecting this instance.
		/// </summary>
		private UnityEngine.UI.RectMask2D m_ParentMask = null;

		#region Backing Fields
		private Canvas m_Canvas = null;
		private CanvasRenderer m_CanvasRenderer = null;
		[SerializeField]
		private HyperText m_HyperText = null;
		#endregion

		/// <summary>
		/// The <see cref="UnityEngine.Canvas"/> on which this instance is drawn.
		/// </summary>
		/// <value>The <see cref="UnityEngine.Canvas"/> on which this instance is drawn.</value>
		public Canvas Canvas
		{
			get
			{
				if (m_Canvas == null)
				{
					// copied from Graphic.CacheCanvas()
					GetComponentsInParent(false, s_Canvases);
					if (s_Canvases.Count > 0)
					{
						m_Canvas = s_Canvases[0];
					}
					s_Canvases.Clear();
				}
				return m_Canvas;
			}
		}
		/// <summary>
		/// Gets the <see cref="UnityEngine.Rect"/> of the <see cref="UnityEngine.Canvas"/> on which this instance is
		/// drawn.
		/// </summary>
		/// <value>
		/// The <see cref="UnityEngine.Rect"/> of the <see cref="UnityEngine.Canvas"/> on which this instance is drawn.
		/// </value>
		private Rect CanvasRect
		{
			get
			{
				this.rectTransform.GetWorldCorners(s_Corners);
				if (this.Canvas != null)
				{
					for (int i = 0; i < 4; ++i)
					{
						s_Corners[i] = this.Canvas.transform.InverseTransformPoint(s_Corners[i]);
					}
				}
				return new Rect(
					s_Corners[0].x, s_Corners[0].y, s_Corners[2].x - s_Corners[0].x, s_Corners[2].y - s_Corners[0].y
				);
			}
		}
		/// <summary>
		/// Gets the <see cref="UnityEngine.CanvasRenderer"/> on this instance.
		/// </summary>
		/// <value>The <see cref="UnityEngine.CanvasRenderer"/> on this instance.</value>
		private CanvasRenderer CanvasRenderer
		{
			get
			{
				return m_CanvasRenderer = m_CanvasRenderer == null ? GetComponent<CanvasRenderer>() : m_CanvasRenderer;
			}
		}
		/// <summary>
		/// Gets or sets the <see cref="HyperText"/> to which this instance belongs.
		/// </summary>
		/// <value>The <see cref="HyperText"/> to which this instance belongs.</value>
		public HyperText HyperText
		{
			get { return m_HyperText; }
			set { m_HyperText = value; }
		}
		/// <summary>
		/// Gets the <see cref="UnityEngine.RectTransform"/> on this instance.
		/// </summary>
		/// <value>The <see cref="UnityEngine.RectTransform"/> on this instance.</value>
		public RectTransform rectTransform { get { return this.transform as RectTransform; } }

		/// <summary>
		/// Cull this object's <see cref="UnityEngine.CanvasRenderer"/> if it is outside the clipping rectangle
		/// </summary>
		/// <param name="clipRect">Clip rectangle.</param>
		/// <param name="validRect">
		/// If set to <see langword="true"/> then <paramref name="clipRect"/> is a valid rectangle.
		/// </param>
		public void Cull(Rect clipRect, bool validRect)
		{
			if (!this.CanvasRenderer.hasMoved)
			{
				return;
			}
			bool cull = !validRect || !clipRect.Overlaps(this.CanvasRect);
			bool cullingChanged = this.CanvasRenderer.cull != cull;
			this.CanvasRenderer.cull = cull;
			if (cullingChanged)
			{
				m_HyperText.SetVerticesDirty();
			}
		}

		/// <summary>
		/// Update the clip parent when the canvas hierarchy changes.
		/// </summary>
		protected override void OnCanvasHierarchyChanged()
		{
			base.OnCanvasHierarchyChanged();
			UpdateClipParent();
		}

		/// <summary>
		/// Update the clip parent when this instance is disabled.
		/// </summary>
		protected override void OnDisable()
		{
			base.OnDisable();
			UpdateClipParent();
		}

		/// <summary>
		/// Update the clip parent when this instance is enabled.
		/// </summary>
		protected override void OnEnable()
		{
			base.OnEnable();
			UpdateClipParent();
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
		/// Recalculates the clipping.
		/// </summary>
		public void RecalculateClipping()
		{
			UpdateClipParent();
		}

		/// <summary>
		/// Sets the clipping rectangle on this object's <see cref="UnityEngine.CanvasRenderer"/>.
		/// </summary>
		/// <param name="clipRect">Clipping rectangle.</param>
		/// <param name="validRect">
		/// If set to <see langword="true"/> then <paramref name="clipRect"/> is a valid rectangle.
		/// </param>
		public void SetClipRect(Rect clipRect, bool validRect)
		{
			if (validRect)
			{
				this.CanvasRenderer.EnableRectClipping(clipRect);
			}
			else
			{
				this.CanvasRenderer.DisableRectClipping();
			}
		}

		/// <summary>
		/// Updates the clip parent.
		/// </summary>
		public void UpdateClipParent()
		{
			if (m_HyperText == null)
			{
				return;
			}
			// copied from MaskableGraphic.UpdateClipParent()
			UnityEngine.UI.RectMask2D newParent = (m_HyperText.maskable && m_HyperText.IsActive()) ?
				UnityEngine.UI.MaskUtilities.GetRectMaskForClippable(this) : null;
			if (newParent != m_ParentMask && m_ParentMask != null)
			{
				m_ParentMask.RemoveClippable(this);
			}
			if (newParent != null)
			{
				newParent.AddClippable(this);
			}
			m_ParentMask = newParent;
		}
#endif
	}
}
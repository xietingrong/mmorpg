// 
// ColorTween.cs
// 
// Copyright (c) 2014-2015, Candlelight Interactive, LLC
// All rights reserved.
// 
// This file is licensed according to the terms of the Unity Asset Store EULA:
// http://download.unity3d.com/assetstore/customer-eula.pdf

using UnityEngine;
using UnityEngine.Events;
using System.Collections;

namespace Candlelight.ColorTween
{
	/// <summary>
	/// Possible color tween modes.
	/// </summary>
	/// <remarks>
	/// This enum mirrors <see cref="UnityEngine.UI.CoroutineTween.ColorTween.ColorTweenMode"/>.
	/// </remarks>
	public enum Mode
	{
		/// <summary>
		/// Interpolate all color channels.
		/// </summary>
		All,
		/// <summary>
		/// Interpolate only the red, blue, and green color channels.
		/// </summary>
		RGB,
		/// <summary>
		/// Interpolate only the alpha channel.
		/// </summary>
		Alpha
	}

	/// <summary>
	/// Info for a color tween.
	/// </summary>
	/// <remarks>
	/// This struct mirrors functionality found in <see cref="UnityEngine.UI.CoroutineTween.ColorTween"/>.
	/// </remarks>
	internal struct Info
	{
		/// <summary>
		/// A color change callback.
		/// </summary>
		internal class Callback : UnityEvent<Color> {}

		#region Backing Fields
		private Callback m_OnColorChange;
		#endregion
		/// <summary>
		/// Gets the duration of the tween.
		/// </summary>
		/// <value>The duration of the tween.</value>
		public float Duration { get; private set; }
		/// <summary>
		/// Gets a value indicating whether this <see cref="Info"/> should ignore time scale.
		/// </summary>
		/// <value><see langword="true"/> if time scale should be ignored; otherwise, <see langword="false"/>.</value>
		public bool IgnoreTimeScale { get; private set; }
		/// <summary>
		/// Gets the on color change callback.
		/// </summary>
		/// <value>The on color change callback.</value>
		private Callback OnColorChange { get { return m_OnColorChange = m_OnColorChange ?? new Callback(); } }
		/// <summary>
		/// Gets the start color.
		/// </summary>
		/// <value>The start color.</value>
		public Color StartColor { get; private set; }
		/// <summary>
		/// Gets the target color.
		/// </summary>
		/// <value>The target color.</value>
		public Color TargetColor { get; private set; }
		/// <summary>
		/// Gets the tween mode.
		/// </summary>
		/// <value>The tween mode.</value>
		public Mode TweenMode { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="Info"/> struct.
		/// </summary>
		/// <param name="duration">Duration of the tween.</param>
		/// <param name="ignoreTimeScale">If set to <see langword="true"/> ignore time scale.</param>
		/// <param name="startColor">Start color.</param>
		/// <param name="targetColor">Target color.</param>
		/// <param name="tweenMode">Tween mode.</param>
		public Info(float duration, bool ignoreTimeScale, Color startColor, Color targetColor, Mode tweenMode): this()
		{
			this.Duration = duration;
			this.IgnoreTimeScale = ignoreTimeScale;
			this.StartColor = startColor;
			this.TargetColor = targetColor;
			this.TweenMode = tweenMode;
			m_OnColorChange = new Callback();
		}

		/// <summary>
		/// Adds the on changed callback.
		/// </summary>
		/// <param name="callback">Callback.</param>
		public void AddOnChangedCallback(UnityAction<Color> callback)
		{
			this.OnColorChange.AddListener(callback);
		}

		/// <summary>
		/// Interpolates between <see cref="Info.StartColor"/> and <see cref="Info.TargetColor"/> with the specified
		/// percentage value and invokes the <see cref="Info.OnColorChange"/> callback.
		/// </summary>
		/// <param name="percentage">Percentage.</param>
		public void TweenValue(float percentage)
		{
			Color result = Color.Lerp(this.StartColor, this.TargetColor, percentage);
			switch (this.TweenMode)
			{
			case Mode.Alpha:
				result.r = this.StartColor.r;
				result.g = this.StartColor.g;
				result.b = this.StartColor.b;
				break;
			case Mode.RGB:
				result.a = this.StartColor.a;
				break;
			}
			m_OnColorChange.Invoke(result);
		}
	}

	/// <summary>
	/// An iterator to invoke as a coroutine.
	/// </summary>
	internal class Iterator : IEnumerator
	{
		/// <summary>
		/// The elapsed time.
		/// </summary>
		private float m_ElapsedTime = 0f;
		/// <summary>
		/// The percentage completion.
		/// </summary>
		private float m_Percentage = 0f;
		/// <summary>
		/// The tween info.
		/// </summary>
		private readonly Info m_TweenInfo;

		/// <summary>
		/// Initializes a new instance of the <see cref="Iterator"/> class.
		/// </summary>
		/// <param name="tweenInfo">Tween info.</param>
		public Iterator(Info tweenInfo)
		{
			m_TweenInfo = tweenInfo;
		}

		/// <summary>
		/// Gets the current value.
		/// </summary>
		/// <value>The current value.</value>
		public object Current { get { return null; } }

		/// <summary>
		/// Moves to the next item in the iterator.
		/// </summary>
		/// <returns><see langword="true"/>, if the iterator advanced; otherwise, <see langword="false"/>.</returns>
		public bool MoveNext()
		{
			if (m_ElapsedTime < m_TweenInfo.Duration)
			{
				m_ElapsedTime += !m_TweenInfo.IgnoreTimeScale ? Time.deltaTime : Time.unscaledDeltaTime;
				m_Percentage = Mathf.Clamp01(m_ElapsedTime / m_TweenInfo.Duration);
				m_TweenInfo.TweenValue(m_Percentage);
				return true;
			}
			m_TweenInfo.TweenValue(1f);
			return false;
		}

		/// <summary>
		/// Reset this instance.
		/// </summary>
		public void Reset()
		{
			throw new System.NotSupportedException();
		}
	}

	/// <summary>
	/// A class to run a color tween.
	/// </summary>
	/// <remarks>
	/// This class mirrors functionality found in <see cref="UnityEngine.UI.CoroutineTween.TweenRunner{T}"/>.
	/// </remarks>
	internal class Runner
	{
		/// <summary>
		/// The <see cref="UnityEngine.MonoBehaviour"/> that will run the coroutine.
		/// </summary>
		private readonly MonoBehaviour m_CoroutineContainer = null;
		/// <summary>
		/// The iterator.
		/// </summary>
		private IEnumerator m_Iterator;

		/// <summary>
		/// Initializes a new instance of the <see cref="Runner"/> class.
		/// </summary>
		/// <param name="container">Container.</param>
		public Runner(MonoBehaviour container)
		{
			m_CoroutineContainer = container;
		}

		/// <summary>
		/// Starts the tween.
		/// </summary>
		/// <param name="colorTweenInfo">Color tween info.</param>
		public void StartTween(Info colorTweenInfo)
		{
			if (m_Iterator != null)
			{
				m_CoroutineContainer.StopCoroutine(m_Iterator);
				m_Iterator = null;
			}
			if (!m_CoroutineContainer.gameObject.activeInHierarchy)
			{
				colorTweenInfo.TweenValue(1f);
				return;
			}
			m_Iterator = new Iterator(colorTweenInfo);
			m_CoroutineContainer.StartCoroutine(m_Iterator);
		}
	}
}
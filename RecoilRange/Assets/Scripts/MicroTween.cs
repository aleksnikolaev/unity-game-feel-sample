using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// A small tween helper written for this sample. It is not DOTween and does not
/// pretend to be: three easing curves and a coroutine are all this scene needs,
/// and keeping it in-repo means the timings can be read next to the code that
/// uses them.
/// </summary>
public static class MicroTween
{
	public static IEnumerator Run(float seconds, Action<float> step, Func<float, float> ease = null)
	{
		if (seconds <= 0f)
		{
			step(1f);
			yield break;
		}

		ease = ease ?? Linear;
		float elapsed = 0f;
		while (elapsed < seconds)
		{
			elapsed += Time.deltaTime;
			float t = Mathf.Clamp01(elapsed / seconds);
			step(ease(t));
			yield return null;
		}
		step(1f);
	}

	public static float Linear(float t)
	{
		return t;
	}

	public static float OutCubic(float t)
	{
		float inv = 1f - t;
		return 1f - inv * inv * inv;
	}

	/// <summary>Up fast, back down slower: the shape a hit wants.</summary>
	public static float Punch(float t)
	{
		return t < 0.3f ? t / 0.3f : 1f - (t - 0.3f) / 0.7f;
	}
}

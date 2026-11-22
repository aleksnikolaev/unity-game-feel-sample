using System.Collections;
using UnityEngine;

/// <summary>
/// The shortest pause in the sample and the one that does the most work. Time
/// is slowed to a crawl for a few tens of milliseconds on a hit, which reads as
/// weight rather than as a pause. It runs on unscaled time, otherwise it would
/// be waiting inside the slowdown it caused.
/// </summary>
public class Hitstop : MonoBehaviour
{
	Coroutine running;

	public void Freeze(float milliseconds, float scale)
	{
		if (milliseconds <= 0f)
		{
			return;
		}
		if (running != null)
		{
			StopCoroutine(running);
			Time.timeScale = 1f;
		}
		running = StartCoroutine(Routine(milliseconds / 1000f, scale));
	}

	IEnumerator Routine(float seconds, float scale)
	{
		Time.timeScale = scale;
		yield return new WaitForSecondsRealtime(seconds);
		Time.timeScale = 1f;
		running = null;
	}

	void OnDisable()
	{
		Time.timeScale = 1f;
	}
}
